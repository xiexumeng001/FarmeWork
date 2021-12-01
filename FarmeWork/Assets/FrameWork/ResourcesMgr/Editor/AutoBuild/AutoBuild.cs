using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

using UnityEditor;
using System.Net;
using System;
#if UNITY_ANDROID
using UnityEditor.Android;
#endif

namespace ShipFarmeWork.Resource.Editor
{
    /// <summary>
    /// 自动打包工具
    /// </summary>
    public class AutoBuild
    {
        //参数路径
        public static string argsFile = System.Environment.CurrentDirectory + "/AutoBuild/buildArgs/Args.txt";
        public static string kyeStorePath = System.Environment.CurrentDirectory + "/AutoBuild/keyStore/onsen.keystore";

        //Icon目录
        public static string iconDir = "icon";
        public static string iconSuffix = "png";
        public static string IconPath = "Assets/StarShip/Res/default/Atlas/GameIcon";

        public struct AutoBuildArgs
        {
            public int platForm;      //选择平台
            public string baseVer;    //基础版本
            public string BunldeVer;  //bundle版本
            public string Server;     //服务器
            public string ChannelId;  //渠道Id
            public int ReleaseModel;  //是否是release模式
            public int CpuDefine;     //CPU定义(0是Mono模式,1是IL2CPP纯ARM不分包,2是IL2CPP纯ARM分包,3是IL2CPP全架构不分包,4是IL2CPP全架构分包)
            public int IsUpdateOss;   //是否更新Oss
            public int IsBuildAs;     //是否构建Apk
        }

        public static AutoBuildArgs GetAutoBuildArgs()
        {

            string str = File.ReadAllText(argsFile);
            str = str.Replace("\r", "");
            str = str.Replace("\n", "");
            string[] strArr = str.Split('-');

            AutoBuildArgs args = new AutoBuildArgs();
            args.platForm = int.Parse(strArr[0]);
            args.baseVer = strArr[1];
            args.BunldeVer = strArr[2];
            args.Server = strArr[3];
            args.ChannelId = strArr[4];
            args.ReleaseModel = int.Parse(strArr[5]);
            args.CpuDefine = int.Parse(strArr[6]);
            args.IsUpdateOss = int.Parse(strArr[7]);
            args.IsBuildAs = int.Parse(strArr[8]);

            Debug.Log("the args is " + args.platForm + "  " + args.baseVer + "  " + args.BunldeVer + "  " + args.Server);

            return args;
        }
        public static void BuildBundle()
        {
            AutoBuildArgs args = GetAutoBuildArgs();

            //打包bundle
            PackageWindow.BuildBundle(args.platForm, args.baseVer, args.BunldeVer);

            //移动streaming
            PackageWindow.CopyToSteamingAsset();
        }

