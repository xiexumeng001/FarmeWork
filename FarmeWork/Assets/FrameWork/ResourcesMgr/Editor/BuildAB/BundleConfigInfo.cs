using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ShipFarmeWork.Resource.Editor
{
    /// <summary>
    /// 打包配置信息解析
    /// </summary>
    public class BundleConfigInfo
    {
        public enum BundleType
        {
            Folder, //将文件夹下的所有文件打成一个bundle(包含子文件夹内的)
            Single, //深度遍历，每个文件一个bundle
            SingleFolder,   //获取文件夹下所有的子文件夹，将每个文件夹下的内容单独打包成Bundle(也包含了OnlyFolder的功能)
            OnlyFolder,     //只是自己的文件夹下的文件(不包含子文件夹内的)
        }

        public BundleType bundleType;
        public string path;
        public string suffix;

        private BundleConfigInfo() { }

        public static BundleConfigInfo Create(string config)
        {
            string[] infos = config.Split(',');
            if (infos.Length != 3)
            {
                UnityEngine.Debug.LogError("解析" + config + "的时候出错啦");
                return null;
            }
            BundleConfigInfo info = new BundleConfigInfo();

            if (infos[0].ToUpper() == "SINGLE")
                info.bundleType = BundleType.Single;
            else if (infos[0].ToUpper() == "FOLDER")
                info.bundleType = BundleType.Folder;
            else if (infos[0].ToUpper() == "SINGLEFOLDER")
                info.bundleType = BundleType.SingleFolder;
            else if (infos[0].ToUpper() == "ONLYFOLDER")
                info.bundleType = BundleType.OnlyFolder;

            info.path = infos[1];
            info.suffix = infos[2];

            return info;
        }

        private string FullPath
        {
            get
            {
                return string.Format("{0}/{1}/{2}", Application.dataPath, ResourceConfig.ResRootDir, path);
            }
        }

        private string FullRootResPath
        {
            get
            {
                return string.Format("{0}/{1}/", Application.dataPath, ResourceConfig.ResRootDir);
            }
        }
        private string AssetRootResPath
        {
            get
            {
                return string.Format("{0}/{1}/", "Assets", ResourceConfig.ResRootDir);
            }
        }

        //添加到打包列表中~
        public void AddToBuildMap(List<AssetBundleBuild> build)
        {
            if (build == null)
                return;

            string[] files = null;

            AssetBundleBuild abb = new AssetBundleBuild();
            switch (bundleType)
            {
                case BundleType.Folder:
                    files = GetFileArr(FullPath, suffix, SearchOption.AllDirectories);
                    if (files.Length == 0) { return; }

                    abb.assetBundleName = GetAbName(path);
                    abb.assetNames = files;
                    build.Add(abb);
                    break;

                case BundleType.Single:
                    files = GetFileArr(FullPath, suffix, SearchOption.AllDirectories);
                    for (int i = 0; i < files.Length; ++i)
                    {
                        //string fileName = Path.GetFileNameWithoutExtension(files[i]);
                        abb.assetBundleName = GetAbName(RemoveSuffix(files[i]));
                        abb.assetNames = new string[1] { files[i] };
                        build.Add(abb);
                    }
                    break;

                case BundleType.SingleFolder:
                    string[] allSubFolders = Directory.GetDirectories(FullPath, "*.*", SearchOption.AllDirectories);
                    foreach (var item in allSubFolders)
                    {
                        string subPath = item.Replace("\\", "/");
                        files = GetFileArr(subPath, suffix, SearchOption.TopDirectoryOnly);
                        if (files.Length == 0) continue;

                        abb.assetBundleName = GetAbName(subPath);
                        abb.assetNames = files;
                        build.Add(abb);
                    }

                    files = GetFileArr(FullPath, suffix, SearchOption.TopDirectoryOnly);
                    if (files.Length > 0)
                    {
                        abb.assetBundleName = GetAbName(path);
                        abb.assetNames = files;
                        build.Add(abb);
                    }
                    break;
                case BundleType.OnlyFolder:
                    files = GetFileArr(FullPath, suffix, SearchOption.TopDirectoryOnly);
                    if (files.Length == 0) { return; }

                    abb.assetBundleName = GetAbName(path);
                    abb.assetNames = files;
                    build.Add(abb);
                    break;
            }
        }

        /// <summary>
        /// 获取所有文件
        /// </summary>
        private string[] GetFileArr(string path, string searchPattern, SearchOption searchOption)
        {
            string[] files = Directory.GetFiles(path, searchPattern, searchOption).Where(file => !file.EndsWith(".meta")).ToArray();
            ProcessPath(files);
            return files;
        }

        /// <summary>
        /// 获取Ab包的名字
        /// </summary>
        /// <returns></returns>
        private string GetAbName(string path)
        {
            return path.Replace(FullRootResPath, string.Empty).Replace(AssetRootResPath, string.Empty) + ResourceConfig.AbSuffix;
        }

        public string RemoveSuffix(string path)
        {
            return path.Substring(0, path.LastIndexOf('.'));
        }

        private void ProcessPath(string[] paths, string frontPath = "Assets")
        {
            for (int i = 0; i < paths.Length; ++i)
            {
                int idx = paths[i].IndexOf(frontPath);
                if (idx >= 0)
                {
                    //考虑是否移除路径
                    var newStr = paths[i].Substring(idx);
                    paths[i] = newStr.Replace("\\", "/");
                }
            }
        }


    }


#if false
                List<string> subFolderList = new List<string>();
                foreach (var item in allSubFolders)
                {
                    string[] folders = Directory.GetDirectories(item, "*.*", SearchOption.TopDirectoryOnly);
                    if (folders == null || folders.Length == 0)
                    {
                        subFolderList.Add(item);
                    }
                }
                foreach (var item in subFolderList)
                {
                    string subPath = item.Replace("\\", "/");
                    files = Directory.GetFiles(subPath, suffix, SearchOption.AllDirectories)
                        .Where(file => !file.EndsWith(".meta")).ToArray();
                    if (files == null || files.Length == 0) continue;

                    ProcessPath(files);

                    subPath = subPath.Replace(string.Format("{0}/{1}/", Application.dataPath, AppConst.GameName), string.Empty);
                    AssetBundleBuild subAbb = new AssetBundleBuild();
                    subAbb.assetBundleName = subPath.Replace("Res/default/", string.Empty).Replace("Assets/" + AppConst.GameName + "/Resources/", string.Empty) + AppConst.ExtName;
                    subAbb.assetNames = files;
                    build.Add(subAbb);
                }
#endif
}