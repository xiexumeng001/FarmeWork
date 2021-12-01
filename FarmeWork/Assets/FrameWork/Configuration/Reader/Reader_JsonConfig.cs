using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

namespace ShipFarmeWork.Configuration
{
    public class Reader_JsonConfig : ReadConfigerBase
    {

        public override ConfigDataList ReadFile<T>(string path, string configName)
        {
            configName = configName + ".json";
            string configStr = ConfigLoad.Load(path, configName);
            T[] datas = JsonMapper.ToObject<T[]>(configStr);
            Dictionary<int, IConfig> allData = new Dictionary<int, IConfig>();
            for (int i = 0; i < datas.Length; i++)
            {
                T data = datas[i];
                allData.Add(data.getID(), data);
            }
            ConfigDataList config = new ConfigDataList(configName, allData);
            return config;
        }

    }
}


//string json ="[{\"Id\":1001,\"Note\":\"xiexumeng\"},{\"Id\":1002,\"Note\":\"nihao\"}]";
