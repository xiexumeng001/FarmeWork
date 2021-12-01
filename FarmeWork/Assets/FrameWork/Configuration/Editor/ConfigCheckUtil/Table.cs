using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShipFarmeWork.Configuration.Editor
{
    /// <summary>
    /// 配表数据
    /// </summary>
    public class Table
    {
        public string Name;
        public List<Dictionary<string, string>> AllTableData = new List<Dictionary<string, string>>();

        public Table(string name)
        {
            Name = name;
        }

        public Dictionary<string, string> GetConfigDataByAttribute(string attriName, string attriNum)
        {
            foreach (var item in AllTableData)
            {
                if (item[attriName] == attriNum)
                {
                    return item;
                }
            }
            return null;
        }


        public bool IsHasConfigDataByAttribute(string attriName, string attriNum)
        {
            return GetConfigDataByAttribute(attriName, attriNum) != null;
        }
    }
}
