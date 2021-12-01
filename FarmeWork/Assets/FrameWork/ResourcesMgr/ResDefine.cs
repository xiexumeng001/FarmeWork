using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ShipFarmeWork.Resource
{
    /// <summary>
    /// 资源定义
    /// </summary>
    public static class ResDefine
    {
        //Ab信息记录文件
        public static string StreamingVersionFile = "StreamingVersion.txt";
        public static string fileList = "filelist.txt";
        public static string Version = "Version:";  //版本

        /// <summary>
        /// 热更地址
        /// </summary>
        public static string HotUpdateUrl
        {
            get
            {
                return string.Format("{0}/{1}/{2}/", ServerResConfig.HotUpdateUrl, PlatformName, ResourceConfig.UserLanguage);
            }
        }

        /// <summary>
        /// 热更记录列表文件
        /// </summary>
        public static string HotUpdateFileListUrl
        {
            get
            {
                return string.Format("{0}{1}", HotUpdateUrl, fileList);
            }
        }

        /// <summary>
        /// 热更详情文件地址
        /// </summary>
        public static string UpgradeDetailFileUrl
        {
            get
            {
                return string.Format("{0}{1}", HotUpdateUrl, "UpgradeDetail.txt");
            }
        }

        public static string UpdateLanguageFileName
        {
            get
            {
                return string.Format("StartUpLangID_{0}", ResourceConfig.UserLanguage);
            }
        }

        /// <summary>
        /// 取得数据存放目录
        /// </summary>
        public static string DataPath
        {
            get
            {
                if (ResourceConfig.LoadResFromStreamingAssets)
                {
                    return Application.streamingAssetsPath + "/";
                }
                return PersistentDataPath;
            }
        }

        /// <summary>
        /// 沙盒路径
        /// </summary>
        public static string PersistentDataPath
        {
            get
            {
                string dir = ResourceConfig.GameName;
                if (Application.isMobilePlatform)
                {
                    return Application.persistentDataPath + "/" + dir + "/";
                }
                if (Application.platform == RuntimePlatform.OSXEditor)
                {
                    int i = Application.dataPath.LastIndexOf('/');
                    return Application.dataPath.Substring(0, i + 1) + dir + "/";
                }
                return "c:/" + dir + "/";
            }
        }

        /// <summary>
        /// 获取资源存放路径
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetDataPath(string fileName)
        {
            return Path.Combine(DataPath, fileName);
        }
        /// <summary>
        /// 获取沙盒路径
        /// </summary>
        /// <returns></returns>
        public static string GetPersistentPath(string fileName)
        {
            return Path.Combine(PersistentDataPath, fileName);
        }
        /// <summary>
        /// 获取Streaming路径
        /// </summary>
        /// <returns></returns>
        public static string GetStreamingAssetsPath(string fileName)
        {
            return Path.Combine(Application.streamingAssetsPath, fileName);
        }


        private const string PLATFORM_ANDROID = "Android";
        private const string PLATFORM_IOS = "iOS";
        private const string PLATFORM_Win = "Win";

        /// <summary>
        /// 平台名称
        /// </summary>
        public static string PlatformName
        {
            get
            {
#if UNITY_ANDROID || UNITY_EDITOR 
                return PLATFORM_ANDROID;
#elif UNITY_IPHONE
                return PLATFORM_IOS;
#elif UNITY_STANDALONE_WIN
				return PLATFORM_Win;
#endif
            }
        }

        /// <summary>
        /// 判断版本A是否大于版本B
        /// </summary>
        /// <param name="versionA"></param>
        /// <param name="versionB"></param>
        /// <returns></returns>
        public static bool VersionIsGreater(string versionA, string versionB)
        {
            return (versionA.CompareTo(versionB) == 1);
        }

        /// <summary>
        /// 计算文件MD5值
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string CalcMD5(string path)
        {
            try
            {
                FileStream file = new FileStream(path, FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (System.Exception ex)
            {
                throw new System.Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
            }
        }
        
        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static long GetFileSize(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            if (fileInfo != null)
                return fileInfo.Length;

            return 0L;
        }

        /// <summary>
        /// 获取大小
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string CountSizeString(long size)
        {
            if (size > 1048576)
            {
                return size / 1048576 + "MB";
            }
            else if (size > 1024)
            {
                return size / 1024 + "KB";
            }
            else
            {
                return size + "B";
            }
        }


    }
}
