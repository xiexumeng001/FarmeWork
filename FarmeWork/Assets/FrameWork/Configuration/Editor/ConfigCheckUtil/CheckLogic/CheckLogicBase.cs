using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShipFarmeWork.Configuration.Editor
{
    /// <summary>
    /// 检查逻辑基类
    /// </summary>
    public abstract class CheckLogicBase
    {
        public abstract string Check(CheckTableData data);
    }
}
