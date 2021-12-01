using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ShipFarmeWork.Resource
{
    /// <summary>
    /// 服务器资源配置
    /// </summary>
    public static class ServerResConfig
    {
        //最低版本
        public static string LowestVersion;
        //版本下载地址
        public static string VersionDownloadURL;
        //新版本下载提示
        public static string ShowVersionDownloadTips;
        //热更地址
        public static string HotUpdateUrl;
        //是否能热更
        public static bool HotUpdate;
        //是否从StreamingAssets加载资源
        public static bool LoadFromStreamingAssets;
        //尝试下载次数
        public static string MaxTryTimes;

        static string ForceHotUpdateFileName = "ForceHotUpdate";  //强制热更文件名称

        public static void Init(string str)
        {
            str = str.Trim('\r', '\n');
            string[] args = str.Split('\n');
            for (int i = 0; i < args.Length; ++i)
            {
                string[] temp = args[i].Split(',');
                if (temp.Length == 2)
                {
                    string _key = temp[0];
                    string _val = temp[1].Trim('\r');
                    if (_key == "LowestVersion") { LowestVersion = _val; }
                    else if (_key == "VersionDownloadURL") { VersionDownloadURL = _val; }
                    else if (_key == "ShowVersionDownloadTips") { ShowVersionDownloadTips = _val; }
                    else if (_key == "HotUpdateUrl") { HotUpdateUrl = _val; }
                    else if (_key == "HotUpdate") { HotUpdate = (_val == "1"); }
                    else if (_key == "LoadFromStreamingAssets") { LoadFromStreamingAssets = (_val == "1"); }
                    else if (_key == "MaxTryTimes") { MaxTryTimes = _val; }
                }
            }

#if UNITY_EDITOR
            if (ResourceConfig.IsTestHot) { HotUpdate = true; }
#endif
            if (!HotUpdate)
            {
                //是否有强制热更文件
                try
                {
                    string to = ResDefine.GetPersistentPath(ForceHotUpdateFileName);
                    if (File.Exists(to))
                    {
                        HotUpdate = true;
                    }
                }
                catch (Exception e) { Debug.LogError(e); }
            }

            ResourceConfig.LoadResFromStreamingAssets = LoadFromStreamingAssets;

            Debug.Log(HotUpdate ? "开启了热更！" : "关闭了热更");
            Debug.Log(string.Format("允许运行的最低版本为 : {0}", ServerResConfig.LowestVersion));
        }
    }
}
