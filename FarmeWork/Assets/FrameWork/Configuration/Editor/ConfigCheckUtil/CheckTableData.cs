using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShipFarmeWork.Configuration.Editor
{
    /// <summary>
    /// 检查数据表的数据
    /// </summary>
    public class CheckTableData
    {
        public string CheckLogic;
        public string ConfigName;
        public string KeyName;
        public string AttriName;
        public string IgronNum;
        public Dictionary<string, string> Data = new Dictionary<string, string>();

        public string this[string key]
        {
            get
            {
                return Data[key];
            }
        }
    }
}