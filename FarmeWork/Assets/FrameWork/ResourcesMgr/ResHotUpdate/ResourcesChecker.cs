using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

namespace ShipFarmeWork.Resource
{
    public enum ResCheckRst
    {
        SUCCESS,
        LOW_VERSION,    //版本等级太低，不能启动
    }
    public enum ResCheckType
    {
        VERIFY,         //文件校验中
        CHECKUPDATE,    //检测文件是否需要更新
        DOWNLOAD,       //文件下载中
    }

    public class ResFileInfo
    {
        public string fileName;
        public string md5;
        public int size;

        public string Desc
        {
            get
            {
                return string.Format("{0}|{1}|{2}", fileName, md5, size);
            }
        }
    }

    /// <summary>
    /// hack【暂改】【热更新】这儿的逻辑有优化,Streaming到沙盒路径下的那一步不需要了,有时间在优化
    /// </summary>
    public class ResourcesChecker
        : MonoBehaviour
    {
        private static ResourcesChecker instance = null;

        private System.Action<ResCheckRst> m_checkCallback = null;

        //当前Persistent版本
        public string LocalVersion = string.Empty;

        //创建实例
        public static ResourcesChecker CreateInstance(GameObject mainObj)
        {
            if (instance != null)
            {
                Debug.LogWarning("资源检查器重复，忽视");
                return instance;
            }
            if (mainObj == null)
            {
                Debug.LogError("创建资源检查器失败！空对象");
                return null;
            }
            instance = mainObj.GetComponent<ResourcesChecker>();
            if (!instance)
                instance = mainObj.AddComponent<ResourcesChecker>();

            return instance;
        }

        public static ResourcesChecker Instance
        {
            get
            {
                if (instance == null)
                    Debug.LogError("请先创建实例");

                return instance;
            }
        }

        //检查资源情况
        public void CheckResources(System.Action<ResCheckRst> checkCallback)
        {
            m_checkCallback = checkCallback;
            StartCoroutine(Do());
        }

        private bool waitForPlayerChoose = false;
        public IEnumerator WaitForPlayerChoose(string title, string detail, string btn)
        {
            //提示玩家并等待
            waitForPlayerChoose = false;

            FirstPage.Instance.SetUpgradeTips(
                 title,
                 detail,
                 delegate ()
                 {
                     waitForPlayerChoose = true;
                     //继续等待
                     FirstPage.Instance.HideUpgradeTips();
                 },
                 null,
                 btn,
                 string.Empty);

            //等待玩家作出回复
            yield return new WaitUntil(() => waitForPlayerChoose);

        }

        private IEnumerator Do()
        {
            //做一次网络连接检测
            //yield return CheckConnection("10000", "10015", "10016");

            //提示正在启动游戏
            FirstPage.Instance.ActiveProgress(true);
            FirstPage.Instance.ActiveProgressText(false);
            FirstPage.Instance.UpdatePromptText("10001", true);

            //yield return new WaitForSeconds(stepHold);

            //获取服务器config，必须要获取到呢~
            yield return GetServerConfig();

            //尝试获取本地版本等级
            LocalVersion = PlayerPrefs.GetString(ResourceConfig.LocalVersionKey, "0.0.0.0");
            Debug.Log(string.Format("本地版本为 : {0}", LocalVersion));
            if (LocalVersion == "0.0.0.0")
            {
                Debug.Log("当前为首次启动游戏，需要等待的久一点。");
                //提示首次启动游戏要等待的久一点
                FirstPage.Instance.UpdatePromptText("10020", true);
            }

            //提前获取下streaming信息,需要版本号
            UpdateAbLine_StreamingToPersistent streamingLine = new UpdateAbLine_StreamingToPersistent();
            yield return streamingLine.GetFileListInfo();
            if (streamingLine.ExcepStopLog != null) { yield break; }

            //判断是否需要强更
            if (ResDefine.VersionIsGreater(ServerResConfig.LowestVersion, streamingLine.streamingVersion) && ResDefine.VersionIsGreater(ServerResConfig.LowestVersion, LocalVersion))
            {
                //需要强更，不用再挣扎了！
                if (m_checkCallback != null)
                {
                    m_checkCallback(ResCheckRst.LOW_VERSION);
                }
                yield break;
            }

            //从Streaming assets文件夹中读取文件
            if (ServerResConfig.LoadFromStreamingAssets)
            {
                SetLocalVersion(streamingLine.streamingVersion);

                Debug.Log("Load res from streaming assets.");
                //yield return new WaitForSeconds(stepHold);

                m_checkCallback(ResCheckRst.SUCCESS);
                yield break;
            }

            //Streaming更新到沙盒路径流程
            yield return streamingLine.StartUpdate(false);
            if (streamingLine.ExcepStopLog != null) { yield break; }

            //更新完毕
            FirstPage.Instance.UpdatePromptText("10024", true);
            FirstPage.Instance.ActiveProgressText(false);

            //服务器资源更新到沙盒路径流程
            if (ServerResConfig.HotUpdate)
            {
                UpdateAbLine_ServerToPersistent serverLine = new UpdateAbLine_ServerToPersistent();
                yield return serverLine.StartUpdate(true);
                if (serverLine.ExcepStopLog != null) { yield break; }
            }
            else
            {
                FirstPage.Instance.UpdatePromptText("10030", true);
            }

            Debug.LogWarning("启动游戏，当前版本号为：" + PlayerPrefs.GetString(ResourceConfig.LocalVersionKey));
            //检测完成
            if (m_checkCallback != null)
            {
                m_checkCallback(ResCheckRst.SUCCESS);
            }
        }


        //设置本地版本
        public void SetLocalVersion(string version)
        {
            LocalVersion = version;
            PlayerPrefs.SetString(ResourceConfig.LocalVersionKey, version);
        }


        //当等待下载中
        public IEnumerator OnWaiting(WWW www)
        {
            float waiTime = 0;
            while (!www.isDone)
            {
                yield return new WaitForSeconds(0.5f);

                waiTime += 0.5f;
                if (waiTime > 15f)
                {
                    //超过15秒没有下载到文件
                    break;
                }
                else if (waiTime > 10f)
                {//超过10秒还没有下载到文件，推测是网络连接出现了问题
                    FirstPage.Instance.UpdatePromptText("10017", true);
                }
                //超过5秒还没有下载到文件
                else if (waiTime > 5f)
                {
                    //都5秒了，估计悬了
                    //都他妈两秒了，还没done?
                    FirstPage.Instance.UpdatePromptText("10010", true);
                }
                //超过2秒还没下载到文件
                else if (waiTime > 2f)
                {
                    //都他妈两秒了，还没done?
                    FirstPage.Instance.UpdatePromptText("10003", true);
                }
            }
        }

        //从服务器上获取配置文件(必须获取到，否则将无法游戏)
        private IEnumerator GetServerConfig()
        {
            bool serverConfigReady = false;
            while (!serverConfigReady)
            {
                //提示正在连接服务器
                FirstPage.Instance.UpdatePromptText("10001", true);

                Debug.LogWarning(string.Format("正在从{0}请求服务器配置", ResourceConfig.HotUpdateConfigUrl));
                //从配置服务器上下载当前最低启动版本情况
                using (WWW www = new WWW(ResourceConfig.HotUpdateConfigUrl))
                {
                    yield return OnWaiting(www);

                    string errorLog = null;
                    if (www.isDone)
                    {
                        //文件下载成功
                        if (www.error == null || www.error == string.Empty)
                        {
                            try
                            {
                                //配置信息初始化
                                ServerResConfig.Init(www.text);
                                serverConfigReady = true;
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
                        Debug.LogError("获取服务器配置文件失败,地址: " + ResourceConfig.HotUpdateConfigUrl + '\n' + errorLog);
                        FirstPage.Instance.UpdatePromptText("10019", false);
                        //提示玩家是否还要继续下载
                        yield return WaitForPlayerChoose("10000", "10032", "10016");
                    }
                }
            }
        }

    }
}