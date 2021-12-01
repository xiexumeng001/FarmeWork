using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using SObject = System.Object;
using UObject = UnityEngine.Object;

namespace ShipFarmeWork.Resource
{
    public class AssetBundleInfoNode
    {
        public string m_bundleName;

        public AssetBundle m_assetBundle;
        public Dictionary<string, CloneanbleAssetNode> m_cloneableAssetDic = new Dictionary<string, CloneanbleAssetNode>();
        public HashSet<System.Object> m_originalAssetRequesters = new HashSet<System.Object>();

        public int m_beDependentOn;                                                   //被引用数量
        public int m_totalAssetsCount;                                                //资源总数
        public bool m_allLoaded = false;                                              //是否被全部加载过了？

        //只是记录资源之间的关系！
        public List<AssetBundleInfoNode> m_dependentNode = new List<AssetBundleInfoNode>();     //引用到的节点
        public List<AssetBundleInfoNode> m_beDependentNode = new List<AssetBundleInfoNode>();   //被引用的节点

        //当前节点的持有对象

        //保留资源请求
        public List<AssetRequest> m_assetsRequest = new List<AssetRequest>();

        private AssetBundleInfoNode() { }

        public AssetBundleInfoNode(string bundleName)
        {
            this.m_bundleName = bundleName.ToLower();
            m_totalAssetsCount = 0;
            m_allLoaded = false;
        }

        public AssetBundle Bundle
        {
            get
            {
                if (m_assetBundle == null)
                {
                    m_assetBundle = AssetBundle.LoadFromFile(ResDefine.GetDataPath(m_bundleName));
                    if (!m_assetBundle)
                    {
                        Debug.LogError(string.Format("Load bundle failed... None bundle named {0}", m_bundleName));
                        return null;
                    }
                    else
                    {
                        m_cloneableAssetDic.Clear();
                        //获取资源总数
                        m_totalAssetsCount = m_assetBundle.GetAllAssetNames().Length;
                    }
                }
                return m_assetBundle;
            }
        }

        //请求这个bundle下所有的资源
        public void RequestAllAssets(System.Object requester, System.Action<string> bundleCallback)
        {
            //建立一个资源加载请求，资源加载完毕后通知
            AssetRequest req = AssetRequest.Create(m_bundleName, null, null, requester, bundleCallback, null);
            if (req != null)
            {
                if (!Bundle || !CheckDependencies())
                {
                    Debug.LogError("依赖项检测失败，无法建立资源加载请求。");
                    return;
                }

                m_assetsRequest.Add(req);

                //使用资源管理器统一的异步接口进行加载
                ResourceManager.Instance.BundleAllAssetsLoadHelperAsync(this);
            }
        }

        //请求这个bundle下的部分资源
        public void RequestAssets(string[] assetsName, System.Type[] assetsType, System.Object requester, System.Action<string, string[], System.Type[]> assetCallback)
        {
            AssetRequest req = AssetRequest.Create(m_bundleName, assetsName, assetsType, requester, null, assetCallback);
            if (req != null)
            {
                if (!Bundle || !CheckDependencies())
                {
                    Debug.LogError("依赖项检测失败，无法建立资源加载请求。");
                    return;
                }

                m_assetsRequest.Add(req);

                ResourceManager.Instance.BundleAssetsLoadHelperAsync(this, assetsName, assetsType);
            }
        }

        //取消请求
        public void CancelRequest(System.Object requester)
        {
            for (int i = m_assetsRequest.Count - 1; i >= 0; --i)
            {
                if (m_assetsRequest[i].Requester == requester)
                {
                    // Debug.Log(string.Format("取消了{0}资源请求！", m_bundleName));
                    m_assetsRequest.RemoveAt(i);
                }
            }
        }

