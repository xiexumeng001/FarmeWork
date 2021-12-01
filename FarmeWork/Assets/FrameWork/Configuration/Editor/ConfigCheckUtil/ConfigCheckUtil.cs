using LitJson;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ShipFarmeWork.Configuration.Editor
{
    /// <summary>
    /// 配表检测工具
    /// </summary>
    public static class ConfigCheckUtil
    {
        public static List<CheckTableData> CheckData;

        /// <summary>
        /// 检测预制体
        /// </summary>
        [MenuItem("Tools/CheckConfig")]
        public static void CheckRes()
        {
            ConfigCheckUtil.Check();
        }

        public static void Check()
        {
            LoadCheckData();

            string errorLog = "";
            foreach (var item in CheckData)
            {
                string fullName = "CheckConfigEditor.Logic." + item.CheckLogic;//命名空间.类型名
                CheckLogicBase logic = Assembly.GetExecutingAssembly().CreateInstance(fullName) as CheckLogicBase;//加载程序集，创建程序集里面的 命名空间.类型名 实例
                if (logic == null)
                {
                    Debug.LogErrorFormat("未找到 {0} 的检查逻辑", item.CheckLogic);
                    return;
                }
                string log = logic.Check(item);
                if (log != null) errorLog = errorLog + "\n\n" + log;
            }
            Debug.LogError("检查结果:\n" + errorLog);
        }

        public static Table LoadConfig(string configName)
        {
            string path = CheckConfigDefine.ConfigPath + configName + CheckConfigDefine.ConfigFix;
            return CheckConfigDefine.LoadTableObj.Load(path);
        }

        /// <summary>
        /// 加载检查数据
        /// </summary>
        public static void LoadCheckData()
        {
            CheckData = new List<CheckTableData>();
            string jsonStr = System.IO.File.ReadAllText(CheckConfigDefine.CheckConfigPath);
            JsonData tableJson = JsonMapper.ToObject(jsonStr);
            foreach (JsonData configData in tableJson)
            {
                CheckTableData checkData = new CheckTableData();
                Dictionary<string, string> dic = new Dictionary<string, string>();
                foreach (KeyValuePair<string, JsonData> data in configData)
                {
                    checkData.Data.Add(data.Key, data.Value.ToString());
                }
                checkData.CheckLogic = checkData.Data["CheckLogic"];
                checkData.ConfigName = checkData.Data["ConfigName"];
                checkData.KeyName = checkData.Data["KeyName"];
                checkData.AttriName = checkData.Data["AttriName"];
                checkData.IgronNum = checkData.Data["IgronNum"];

                CheckData.Add(checkData);
            }
        }
    }
}