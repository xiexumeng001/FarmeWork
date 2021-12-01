using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

using UnityEditor;
using System.Net;

namespace ShipFarmeWork.Resource.Editor
{
    /// <summary>
    /// oss自动上传工具(只是写入到上传文件,具体上传由python执行)
    /// </summary>
    public class AutoOss
    {
        public static string ossConfigPath = System.Environment.CurrentDirectory + "\\AutoBuild\\ossConfig\\";
        //public static string ossConfigPath = "C:\\Users\\RBW\\Desktop\\pythonCdnSdk\\ossConfig\\";
        public static string ossBaseInfoFileName = ossConfigPath + "baseInfo.json";
        public static string ossUpdateFileName = ossConfigPath + "ossUpdateInfo.txt";

        /// <summary>
        /// 自动上传oss
        /// </summary>
        public static void UpAbToOss()
        {
            AutoBuild.AutoBuildArgs args = AutoBuild.GetAutoBuildArgs();

            //获取oss基础信息
            OssBaseInfo ossBaseInfo = GetOssBaseInfo();
            if (ossBaseInfo == null) return;

            //获取远端配置
            bool isOk = DownLoadServerConfigFile();
            if (!isOk) return;

            //读取远端file文件
            Dictionary<string, ResFileInfo> serverFileDic = DownLoadServerFileList();
            if (serverFileDic == null) { return; }

            //读取本地file文件
            string localPath = Packager.GetBundleVersionPath(args.BunldeVer, true);
            Dictionary<string, ResFileInfo> localFileDic = LoadLocalFileList(localPath);
            if (localFileDic == null) { return; }

            //比较获取更新与移除的文件
            List<ResFileInfo> updateFileList = new List<ResFileInfo>();
            List<ResFileInfo> removeFileList = new List<ResFileInfo>();
            foreach (var item in localFileDic)
            {
                if (!serverFileDic.ContainsKey(item.Key) || serverFileDic[item.Key].md5 != item.Value.md5)
                {//服务器没有 或者 不一样,就添加
                    updateFileList.Add(item.Value);
                }
            }
            foreach (var item in serverFileDic)
            {
                if (!localFileDic.ContainsKey(item.Key))
                {//客户端没有,就删除
                    removeFileList.Add(item.Value);
                }
            }
            if (updateFileList.Count > 0 || serverFileDic.Count > 0)
            {//添加file文件
                ResFileInfo resFileInfo = new ResFileInfo();
                resFileInfo.fileName = ResDefine.fileList;
                updateFileList.Add(resFileInfo);
            }

            string ossPath = ResDefine.HotUpdateUrl.Replace(ossBaseInfo.cdnHttp, "");
            //写入文件
            OssUpdateInfo ossUpdate = new OssUpdateInfo();
            foreach (var item in updateFileList)
            {
                OssFileUpdateRecord record = new OssFileUpdateRecord();
                record.LocalFilePath = localPath + item.fileName;
                record.OssFilePath = ossPath + item.fileName;
                ossUpdate.UpdateRecordList.Add(record);
            }
            foreach (var item in removeFileList)
            {
                OssFileDeletaRecord record = new OssFileDeletaRecord();
                record.OssFilePath = ossPath + item.fileName;
                ossUpdate.DeleteRecordList.Add(record);
            }
            string jsonStr = JsonUtility.ToJson(ossUpdate);
            File.WriteAllText(ossUpdateFileName, jsonStr);
            Debug.LogWarning("写入oss完毕");
        }

        [System.Serializable]
        public class OssBaseInfo
        {
            public string endpoint;
            public string bucket_name;
            public string cdnHttp;
            public string access_key_id;
            public string access_key_secret;
        }
        [System.Serializable]
        public class OssUpdateInfo
        {
            public List<OssFileUpdateRecord> UpdateRecordList = new List<OssFileUpdateRecord>();
            public List<OssFileDeletaRecord> DeleteRecordList = new List<OssFileDeletaRecord>();
        }
        [System.Serializable]
        public class OssFileUpdateRecord
        {
            public string LocalFilePath;
            public string OssFilePath;
        }
        [System.Serializable]
        public class OssFileDeletaRecord
        {
            public string OssFilePath;
        }


