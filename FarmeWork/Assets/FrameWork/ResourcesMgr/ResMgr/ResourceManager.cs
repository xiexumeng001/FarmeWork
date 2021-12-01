using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using UObject = UnityEngine.Object;
using SObject = System.Object;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif
using System;

namespace ShipFarmeWork.Resource
{
#if UNITY_EDITOR
    public class CloneNodeInEditorMode
    {
        public GameObject cloned;
        public SObject requester;
        public string bundle;
        public string asset;
    }
#endif

    public class ResourceManager : MonoBehaviour
    {
        public bool inited = false;
        private static ResourceManager instance;
        public static ResourceManager Instance { get { return instance; } }

        public static bool EditorMode = !ResourceConfig.BundleModeInEditor; //在编辑器上直接读取
        private Action OnResInitSuccess;   //当资源初始化成功

        //保存bundle信息的节点
        private Dictionary<string, AssetBundleInfoNode> m_assetBundleInfoNodes = new Dictionary<string, AssetBundleInfoNode>();
        //请求和bundle节点的关系表(只要是请求过的都会被放入到这个表中)，用来减少每次释放遍历的量
        private Dictionary<SObject, List<AssetBundleInfoNode>> m_requesterRelationShip = new Dictionary<SObject, List<AssetBundleInfoNode>>();
        public Dictionary<SObject, List<AssetBundleInfoNode>> RequesterRelationShip { get { return m_requesterRelationShip; } }

        private void Awake()
        {
            instance = this;

            Debug.Log("IsSafeCheck:" + ResourceConfig.IsSafeCheck);
        }

        /// <summary>
        /// 初始化资源
        /// </summary>
        public void InitRes(Action onResInitSuccess)
        {
            OnResInitSuccess = onResInitSuccess;

            //初始化热更界面
            FirstPage.Create();

            //开始热更
            ResourcesChecker resCheck = ResourcesChecker.CreateInstance(gameObject);
            resCheck.CheckResources(OnResourceCheckEnd);
        }

        private void OnResourceCheckEnd(ResCheckRst rst)
        {
            if (rst == ResCheckRst.LOW_VERSION)
            {
                //低版本强更
                FirstPage.Instance.UpdatePromptText("10023", false);
                string detail = ServerResConfig.ShowVersionDownloadTips;
                FirstPage.Instance.SetUpgradeTips(
                "10014",
                detail,
                delegate ()
                {
                    if (!string.IsNullOrEmpty(ServerResConfig.VersionDownloadURL))
                    {
                        WWW www = new WWW(ServerResConfig.VersionDownloadURL);
                        Application.OpenURL(www.url);
                    }
                    Application.Quit();
                },
                null,
                "10012",
                string.Empty);
            }
            else if (rst == ResCheckRst.SUCCESS)
            {
                Init(ResDefine.GetDataPath("StreamingAssets"));

                FirstPage.Instance.UpdatePromptText("10031", false);
                FirstPage.Instance.ActiveProgress(false);
                //初始化成功的回调
                if (OnResInitSuccess != null) OnResInitSuccess();

                FirstPage.Instance.Close();
            }
        }

        public void Init(string manifestPath)
        {
            if (inited)
                return;
            inited = true;

            if (ResourceConfig.IsSafeCheck)
            {
                ResourceSafe safe = gameObject.AddComponent<ResourceSafe>();
                safe.StartCheck(this);
            }

#if UNITY_EDITOR
            //创建资源关联
            if (EditorMode)
            {
                WarmUpShaders();
                return;
            }
            else
                instance.SetupBundleRelationships(manifestPath);
#else
            
            instance.SetupBundleRelationships(manifestPath);
#endif
        }

