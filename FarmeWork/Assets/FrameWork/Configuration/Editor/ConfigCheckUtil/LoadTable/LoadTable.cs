using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShipFarmeWork.Configuration.Editor
{
    /// <summary>
    /// 加载配表基类
    /// </summary>
    public abstract class LoadTable
    {
        public abstract Table Load(string configName);
    }

}