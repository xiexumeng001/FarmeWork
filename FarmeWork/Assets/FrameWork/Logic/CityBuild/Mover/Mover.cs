using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

namespace ShipFarmeWork.Logic.CityMap
{

    /// <summary>
    /// 移动数据
    /// </summary>
    public interface IMoveData
    {
        //当前所在舱室
        ICanBuild InBuilder { get; }
        //所在格子
        RunGrid OccuGrid { get; set; }
        //所在位置
        Vector3 LocalPos { get; }
        //移动速度
        float MoveSpeed { get; }
        //帧率
        float GetSecondFarmeRate();
    }


    /// <summary>
    /// 移动者
    /// </summary>
    public class Mover
    {
        //当开始移动
        public Action OnStartMoveDel;
        //当结束移动
        public Action OnEndMoveDel;
        //当开始移动向下一个格子
        public Action OnStartMoveToNextGridDel;
        //当进入下一个格子
        public Action OnEnterNextGridDel;
        //当请求进入建筑
        public Action<ICanBuild, RunGrid> RequestEnterBuildDel;
        //当进入房间的回调
        public Action<ICanBuild> OnEnterRoomDel;
        //当离开建筑的回调
        public Action OnOutRoomDel;

        //当人物位置更新
        public Action<Vector3> OnPosUpdateDel;

        public IMoveData Data;

        private ICanBuild _GloabBuild;
        public ICanBuild GloabBuild { get { return _GloabBuild; } }

        //hack【暂改】看起来这个可以删除了,把数据拉出来
        public RoadMoveInfo CurrenMoveInfo;     //当前移动信息


        public Mover(IMoveData data)
        {
            Data = data;
        }


        /// <summary>
        /// 移动中
        /// </summary>
        public void Moving()
        {
            //移动位置
            Vector3 moveVec = CurrenMoveInfo.DirecUnit * Data.MoveSpeed * Data.GetSecondFarmeRate();
            UpdatePos(Data.LocalPos + moveVec);

            bool isArrive = false;
            //是否到达格子
            switch (CurrenMoveInfo.MoveDire)
            {
                case DirectionEnum.LeftUp:
                case DirectionEnum.LeftDown:
                    isArrive = (Data.LocalPos.x <= CurrenMoveInfo.GloabPos.x);
                    break;
                case DirectionEnum.RightUp:
                case DirectionEnum.RightDown:
                    isArrive = (Data.LocalPos.x >= CurrenMoveInfo.GloabPos.x);
                    break;
            }

            //若当前占用的不是下一个格子
            if (Data.OccuGrid != CurrenMoveInfo.NextGrid)
            {//判断是否在下一个格子内部
                bool isIn = CurrenMoveInfo.NextGrid.IsIn(Data.LocalPos);
                if (isArrive && !isIn)
                {//若到达并且不在格子内部,则更新人物位置
                    UpdatePos(CurrenMoveInfo.GloabPos);
                }
                if (isIn || isArrive) {
                    OnEnterNextGrid();
                }
            }
            if (isArrive) { OnArriveGloaPos(); }
        }


        /// <summary>
        /// 更新人物位置
        /// </summary>
        /// <param name="localPos"></param>
        public void UpdatePos(Vector3 localPos)
        {
            if (OnPosUpdateDel != null) OnPosUpdateDel(localPos);
        }

        /// <summary>
        /// 生物移动至某个格子
        /// </summary>
        /// <param name="gloabGrid"></param>
        public bool MoveToGrid(RunGrid gloabGrid, System.Action onArrive, bool isMoveToStart = true)
        {
            if (gloabGrid == null) { Debug.LogWarning("目标格子是空的"); return false; }
            //获取路径信息
            List<RunGrid> loadGrid = Data.OccuGrid.Map.GetRoad(Data.OccuGrid, gloabGrid);

            if (loadGrid != null)
            {
                MoveByRoad(loadGrid, onArrive, isMoveToStart);
                return true;
            }
            else
            {
                Debug.LogWarning(" no found GridRoad --- ");
            }
            return false;
        }

        public void MoveByRoad(List<RunGrid> loadGrid, System.Action onArrive, bool isMoveToStart = true)
        {
            //若之前的数据没清除,就先清除下
            if (CurrenMoveInfo != null)
            {
                ClearMoveInfo();
            }

            RunGrid gloabGrid = loadGrid[0];
            //更改目标建筑
            _GloabBuild = gloabGrid.Builder;

            CurrenMoveInfo = new RoadMoveInfo(loadGrid, gloabGrid, onArrive, isMoveToStart);
            MoveToNextPos();
            if (OnStartMoveDel != null) OnStartMoveDel();
        }


