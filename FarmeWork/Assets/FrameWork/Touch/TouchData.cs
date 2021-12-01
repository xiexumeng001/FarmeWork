using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShipFarmeWork.Touch
{

    public class TouchData
    {
        /// <summary>
        /// 屏幕坐标
        /// </summary>
        public Vector3 PosScreen;
    }

    /// <summary>
    /// 当按下的数据
    /// </summary>
    public class OnTouchDownData : TouchData
    {
     public   GameObject TouchDownGameObject;
    }


    /// <summary>
    /// 当抬起的数据
    /// </summary>
    public class OnTouchUpData : TouchData
    {
        public GameObject TouchOnTouchUpData;
    }


    /// <summary>
    /// 当拖拽的数据
    /// </summary>
    public class OnDragData : TouchData
    {

        /// <summary>
        /// 屏幕上的拖拽移动方向(代表了拖拽的方向与距上一帧拖拽的距离)
        /// </summary>
        public Vector3 DragVector;
    }

    /// <summary>
    /// 当拖拽结束的数据
    /// </summary>
    public class OnDragEndData : TouchData {

    }


    /// <summary>
    /// 当双指缩放的数据
    /// </summary>
    public class OnDoubleTouchData : TouchData
    {
        /// <summary>
        /// 缩放值
        /// 负的为缩,正的拉伸
        /// </summary>
        public float ScaleNum;
    }

}
