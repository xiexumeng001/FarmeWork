using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShipFarmeWork.Resource
{
    /// <summary>
    /// 资源配置
    /// </summary>
    public static class ResourceConfig
    {
        public static string ResRootDir = "Test/Res";     //资源根目录
        public static string GameName = "ShipFarmeWork";  //游戏名称
        public static string AbSuffix = ".unity3d";       //AB包后缀

        public static bool IsTestHot = true;             //是否测试热更
        public static bool BundleModeInEditor = true;     //在编辑器上是否使用Bundle包
        public static bool LoadResFromStreamingAssets = false;   //是否直接从StreamingAssets路径下加载资源(服务器的配置会修改)

        public static string HotUpdateConfigUrl = "https://starship.shijunhuyu.com/FarmeWorkTest/ServerConfig.txt";  //热更地址

        //获取当前语言
        public static string UserLanguage = "CN";

#if UNITY_EDITOR
        public static bool IsSafeCheck = true;   //是否做安全检查
#else
        public static bool IsSafeCheck = Debug.isDebugBuild;   //是否做安全检查
#endif


        public const string LocalVersionKey = "LOCAL_VERSION_KEY";  //本地版本的键值

        //ShaderAB的名称

        public static string[] ShaderAbNameArr =
        {
            //"Atlas/Bg"
        };

        static ResourceConfig()
        {
            for (int i = 0; i < ShaderAbNameArr.Length; i++)
            {
                ShaderAbNameArr[i] = ShaderAbNameArr[i].ToLower();
            }
        }
    }
}