        /// <summary>
        /// 当进入下一个格子,触发进入舱室,离开舱室等各种逻辑
        /// </summary>
        private void OnEnterNextGrid()
        {
            //更新格子
            //float Offset = Mathf.Sqrt(Mathf.Pow(Data.OccuGrid.Line - CurrenMoveInfo.NextGrid.Line, 2) + Mathf.Pow(Data.OccuGrid.Row - CurrenMoveInfo.NextGrid.Row, 2));
            //if (Offset>1)
            //{
            //    Debug.LogError(string.Format("现在的格子({0},{1})，下一个格子({2},{3})", Data.OccuGrid.Line, Data.OccuGrid.Row, CurrenMoveInfo.NextGrid.Line, CurrenMoveInfo.NextGrid.Row));
            //}
            Data.OccuGrid = CurrenMoveInfo.NextGrid;

            if (OnEnterNextGridDel != null) OnEnterNextGridDel();

            //是否进出舱室
            ICanBuild build = Data.InBuilder;
            RunGrid occuGrid = Data.OccuGrid;
            if (build == null)
            {
                if (occuGrid.Builder != null) { if (OnEnterRoomDel != null) OnEnterRoomDel(occuGrid.Builder); }
            }
            else
            {
                if (occuGrid.Builder == null) { if (OnOutRoomDel != null) OnOutRoomDel(); }
                else
                {
                    if (occuGrid.Builder != build)
                    {
                        if (OnOutRoomDel != null) OnOutRoomDel();
                        if (OnEnterRoomDel != null) OnEnterRoomDel(occuGrid.Builder);
                    }
                }
            }
        }



        /// <summary>
        /// 移动至下个位置
        /// </summary>
        public void MoveToNextPos()
        {

            CurrenMoveInfo.UpdateToNextMoveInfo(Data.LocalPos);
            if (OnStartMoveToNextGridDel != null) OnStartMoveToNextGridDel();

            //到达下一个格子是否要进门,放在这儿可以在人物开始移动时也判断上
            RunGrid nextGrid = CurrenMoveInfo.NextGrid;
            if (nextGrid.IsDoor)
            {
                if (Data.OccuGrid.Builder != nextGrid.Builder)
                {//进门
                    if (RequestEnterBuildDel != null) RequestEnterBuildDel(nextGrid.Builder, nextGrid);
                }
                else
                {
                    if (!Data.OccuGrid.IsDoor)
                    {//出门
                        if (RequestEnterBuildDel != null) RequestEnterBuildDel(nextGrid.Builder, nextGrid);
                    }
                }
            }
        }


        /// <summary>
        /// 当到达目标位置,纯运动信息的切换,使运动继续下去,并触发当到达的逻辑
        /// </summary>
        private void OnArriveGloaPos()
        {

            if (CurrenMoveInfo.IsArriveEnd())
            {
                RoadMoveInfo moveInfo = CurrenMoveInfo;
                if (OnEndMoveDel != null) OnEndMoveDel();
                if (CurrenMoveInfo.OnArrive != null) CurrenMoveInfo.OnArrive(); //当到达

                if (moveInfo == CurrenMoveInfo)
                {//如果移动信息没变,那就清楚了,变了那就是当结束的回调里有了新的移动信息
                    ClearMoveInfo();
                }
            }
            else
            {//继续下个位置
                MoveToNextPos();
            }
        }

        /// <summary>
        /// 清除移动信息
        /// </summary>
        public void ClearMoveInfo()
        {
            try
            {
                if (CurrenMoveInfo != null)
                {

                    CurrenMoveInfo.OnArrive = null;

                    //复原格子颜色
                    List<RunGrid> _LoadGrid = CurrenMoveInfo.LoadGrid;
                    RunGrid startGrid = _LoadGrid[_LoadGrid.Count - 1];
                    RunGrid endGrid = _LoadGrid[0];
                    startGrid.Map.UpdateDrawGridColor(startGrid, GridColorEnum.SearchStart, false);
                    endGrid.Map.UpdateDrawGridColor(endGrid, GridColorEnum.SearchEnd, false);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            _GloabBuild = null;
            CurrenMoveInfo = null;
        }

    }
}


#region 废弃逻辑
////当前所在舱室
//private ICanBuild _InBuilder;
//public ICanBuild InBuilder { get { return _InBuilder; } }
////所在格子
//private CityGrid _OccuGrid;
//public CityGrid OccuGrid { get { return _OccuGrid; } }
////所在位置
//private Vector3 _LocalPos;
//public Vector3 LocalPos
//{
//    get { return new Vector3(_LocalPos.x, _LocalPos.y, _LocalPos.z); }
//}

/// <summary>
/// 设置占用格子
/// </summary>
//public void SetOccuGrid(CityGrid grid)
//{
//    _OccuGrid = grid;
//    UpdatePos(_OccuGrid.Pos);
//}

//public void SetInBuild(ICanBuild build)
//{
//    _InBuilder = build;
//}
#endregion