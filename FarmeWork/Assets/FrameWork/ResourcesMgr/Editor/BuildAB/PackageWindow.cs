using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using System.IO;
using System.Linq;

namespace ShipFarmeWork.Resource.Editor
{
    public class PackageWindow : EditorWindow
    {
        [MenuItem("Sg打包/打包窗口", false, 400)]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<PackageWindow>("打包设置");
        }

#if UNITY_EDITOR_WIN
        static int platformInt = 0;
#elif UNITY_EDITOR_OSX
    static int platformInt = 1;
#endif

        static string[] platforms = { "Android", "iOS", "Win" };

        static string baseVersion = "1.0.0";
        static string bundleVersion = "0";
        static bool _AutomaticCopy = false;


        static string oldVersion = "1.0.0.0";
        static string newVersion = "1.0.0.1";
        static string changeAndUplvConfigs = "Type,Path,Suffix\n\n\n\n\n";


        void OnGUI()
        {
            if (UnityEditor.EditorApplication.isPlaying)
            {
                GUILayout.Label("请在非运行状态整这个！");
                return;
            }
            GUILayout.BeginVertical();


            GUILayout.BeginHorizontal();
            platformInt = GUILayout.Toolbar(platformInt, platforms);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Base Version");
            baseVersion = GUILayout.TextField(baseVersion);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Bundle Version");
            bundleVersion = GUILayout.TextField(bundleVersion);
            GUILayout.EndHorizontal();

            if (GUILayout.Button("打包Bundle资源"))
            {
                BuildBundle();
            }

            GUILayout.Space(20);
            //GUILayout.BeginVertical("", "box");
            {
                GUILayout.Label("更新某些资源并升级版本(预制体等不可单独更新,因为有依赖关系)");
                GUILayout.BeginHorizontal();
                GUILayout.Label("oldVersion:", GUILayout.Width(60f));
                oldVersion = GUILayout.TextField(oldVersion, GUILayout.Width(100f));
                GUILayout.Label("newVersion:", GUILayout.Width(60f));
                newVersion = GUILayout.TextField(newVersion, GUILayout.Width(100f));
                GUILayout.EndHorizontal();
                GUILayout.Label("替换的资源(Lua资源默认更新):");
                changeAndUplvConfigs = GUILayout.TextArea(changeAndUplvConfigs);
                if (GUILayout.Button("更新升级代码资源"))
                {
                    ChangeBuildBundleAndUpLv();
                }
            }
            //GUILayout.EndVertical();

            GUILayout.Space(20);

            _AutomaticCopy = GUILayout.Toggle(_AutomaticCopy, "是否自动复制");

            if (GUILayout.Button("复制到StreamingAssets (可选)"))
            {
                CopyToSteamingAsset();
            }
            GUILayout.Space(10);

            if (GUILayout.Button("检测重复资源(StreamingAssets)"))
            {
                PackagerRedundancyCheck check = new PackagerRedundancyCheck(Application.streamingAssetsPath, "StreamingAssets", "*" + ResourceConfig.AbSuffix);
                check.StartCheck();
            }

            if (GUILayout.Button("检查Material文件是否有隐藏依赖"))
            {
                CheckMatrialHideDependOn();
            }

            GUILayout.EndVertical();

            //if (GUILayout.Button("出Apk之前准备"))
            //{
            //    BuildApkPrepare();
            //}

        }

        public static void BuildBundle()
        {
            if (platformInt == 0)
                Packager.buildTarget = BuildTarget.Android;
            else if (platformInt == 1)
                Packager.buildTarget = BuildTarget.iOS;
            else if (platformInt == 2)
                Packager.buildTarget = BuildTarget.StandaloneWindows64;// win平台的资源是有区别的？


            Packager.baseVersion = baseVersion;
            Packager.bundleVersion = bundleVersion;
            Packager.replaceExistAsset = true;
            Packager.changeSingleAndUpLv = false;
            Packager.inputAbConfigs = "";

            Packager.OnlyBuildBundle();

            EditorUtility.DisplayDialog("通知", "资源包:" + baseVersion + "." + bundleVersion + "打好了", "ok");

            // 自动复制
            if (_AutomaticCopy)
            {
                CopyToSteamingAsset();
            }

        }

