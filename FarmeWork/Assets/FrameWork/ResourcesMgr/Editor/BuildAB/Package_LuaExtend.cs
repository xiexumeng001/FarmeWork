//hack【资源管理】Lua扩展
#if false
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ShipFarmeWork.Resource.Editor
{
    public class Package_LuaExtend
    {
        /// <summary>
        /// 处理Lua代码包
        /// </summary>
        static void HandleLuaBundle()
        {
            string streamDir = Application.dataPath + "/" + AppConst.LuaTempDir;            // Asset/Lua
            if (!Directory.Exists(streamDir)) Directory.CreateDirectory(streamDir);

            string[] srcDirs = {
            CustomSettings.luaDir,
            CustomSettings.toluaLuaDir ,
            CustomSettings.gameLuaDir,
        };
            for (int i = 0; i < srcDirs.Length; i++)
            {
                if (AppConst.LuaByteMode)
                {
                    string sourceDir = srcDirs[i];
                    string[] files = Directory.GetFiles(sourceDir, "*.lua", SearchOption.AllDirectories);
                    int len = sourceDir.Length;

                    if (sourceDir[len - 1] == '/' || sourceDir[len - 1] == '\\')
                    {
                        --len;
                    }
                    for (int j = 0; j < files.Length; j++)
                    {
                        string str = files[j].Remove(0, len);
                        string dest = streamDir + str + ".bytes";
                        string dir = Path.GetDirectoryName(dest);
                        Directory.CreateDirectory(dir);
                        EncodeLuaFile(files[j], dest);
                    }
                }
                else
                {
                    ToLuaMenu.CopyLuaBytesFiles(srcDirs[i], streamDir);
                }
            }
            string[] dirs = Directory.GetDirectories(streamDir, "*", SearchOption.AllDirectories);
            for (int i = 0; i < dirs.Length; i++)
            {
                string name = dirs[i].Replace(streamDir, string.Empty);
                name = name.Replace('\\', '_').Replace('/', '_');
                name = "lua/lua_" + name.ToLower() + AppConst.ExtName;

                string path = "Assets" + dirs[i].Replace(Application.dataPath, "");
                AddBuildMap(name, "*.bytes", path);
            }
            AddBuildMap("lua/lua" + AppConst.ExtName, "*.bytes", "Assets/" + AppConst.LuaTempDir);

            AssetDatabase.Refresh();
        }

        public static void EncodeLuaFile(string srcFile, string outFile)
        {
            if (!srcFile.ToLower().EndsWith(".lua"))
            {
                File.Copy(srcFile, outFile, true);
                return;
            }
            bool isWin = true;
            string luaexe = string.Empty;
            string args = string.Empty;
            string exedir = string.Empty;
            string currDir = Directory.GetCurrentDirectory();
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                isWin = true;
                luaexe = "luajit.exe";
                args = "-b " + srcFile + " " + outFile;
                exedir = AppDataPath.Replace("assets", "") + "LuaEncoder/luajit/";
            }
            else if (Application.platform == RuntimePlatform.OSXEditor)
            {
                isWin = false;
                luaexe = "./luac";
                args = "-o " + outFile + " " + srcFile;
                exedir = AppDataPath.Replace("assets", "") + "LuaEncoder/luavm/";
            }
            Directory.SetCurrentDirectory(exedir);
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = luaexe;
            info.Arguments = args;
            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.ErrorDialog = true;
            info.UseShellExecute = isWin;
            Debug.Log(info.FileName + " " + info.Arguments);

            Process pro = Process.Start(info);
            pro.WaitForExit();
            Directory.SetCurrentDirectory(currDir);
        }

        /// <summary>
        /// 将一个文件夹下的资源生成一个AssetBundleBuild打入map
        /// </summary>
        static void AddBuildMap(string bundleName, string pattern, string path)
        {
            if (path.Contains(".svn"))
            {
                return;
            }
            string[] files = Directory.GetFiles(path, pattern);
            if (files.Length == 0) return;

            for (int i = 0; i < files.Length; i++)
            {
                files[i] = files[i].Replace('\\', '/');
            }
            AssetBundleBuild build = new AssetBundleBuild();
            build.assetBundleName = bundleName;
            build.assetNames = files;
            //	UnityEngine.Debug.Log ("files:"+files+"|||"+bundleName+"-----"+path);
            maps.Add(build);
        }

        /// <summary>
        /// 清除临时文件夹
        /// </summary>
        public static void ClearTempFolder()
        {
            UnityEngine.Debug.Log("正在清除临时文件夹");
            // 清除lua临时目录
            ResetFolder(Application.dataPath + "/" + AppConst.LuaTempDir, false);

        }

    }
}
#endif