        //直接获取资源
        public UObject GetAsset(string assetName, System.Type assetType, SObject requester)
        {
            if (!Bundle || !CheckDependencies())
            {
                Debug.LogError("依赖项检测失败，无法直接获取资源呢！");
                return null;
            }
            //记录请求，没有请求人，也可以给资源，但是不安全
            if (requester != null && !m_originalAssetRequesters.Contains(requester))
            {
                m_originalAssetRequesters.Add(requester);
            }
            var asset = Bundle.LoadAsset(assetName, assetType);

            if (ResourceConfig.IsSafeCheck)
            {
                if (requester != null && asset != null && !(asset is GameObject))
                {
                    ResourceSafe.Instance.AddAssetRequestSafeRecord(requester, m_bundleName, assetName, asset);
                }
            }
            return asset;
        }

        //直接获取全部资源
        public UObject[] GetAssets(SObject requester, System.Type assetType)
        {
            if (!Bundle || !CheckDependencies())
            {
                Debug.LogError("依赖项检测失败，无法直接获取资源呢！");
                return null;
            }
            //记录请求，没有请求人，也可以给资源，但是不安全
            if (requester != null && !m_originalAssetRequesters.Contains(requester))
            {
                m_originalAssetRequesters.Add(requester);
            }
            var assets = Bundle.LoadAllAssets(assetType);
            return assets;
        }

        public void ReturnClones(string assetName, GameObject[] clones)
        {
            if (assetName == string.Empty
                || clones == null
                || clones.Length == 0)
                return;
            assetName = assetName.ToLower();
            if (!m_cloneableAssetDic.ContainsKey(assetName))
            {
                Debug.LogWarning(string.Format("尝试归还{0}的clone失败，没有这个asset"));
                return;
            }

            CloneanbleAssetNode cloneNode = m_cloneableAssetDic[assetName];

            cloneNode.ReturnClones(clones);
        }

        //归还单个资源
        public void ReturnCloneOne(string assetName, GameObject clone)
        {
            if (assetName == string.Empty
                || clone == null)
                return;
            assetName = assetName.ToLower();
            if (!m_cloneableAssetDic.ContainsKey(assetName))
            {
                Debug.LogWarning(string.Format("尝试归还{0}的clone失败，没有这个asset",assetName));
                return;
            }

            CloneanbleAssetNode cloneNode = m_cloneableAssetDic[assetName];

            cloneNode.ReturnCloneOne(clone);
        }



        //直接获取资源
        public T GetAsset<T>(string assetName, SObject requester)
            where T : UObject
        {
            if (!Bundle || !CheckDependencies())
            {
                Debug.LogError("依赖项检测失败，无法直接获取资源呢！");
                return null;
            }
            //记录请求，没有请求人，也可以给资源，但是不安全
            if (requester != null && !m_originalAssetRequesters.Contains(requester))
            {
                m_originalAssetRequesters.Add(requester);
            }
            var asset = Bundle.LoadAsset<T>(assetName);

            if (ResourceConfig.IsSafeCheck)
            {
                if (requester != null && asset != null && !(asset is GameObject))
                {
                    ResourceSafe.Instance.AddAssetRequestSafeRecord(requester, m_bundleName, assetName, asset);
                }
            }
            return asset;
        }

        //获取资源的副本
        public GameObject CloneAsset(string assetName, SObject requester)
        {
            //检查资源
            if (requester == null
                || !Bundle
                || !Bundle.Contains(assetName))
                return null;

            CloneanbleAssetNode assetInfoNode = null;
            assetName = assetName.ToLower();
            if (!m_cloneableAssetDic.TryGetValue(assetName, out assetInfoNode))
            {
                //尝试获取(因为GameObj通常为clone,因此暂时无需requester)
                GameObject origin = GetAsset<GameObject>(assetName, null);
                if (!origin)
                {
                    Debug.LogError(string.Format("Clone gameobj: {0}失败，没有这个对象！", assetName));
                    return null;
                }
                assetInfoNode = CloneanbleAssetNode.Create(origin);
                if (assetInfoNode != null)
                    m_cloneableAssetDic.Add(assetName, assetInfoNode);
            }
            if (assetInfoNode != null)
            {
                var asset = assetInfoNode.Clone(requester);

                if (ResourceConfig.IsSafeCheck)
                {
                    if (requester != null && asset != null)
                    {
                        ResourceSafe.Instance.AddAssetRequestSafeRecord(requester, m_bundleName, assetName, asset);
                    }
                }
                return asset;
            }
            return null;
        }

