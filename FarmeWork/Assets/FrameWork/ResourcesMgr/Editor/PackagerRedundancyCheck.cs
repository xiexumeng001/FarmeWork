using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

namespace ShipFarmeWork.Resource.Editor
{
    class PackagerRedundancyCheck
    {
        class CRedAsset
        {
            public CRedAsset()
            { }
            public string mName = "";
            public string mType = "";
            public List<string> mUsers = new List<string>();
        }

        //AB信息
        string mABPath;
        string mMainAb;
        string kSearchPattern;
        //string mABPath = "O:/AllUnity/unity_project/ResLoad/ResLoad/Assets/StreamingAssets";
        //string mMainAb = "StreamingAssets";
        //string kSearchPattern = "*.unity3d";

        //输出路径
        const string kABRedundencyDir = "/StreamingAssets";
        string kResultPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + kABRedundencyDir;

        //const string kABPath = "Assets/test_res/ABoutput_red";
        //const string kManiFest = "ABoutput";
        //bool mIsForQ6 = false;

        List<string> mAllABFiles = null;
        Dictionary<string, string> mAssetGenMap = null;     //资源路径 与 ab包的对应
        Dictionary<string, CRedAsset> mRedAssetMap = null;  //重复依赖的资源
        float mCheckTime = 0f;

        //[MenuItem("AB冗余检测/AB检测")]
        ////[MenuItem("Q5/Bundle相关/Bundle冗余检测")]
        //public static void Launch()
        //{
        //    ABRedundancyChecker.Ins.StartCheck();
        //}

        public PackagerRedundancyCheck(string path, string mainAb, string pattern)
        {
            mABPath = path;
            mMainAb = mainAb;
            kSearchPattern = pattern;
        }

        public void StartCheck()
        {
            EditorUtility.DisplayCancelableProgressBar("AB资源冗余检测中", "资源读取中......", 0f);
            mCheckTime = UnityEngine.Time.realtimeSinceStartup;
            if (mAllABFiles == null)
                mAllABFiles = new List<string>();
            if (mAssetGenMap == null)
                mAssetGenMap = new Dictionary<string, string>();
            if (mRedAssetMap == null)
                mRedAssetMap = new Dictionary<string, CRedAsset>();

            if (!GenAssetMap(mABPath, mMainAb))
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("错误", "请检查是否选择正确的AB资源", "Ok");
                return;
            }

            GetAllFiles(mAllABFiles, mABPath, kSearchPattern);
            int startIndex = 0;

            EditorApplication.CallbackFunction myUpdate = null;
            string mABPathLower = mABPath.ToLower();
            myUpdate = () =>
            {
                string file = "";
                AssetBundle ab = null;
                try
                {
                    file = mAllABFiles[startIndex];
                    ab = CreateABAdapter(file);
                    file = file.Replace(mABPathLower, "");
                //string[] arr = file.Split('/');
                CheckABInfo(ab, file /*arr[arr.Length - 1]*/);
                }
                catch (Exception e)
                {
                    Debug.LogError("MyError:" + e.StackTrace);
                }
                finally
                {
                    if (ab != null)
                        ab.Unload(true);
                }

                bool isCancel = EditorUtility.DisplayCancelableProgressBar("AB资源冗余检测中", file, (float)startIndex / (float)mAllABFiles.Count);
                startIndex++;
                if (isCancel || startIndex >= mAllABFiles.Count)
                {
                    EditorUtility.ClearProgressBar();
                    if (!isCancel)
                    {
                        CullNotRed();
                        mCheckTime = UnityEngine.Time.realtimeSinceStartup - mCheckTime;
                        EditorUtility.DisplayDialog("AssetBundle资源冗余检测结果", Export(), "Ok");
                    }

                    mAllABFiles.Clear();
                    mAllABFiles = null;
                    mAssetGenMap.Clear();
                    mAssetGenMap = null;
                    mRedAssetMap.Clear();
                    mRedAssetMap = null;
                    Resources.UnloadUnusedAssets();
                    EditorUtility.UnloadUnusedAssetsImmediate();
                    GC.Collect();
                    EditorApplication.update -= myUpdate;
                    startIndex = 0;
                }
            };

