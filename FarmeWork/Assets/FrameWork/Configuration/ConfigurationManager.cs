using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

namespace ShipFarmeWork.Configuration
{

    /// <summary>
    /// 数据消息的对外接口
    /// 保存着所有从配置文档中读取来的消息
    /// </summary>
    public class ConfigurationManager
    {
        /// <summary>
        /// 使用的加载器
        /// 默认为json
        /// </summary>
        public static ReadConfigerBase useReader = new Reader_JsonConfig();

        /// <summary>
        /// 所有的配置文件数据
        /// </summary>
        private static Dictionary<string, ConfigDataList> allConfigs = new Dictionary<string, ConfigDataList>();

        /// <summary>
        /// 读取配置文件
        /// </summary>
        /// <param name="comfigName"></param>
        public static ConfigDataList ReadConfigFile<T>(string path, string configName) where T : class, IConfig, new()
        {
            if (allConfigs.ContainsKey(configName))
            {
                Debug.LogError("已经加载过名叫 " + configName + " 的配置文件");
            }
            else
            {
                ConfigDataList config = useReader.ReadFile<T>(path, configName);
                allConfigs.Add(configName, config);
            }
            return allConfigs[configName];
        }

#if UNITY_EDITOR
        public static ConfigDataList ReloadConfigFile<T>(string path, string configName) where T : class, IConfig, new()
        {
            if (allConfigs.ContainsKey(configName))
            {
                allConfigs.Remove(configName);
            }
            return ReadConfigFile<T>(path, configName);
        }
#endif


        public static ConfigDataList GetConfigDataList(string configName) {
            if (allConfigs.ContainsKey(configName))
            {
                return allConfigs[configName];
            }
            return null;
        }

    }
}