        public void SetAssetCapacity(string assetName, int capacity)
        {
            if (capacity < 0)
                return;

            //检查资源
            if (!Bundle
                || !Bundle.Contains(assetName))
                return;

            CloneanbleAssetNode assetInfoNode = null;
            assetName = assetName.ToLower();
            if (!m_cloneableAssetDic.TryGetValue(assetName, out assetInfoNode))
            {
                //尝试获取(因为GameObj通常为clone,因此暂时无需requester)
                GameObject origin = GetAsset<GameObject>(assetName, null);
                if (!origin)
                {
                    Debug.LogError(string.Format("Capacity gameobj: {0}失败，没有这个对象！", assetName));
                    return;
                }
                assetInfoNode = CloneanbleAssetNode.Create(origin);
                if (assetInfoNode != null)
                    m_cloneableAssetDic.Add(assetName, assetInfoNode);
            }
            if (assetInfoNode != null)
                assetInfoNode.PoolCapacity = capacity;

        }

        //归还资源占用
        public void ReturnAssetByRequester(SObject requester, bool unloadAll)
        {
            if (requester == null)
                return;

            //清空元资源占用
            m_originalAssetRequesters.Remove(requester);
            if (ResourceConfig.IsSafeCheck)
            {
                ResourceSafe.Instance.RemoveAssetRequestSafeRecord(requester, m_bundleName);
            }

            //清空clone资源占用
            foreach (var asset in m_cloneableAssetDic)
            {
                if (asset.Value != null)
                {
                    asset.Value.ReturnByQuester(requester);
                }
            }

            Unload(unloadAll);
        }

        public bool CheckAssetInThisBundle(string[] assetName)
        {
            for (int i = 0; i < assetName.Length; ++i)
            {
                if (!Bundle.Contains(assetName[i]))
                {
                    Debug.LogError(string.Format("{0}不包含名为：{1}的资源！", m_bundleName, assetName[i]));
                    return false;
                }
            }
            return true;
        }

        //通常只发生在准备或直接加载资源时，需要先将依赖的AB文件加载
        public bool CheckDependencies()
        {
            for (int i = 0; i < m_dependentNode.Count; ++i)
            {
                if (m_dependentNode[i].Bundle == null)
                    return false;

                //这时需要为每一个依赖项进行标记
                if (!m_dependentNode[i].m_originalAssetRequesters.Contains(this))
                    m_dependentNode[i].m_originalAssetRequesters.Add(this);

                if (!m_dependentNode[i].CheckDependencies())
                    return false;
            }
            
            return true;
        }

        public void AssetsLoaded(string[] assets, System.Type[] types)
        {
            //检查所有的请求
            for (int i = m_assetsRequest.Count - 1; i >= 0; --i)
            {
                //通知并尝试移除
                if (m_assetsRequest[i].AssetsReady(assets, types))
                    m_assetsRequest.RemoveAt(i);
            }
        }

        public void AllAssetsLoaded()
        {
            // Debug.Log(m_bundleName + "全部资源读取完毕");
            m_allLoaded = true;
            //通知并清空
            for (int i = m_assetsRequest.Count - 1; i >= 0; --i)
            {
                m_assetsRequest[i].AllAssetsReady();
            }
            m_assetsRequest.Clear();
        }