        public static OssBaseInfo GetOssBaseInfo()
        {
            if (!File.Exists(ossBaseInfoFileName))
            {
                Debug.LogError("未获取到oss基础信息,路径:" + ossBaseInfoFileName);
                return null;
            }
            string ossBaseStr = File.ReadAllText(ossBaseInfoFileName);
            OssBaseInfo ossBaseInfo = JsonUtility.FromJson<OssBaseInfo>(ossBaseStr);
            Debug.LogWarning("获取基础信息成功");
            return ossBaseInfo;
        }

        /// <summary>
        /// 下载ServerConfig配置文件
        /// </summary>
        /// <returns></returns>
        public static bool DownLoadServerConfigFile()
        {
            using (WebClient client = new WebClient())
            {
                try
                {
                    byte[] buffer = client.DownloadData(ResourceConfig.HotUpdateConfigUrl);
                    string serverConfig = System.Text.Encoding.ASCII.GetString(buffer);
                    ServerResConfig.Init(serverConfig);
                    Debug.LogWarning("下载远端配置文件成功 ");
                    Debug.Log("downloadSuccess,serveraddr:" + ResourceConfig.HotUpdateConfigUrl + "  HotUpdateUrl:" + ServerResConfig.HotUpdateUrl);
                    return true;
                }
                catch (WebException e)
                {
                    Debug.LogError("下载远端配置文件失败 " + e);
                    return false;
                }
            }
        }

        /// <summary>
        /// 下载远端FileList文件
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, ResFileInfo> DownLoadServerFileList()
        {
            Dictionary<string, ResFileInfo> serverFileDic = new Dictionary<string, ResFileInfo>();
            try
            {
                using (WebClient client = new WebClient())
                {
                    //读取远端file文件
                    byte[] serverFileBs = client.DownloadData(ResDefine.HotUpdateFileListUrl);
                    string serverFileRecord = System.Text.Encoding.ASCII.GetString(serverFileBs);
                    string[] serverFileMd5Arr = serverFileRecord.Split('\n');
                    for (int i = 0; i < serverFileMd5Arr.Length; i++)
                    {
                        if (i == 0) { continue; }
                        string[] temps = serverFileMd5Arr[i].Split('|');
                        if (temps.Length == 3)
                        {
                            string fileName = temps[0];
                            string md5 = temps[1];
                            int size = int.Parse(temps[2]);
                            ResFileInfo info = new ResFileInfo();
                            info.fileName = fileName;
                            info.md5 = md5;
                            info.size = size;
                            serverFileDic.Add(fileName, info);
                        }
                    }
                    Debug.LogWarning("下载fileList文件成功");
                    return serverFileDic;
                }
            }
            catch (WebException e)
            {
                if (e.Response is HttpWebResponse httpRes)
                {
                    if (httpRes.StatusCode == HttpStatusCode.NotFound)
                    {//文件不存在,则全更新
                        Debug.LogWarning("下载fileList文件不存在,全更");
                        return serverFileDic;
                    }
                }
                //其他报错返回空,不忘下走了
                Debug.LogError("下载fileList文件失败 " + e);
                return null;
            }
        }


        /// <summary>
        /// 加载本地文件
        /// </summary>
        /// <param name="bundleVer"></param>
        public static Dictionary<string, ResFileInfo> LoadLocalFileList(string localPath)
        {
            Dictionary<string, ResFileInfo> localFileDic = new Dictionary<string, ResFileInfo>();
            string localFilePath = localPath + ResDefine.fileList;
            if (!File.Exists(localFilePath))
            {
                Debug.LogError("本地fileList文件不存在 " + localFilePath);
                return null;
            }
            string[] localFileMd5Arr = File.ReadAllLines(localFilePath);
            for (int i = 0; i < localFileMd5Arr.Length; i++)
            {
                if (i == 0) { continue; }
                string[] temps = localFileMd5Arr[i].Split('|');
                if (temps.Length == 3)
                {
                    string fileName = temps[0];
                    string md5 = temps[1];
                    int size = int.Parse(temps[2]);
                    ResFileInfo info = new ResFileInfo();
                    info.fileName = fileName;
                    info.md5 = md5;
                    info.size = size;
                    localFileDic.Add(fileName, info);
                }
            }
            Debug.LogWarning("加载本地fileList文件成功");
            return localFileDic;
        }
    }
}