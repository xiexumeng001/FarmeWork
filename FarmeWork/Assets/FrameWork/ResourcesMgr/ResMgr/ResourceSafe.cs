using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;

namespace ShipFarmeWork.Resource
{
    /// <summary>
    /// 资源请求记录
    /// </summary>
    public class SafeRequestRecord
    {
        public string RequestName;
        public string BundleName;
        public string AssetName;
        public UnityEngine.Object Res;
    }

    /// <summary>
    /// 资源安全
    /// </summary>
    public class ResourceSafe : MonoBehaviour
    {
        public static ResourceSafe Instance = null;
        public ResourceManager ResMgr;
        public Dictionary<AssetBundleInfoNode, bool> checkedResDic;
        public WaitForSeconds Second = new WaitForSeconds(5);

        public Dictionary<System.Object, List<SafeRequestRecord>> SafeRequestRecordDic = new Dictionary<System.Object, List<SafeRequestRecord>>();

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            Instance = null;
            checkedResDic.Clear();
            SafeRequestRecordDic.Clear();
        }

        public void StartCheck(ResourceManager resMgr)
        {
            ResMgr = resMgr;
            checkedResDic = new Dictionary<AssetBundleInfoNode, bool>();
            StartCoroutine(Check());
        }

        private IEnumerator Check()
        {
            while (true)
            {
                try
                {
                    CheckRequest();
                    CheckCloneRes();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
                yield return Second;
            }
        }


        /// <summary>
        /// 检查资源记录(用来检查图片,材质等资源的记录)
        /// </summary>
        public void CheckRequest()
        {
            foreach (var item in SafeRequestRecordDic)
            {
                bool isLose = false;
                if (item.Key is MonoBehaviour component)
                {
                    isLose = (component == null);
                }
                else if (item.Key is GameObject game)
                {
                    isLose = (game == null);
                }
                if (isLose)
                {
                    string requName = "";
                    string log = "";
                    for (int i = 0; i < item.Value.Count; i++)
                    {
                        SafeRequestRecord record = item.Value[i];
                        requName = record.RequestName;
                        log = log + record.BundleName + " 的 " + record.AssetName + "\t";
                    }
                    Debug.LogErrorFormat("请求者{0}为空,但未释放对 {1} 的占用", requName, log);
                }
            }
        }

        /// <summary>
        /// 检查克隆资源
        /// </summary>
        public void CheckCloneRes()
        {
            foreach (var item in SafeRequestRecordDic)
            {
                for (int i = 0; i < item.Value.Count; i++)
                {
                    SafeRequestRecord record = item.Value[i];
                    if (record.Res is GameObject game)
                    {
                        if (game == null)
                        {
                            Debug.LogErrorFormat("预制体{0}的实例物体已被销毁,但请求者{1}未释放", record.AssetName, record.RequestName);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 添加资源请求安全记录
        /// </summary>
        public void AddAssetRequestSafeRecord(System.Object requester, string bundleName, string resName, UnityEngine.Object res)
        {
            if (!SafeRequestRecordDic.ContainsKey(requester))
            {
                SafeRequestRecordDic.Add(requester, new List<SafeRequestRecord>());
            }
            SafeRequestRecord record = new SafeRequestRecord();
            record.RequestName = requester.ToString();
            record.BundleName = bundleName;
            record.AssetName = resName;
            record.Res = res;
            SafeRequestRecordDic[requester].Add(record);
        }

        /// <summary>
        /// 移除记录
        /// </summary>
        /// <param name="requester"></param>
        public void RemoveAssetRequestSafeRecord(System.Object requester)
        {
            if (SafeRequestRecordDic.ContainsKey(requester))
            {
                SafeRequestRecordDic.Remove(requester);
            }
        }

        /// <summary>
        /// 移除记录
        /// </summary>
        /// <param name="clone"></param>
        public void RemoveAssetRequestSafeRecord(GameObject clone)
        {
            List<System.Object> removeRequestList = new List<System.Object>();
            foreach (var item in SafeRequestRecordDic)
            {
                for (int i = 0; i < item.Value.Count; i++)
                {
                    SafeRequestRecord record = item.Value[i];
                    if (record.Res == clone)
                    {
                        item.Value.RemoveAt(i);
                        i--;
                    }
                }
                if (item.Value.Count == 0)
                {
                    removeRequestList.Add(item.Key);
                }
            }
            for (int i = 0; i < removeRequestList.Count; i++)
            {
                if (SafeRequestRecordDic.ContainsKey(removeRequestList[i]))
                {
                    SafeRequestRecordDic.Remove(removeRequestList[i]);
                }
            }
        }

        /// <summary>
        /// 移除记录
        /// </summary>
        public void RemoveAssetRequestSafeRecord(System.Object requester, string bundleName)
        {
            if (SafeRequestRecordDic.ContainsKey(requester))
            {
                List<SafeRequestRecord> recordList = SafeRequestRecordDic[requester];
                for (int i = 0; i < recordList.Count; i++)
                {
                    SafeRequestRecord record = recordList[i];
                    if (record.BundleName == bundleName)
                    {
                        recordList.RemoveAt(i);
                        i--;
                    }
                }
                if (recordList.Count == 0)
                {
                    SafeRequestRecordDic.Remove(requester);
                }
            }
        }
    }
}


#if false
checkedResDic.Clear();
foreach (var item in ResMgr.RequesterRelationShip)
{
    for (int i = 0; i < item.Value.Count; i++)
    {
        AssetBundleInfoNode node = item.Value[i];
        if (checkedResDic.ContainsKey(node))
        {
            continue;
        }
        checkedResDic.Add(node, true);

        foreach (var nodeItem in node.m_cloneableAssetDic)
        {
            for (int j = 0; j < nodeItem.Value.Clones.Count; j++)
            {
                AssetClone assetClone = nodeItem.Value.Clones[j];
                if (assetClone.CloneObj == null)
                {
                    Debug.LogErrorFormat("{0}资源已不在,但是请求者{1}未释放", assetClone.AssetName, assetClone.Requester);
                }
            }
        }
        CheckAssetBundleCloneRe(node);
    }
}
#endif