        //创建资源关系列表
        private void SetupBundleRelationships(string bundleManifestPath)
        {
            var bundle = AssetBundle.LoadFromFile(bundleManifestPath);
            if (bundle)
            {
                AssetBundleManifest abManifest = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                //建立关系
                if (abManifest == null)
                {
                    FirstPage.Instance.UpdatePromptText("10044", false);
                    throw new System.Exception("Setup bundle info nodes failed, none manifest file...");
                    //Debug.LogError("Setup bundle info nodes failed, none manifest file...");
                    //return;
                }
                string[] assets = abManifest.GetAllAssetBundles();
                foreach (var item in assets)
                {
                    if (!m_assetBundleInfoNodes.ContainsKey(item.ToLower()))
                    {
                        AssetBundleInfoNode node = new AssetBundleInfoNode(item);
                        m_assetBundleInfoNodes.Add(item.ToLower(), node);
                    }
                }
                //建立关系
                foreach (var item in m_assetBundleInfoNodes)
                {
                    //获取依赖
                    string[] dependencies = abManifest.GetDirectDependencies(item.Key);

                    //建立依赖关系
                    foreach (var dependence in dependencies)
                    {
                        //忽视对shaders的依赖
                        string abName = dependence.Substring(0, dependence.LastIndexOf("."));
                        bool isContain = false;
                        foreach (var shaderAbName in ResourceConfig.ShaderAbNameArr)
                        {
                            if (shaderAbName.ToLower().Equals(abName))
                            {
                                isContain = true; continue;
                            }
                        }
                        if (isContain)
                        {
                            Debug.Log(string.Format("忽视{0}对{1}的依赖关系", item, dependence));
                            continue;
                        }
                        if (m_assetBundleInfoNodes.ContainsKey(dependence.ToLower()))
                        {
                            AssetBundleInfoNode son = m_assetBundleInfoNodes[dependence];
                            AssetBundleInfoNode parent = item.Value;
                            if (parent.m_dependentNode.Contains(son))
                            {
                                //Debug.LogError(string.Format("Add dependence repetitive:{0} <- {1}", parent.m_bundleName, son.m_bundleName));
                            }
                            else if (son.m_beDependentNode.Contains(parent))
                            {
                                //Debug.LogError(string.Format("Add dependence repetitive:{0} -> {1}", son.m_bundleName, parent.m_bundleName));
                            }
                            else
                            {
                                //只记录关系，不增加引用计数
                                son.m_beDependentNode.Add(parent);
                                parent.m_dependentNode.Add(son);
                            }
                        }
                        else
                        {
                            Debug.LogError(string.Format("Bundle node error: can not find out dependence node, file name : {0}", dependence));
                        }
                    }
                }

                bundle.Unload(true);
                WarmUpShaders();
                //Desc();
            }
            else
            {
                FirstPage.Instance.UpdatePromptText("10044", false);
                throw new System.Exception("获取manifest文件出错！！");
            }
        }

        //获取bundle info node
        private AssetBundleInfoNode Get(string abName)
        {
            AssetBundleInfoNode node = null;
            if (!abName.Contains(ResourceConfig.AbSuffix)) { abName = abName + ResourceConfig.AbSuffix; }
            abName = abName.ToLower();

            if (m_assetBundleInfoNodes.TryGetValue(abName.ToLower(), out node))
                return node;
            else
            {
                Debug.LogError(string.Format("获取资源包 : {0} 失败了！", abName));
                return null;
            }
        }

#if UNITY_EDITOR
        private string GetInEditorPath(string relativePath, string fileName, System.Type type)
        {
            relativePath = relativePath.Trim();
            //先尝试移除后缀
            if (Path.HasExtension(relativePath))
            {
                relativePath = relativePath.Replace(Path.GetExtension(relativePath), string.Empty);
            }
            //做一次文件存在的判断
            string path = string.Format("{0}/{1}/{2}", Application.dataPath, ResourceConfig.ResRootDir, relativePath);
            //这是一个文件夹，表示fileName是有用的
            if (Directory.Exists(path))
            {
                path = string.Format("{0}/{1}/{2}/{3}", Application.dataPath, ResourceConfig.ResRootDir, relativePath, fileName);
            }

            //表示这是一个文件夹
            string suffix = string.Empty;
            if (type == typeof(Sprite) || type == typeof(Texture2D))
            {
                //图片需要多测试一次
                suffix = ".png";
                path += suffix;
                if (!File.Exists(path))
                    path = path.Replace(".png", ".jpg");
            }
            else if (type == typeof(GameObject))
            {
                suffix = ".prefab";
                path += suffix;
            }
            else if (type == typeof(Material))
            {
                suffix = ".mat";
                path += suffix;
            }
            else if (type == typeof(AudioClip))
            {
                suffix = ".ogg";
                path += suffix;
            }
            else if (type == typeof(Shader))
            {
                suffix = ".shader";
                path += suffix;

            }
            else if (type == typeof(UnityEngine.Tilemaps.TileBase))
            {
                suffix = ".asset";
                path += suffix;
            }
            else if (type == typeof(TextAsset))
            {
                //如果是TextAsset的话,就不加了,有可能是Json,也可能是xml,还可能是其他的,各自加各自的
            }
            else if (type == typeof(AnimationClip))
            {
                suffix = ".anim";
                path += suffix;
            }
            else
            {
                Debug.LogError(string.Format("本地尝试读取了错误的文件->{0},类型->{1}", fileName, type));
            }
            return path.Replace(Application.dataPath, "Assets").Replace("\\", "/");
        }

