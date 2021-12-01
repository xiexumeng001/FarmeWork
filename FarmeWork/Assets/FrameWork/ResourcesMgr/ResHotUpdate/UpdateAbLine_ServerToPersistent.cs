using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ShipFarmeWork.Resource
{
    /// <summary>
    /// 服务器更新资源到Persistent路径
    /// </summary>
    public class UpdateAbLine_ServerToPersistent : UpdateAbLine
    {
        public string hotUpdateVersion = string.Empty;
        List<ResFileInfo> fileList = new List<ResFileInfo>();
        List<ResFileInfo> needCopyList = new List<ResFileInfo>();

        //获取FileList信息
        public override IEnumerator GetFileListInfo()
        {
            bool isReady = false;
            while (!isReady)
            {
                Debug.LogWarning(string.Format("正在从{0}请求热更文件", ResDefine.HotUpdateFileListUrl));

                //提示正在连接服务器
                FirstPage.Instance.UpdatePromptText("10005", true);

                //从配置服务器上下载当前最低启动版本情况
                using (WWW www = new WWW(ResDefine.HotUpdateFileListUrl))
                {
                    yield return ResourcesChecker.Instance.OnWaiting(www);

                    string errorLog = null;
                    if (www.isDone)
                    {
                        //文件下载成功
                        if (www.error == null || www.error == string.Empty)
                        {
                            isReady = true;
                            try
                            {
                                //找到文件
                                string temp = www.text;
                                string[] args = temp.Split('\n');
                                AnalysisFileListInfo(fileList, args, out hotUpdateVersion);
                                Debug.Log(string.Format("Hot Upate Version : {0}", hotUpdateVersion));
                            }
                            catch (Exception e)
                            {
                                errorLog = e.ToString();
                            }
                        }
                        else { errorLog = www.error; }
                    }
                    else { errorLog = "下载超时"; }

                    if (errorLog != null)
                    {
                        Debug.LogError("服务器热更文件获取失败,地址: " + ResDefine.HotUpdateFileListUrl + "\n" + errorLog);
                        //提示玩家是否还要继续下载
                        FirstPage.Instance.UpdatePromptText("10027", true);
                        yield return ResourcesChecker.Instance.WaitForPlayerChoose("10000", "10032", "10016");
                    }
                }
            }
        }

        //是否能热更
        protected override bool IsCanHotUpdate()
        {
            return ResDefine.VersionIsGreater(hotUpdateVersion, ResourcesChecker.Instance.LocalVersion);
        }

        //比较MD5值
        protected override IEnumerator CompareAbMD5()
        {
            FirstPage.Instance.UpdatePromptText("10042", true);

            long totalSize = 0;
            //需要做文件校验
            for (int i = 0; i < fileList.Count; i++)
            {
                //每比对20个就过一帧,别卡
                if (i % 20 == 0)
                {
                    yield return 0;
                }

                bool isAdd = false;
                ResFileInfo fileInfo = fileList[i];
                string fileName = fileInfo.fileName;
                try
                {
                    string persistentFilePath = ResDefine.GetPersistentPath(fileName);
                    if (File.Exists(persistentFilePath))
                    {
                        string persistentMd5 = ResDefine.CalcMD5(persistentFilePath);
                        isAdd = (fileInfo.md5 != persistentMd5);
                    }
                    else { isAdd = true; }
                    //先直接把服务器上的记录上,更新完成,有错的再删
                    PersistentFilesDic[fileName] = fileInfo;
                }
                catch (Exception e)
                {
                    Debug.LogError("和服务器比对MD5出错,资源名称是: " + fileName + "\n" + e);
                    isAdd = true;
                }
                if (isAdd)
                {
                    needCopyList.Add(fileInfo);
                    totalSize += fileInfo.size;
                    //Debug.Log(string.Format("有文件需要更新哦：{0},size{1},md5:{2}->{3}", tempFileList[i].fileName, tempFileList[i].size, persistentMd5, tempFileList[i].md5));
                }
            }


            // 此处可做是否下载判定
            if (needCopyList.Count > 0 && totalSize > 0)
            {
                //只有>100k才提示热更
                if (totalSize >= 100000)
                {
                    FirstPage.Instance.UpdatePromptText("10026", false);
                    bool agreeDownLoad = false;
                    //string detailFrom = string.Format("{0}{1}?{2}", AppConst.HotUpdateUrl, AppConst.UpgradeDetailFileName, System.DateTime.Now.Ticks);
                    Debug.Log("正在请求热更服务器的文件列表" + ResDefine.UpgradeDetailFileUrl);
                    //string detailTo = GetPersistentPath(AppConst.UpgradeDetailFileName);
                    string upgradeDetail = string.Empty;
                    //尝试获取升级说明
                    using (WWW www = new WWW(ResDefine.UpgradeDetailFileUrl))
                    {
                        yield return www;
                        if (www.isDone)
                        {
                            if (www.error == null || www.error == string.Empty)
                            {
                                //找到文件
                                upgradeDetail = www.text;
                            }
                            else
                            {
                                Debug.LogError("Get Upgrade detail failed!");
                            }
                        }
                    }

                    string str = string.Empty;
                    string detail = string.Empty;
                    string sizeStr = ResDefine.CountSizeString(totalSize);
                    bool isLong = false;
                    if (string.IsNullOrEmpty(upgradeDetail))
                    {
                        detail = string.Format(FirstPage.Instance.GetStartUpLangID("10041"), sizeStr);
                    }
                    else
                    {
                        detail = string.Format("{0}\n{1}", string.Format(FirstPage.Instance.GetStartUpLangID("10011"), sizeStr), upgradeDetail);
                        isLong = true;
                    }

                    FirstPage.Instance.SetUpgradeTips(
                    "10014",
                    detail,
                    delegate ()
                    {
                        agreeDownLoad = true;
                        FirstPage.Instance.HideUpgradeTips();
                    },
                    null,
                    "10012",
                    string.Empty,
                    isLong);

                    //等待用户同意下载
                    yield return new WaitUntil(() => agreeDownLoad);

                    FirstPage.Instance.ActiveProgressText(true);
                    FirstPage.Instance.UpdatePromptText("10008", true);
                    FirstPage.Instance.UpdateProgress(0f);
                }
            }
        }

        //复制Ab
        protected override IEnumerator CopyAb()
        {
            bool bundleFilesReady = false;
            bool isDownLoadOk = false;
            //移动到临时列表中呢
            List<ResFileInfo> tempDownLoadList = new List<ResFileInfo>();
            foreach (var item in needCopyList)
            {
                tempDownLoadList.Add(item);
            }

            int tryTimes = 0;
            int maxTryTimes = 5;
            if (!string.IsNullOrEmpty(ServerResConfig.MaxTryTimes)) { maxTryTimes = int.Parse(ServerResConfig.MaxTryTimes); }
            while (needCopyList.Count > 0 && !bundleFilesReady)
            {
                ++tryTimes;

                yield return DownLoadFile(tempDownLoadList);

                //检测下下载的有没有问题
                tempDownLoadList.Clear();
                for (int i = 0; i < needCopyList.Count; ++i)
                {
                    string fileName = needCopyList[i].fileName;
                    string toLocal = ResDefine.GetPersistentPath(fileName);
                    bool isSame = true;
                    try
                    {
                        if (File.Exists(toLocal))
                        {
                            string fileMD5 = ResDefine.CalcMD5(toLocal);
                            isSame = fileMD5.Equals(needCopyList[i].md5);

                            if (!isSame)
                            {
                                ResFileInfo fileInfo = new ResFileInfo();
                                fileInfo.fileName = fileName;
                                fileInfo.md5 = fileMD5;
                                fileInfo.size = needCopyList[i].size;
                                PersistentFilesDic[fileName] = fileInfo;
                            }
                        }
                        else
                        {
                            isSame = false;

                            PersistentFilesDic[fileName] = null;
                        }
                    }
                    catch (Exception e)
                    {
                        isSame = false;
                        Debug.LogError("获取MD5失败,路径是:" + toLocal + "\n" + e);
                    }
                    if (!isSame) { tempDownLoadList.Add(needCopyList[i]); }
                }

                isDownLoadOk = (tempDownLoadList.Count == 0);
                //没有问题或者超过最大次数了,就过
                bundleFilesReady = isDownLoadOk || (tryTimes >= maxTryTimes);
            }

            //下载成功保存版本,如果有未下载成功的,就不保存了,下一次继续校验下载
            if (isDownLoadOk) { ResourcesChecker.Instance.SetLocalVersion(hotUpdateVersion); }
        }

        protected override void OnNoUpdate()
        {
            FirstPage.Instance.UpdatePromptText("10029", true);
        }

        //下载文件
        private IEnumerator DownLoadFile(List<ResFileInfo> tempDownLoadList)
        {
            //计算全部下载大小
            float totalSize = 0.1f;
            for (int i = 0; i < tempDownLoadList.Count; ++i)
            {
                totalSize += tempDownLoadList[i].size;
            }
            float totalDownLoadSize = 0f;
            long startDownLoadTick = DateTime.Now.Ticks;
            //开始下载文件
            for (int i = 0; i < tempDownLoadList.Count; ++i)
            {
                string fileName = tempDownLoadList[i].fileName;
                string fromUrl = string.Format("{0}{1}", ResDefine.HotUpdateUrl, fileName);
                string toLocal = ResDefine.GetPersistentPath(fileName);
                Debug.Log(string.Format("正在下载文件{0}->{1}", fromUrl, toLocal));
                using (WWW www = new WWW(fromUrl))
                {
                    yield return www;
                    if (www.isDone && www.error == null || www.error == string.Empty)
                    {
                        try
                        {
                            if (File.Exists(toLocal))
                                File.Delete(toLocal);

                            if (!Directory.Exists(Path.GetDirectoryName(toLocal)))
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(toLocal));
                            }

                            File.WriteAllBytes(toLocal, www.bytes);
                            string fileMD5 = ResDefine.CalcMD5(toLocal);
                            if (tempDownLoadList[i].md5.Equals(fileMD5))
                            {
                                Debug.Log(string.Format("文件下载成功，md5相同为：{0}", fileMD5));
                            }
                            else
                            {
                                Debug.LogError(string.Format("文件下载成功，md5不相同为：{0}->{1}", fileMD5, tempDownLoadList[i].md5));
                            }

                            //已经下载量
                            totalDownLoadSize += tempDownLoadList[i].size;
                        }
                        catch (Exception e)
                        {
                            Debug.LogErrorFormat("下载服务器文件失败,{0},{1}\n{2} ", fromUrl, toLocal, e);
                        }
                    }
                    else
                    {
                        Debug.LogErrorFormat("下载服务器文件失败,{0},{1} ", fromUrl, toLocal);
                    }
                    //更新下载速度
                    float rate = totalDownLoadSize / totalSize;
                    float timeElpasedSec = ((DateTime.Now.Ticks - startDownLoadTick) / 10000000.0f);
                    timeElpasedSec = Mathf.Max(timeElpasedSec, 0.001f);
                    float downLoadSpeed = totalDownLoadSize / timeElpasedSec;
                    string strDownLoadSpeed = string.Format("({0}/s)", ResDefine.CountSizeString((long)downLoadSpeed));
                    FirstPage.Instance.UpdateProgress(rate);
                    FirstPage.Instance.UpdateDownLoadSpeed(strDownLoadSpeed);
                }
            }

            FirstPage.Instance.UpdateDownLoadSpeed(string.Empty);
        }

    }
}