            EditorApplication.update += myUpdate;
        }

        //适配项目打包（有加密） 或 原生打包
        AssetBundle CreateABAdapter(string path)
        {
            //if (mIsForQ6)
            //    return UtilCommon.CreateBundleFromFile(path);
            //else
            return AssetBundle.LoadFromFile(path);
        }

        bool GenAssetMap(string path, string maniFest)
        {
            path = path.Replace("\\", "/");
            AssetBundle maniFestAb = CreateABAdapter(System.IO.Path.Combine(path, maniFest));
            if (maniFestAb == null)
                return false;

            AssetBundleManifest manifest = maniFestAb.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            if (manifest == null)
                return false;

            string[] allBundles = manifest.GetAllAssetBundles();
            maniFestAb.Unload(true);
            foreach (string abName in allBundles)
            {
                string filePath = System.IO.Path.Combine(path, abName);
                AssetBundle ab = CreateABAdapter(filePath);
                foreach (string asset in ab.GetAllAssetNames())
                {
                    mAssetGenMap.Add(asset.ToLower(), abName);
                }
                foreach (string asset in ab.GetAllScenePaths())
                {
                    mAssetGenMap.Add(asset.ToLower(), abName);
                }
                ab.Unload(true);
            }

            if (mAssetGenMap.Count == 0)
                return false;

            return true;
        }

        void CheckABInfo(AssetBundle ab, string abPath)
        {
            EditorSettings.serializationMode = SerializationMode.ForceText;
            string[] names = ab.GetAllAssetNames();
            string[] dependencies = AssetDatabase.GetDependencies(names, false);
            string[] allDepen = dependencies.Length > 0 ? dependencies : names;

            string currDep = "";
            for (int i = 0; i < allDepen.Length; i++)
            {
                currDep = allDepen[i].ToLower();
                CalcuDenpend(currDep, abPath);
                //UnityEngine.Object obj = ab.LoadAsset(currDep, typeof(UnityEngine.Object));
                //if (obj != null)
                //{
                //    Debugger.Log("--- obj type:{0}", GetObjectType(obj));
                //}
            }
        }

        void CalcuDenpend(string depName, string abPath)
        {
            if (depName.EndsWith(".cs"))
                return;
            if (depName.EndsWith(".anim"))
                return;
            if (depName.EndsWith(".controller"))
                return;

            if (!mAssetGenMap.ContainsKey(depName)) //不存在这个ab，记录一下
            {
                if (!mRedAssetMap.ContainsKey(depName))
                {
                    CRedAsset ra = new CRedAsset();
                    ra.mName = depName;
                    ra.mType = "我了个去";
                    mRedAssetMap.Add(depName, ra);
                    ra.mUsers.Add(abPath);
                }
                else
                {
                    CRedAsset ra = mRedAssetMap[depName];
                    ra.mUsers.Add(abPath);
                }
            }
        }

        // mRedAssetMap 中 CRedAsset 的 mUsers 只有一个的，视为不冗余的资源，直接打到了该 ab 中
        void CullNotRed()
        {
            List<string> keys = new List<string>();
            foreach (var item in mRedAssetMap)
            {
                if (item.Value.mUsers.Count == 1)
                    keys.Add(item.Key);
            }

            foreach (var value in keys)
                mRedAssetMap.Remove(value);
        }

        List<string> GetAllFiles(List<string> files, string folder, string pattern)
        {
            folder = folder.Replace("\\", "/");
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(folder);
            foreach (var file in dir.GetFiles(pattern))
            {
                files.Add((System.IO.Path.Combine(folder, file.Name).Replace("\\", "/")).ToLower());
            }
            foreach (var sub in dir.GetDirectories())
            {
                files = GetAllFiles(files, System.IO.Path.Combine(folder, sub.Name), pattern);
            }
            return files;
        }

        string GetObjectType(UnityEngine.Object obj)
        {
            string longType = obj.GetType().ToString();
            string[] longTypeArr = longType.Split('.');
            return longTypeArr[longTypeArr.Length - 1];
        }

