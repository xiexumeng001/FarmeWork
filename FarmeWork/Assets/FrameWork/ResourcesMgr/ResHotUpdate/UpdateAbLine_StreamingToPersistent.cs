using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ShipFarmeWork.Resource
{
    /// <summary>
    /// Streaming更新资源到Persistent路径
    /// </summary>
    public class UpdateAbLine_StreamingToPersistent : UpdateAbLine
    {
        public string streamingVersion = null;
        List<ResFileInfo> fileList = new List<ResFileInfo>();
        List<ResFileInfo> needCopyList = new List<ResFileInfo>();
        Dictionary<string, ResFileInfo> streamingFileDic = new Dictionary<string, ResFileInfo>();

        public override IEnumerator GetFileListInfo()
        {
            //提示要开始获取本地版本等级及文件列表
            FirstPage.Instance.UpdatePromptText("10021", true);

            string filePath = ResDefine.GetStreamingAssetsPath(ResDefine.StreamingVersionFile);

            fileList = new List<ResFileInfo>();
            string[] args = null;
#if UNITY_EDITOR || UNITY_IOS
            try
            {
                args = File.ReadAllLines(filePath);
            }
            catch (Exception e) { ExcepStopLog = e.ToString(); }
#else
            using (WWW www = new WWW(filePath))
            {
                yield return www;
                if (www.isDone)
                {
                    if (www.error == null || www.error == string.Empty)
                    {
                        try
                        {
                            //找到文件
                            string temp = www.text;
                            args = temp.Split('\n');
                        }
                        catch (Exception e) { ExcepStopLog = e.ToString(); }
                    }
                    else { ExcepStopLog = www.error; }
                }
            }
#endif
            if (ExcepStopLog == null)
            {
                try
                {
                    if (args != null && args.Length > 0)
                    {
                        AnalysisFileListInfo(fileList, args, out streamingVersion);
                        Debug.Log(string.Format("StreamingVersion Version : {0}", streamingVersion));
                    }
                    else { ExcepStopLog = "获取到的streamingfile文件的信息是空的"; }
                }
                catch (Exception e) { ExcepStopLog = e.ToString(); }
            }
            if (ExcepStopLog != null)
            {
                //提示本地版本等级及文件列表获取失败
                Debug.LogError("获取streamingFile失败:\n" + ExcepStopLog);
                FirstPage.Instance.UpdatePromptText("10022", false);
                yield break;
            }
        }

        protected override bool IsCanHotUpdate()
        {
            return ResDefine.VersionIsGreater(streamingVersion, ResourcesChecker.Instance.LocalVersion);
        }

        protected override IEnumerator CompareAbMD5()
        {
            FirstPage.Instance.UpdatePromptText("10018", true);

            //校验stremaing文件和per文件
            for (int i = 0; i < fileList.Count; ++i)
            {
                bool isAdd = false;
                ResFileInfo fileInfo = fileList[i];
                string fileName = fileInfo.fileName;
                try
                {
                    string persistentFilePath = ResDefine.GetPersistentPath(fileName);
                    if (File.Exists(persistentFilePath))
                    {
                        string persistentMd5 = ResDefine.CalcMD5(persistentFilePath);
                        isAdd = fileInfo.md5 != persistentMd5;
                    }
                    else { isAdd = true; }
                    PersistentFilesDic[fileName] = fileInfo;
                }
                catch (Exception e)
                {
                    isAdd = true;
                    Debug.LogError(" 校验StreammingMD5失败,资源是 " + fileName + "\n" + e);
                }
                if (isAdd) { needCopyList.Add(fileInfo); }
            }

            //需要进行文件拷贝 
            if (needCopyList.Count > 0)
            {
                FirstPage.Instance.ActiveProgressText(true);
                FirstPage.Instance.UpdateProgress(0f);
                FirstPage.Instance.UpdatePromptText("10004", true);
                //yield return new WaitForSeconds(stepHold);
            }
            yield return null;
        }

        protected override IEnumerator CopyAb()
        {
            //文件拷贝
            for (int i = 0; i < needCopyList.Count; ++i)
            {
                string from = ResDefine.GetStreamingAssetsPath(needCopyList[i].fileName);
                string to = ResDefine.GetPersistentPath(needCopyList[i].fileName);
                Debug.Log(string.Format("Copy file from {0} -> {1}", from, to));

                try
                {
                    if (File.Exists(to)) { File.Delete(to); }
                    string toDir = Path.GetDirectoryName(to);
                    if (!Directory.Exists(toDir)) { Directory.CreateDirectory(toDir); }
                    //更新进度
                    float _progress = (i + 1f) / needCopyList.Count;
                    FirstPage.Instance.UpdateProgress(_progress);
                }
                catch (Exception e)
                {
                    ExcepStopLog = e.ToString();
                }
                if (ExcepStopLog == null)
                {
#if UNITY_EDITOR || UNITY_IOS
                    try
                    {
                        //文件拷贝
                        File.Copy(from, to, true);
                    }
                    catch (Exception e) { ExcepStopLog = e.ToString(); }
                    yield return null;
#else
                    using (WWW www = new WWW(from))
                    {
                        yield return www;
                        if (www.isDone && www.error == null || www.error == string.Empty)
                        {
                            try
                            {
                                File.WriteAllBytes(to, www.bytes);
                            }
                            catch (Exception e) { ExcepStopLog = e.ToString(); }
                        }
                        else { ExcepStopLog = www.error; }
                    }
#endif
                }
                if (ExcepStopLog != null)
                {
                    Debug.LogErrorFormat("streaming资源复制失败,{0},{1}\n{2}", from, to, ExcepStopLog);
                    FirstPage.Instance.UpdatePromptText("10043", false);
                    yield break;
                }
            }

            //更新本地版本
            ResourcesChecker.Instance.SetLocalVersion(streamingVersion);
        }

        protected override void OnNoUpdate()
        {

        }
    }
}