        /// <summary>
        /// 替换bundle并且升级版本
        /// </summary>
        public static void ChangeBuildBundleAndUpLv()
        {
            if (platformInt == 0)
                Packager.buildTarget = BuildTarget.Android;
            else if (platformInt == 1)
                Packager.buildTarget = BuildTarget.iOS;
            else if (platformInt == 2)
                Packager.buildTarget = BuildTarget.StandaloneWindows64;// win平台的资源是有区别的？

            string[] newVersionStrArr = newVersion.Split('.');
            string[] oldVersionStrArr = oldVersion.Split('.');

            string oldVersionPath = Packager.GetBundleVersionPath(oldVersionStrArr[3], true);
            if (!Directory.Exists(oldVersionPath))
            {
                Debug.LogError("没找到老版本");
                return;
            }

            string newBaseVersion = newVersionStrArr[0] + '.' + newVersionStrArr[1] + '.' + newVersionStrArr[2];
            baseVersion = newBaseVersion;
            bundleVersion = newVersionStrArr[3];

            Packager.baseVersion = baseVersion;
            Packager.bundleVersion = bundleVersion;
            Packager.replaceExistAsset = true;
            Packager.changeSingleAndUpLv = true;
            Packager.inputAbConfigs = changeAndUplvConfigs.TrimEnd('\n');
            //打包
            Packager.OnlyBuildBundle();
            //替换
            Packager.CopyOldToNew(oldVersionStrArr[3], newVersionStrArr[3]);

            EditorUtility.DisplayDialog("通知", "资源包:" + baseVersion + "." + bundleVersion + "打好了", "ok");

            // 自动复制
            if (_AutomaticCopy)
            {
                CopyToSteamingAsset();
            }

            string path = Path.Combine(Application.dataPath, "UpBundleRecord.txt");
            string writeLog = string.Format("版本:{0}=>{1}\n更新的资源:{2}\n\n\n", oldVersion, newVersion, Packager.inputAbConfigs);
            if (File.Exists(path))
            {
                writeLog = writeLog + File.ReadAllText(path);
            }
            File.WriteAllText(path, writeLog);

            AssetDatabase.Refresh();
        }


        /// <summary>
        /// 复制到StreamingAsset文件夹中
        /// </summary>
        public static void CopyToSteamingAsset()
        {
            Packager.baseVersion = baseVersion;
            Packager.bundleVersion = bundleVersion;
            Packager.CopyBundleToStreamingAssets(bundleVersion);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 给自动打包用的
        /// </summary>
        public static void BuildBundle(int platFom, string baseVer, string bundleVer)
        {
            //设置参数
            platformInt = platFom;
            baseVersion = baseVer;
            bundleVersion = bundleVer;

            BuildBundle();
        }

        /// <summary>
        /// 检查材质隐藏依赖
        /// </summary>
        public static void CheckMatrialHideDependOn()
        {
            string[] allMaterial = Directory.GetFiles(Application.dataPath, "*.mat", SearchOption.AllDirectories);
            foreach (var item in allMaterial)
            {
                string file = "Assets" + item.Replace(Application.dataPath, "");
                Material oldMa = AssetDatabase.LoadAssetAtPath<Material>(file);
                Material newMa = new Material(oldMa.shader);
                string[] properArr = oldMa.GetTexturePropertyNames();
                foreach (var properItem in properArr)
                {
                    if (!newMa.HasProperty(properItem))
                    {
                        Texture texture = oldMa.GetTexture(properItem);
                        if (texture != null)
                        {
                            string textPath = AssetDatabase.GetAssetPath(texture);
                            Debug.Log(string.Format("材质名称:{0},属性名:{1},隐藏依赖了:{2}", file, properItem, textPath), oldMa);
                        }
                    }
                }
            }

            Debug.Log("检查完毕");
        }

        //public static void BuildApkPrepare()
        //{
        //    //XLua的宏定义
        //    var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
        //    string newSymbol = "HOTFIX_ENABLE";
        //    if (symbols != null && symbols.Length > 0)
        //    {
        //        if (!symbols.Contains("HOTFIX_ENABLE"))
        //        {
        //            newSymbol = string.Format("{0};{1}", symbols, newSymbol);
        //        }
        //    }
        //    PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, newSymbol);

        //    //Xlua生成warp文件
        //    CSObjectWrapEditor.Generator.ClearAll();
        //    CSObjectWrapEditor.Generator.GenAll();
        //}
    }
}