        private string AppendSlash(string path)
        {
            if (path == null || path == "")
                return "";
            int idx = path.LastIndexOf('/');
            if (idx == -1)
                return path + "/";
            if (idx == path.Length - 1)
                return path;
            return path + "/";
        }

        string Export()
        {
            if (mRedAssetMap.Count == 0)
                return "未检查到有资源冗余";

            List<CRedAsset> raList = mRedAssetMap.Values.ToList<CRedAsset>();
            string currTime = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string path = string.Format("{0}/{1}_{2}.md", kResultPath, "ABRedundency", currTime);
            if (!System.IO.Directory.Exists(kResultPath))
                System.IO.Directory.CreateDirectory(kResultPath);

            using (FileStream fs = File.Create(path))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format("## 资源总量:{0}，冗余总量:{1}，检测时间:{2}，耗时：{3:F2}s\r\n---\r\n", mAllABFiles.Count, raList.Count, currTime, mCheckTime));
                sb.Append("| 排序 | 资源名称 | 资源类型 | AB文件数量 | AB文件名 |\r\n");
                sb.Append("|---|---|:---:|:---:|---|\r\n");

                CRedAsset ra = null;

                StringBuilder abNames = new StringBuilder();

                raList.Sort((CRedAsset ra1, CRedAsset ra2) =>
                {//排序优先级： ab文件个数 -> 名字
                 //int ret = ra2.mUsers.Count.CompareTo(ra1.mUsers.Count);
                 //if (ret == 0)
                 //    ret = ra1.mName.CompareTo(ra2.mName);
                 //return ret;
                 //按名称排序
                return ra1.mName.CompareTo(ra2.mName);
                });

                for (int i = 0; i < raList.Count; i++)
                {
                    ra = raList[i];
                    foreach (var abName in ra.mUsers)
                        abNames.Append(string.Format("**{0}**, ", abName));
                    //abNames.Append(string.Format("{0}<br>", abName)); //另一种使用换行

                    sb.Append(string.Format("| {0} | **{1}** | {2} | {3} | {4} |\r\n"
                        , i + 1, ra.mName, ra.mType, ra.mUsers.Count, abNames.ToString()));
                    abNames.Length = 0;
                }
                byte[] info = new UTF8Encoding(true).GetBytes(sb.ToString());
                fs.Write(info, 0, info.Length);
            }
            return "有冗余，导出结果：" + path.Replace("\\", "/");
        }

        //---------------- gui begin ------------

#if false
    [MenuItem("AB冗余检测/AB检测")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(ABRedundancyChecker), false, "AB资源冗余检测");
    }

    void Awake()
    {
        mInstance = this;
    }
    string mSelPath = "";
    public void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("AB路径:" + mABPath, EditorStyles.boldLabel);
        EditorGUILayout.LabelField("主AB文件:" + mMainAb, EditorStyles.boldLabel);
        EditorGUILayout.Space();
        if (GUILayout.Button("开始检测"))
        {
            StartCheck();
        }
    }
#endif

#if false
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("AB路径:" + mABPath, EditorStyles.boldLabel);
        EditorGUILayout.LabelField("主AB文件:" + mMainAb, EditorStyles.boldLabel);
        EditorGUILayout.Space();
        GUILayout.Label(mSelPath);
        EditorGUILayout.Space();
        if (GUILayout.Button("选择主AB文件"))
            mSelPath = EditorUtility.OpenFilePanelWithFilters("选择主AB文件", mSelPath, null);
        EditorGUILayout.Space();
        //mIsForQ6 = EditorGUILayout.Toggle("是否Q6(Q6有解密机制)", mIsForQ6);
        EditorGUILayout.Space();
        if (GUILayout.Button("开始检测"))
        {
            if (mSelPath == "")
                EditorUtility.DisplayDialog("错误", "请先 选择主AB文件", "Ok");
            else
            {
                mSelPath = mSelPath.Replace("\\", "/");
                string[] arr = mSelPath.Split('/');
                mMainAb = arr[arr.Length - 1];
                mABPath = mSelPath.Substring(0, mSelPath.LastIndexOf('/'));
                StartCheck();
            }
        }
#endif
    }
}