using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShipFarmeWork.Logic.CityMap
{
    /// <summary>
    /// 基础格子
    /// </summary>
    public class BaseGrid
    {

        /// <summary>
        /// 格子所在行
        /// </summary>
        public int Line;
        /// <summary>
        /// 格子所在列
        /// </summary>
        public int Row;

        /// <summary>
        /// 格子在世界空间中的位置
        /// </summary>
        private Vector3 worldPosition;

        /// <summary>
        /// 格子在世界空间中的位置
        /// </summary>
        public Vector3 WorldPosition
        {
            get
            {
                return worldPosition;
            }

            set
            {
                worldPosition = value;
            }
        }
    }
}