        private List<CloneNodeInEditorMode> m_cloneNodes = new List<CloneNodeInEditorMode>();

        private GameObject CloneGameObjInEditorMode(GameObject model, SObject requester, string bundle, string asset)
        {
            if (model == null)
                return null;

            GameObject clone = Instantiate(model);
            if (clone != null)
            {
                clone.SetActive(false);
                CloneNodeInEditorMode node = new CloneNodeInEditorMode();
                node.cloned = clone;
                node.requester = requester;
                node.bundle = bundle;
                node.asset = asset;
                m_cloneNodes.Add(node);
            }

            return clone;
        }
#endif

        public bool RequestAllAssetsFromBundles(string[] bundles, SObject requester, System.Action<string> bundleCompleteCallback)
        {
            if (bundles == null || bundles.Length == 0)
                return false;

            bool allRight = true;

            //本地下请求资源
#if UNITY_EDITOR
            if (EditorMode)
            {
                //编辑器模式下不做检测，直接认为存在
                foreach (var item in bundles)
                {
                    bundleCompleteCallback(item);
                }
                return true;
            }
#endif

            //使用bundle方式请求全部资源
            for (int i = 0; i < bundles.Length; ++i)
            {
                AssetBundleInfoNode node = Get(bundles[i]);
                if (node != null)
                {
                    //已经全部加载过了
                    if (node.m_allLoaded)
                    {
                        if (bundleCompleteCallback != null)
                            bundleCompleteCallback(bundles[i]);
                    }
                    //可能并没有全部加载，开始加载吧
                    else
                    {
                        node.RequestAllAssets(requester, bundleCompleteCallback);
                        SetupRequesterRelationShip(requester, node);
                    }
                }
                else
                    allRight = false;
            }

            return allRight;
        }

        public bool RequestAssetsFromBundle(string bundle, string[] assets, System.Type[] types, SObject requester, System.Action<string, string[], System.Type[]> assetsCompleteCallback)
        {
            if (bundle == string.Empty
                || assets == null
                || types == null
                || assets.Length != types.Length)
                return false;


            //本地下请求资源
#if UNITY_EDITOR
            if (EditorMode)
            {
                //编辑器模式下不做检测，直接认为存在
                assetsCompleteCallback(bundle, assets, types);
                return true;
            }
#endif

            AssetBundleInfoNode node = Get(bundle);
            if (node != null)
            {
                if (!node.CheckAssetInThisBundle(assets))
                    return false;

                if (node.m_allLoaded)
                {
                    if (assetsCompleteCallback != null)
                        assetsCompleteCallback(bundle, assets, types);
                }
                else
                {
                    node.RequestAssets(assets, types, requester, assetsCompleteCallback);
                    SetupRequesterRelationShip(requester, node);
                }

                return true;
            }
            return false;
        }

