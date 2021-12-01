using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShipFarmeWork.Logic.CityMap
{
    /// <summary>
    /// 地图的接口类
    /// </summary>
    public interface IMap
    {
        /// <summary>
        /// 游戏中的唯一标识
        /// </summary>
        int GUID { get; set; }

        /// <summary>
        /// 当被建造
        /// </summary>
        void OnBeBuild(ICanBuild builder);
    }

}
