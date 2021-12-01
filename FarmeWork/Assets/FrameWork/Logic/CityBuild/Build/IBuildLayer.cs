using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShipFarmeWork.Logic.CityMap
{
    /// <summary>
    /// 建造层级接口
    /// </summary>
    public interface IBuildLayer
    {
        /// <summary>
        /// 宽
        /// </summary>
        int Width { get; set; }

        /// <summary>
        /// 高
        /// </summary>
        int Height { get; set; }

        /// <summary>
        /// 临时层级
        /// </summary>
        int TempLayer { get; set; }

        /// <summary>
        /// 基础层级
        /// </summary>
        int BaseLayer { get; set; }

        /// <summary>
        /// 连接的建筑
        /// </summary>
        List<IBuildLayer> LineBuild { get; set; }

        /// <summary>
        /// 获取行
        /// </summary>
        int GetBaseLine();

        /// <summary>
        /// 获取列
        /// </summary>
        int GetBaseRow();
    }
}
