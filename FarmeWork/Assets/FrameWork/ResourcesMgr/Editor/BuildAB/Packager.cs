using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Debug = UnityEngine.Debug;
using System.Text.RegularExpressions;

namespace ShipFarmeWork.Resource.Editor
{
    public class Packager
    {
        public static string packagePath = "../Package/";

        public static bool miniPackage = false; //小资源包
        public static string platform = string.Empty;
        static List<AssetBundleBuild> maps = new List<AssetBundleBuild>();


        //bundle文件所在的位置
        private static string bundlePath = "";
        //当前打包平台
        public static BuildTarget buildTarget = BuildTarget.NoTarget;
        //定义的打包宏（至少包含这俩）
        public static string defineSymbols = "USING_DOTWEENING;";
        //当前基础版本号
        public static string baseVersion = "1.0.0";
        //当前的资源版本号
        public static string bundleVersion = "0";
        //替换存在资源
        public static bool replaceExistAsset = false;
        //改变单个并且升级版本
        public static bool changeSingleAndUpLv = false;
        //输入的配置文本
        public static string inputAbConfigs = "";

        //打包配置文件~
        private static Dictionary<string, BundleConfigInfo> bundleConfigs = new Dictionary<string, BundleConfigInfo>();

        //获取打包后记录文件名称 MD5的列表目录
        private static string StreamingFileListPath
        {
            get
            {
                return Path.Combine(Application.streamingAssetsPath, ResDefine.fileList);
            }
        }

        //只是打资源bundle~
        public static void OnlyBuildBundle()
        {
            CheckBuildTarget();
            //将bundle文件生成到对应的文件夹
            BuildBundleResToResFolder();
        }