        public UObject GetAsset(string bundle, string asset, System.Type assetType, SObject requester)
        {
            if (bundle == string.Empty
                || asset == string.Empty
                || assetType == null)
                return null;

            //本地下请求资源
#if UNITY_EDITOR
            if (EditorMode)
            {
                string inEditorPath = GetInEditorPath(bundle, asset, assetType);
                var obj = AssetDatabase.LoadAssetAtPath(inEditorPath, assetType);
                if (obj == null)
                {
                    bundle = bundle.Replace("/" + asset, "");
                    inEditorPath = GetInEditorPath(bundle, asset, assetType);
                    obj = AssetDatabase.LoadAssetAtPath(inEditorPath, assetType);
                }

                if (ResourceConfig.IsSafeCheck)
                {
                    if (obj != null && !(obj is GameObject))
                    {
                        ResourceSafe.Instance.AddAssetRequestSafeRecord(requester, bundle, asset, obj);
                    }
                }
                return obj;
            }
#endif

            AssetBundleInfoNode node = Get(bundle);
            if (node == null)
                return null;

            SetupRequesterRelationShip(requester, node);

            var objRequest = node.GetAsset(asset, assetType, requester);
#if UNITY_EDITOR

            if (assetType == typeof(Material))
            {
                Material mat = objRequest as Material;
                mat.shader = Shader.Find(mat.shader.name);
            }
#endif
            return objRequest;
        }

        public T GetAsset<T>(string bundle, string asset, SObject requester)
            where T : UObject
        {
            return (T)GetAsset(bundle, asset, typeof(T), requester);
        }

        public UObject[] GetAssets(string bundle, SObject requester, System.Type assetType)
        {
            if (bundle == string.Empty)
                return null;

#if UNITY_EDITOR
            if (EditorMode)
            {
                string path = string.Format("{0}/{1}/{2}", Application.dataPath, ResourceConfig.ResRootDir, bundle);
                if (!Directory.Exists(path))
                {
                    Debug.LogError(string.Format("本地尝试获取所有资源出错，目标位置并非一个文件夹：{0}", bundle));
                    return null;
                }
                List<UObject> objects = new List<UObject>();
                if (assetType == typeof(GameObject))
                {
                    string[] files = Directory.GetFiles(path, "*.prefab", SearchOption.AllDirectories);
                    for (int i = 0; i < files.Length; ++i)
                    {
                        UObject obj = AssetDatabase.LoadAssetAtPath(files[i].Replace(Application.dataPath, "Assets"), assetType);
                        if (obj != null)
                            objects.Add(obj);
                    }
                }
                else if (assetType == typeof(Sprite) || assetType == typeof(Texture2D))
                {
                    string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".png") || s.EndsWith(".jpg")).ToArray();
                    for (int i = 0; i < files.Length; ++i)
                    {
                        UObject obj = AssetDatabase.LoadAssetAtPath(files[i].Replace(Application.dataPath, "Assets"), assetType);
                        if (obj != null)
                            objects.Add(obj);
                    }
                }
                if (assetType == typeof(GameObject))
                {
                    string[] files = Directory.GetFiles(path, "*.mat", SearchOption.AllDirectories);
                    for (int i = 0; i < files.Length; ++i)
                    {
                        UObject obj = AssetDatabase.LoadAssetAtPath(files[i].Replace(Application.dataPath, "Assets"), assetType);
                        if (obj != null)
                            objects.Add(obj);
                    }
                }
                if (assetType == typeof(Shader))
                {
                    string[] files = Directory.GetFiles(path, "*.shader", SearchOption.AllDirectories);
                    for (int i = 0; i < files.Length; ++i)
                    {
                        UObject obj = AssetDatabase.LoadAssetAtPath(files[i].Replace(Application.dataPath, "Assets"), assetType);
                        if (obj != null)
                            objects.Add(obj);
                    }
                }
                return objects.ToArray();
            }
#endif
            AssetBundleInfoNode node = Get(bundle);
            if (node == null)
            {
                Debug.LogError(string.Format("尝试获取 {0} 全部资源失败，没有这个bundle", bundle));
                return null;
            }

