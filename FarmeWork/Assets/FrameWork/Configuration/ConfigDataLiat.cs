using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ShipFarmeWork.Configuration
{
    /// <summary>
    /// 配置文件的数据集合类
    /// </summary>
    public class ConfigDataList
    {
        public string name;
        /// <summary>
        /// 数据字典
        /// </summary>
        private Dictionary<int, IConfig> allData;

        public ConfigDataList(string name, Dictionary<int, IConfig> allData)
        {
            this.name = name;
            this.allData = allData;
        }

        public IConfig this[int id]
        {
            get
            {
                if (!allData.ContainsKey(id))
                {
                    Debug.LogWarning(name + " 配置文件中不存在数" + id);
                    return null;
                }
                return allData[id];
            }
        }

        public bool IsContain(int id)
        {
            return allData.ContainsKey(id);
        }


        /// <summary>
        /// 获取全部数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public Dictionary<int, IConfig> GetValues()
        {
            return allData;
        }

#if UNITY_EDITOR

        public void AddData(int id, IConfig value)
        {
            allData.Add(id, value);
        }
#endif
    }
}