        /// <summary>
        /// 设置一些配置
        /// </summary>
        public static void SetConfig()
        {
            //设置宏定义
            SetDefine(BuildTargetGroup.Android);
            //设置版本
            SetVersion(BuildTargetGroup.Android);
            //设置CPU架构
            SetCpuDefine(BuildTargetGroup.Android);
            //设置图标
            SetIcon(BuildTargetGroup.Android);

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 设置宏定义定义
        /// </summary>
        /// <param name="platform"></param>
        public static void SetDefine(BuildTargetGroup platform)
        {
            AutoBuildArgs args = GetAutoBuildArgs();
            var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(platform);

            //string newSymbol = args.ChannelId + ";" + args.Server;
            string newSymbol = args.Server;
            if (args.ReleaseModel == 1)
            {
                newSymbol = newSymbol + ";ReleaseModel";
            }

            if (symbols != null && symbols.Length > 0)
            {
                newSymbol = string.Format("{0};{1}", symbols, newSymbol);
            }
            PlayerSettings.SetScriptingDefineSymbolsForGroup(platform, newSymbol);
        }

        /// <summary>
        /// 设置CPU架构
        /// </summary>
        /// <param name="platform"></param>
        public static void SetCpuDefine(BuildTargetGroup platform)
        {
            AutoBuildArgs args = GetAutoBuildArgs();
            if (args.CpuDefine == 0)
            {
                //代码架构
                PlayerSettings.SetScriptingBackend(platform, ScriptingImplementation.Mono2x);
                //CPU架构
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.X86;
                //是否分包
                PlayerSettings.Android.useAPKExpansionFiles = false;
            }
            else if (args.CpuDefine == 1)
            {
                PlayerSettings.SetScriptingBackend(platform, ScriptingImplementation.IL2CPP);
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
                PlayerSettings.Android.useAPKExpansionFiles = false;
            }
            else if (args.CpuDefine == 2)
            {
                PlayerSettings.SetScriptingBackend(platform, ScriptingImplementation.IL2CPP);
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
                PlayerSettings.Android.useAPKExpansionFiles = true;
            }
            else if (args.CpuDefine == 3)
            {
                PlayerSettings.SetScriptingBackend(platform, ScriptingImplementation.IL2CPP);
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.All;
                PlayerSettings.Android.useAPKExpansionFiles = false;
            }
            else if (args.CpuDefine == 4)
            {
                PlayerSettings.SetScriptingBackend(platform, ScriptingImplementation.IL2CPP);
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.All;
                PlayerSettings.Android.useAPKExpansionFiles = true;
            }
        }

        /// <summary>
        /// 设置Icon
        /// </summary>
        public static void SetIcon(BuildTargetGroup platform)
        {
            //加载Icon资源
            int[] iconSizes = PlayerSettings.GetIconSizesForTargetGroup(platform);
            Texture2D[] texArray = new Texture2D[iconSizes.Length];
            for (int i = 0; i < iconSizes.Length; ++i)
            {
                int iconSize = iconSizes[i];
                string iconName = (iconSize + "x" + iconSize);
                string path = string.Format("{0}/{1}.{2}", IconPath, iconName, iconSuffix);
                //string path = string.Format("Assets/Plugins/Android/{0}/{1}.{2}", iconDir, iconName, iconSuffix);
                Texture2D tex2D = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
                texArray[i] = tex2D;

                if (tex2D == null) { throw new System.Exception("加载Icon失败,Icon路径是 " + path); }
            }

            //设置Icon
#if UNITY_ANDROID
        SetAndroidKindIcon(AndroidPlatformIconKind.Round, texArray);   //8.0的Icon
        SetAndroidKindIcon(AndroidPlatformIconKind.Legacy, texArray);  //其他版本的Icon
#endif

            AssetDatabase.SaveAssets();
        }

        public static void SetAndroidKindIcon(PlatformIconKind kind, Texture2D[] texArray)
        {
            //设置Icon
            BuildTargetGroup platform = BuildTargetGroup.Android;
            var icons = PlayerSettings.GetPlatformIcons(platform, kind);
            for (int i = 0, length = icons.Length; i < length; ++i)
            {
                //将转换后获得的Texture2D数组，逐个赋值给icons
                icons[i].SetTexture(texArray[i]);
            }
            PlayerSettings.SetPlatformIcons(platform, kind, icons);
        }

        /// <summary>
        /// 设置版本
        /// </summary>
        /// <param name="platform"></param>
        public static void SetVersion(BuildTargetGroup platform)
        {
            AutoBuildArgs args = GetAutoBuildArgs();
            PlayerSettings.bundleVersion = args.baseVer;
            PlayerSettings.Android.bundleVersionCode = int.Parse(args.BunldeVer);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// 设置keystore
        /// </summary>
        public static void SetKeyStore()
        {
            //设置keystoreName
            PlayerSettings.Android.keystoreName = kyeStorePath;
            //PlayerSettings.Android.keystoreName = "C:\\Users\\RBW\\Desktop\\keyStore\\onsen.keystore";
            //设置keystore密码
            PlayerSettings.Android.keystorePass = "v9XDoVJld^3fls";
            //设置keyaliasName
            PlayerSettings.Android.keyaliasName = "onsen";
            //设置keyalias密码
            PlayerSettings.Android.keyaliasPass = "v9XDoVJld^3fls";

            AssetDatabase.SaveAssets();
            Debug.LogWarning("Set KeyStore");
        }

        public static void BuildApk()
        {
            SetKeyStore();

            AutoBuildArgs args = GetAutoBuildArgs();
            //设置
            string outName = "";
            if (args.IsBuildAs == 1)
            {
                outName = GetASName();
                PerformBuild.SetASProject();
            }
            else
            {
                outName = GetApkName();
                PerformBuild.SetApk();
            }
            //构建
#if ReleaseModel
        Debug.LogWarning("ReleaseModel Bulild:" + outName);
        PerformBuild.CommandLineBuildAndroid(outName, BuildOptions.None);
#else
            Debug.LogWarning("DebugModle Bulild:" + outName);
            PerformBuild.CommandLineBuildAndroid(outName, BuildOptions.Development | BuildOptions.ConnectWithProfiler);
#endif
        }

        public static string GetApkName()
        {
            AutoBuildArgs args = GetAutoBuildArgs();
            TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            string time = Convert.ToInt64(ts.TotalSeconds).ToString();
            return "Apk/" + Application.productName + "_" + args.baseVer + "." + args.BunldeVer + "_" + args.ChannelId + "_" + time + ".apk";
        }

        public static string GetASName()
        {
            AutoBuildArgs args = GetAutoBuildArgs();
            TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            string time = Convert.ToInt64(ts.TotalSeconds).ToString();
            return "AS/" + Application.productName + "_" + args.baseVer + "." + args.BunldeVer + "_" + args.ChannelId + "_" + time;
        }

        //hack【资源管理】【自动打包】【Lua文件逻辑】
#if HOTFIX_ENABLE
    /// <summary>
    /// 生成XLua适配文件
    /// </summary>
    public static void BuildXLuaWrap()
    {
#if HOTFIX_ENABLE
        Debug.LogWarning("Open XLua Hot");
#else
        Debug.LogWarning("No Open XLua Hot");
#endif

        var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
        Debug.LogWarning("Current DefeineSymbol is " + symbols);

        Debug.LogWarning("the Editor is isCompiling: " + EditorApplication.isCompiling.ToString());

        Debug.LogWarning("start xlua GenAll");
        CSObjectWrapEditor.Generator.GenAll();
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 清除XLua适配文件
    /// </summary>
    public static void ClearXLuaWrap()
    {
        Debug.LogWarning("start clear");
        CSObjectWrapEditor.Generator.ClearAll();
        AssetDatabase.Refresh();
    }


                /// <summary>
        /// 打开Xlua宏定义
        /// </summary>
        public static void OpenXluaHot()
        {
#if HOTFIX_ENABLE

#else
            //打开XLua的宏定义
            var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
            string newSymbol = "HOTFIX_ENABLE";
            if (symbols != null && symbols.Length > 0)
            {
                newSymbol = string.Format("{0};{1}", symbols, newSymbol);
            }
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, newSymbol);
#endif
            AssetDatabase.Refresh();
        }

#endif

    }
}