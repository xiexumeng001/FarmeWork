using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ShipFarmeWork.Configuration.Editor
{
    /// <summary>
    /// 检查预制体
    /// </summary>
    public class CheckRes : CheckLogicBase
    {
        public override string Check(CheckTableData data)
        {
            string resPath = data["ResPath"];
            string resSuffix = data["ResSuffix"];

            Table config = ConfigCheckUtil.LoadConfig(data.ConfigName);

            string log = "";
            foreach (var item in config.AllTableData)
            {
                string path = resPath + "\\" + item[data.AttriName] + resSuffix; //+ ".prefab";
                string guid = AssetDatabase.AssetPathToGUID(path);
                if (string.IsNullOrEmpty(guid))
                {
                    log = log + ";" + item[data.KeyName];
                }
            }
            if (!string.IsNullOrEmpty(log))
            {
                log = string.Format("{0}表的属性{1}为找到对应资源的有:{2}", data.ConfigName, data.AttriName, log);
            }
            return log;
        }
    }

    ///// <summary>
    ///// 检查预制体
    ///// </summary>
    //public class CheckPrefabs : CheckLogicBase
    //{
    //    public override void Check(CheckTableData data)
    //    {
    //        string resPath = data["ResPath"];
    //        string resSuffix = data["ResSuffix"];

    //        Table config = ConfigCheckUtil.LoadConfig(data.ConfigName);

    //        string log = "";
    //        foreach (var item in config.AllTableData)
    //        {
    //            string path = resPath + "\\" + item[data.AttriName] + resSuffix; //+ ".prefab";
    //            string guid = AssetDatabase.AssetPathToGUID(path);
    //            if (string.IsNullOrEmpty(guid))
    //            {
    //                log = log + ";" + item[data.KeyName];
    //            }
    //        }
    //        Debug.LogWarningFormat("配表{0},属性{1},未加载到资源的id:{2}", data.ConfigName, data.AttriName, log);
    //    }
    //}
}