        //判断打包平台和当前平台是否一致
        private static void CheckBuildTarget()
        {
            if (buildTarget == BuildTarget.NoTarget)
            {
                Debug.LogError("切换平台失败！没有设置任何平台");
                return;
            }

            if (buildTarget != EditorUserBuildSettings.activeBuildTarget)
            {

                switch (buildTarget)
                {
                    case BuildTarget.iOS:
                        Debug.Log("当前并非iOS平台，需要切换");
                        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
                        break;
                    case BuildTarget.Android:
                        Debug.Log("当前并非安卓平台，需要切换");
                        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                Debug.Log("平台一致，不用切换平台，当前平台为：");
                switch (buildTarget)
                {
                    case BuildTarget.iOS:
                        Debug.Log("iOS");
                        break;
                    case BuildTarget.Android:
                        Debug.Log("Android");
                        break;
                    default:
                        break;
                }
            }
        }

        //打bundle资源，并且拷贝到对应的资源文件夹内
        private static void BuildBundleResToResFolder()
        {
            bundlePath = GetBundleVersionPath(bundleVersion, true);
            Debug.Log("Start build bundle res to res folder...");
            Debug.Log(bundlePath);
            if (Directory.Exists(bundlePath))
            {
                if (replaceExistAsset)
                {
                    Debug.Log("注意，资源文件夹将被替换！");
                    Directory.Delete(bundlePath, true);
                    Directory.CreateDirectory(bundlePath);
                }
                else
                {
                    Debug.LogError("资源文件夹已存在，因此无法生成新的资源文件。");
                    return;
                }
            }
            else
            {
                Debug.Log("资源文件夹为空，开始创建资源文件夹！");
                Directory.CreateDirectory(bundlePath);
            }

            switch (buildTarget)
            {
                case BuildTarget.iOS:
                    Packager.BuildResToTargetFolder(buildTarget, bundlePath);
                    break;
                case BuildTarget.Android:
                    Packager.BuildResToTargetFolder(buildTarget, bundlePath);
                    break;
                case BuildTarget.StandaloneWindows64:
                    Packager.BuildResToTargetFolder(buildTarget, bundlePath);
                    break;
                default:
                    Debug.LogError("未知平台类型呢~");
                    break;
            }
        }


        static void BuildeAssetBundle(BuildTarget target, BuildAssetBundleOptions options)
        {
            //清除Streaming文件夹
            string targetUpdatePath = Application.streamingAssetsPath;
            ResetFolder(targetUpdatePath, true);

            maps.Clear();
            //其余部分
            foreach (var item in bundleConfigs)
            {
                item.Value.AddToBuildMap(maps);
            }

            //记录bundle信息
            StringBuilder sb = new StringBuilder();
            foreach (var item in maps)
            {
                for (int i = 0; i < item.assetNames.Length; ++i)
                {
                    //重新拼一下路径
                    string newPath = Path.Combine(Application.dataPath, item.assetNames[i].Replace("Assets/", string.Empty));
                    newPath.Replace("\\", "/");
                    string newMetaPath = newPath + ".meta";
                    string fileMD5 = ResDefine.CalcMD5(newPath);
                    string metaMD5 = ResDefine.CalcMD5(newMetaPath);
                    sb.AppendFormat("{0}|{1}|{2}|{3}\n", item.assetBundleName, newPath, fileMD5, metaMD5);
                }
            }
            File.WriteAllText(Path.Combine(Application.dataPath, "TempBundleInfo.txt"), sb.ToString());

            //打包
            BuildPipeline.BuildAssetBundles(targetUpdatePath, maps.ToArray(), options, target);
        }

        /// <summary>
        /// 构建各个版本资源
        /// </summary>
        static void CopyBundles(BuildTarget target, BuildAssetBundleOptions options, string copyTo)
        {
            string _packagePath = Path.Combine(packagePath, target.ToString());
            string _updatePath = copyTo;

            ResetFolder(_packagePath, true);
            ResetFolder(_updatePath, true);

            //将streaming Assets下的所有文件拷贝到Package中
            string[] all = Directory.GetFiles(Application.streamingAssetsPath, "*.*", SearchOption.AllDirectories)
                .Where(file => !file.EndsWith(".meta")).ToArray();
            foreach (var item in all)
            {
                string newPath = item.Replace(Application.streamingAssetsPath, _packagePath).Replace("\\", "/");
                if (!Directory.Exists(Path.GetDirectoryName(newPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(newPath));
                }
                File.Copy(item, newPath);
            }

            //将Streaming Assets下所有非meta和manifest文件拷贝到Update中
            all = Directory.GetFiles(Application.streamingAssetsPath, "*.*", SearchOption.AllDirectories)
                .Where(file => !file.EndsWith(".meta") && !file.EndsWith(".manifest")).ToArray();
            foreach (var item in all)
            {
                string newPath = item.Replace(Application.streamingAssetsPath, _updatePath).Replace("\\", "/");
                if (!Directory.Exists(Path.GetDirectoryName(newPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(newPath));
                }
                File.Copy(item, newPath);
            }

            //删除streaming assets下的manifest文件
            all = Directory.GetFiles(Application.streamingAssetsPath, "*.*", SearchOption.AllDirectories)
                 .Where(file => file.EndsWith(".manifest") || file.EndsWith(".meta")).ToArray();
            foreach (var file in all)
            {
                File.Delete(file);
            }
        }

        /// <summary>
        /// 复制旧版本资源到新版本
        /// </summary>
        public static void CopyOldToNew(string oldVersion, string newVersion)
        {
            string oldbundlePath = GetBundleVersionPath(oldVersion, true);
            string newBundlePath = GetBundleVersionPath(newVersion, true);
            if (System.IO.Directory.Exists(oldbundlePath))
            {
                string[] allFiles = System.IO.Directory.GetFiles(oldbundlePath, "*.*", System.IO.SearchOption.AllDirectories);
                foreach (var item in allFiles)
                {
                    string newPath = item.Replace(oldbundlePath, newBundlePath).Replace("\\", "/");

                    string filename = System.IO.Path.GetFileName(newPath);
                    if (filename == "StreamingAssets" || filename == "StreamingAssets.manifest")
                    {
                        File.Delete(newPath);
                    }
                    if (filename == ResDefine.fileList)
                    {
                        fileListInfo oldFileInfo = GetFileListDic(item);
                        fileListInfo newFileInfo = GetFileListDic(newPath);
                        oldFileInfo.versionStr = newFileInfo.versionStr;
                        foreach (var record in newFileInfo.recordDic)
                        {
                            string newStr = newFileInfo.recordList[record.Value];
                            int oldIndex = oldFileInfo.recordDic[record.Key];
                            oldFileInfo.recordList[oldIndex] = newStr;
                        }
                        File.Delete(newPath);
                        File.WriteAllLines(newPath, oldFileInfo.ToRecordArr());
                    }
                    if (!File.Exists(newPath))
                    {
                        string newDire = Path.GetDirectoryName(newPath);
                        if (!Directory.Exists(newDire))
                        {
                            Directory.CreateDirectory(newDire);
                        }
                        File.Copy(item, newPath);
                    }
                }
            }
        }

        public class fileListInfo
        {
            public string versionStr;
            public string streamingStr;

            public List<string> recordList = new List<string>();
            public Dictionary<string, int> recordDic = new Dictionary<string, int>();

            public string[] ToRecordArr()
            {
                recordList.Insert(0, versionStr);
                recordList.Insert(1, streamingStr);
                return recordList.ToArray();
            }
        }
        /// <summary>
        /// 获取FileList的目录
        /// </summary>
        /// <returns></returns>
        public static fileListInfo GetFileListDic(string path)
        {
            fileListInfo fileInfo = new fileListInfo();
            string[] TempFiles = File.ReadAllLines(path);
            for (int i = 0; i < TempFiles.Length; i++)
            {
                if (i == 0) { fileInfo.versionStr = TempFiles[i]; continue; }

                string[] arrs = TempFiles[i].Split('|');
                if (arrs[0] == "StreamingAssets")
                {
                    fileInfo.streamingStr = TempFiles[i];
                }
                else
                {
                    fileInfo.recordList.Add(TempFiles[i]);
                    fileInfo.recordDic.Add(arrs[0], fileInfo.recordList.Count - 1);
                }
            }
            return fileInfo;
        }

        //重置文件夹
        private static void ResetFolder(string path, bool renew)
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);

            if (renew)
                Directory.CreateDirectory(path);
        }

        public static void BuildResToTargetFolder(BuildTarget target, string targetFolder)
        {
            Debug.Log("重置Streaming Assets文件夹...");
            ResetFolder(Application.streamingAssetsPath, true);

            AssetDatabase.Refresh();

            Debug.Log("正在读取BundleConfig");
            //读取bundle配置文件
            LoadBundleConfig();

            BuildAssetBundleOptions options =
                BuildAssetBundleOptions.DeterministicAssetBundle |
                BuildAssetBundleOptions.ChunkBasedCompression;

            Debug.Log("正在生成Bundle文件");
            BuildeAssetBundle(target, options);

            Debug.Log("正在记录文件MD5");
            //记录文件列表和版本号
            CreateBundleListFile(StreamingFileListPath);

            Debug.Log("正在将资源拷贝到资源版本路径下");
            // 拷贝资源到指定分包
            CopyBundles(target, options, targetFolder);
            
            Debug.Log("Bundle生成完毕");
            AssetDatabase.Refresh();
        }

        //读取打包配置文件表
        private static void LoadBundleConfig()
        {
            string[] config = null;
            if (!changeSingleAndUpLv)
            {
                config = File.ReadAllLines(string.Format("{0}/{1}/{2}", Application.dataPath, ResourceConfig.ResRootDir, "../BundleConfig.csv"));
            }
            else
            {
                config = inputAbConfigs.Split('\n');
            }
            //第一行是标题...
            for (int i = 1; i < config.Length; ++i)
            {
                string[] paras = config[i].Split(',');
                if (paras.Length != 3)
                {
                    UnityEngine.Debug.LogError("解析" + config[i] + "的时候出错啦");
                    continue;
                }
                string path = paras[1];
                if (miniPackage && path.ToLower().Contains("audioclips"))
                {
                    Debug.Log("mini包跳过音效文件");
                    continue;
                }
                BundleConfigInfo info = null;
                if (bundleConfigs.TryGetValue(path, out info))
                {
                    //已经有这个记录了，没准需要追加一个后缀
                }
                else
                {
                    info = BundleConfigInfo.Create(config[i]);
                    bundleConfigs.Add(path, info);
                }
            }
        }

        /// <summary>
        /// 创建Bundle信息文件
        /// </summary>
        /// <param name="path"></param>
        private static void CreateBundleListFile(string path)
        {
            if (File.Exists(path))
                File.Delete(path);

            string[] bundleFiles = Directory.GetFiles(
                Application.streamingAssetsPath,
                "*.*",
                SearchOption.AllDirectories).Where(file => file.EndsWith(".unity3d") || file.EndsWith("StreamingAssets") || file.Contains("StartUpLangID")).ToArray();

            List<string> fileList = new List<string>();

            //记录当前版本号～
            fileList.Add(string.Format("{0}{1}.{2}", ResDefine.Version, baseVersion, bundleVersion));

            foreach (var file in bundleFiles)
            {
                string newName = file.Replace(Application.streamingAssetsPath, string.Empty);
                newName = newName.Replace("\\", "/");
                newName = newName.TrimStart('/');
                long size = ResDefine.GetFileSize(file);
                string md5 = ResDefine.CalcMD5(file);

                fileList.Add(string.Format("{0}|{1}|{2}", newName, md5, (int)size));
            }

            //记录当前streamingAssets版本号
            File.WriteAllLines(path, fileList.ToArray());
        }

        public static void CopyBundleToStreamingAssets(string bundleVersion)
        {
            bundlePath = GetBundleVersionPath(bundleVersion, false);
            //Application.dataPath + "/../../BundleVersions/" + bundleVersion + "/" + language;
            //判断是否有同级目录存在
            if (Directory.Exists(bundlePath))
            {
                //先清空streamingAssets文件夹
                if (Directory.Exists(Application.streamingAssetsPath))
                    Directory.Delete(Application.streamingAssetsPath, true);

                Directory.CreateDirectory(Application.streamingAssetsPath);

                string[] files = null;
                //遍历文件列表，根据打进package的资源大小调整拷贝bundle文件
                string fileListPath = Path.Combine(bundlePath, ResDefine.fileList);
                if (!File.Exists(fileListPath))
                {
                    Debug.LogError("严重错误！无法从bundle文件夹中找到文件列表！");
                    return;
                }
                files = File.ReadAllLines(fileListPath);
                int copyProgress = 0;
                foreach (var file in files)
                {
                    ++copyProgress;
                    string[] paras = file.Split('|');
                    if (paras.Length == 3)
                    {
                        string path = paras[0].ToString();
                        //拷贝文件
                        string from = Path.Combine(bundlePath, path).Replace("\\", "/");
                        string to = from.Replace(bundlePath, Application.streamingAssetsPath);
                        if (!Directory.Exists(Path.GetDirectoryName(to)))
                            Directory.CreateDirectory(Path.GetDirectoryName(to));

                        File.Copy(from, to);
                        EditorUtility.DisplayProgressBar("拷贝文件", "正在拷贝文件:" + path, (copyProgress * 1f / file.Length));
                    }
                }

                //最终根据导入的资源多少生成StreamingAssets文件列表
                CreateBundleListFile(Path.Combine(Application.streamingAssetsPath, ResDefine.StreamingVersionFile));
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("恭喜", "拷贝完成", "ok");
            }
            else
            {
                EditorUtility.DisplayDialog("呵呵", "都没有这个bundle目录，复制你妹啊！", "爸爸，我错了。");
                return;
            }
        }

        public static string GetBundleVersionPath(string bundleVersion, bool isSuffix)
        {
            string path = Application.dataPath + "/../../BundleVersions/" + bundleVersion + "/" + ResourceConfig.UserLanguage;
            if (isSuffix)
            {
                path = path + "/";
            }
            return path;
            //return Application.dataPath + "/../../BundleVersions/" + bundleVersion + "/" + Packager.language + "/";
        }
    }
}