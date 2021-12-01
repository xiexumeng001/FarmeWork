using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ShipFarmeWork.Resource;

namespace ShipFarmeWork.Configuration
{
    public static class ConfigLoad
    {
        /// <summary>
        /// 加载配置表接口
        /// </summary>
        /// <param name="path">配置表所在的路径</param>
        /// <param name="configName">配置表的名称</param>
        /// <returns>配置表的字符串</returns>
        public static string Load(string path, string configName)
        {
            TextAsset text = (TextAsset)ResHelp.GetAsset(path, configName, typeof(TextAsset), ResHelp.CommonGame);
            return text.text;
        }

    }
}