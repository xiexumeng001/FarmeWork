using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ShipFarmeWork.Resource;
using System.IO;
using System;

//更新资源流程
public abstract class UpdateAbLine
{
    //当前本地文件的文件列表
    protected Dictionary<string, ResFileInfo> PersistentFilesDic = new Dictionary<string, ResFileInfo>();
    public string ExcepStopLog = null;

    public IEnumerator StartUpdate(bool isNeedGetFileListInfo)
    {
        if (isNeedGetFileListInfo)
        {
            yield return GetFileListInfo();
            if (ExcepStopLog != null) yield break;
        }
        if (IsCanHotUpdate())
        {
            yield return CompareAbMD5();
            if (ExcepStopLog != null) yield break;

            yield return CopyAb();
            if (ExcepStopLog != null) yield break;

            WirteLocalFileList();
        }
        else
        {
            OnNoUpdate();
        }
    }

    //获取FileList信息
    public abstract IEnumerator GetFileListInfo();
    //是否能热更
    protected abstract bool IsCanHotUpdate();
    //比较MD5值
    protected abstract IEnumerator CompareAbMD5();
    //复制Ab
    protected abstract IEnumerator CopyAb();
    //当不更新
    protected abstract void OnNoUpdate();

    //写入本地FileList文件,读取资源的时候可能会用到
    private void WirteLocalFileList()
    {
        if (PersistentFilesDic != null && PersistentFilesDic.Count > 0)
        {
            List<string> localFileList = new List<string>();
            foreach (var fileInfo in PersistentFilesDic)
            {
                if (fileInfo.Value != null)
                {
                    localFileList.Add(fileInfo.Value.Desc);
                }
            }
            string localFileListFilePath = ResDefine.GetPersistentPath(ResDefine.fileList);
            if (File.Exists(localFileListFilePath)) { File.Delete(localFileListFilePath); }

            File.WriteAllLines(localFileListFilePath, localFileList.ToArray());
        }
    }

    //解析FileList文件的信息
    protected void AnalysisFileListInfo(List<ResFileInfo> tempFileList, string[] args, out string version)
    {
        tempFileList.Clear();
        version = "0.0.0.0";
        for (int i = 0; i < args.Length; ++i)
        {
            if (i == 0)
            {
                //获取版本等级
                version = args[0].Replace(ResDefine.Version, string.Empty);
            }
            else if (args[i].Length < 1)
            {
                continue;
            }
            else
            {
                string[] temps = args[i].Split('|');
                if (temps.Length == 3)
                {
                    string fileName = temps[0];
                    string md5 = temps[1];
                    int size = int.Parse(temps[2]);
                    ResFileInfo info = new ResFileInfo();
                    info.fileName = fileName;
                    info.md5 = md5;
                    info.size = size;
                    tempFileList.Add(info);
                }
            }
        }
    }
}