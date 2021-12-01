using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShipFarmeWork.Logic.CityMap
{
    /// <summary>
    /// 建造接口,继承此接口,则可以被建造
    /// </summary>
    public interface ICanBuild
    {
        /// <summary>
        /// 建造物的唯一ＩＤ
        /// </summary>
        int GUID { get; set; }
        /// <summary>
        /// 建造类型
        /// </summary>
        BuildType buildType { get; set; }
        /// <summary>
        /// 建造物的宽(格子数)
        /// </summary>
        int Width { get; set; }
        /// <summary>
        /// 建造物的高(格子数)
        /// </summary>
        int Height { get; set; }

        /// <summary>
        /// 占用的格子
        /// </summary>
        List<BuildGrid> OccuGrid { get; set; }

        /// <summary>
        /// 此建筑中可行走的格子点
        /// </summary>
        List<Vector2Int> CanRunGridPos { get; set; }
        /// <summary>
        /// 门占的格子
        /// </summary>
        List<Vector2Int> DoorGridPos { get; set; }
        /// <summary>
        /// 可行走的格子集合
        /// </summary>
        //List<CityGrid> CanRunGridList { get; set; }
        List<RunGrid> CanRunGridList { get; set; }

        /// <summary>
        /// 被点击的格子
        /// </summary>
        BuildGrid ClickGrid { get; set; }

        /// <summary>
        /// 格子与人物的对应关系
        /// </summary>
        //hack【暂改】这儿主要是为了让人物不重叠的,但是先放到后面写
        //Dictionary<CityGrid, ICanMove> GridMoverRelation { get; set; }

        void OnAddGridInfo();
        /// <summary>
        /// 当移除格子信息
        /// </summary>
        void OnRemoveGridInfo();

        void OnBePlaceToMap(IMap map);

        /// <summary>
        /// 当移除成功
        /// </summary>
        void OnRemoveSuccess();

        /// <summary>
        /// 当移动成功
        /// </summary>
        void OnMoveSuccess();
    }

    /// <summary>
    /// 建筑类型
    /// </summary>
    public enum BuildType
    {

        /// <summary>
        /// 房间类型
        /// </summary>
        Room = 1,
        /// <summary>
        /// 道路类型
        /// </summary>
        Road = 2,
    }

    /// <summary>
    /// 房间位置类型
    /// </summary>
    public enum BuildPosType {

        None,       //无类型

        DoorOut,    //门外点
        DoorIn,     //门内点

        DoorOutGrid,  //门外格子点
        
    }

}