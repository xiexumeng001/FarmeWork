using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShipFarmeWork.Logic.CityMap
{
    /// <summary>
    /// 可移动者
    /// </summary>
    public interface ICanMove
    {
        /// <summary>
        /// 当前所在建筑
        /// </summary>
        ICanBuild InBuilder { get; }
        /// <summary>
        /// 当前所在格子
        /// </summary>
        RunGrid OccuGrid { get; }

        /// <summary>
        /// 所在位置
        /// </summary>
        Vector3 LocalPos { get; }

        /// <summary>
        /// 移动至某个建筑
        /// </summary>
        bool MoveToBuild(ICanBuild builder,System.Action onArrive);

        /// <summary>
        /// 移动至某个格子
        /// </summary>
        /// <param name="gloabGrid"></param>
        bool MoveToGrid(RunGrid gloabGrid, System.Action onArrive);

        /// <summary>
        /// 更新人物位置
        /// </summary>
        /// <param name="localPos"></param>
        void UpdatePos(Vector3 localPos);

    }
}
