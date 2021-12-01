using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShipFarmeWork.Configuration.Editor
{
    public class LoadTable_LitJson : LoadTable
    {
        public override Table Load(string configPath)
        {
            string jsonStr = System.IO.File.ReadAllText(configPath);
            JsonData tableJson = JsonMapper.ToObject(jsonStr);
            Table table = new Table(configPath);
            foreach (JsonData configData in tableJson)
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                foreach (KeyValuePair<string, JsonData> data in configData)
                {
                    dic.Add(data.Key, data.Value.ToString());
                }
                table.AllTableData.Add(dic);
            }
            return table;
        }
    }
}