            return node.GetAssets(requester, assetType);
        }

        public GameObject CloneAsset(string bundle, string asset, SObject requester)
        {
            if (bundle == string.Empty
                || asset == string.Empty
                || requester == null)
            {
                Debug.LogError(string.Format("克隆资源出错：{0} -> {1} from {2}", bundle ?? "null", asset ?? "null", requester ?? "null"));
                return null;
            }

            //hack【资源管理】Single打包情况目前没有什么好的处理办法
            bundle = bundle + "/" + asset;
            //本地下请求资源
#if UNITY_EDITOR
            if (EditorMode)
            {
                var obj = GetAsset<GameObject>(bundle, asset, requester);
                if (obj != null)
                {
                    GameObject cloneGame = CloneGameObjInEditorMode(obj, requester, bundle, asset);

                    if (ResourceConfig.IsSafeCheck)
                    {
                        if (requester != null && obj != null)
                        {
                            ResourceSafe.Instance.AddAssetRequestSafeRecord(requester, bundle, asset, cloneGame);
                        }
                    }

                    return cloneGame;
                }
                else
                {
                    return null;
                }

            }
#endif

            AssetBundleInfoNode node = Get(bundle);
            if (node == null)
            {
                Debug.LogError(string.Format("clone {0} 失败，没有这个bundle", bundle));
                return null;
            }

            SetupRequesterRelationShip(requester, node);

#if UNITY_EDITOR
            GameObject cloneObj = node.CloneAsset(asset, requester);
            Renderer[] renderers = cloneObj.GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
            {
                renderer.material.shader = Shader.Find(renderer.material.shader.name);
            }
            Image[] images = cloneObj.GetComponentsInChildren<Image>(true);
            foreach (var item in images)
            {
                item.material.shader = Shader.Find(item.material.shader.name);
            }
            return cloneObj;
#endif

            return node.CloneAsset(asset, requester);
        }

        public void SetClonePoolCapacity(string bundle, string asset, int capacity)
        {
#if UNITY_EDITOR
            if (EditorMode)
            {
                return;
            }
#endif

            if (bundle == string.Empty || asset == string.Empty)
                return;

            AssetBundleInfoNode node = Get(bundle);
            if (node == null)
            {
                Debug.LogError(string.Format("Capackty {0} 失败，没有这个bundle", bundle));
                return;
            }

            node.SetAssetCapacity(asset, capacity);
        }

        public void ReturnClones(string bundle, string assetName, GameObject[] clones)
        {
#if UNITY_EDITOR
            if (EditorMode)
            {
                if (ResourceConfig.IsSafeCheck)
                {
                    for (int i = 0; i < clones.Length; i++)
                    {
                        ResourceSafe.Instance.RemoveAssetRequestSafeRecord(clones[i]);
                    }
                }

                for (int i = m_cloneNodes.Count - 1; i >= 0; --i)
                {
                    var item = m_cloneNodes[i];
                    foreach (var clone in clones)
                    {
                        if (item.cloned.GetInstanceID() == clone.GetInstanceID()
                            && item.bundle == bundle && item.asset == assetName)
                        {
                            Destroy(item.cloned);
                            m_cloneNodes.RemoveAt(i);
                            break;
                        }
                    }
                }
                return;
            }
#endif
            if (bundle == string.Empty
                || assetName == string.Empty
                || clones == null
                || clones.Length == 0)
                return;

            AssetBundleInfoNode node = Get(bundle);
            if (node == null)
            {
                Debug.LogError(string.Format("尝试归还{0}的clone失败  ，没有这个bundle", bundle));
            }
            node.ReturnClones(assetName, clones);
        }

        //归还单个克隆物体
        public void ReturnCloneOne(string bundle, string assetName, GameObject clone)
        {
#if UNITY_EDITOR
            if (EditorMode)
            {
                if (ResourceConfig.IsSafeCheck)
                {
                    ResourceSafe.Instance.RemoveAssetRequestSafeRecord(clone);
                }

                if (bundle == string.Empty
                    || assetName == string.Empty
                    || clone == null)
                    return;

                for (int i = m_cloneNodes.Count - 1; i >= 0; --i)
                {
                    var item = m_cloneNodes[i];
                    if (item.cloned.GetInstanceID() == clone.GetInstanceID()
                        && item.bundle == bundle
                        && item.asset == assetName)
                    {
                        Destroy(item.cloned);
                        m_cloneNodes.RemoveAt(i);
                        break;
                    }
                }
                return;
            }
#endif
            if (bundle == string.Empty
                || assetName == string.Empty
                || clone == null)
                return;

            //hack【资源管理】Single打包情况目前没有什么好的处理办法,而且返还资源的接口有 ReturnBundleByName 这个,不能用这个方法
            bundle = bundle + "/" + assetName;
            AssetBundleInfoNode node = Get(bundle);
            if (node == null)
            {
                Debug.LogError(string.Format("尝试归还{0}的clone失败  ，没有这个bundle", bundle));
            }
            //Debug.LogError("bundle:"+ bundle + "assetName:"+ assetName);
            node.ReturnCloneOne(assetName, clone);
        }


        public void ReturnBundleByName(SObject requester, string bundle, bool unloadAll = true)
        {
#if UNITY_EDITOR
            if (EditorMode)
            {
                if (ResourceConfig.IsSafeCheck)
                {
                    ResourceSafe.Instance.RemoveAssetRequestSafeRecord(requester, bundle);
                }
                for (int i = m_cloneNodes.Count - 1; i >= 0; --i)
                {
                    var item = m_cloneNodes[i];
                    if (item.requester == requester && item.bundle == bundle)
                    {
                        Destroy(item.cloned);
                        m_cloneNodes.RemoveAt(i);
                    }
                }
                return;
            }
#endif
            if (requester == null || bundle == string.Empty)
                return;

            List<AssetBundleInfoNode> tempList = null;
            if (!m_requesterRelationShip.TryGetValue(requester, out tempList))
            {
                Debug.LogError(string.Format("尝试按照名字归还bundle失败：{0}，没有关联过这个requester"));
                return;
            }

            AssetBundleInfoNode node = Get(bundle);
            if (node == null)
            {
                Debug.LogError(string.Format("尝试按照名字归还bundle失败：{0}，没有找到这个bundle"));
                return;
            }

            if (tempList.Contains(node))
            {
                node.ReturnAssetByRequester(requester, unloadAll);
            }
            else
                Debug.LogError(string.Format("尝试按照名字归还bundle失败：{0}，requester 没有关联过这个 bundle"));

        }

        public void CancleRequest(SObject requester)
        {
#if UNITY_EDITOR
            if (EditorMode)
            {
                return;
            }
#endif
            //本地下请求资源
            if (requester == null)
                return;

            if (!m_requesterRelationShip.ContainsKey(requester) || m_requesterRelationShip[requester] == null)
                return;

            foreach (var item in m_requesterRelationShip[requester])
            {
                item.CancelRequest(requester);
            }
        }

        private void SetupRequesterRelationShip(SObject requester, AssetBundleInfoNode abInfoNode)
        {
            List<AssetBundleInfoNode> temp = null;
            if (!m_requesterRelationShip.TryGetValue(requester, out temp))
            {
                temp = new List<AssetBundleInfoNode>();
                m_requesterRelationShip.Add(requester, temp);
            }
            if (!temp.Contains(abInfoNode))
                temp.Add(abInfoNode);
        }

        public void ReturnAllByRequester(SObject requester, bool cancelRequest = true, bool unloadAll = true)
        {
#if UNITY_EDITOR
            if (EditorMode)
            {
                if (ResourceConfig.IsSafeCheck)
                {
                    ResourceSafe.Instance.RemoveAssetRequestSafeRecord(requester);
                }

                for (int i = m_cloneNodes.Count - 1; i >= 0; --i)
                {
                    var item = m_cloneNodes[i];
                    if (item.requester == requester)
                    {
                        Destroy(item.cloned);
                        m_cloneNodes.RemoveAt(i);
                    }
                }
                return;
            }
#endif

            if (requester == null)
                return;

            if (!m_requesterRelationShip.ContainsKey(requester) || m_requesterRelationShip[requester] == null)
            {
                // Debug.LogWarning("尝试归还资源失败，资源管理器没有保存这个 requester 对应的Asset Bundle Info Node");
                return;
            }

            if (cancelRequest)
            {
#if UNITY_EDITOR
                // Debug.LogWarning(string.Format("<color=#ffff00>cancle request -> {0}</color>", requester.ToString()));
#endif
                CancleRequest(requester);
            }

            foreach (var item in m_requesterRelationShip[requester])
            {

#if UNITY_EDITOR
                // Debug.LogWarning(string.Format("<color=#ffff00>ReturnAssetByRequester -> {0}</color>", requester.ToString()));
#endif
                item.ReturnAssetByRequester(requester, unloadAll);
            }

            if (cancelRequest)
            {

#if UNITY_EDITOR
                // Debug.LogWarning(string.Format("<color=#ffff00>remove from relation ship -> {0}</color>", requester.ToString()));
#endif
                //如果在归还资源时同时移除了回调，则将重置这个节点和AssetBundleInfoNode的关系
                m_requesterRelationShip.Remove(requester);
            }
        }

        public void BundleAllAssetsLoadHelperAsync(AssetBundleInfoNode node)
        {
            if (node.m_allLoaded || !node.Bundle)
                return;

            StartCoroutine(LoadAllAssets(node));
        }

        public void BundleAssetsLoadHelperAsync(AssetBundleInfoNode node, string[] assets, System.Type[] types)
        {
            if (node.m_allLoaded || !node.Bundle)
                return;

            StartCoroutine(LoadAssets(node, assets, types));
        }

        private IEnumerator LoadAssets(AssetBundleInfoNode node, string[] assets, System.Type[] types)
        {
            if (node == null
                || assets == null
                || types == null
                || assets.Length != types.Length)
            {
                Debug.LogError("尝试异步加载资源出错了！");
                yield break;
            }

            List<string> assetNames = new List<string>();
            List<System.Type> assetTypes = new List<System.Type>();
            for (int i = 0; i < assets.Length; ++i)
            {
                AssetBundleRequest req = node.Bundle.LoadAssetAsync(assets[i], types[i]);
                yield return req;
                if (req.isDone && req.asset != null)
                {
                    assetNames.Add(req.asset.name);
                    assetTypes.Add(req.asset.GetType());
                }
                else
                {
                    Debug.LogError(string.Format("错误！加载{0}下{1}({2})时可能出现了问题！！推测为类型错误！可能导致无法移除请求！", node.m_bundleName, assets[i], types[i].ToString()));
                }
            }

            if (assetNames.Count > 0)
            {
                node.AssetsLoaded(assetNames.ToArray(), assetTypes.ToArray());
            }
        }

        private IEnumerator LoadAllAssets(AssetBundleInfoNode node)
        {
            AssetBundleRequest req = node.Bundle.LoadAllAssetsAsync();
            yield return req;
            //读取完毕
            node.AllAssetsLoaded();
        }

        /// <summary>
        /// 重置资源管理器，彻底释放所有他妈的资源草
        /// 调用过这个函数前再次使用请一定要调用一次Init()
        /// </summary>
        public void Renew()
        {
            foreach (var item in m_assetBundleInfoNodes)
            {
                item.Value.ForceUnload();
            }
            Debug.LogWarning("Renew resource manager!");
            Debug.LogWarning("Invoke init function before use it again.");
            m_assetBundleInfoNodes.Clear();
            m_requesterRelationShip.Clear();
            inited = false;
        }

        public void Desc()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("当前Bundle节点情况：\n");
            builder.Append("已被激活：\n");
            foreach (var item in m_assetBundleInfoNodes)
            {
                if (item.Value.m_assetBundle != null)
                {
                    builder.Append(item.Value.Desc(false));
                    builder.Append("==========================\n\n");
                }
            }
            builder.Append("\n未被激活：\n");
            foreach (var item in m_assetBundleInfoNodes)
            {
                if (item.Value.m_assetBundle == null)
                {
                    builder.Append(item.Value.Desc(false));
                    builder.Append("==========================\n\n");
                }
            }
            Debug.Log(builder);
        }

        public void DescUsed()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("当前激活的Bundle节点情况\n");
            foreach (var item in m_assetBundleInfoNodes)
            {
                if (item.Value.m_assetBundle == null)
                    continue;

                builder.Append(item.Value.Desc(false));
            }
            Debug.Log(builder);
        }

        string debugText_4 = "";
        string debugText_5 = "";
        string debugText_6 = "";

        private void OnGUI()
        {
            return;
            debugText_4 = GUI.TextField(new Rect(Screen.width - 400, 240, 200, 50), debugText_4);
            debugText_5 = GUI.TextField(new Rect(Screen.width - 400, 300, 200, 50), debugText_5);
            debugText_6 = GUI.TextField(new Rect(Screen.width - 400, 360, 200, 50), debugText_6);
            if (GUI.Button(new Rect(Screen.width - 200, 240, 200, 50), "Clone"))
            {
                int count = 0;
                int.TryParse(debugText_4, out count);
                while (count > 0)
                {
                    --count;
                    var clone = CloneAsset("abs/prefabs/ui/uiview_mainbattle.unity3d", "UIView_MainBattle", this);
                    clone.transform.SetParent(null);
                    clone.name = "a";
                }
            }
            else if (GUI.Button(new Rect(Screen.width - 200, 300, 200, 50), "Return"))
            {
                ReturnAllByRequester(this);
            }
            else if (GUI.Button(new Rect(Screen.width - 200, 360, 200, 50), "Capacity"))
            {
                int count = 0;
                int.TryParse(debugText_6, out count);
                SetClonePoolCapacity("abs/prefabs/ui/uiview_mainbattle.unity3d", "UIView_MainBattle", count);
            }
            else if (GUI.Button(new Rect(Screen.width - 200, 140, 200, 50), "Desc"))
            {
                Desc();
            }
        }

        public void ResetBundleRelationships(string bundleManifestPath)
        {
            var bundle = AssetBundle.LoadFromFile(bundleManifestPath);
            if (bundle)
            {
                AssetBundleManifest abManifest = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                //建立关系
                if (abManifest == null)
                {
                    Debug.LogError("Setup bundle info nodes failed, none manifest file...");
                    return;
                }
                string[] assets = abManifest.GetAllAssetBundles();
                foreach (var item in assets)
                {
                    if (!m_assetBundleInfoNodes.ContainsKey(item.ToLower()))
                    {
                        AssetBundleInfoNode node = new AssetBundleInfoNode(item);
                        m_assetBundleInfoNodes.Add(item.ToLower(), node);
                        Debug.LogWarning("Attached" + item.ToLower());
                    }
                }//建立关系
                foreach (var item in m_assetBundleInfoNodes)
                {
                    //获取依赖
                    string[] dependencies = abManifest.GetDirectDependencies(item.Key);

                    //建立依赖关系
                    foreach (var dependence in dependencies)
                    {
                        //忽视对shaders的依赖
                        if (dependence.Contains("shaders.unity3d"))
                        {
                            Debug.Log(string.Format("重置关系树时，忽视{0}对{1}的依赖关系", item, dependence));
                            continue;
                        }
                        if (m_assetBundleInfoNodes.ContainsKey(dependence.ToLower()))
                        {
                            AssetBundleInfoNode son = m_assetBundleInfoNodes[dependence];
                            AssetBundleInfoNode parent = item.Value;
                            if (!parent.m_dependentNode.Contains(son))
                            {
                                parent.m_dependentNode.Add(son);
                                //Debug.LogError(parent.Bundle.name + " -> " + son.Bundle.name);
                            }
                            if (!son.m_beDependentNode.Contains(parent))
                            {
                                son.m_beDependentNode.Add(parent);
                                //Debug.LogError(parent.Bundle.name + " <- " + son.Bundle.name);
                            }
                        }
                    }
                }
                bundle.Unload(true);
                WarmUpShaders();
            }
        }

        public void WarmUpShaders()
        {
            object obj = new SObject();
            foreach (var shaderAbName in ResourceConfig.ShaderAbNameArr)
            {
                UObject[] shaders = GetAssets(shaderAbName, obj, typeof(Shader));
                foreach (var item in shaders)
                {
                    Debug.Log("预热" + item.name);
                }
            }

            Shader.WarmupAllShaders();
            ReturnAllByRequester(obj);

            Debug.Log("Warm up all shaders.");
        }
    }
}