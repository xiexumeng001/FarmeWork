using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShipFarmeWork.Configuration
{
    /// <summary>
    /// 读取配置文件器
    /// </summary>
    public abstract class ReadConfigerBase
    {
        public abstract ConfigDataList ReadFile<T>(string path, string configName) where T : class, IConfig, new();
    }
}