        /// <summary>
        /// 卸载资源，彻底卸载
        /// </summary>
        /// <param name="unloadAll"></param>
        private void Unload(bool unloadAll)
        {
            //都没加过Assetbundle，填什么乱
            if (m_assetBundle == null)
            {
                Debug.LogWarning(string.Format("想要尝试卸载一个没有被加载过的bundle：{0}", m_bundleName));
                return;
            }

            if (!CheckCanUnload())
            {
                // Debug.Log(string.Format("尝试卸载bundle及关联池，失败。", m_bundleName));
                return;
            }

            //检测通过，可以移除自己的占用了（对于依赖项的占用）
            for (int i = 0; i < m_dependentNode.Count; ++i)
            {
                m_dependentNode[i].ReturnAssetByRequester(this, unloadAll);
            }

            //开始卸载
            DoUnload(unloadAll);
        }

        //执行卸载，通常在检测通过后执行
        private void DoUnload(bool unloadAll)
        {
            m_assetBundle.Unload(unloadAll);
            m_assetBundle = null;

#if UNITY_EDITOR
            Debug.LogWarning(string.Format("<color=#00ff00>卸载bundle -> {0}</color>", m_bundleName));
#endif

            //删除clone
            foreach (var item in m_cloneableAssetDic)
            {
                item.Value.Unload();
            }
            m_cloneableAssetDic.Clear();
            //清空元资源的占用
            m_originalAssetRequesters.Clear();
            //清空资源请求列表
            m_assetsRequest.Clear();
            m_allLoaded = false;

            // Debug.Log(string.Format("{0} 资源删除完毕。删除方式{1}", m_bundleName, unloadAll ? "彻底" : "非彻底"));
        }

        //检查依赖性
        private bool CheckCanUnload()
        {
            // Debug.Log(string.Format("开始检测{0}的依赖性:", m_bundleName));

            if (m_assetsRequest.Count > 0)
            {
                // Debug.Log(string.Format("{0}的依赖性检测  不通过，当前有对象正在请求资源。", m_bundleName));
                return false;
            }

            if (m_originalAssetRequesters.Count > 0)
            {
                // Debug.Log(string.Format("{0}的依赖性检测  不通过，当前 元资源 被持有。", m_bundleName));
                return false;
            }

            foreach (var item in m_cloneableAssetDic)
            {
                if (item.Value != null && !item.Value.CheckCanUnload())
                {
                    // Debug.Log(string.Format("{0}的依赖性检测  不通过，当前 有clone对象为激活状态中，unload存在风险。", m_bundleName));
                    return false;
                }
            }

            return true;
        }

        //获取当前资源的加载情况
        public string Desc(bool showDetail)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("Bundle Name:{0}\n", m_bundleName);
            builder.AppendFormat("被依赖情况:\n");
            for (int i = 0; i < m_beDependentNode.Count; ++i)
            {
                builder.AppendFormat("{0} : {1}\n", i, m_beDependentNode[i].m_bundleName);
            }
            builder.AppendFormat("被依赖情况合计:{0}\n", m_beDependentNode.Count);

            builder.AppendFormat("依赖情况:\n");
            for (int i = 0; i < m_dependentNode.Count; ++i)
            {
                builder.AppendFormat("{0} : {1}\n", i, m_dependentNode[i].m_bundleName);
            }

            builder.AppendFormat("资源总数：{0}\n", m_totalAssetsCount);
            builder.AppendFormat("Bundle文件是否已经加载？ {0}\n", m_assetBundle == null ? "否" : "是");
            builder.AppendFormat("所有资源是否已经全部加载？ {0}\n", m_allLoaded ? "是" : "否");

            if (m_assetsRequest.Count == 0)
                builder.Append("当前请求数为 0");
            else
            {
                builder.Append("打印当前请求情况");
                for (int i = 0; i < m_assetsRequest.Count; ++i)
                {
                    builder.AppendFormat("当前{0}号请求情况：");
                    builder.Append(m_assetsRequest[i].Desc());
                }
            }

            return builder.ToString();
        }

        //弓虽卸
        public void ForceUnload()
        {
            //都没加过Assetbundle，填什么乱
            if (m_assetBundle == null)
            {
                return;
            }
            DoUnload(true);
        }
    }
}