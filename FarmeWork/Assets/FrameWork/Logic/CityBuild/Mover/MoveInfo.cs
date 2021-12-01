using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

namespace ShipFarmeWork.Logic.CityMap
{
    //方向
    public enum DirectionEnum
    {
        LeftUp,    //左上
        RightUp,   //右上
        LeftDown,  //左下
        RightDown, //右下
    }

    /// <summary>
    /// 移动信息
    /// </summary>
    public class MoveInfo
    {
        public Vector3 DirecUnit;     //方向单位向量
        public Vector3 GloabPos;      //目标位置
        public float MoveSpeed;       //移动速度

        public Action OnArrive;  //当到达目标位置

        //生物移动方向
        private DirectionEnum _MoveDire = DirectionEnum.RightDown;
        public DirectionEnum MoveDire { get { return _MoveDire; } }

        public void UpdateToNextMoveInfo(Vector3 currenPos)
        {

            //更新下个位置
            ToNextPos();

            //确定朝向与移动方向
            DirecUnit = Vector3.Normalize(GloabPos - currenPos);//CommonUnit.GetDicUnitVec2(currenPos, GloabPos);
            if (DirecUnit.x > 0)
            {
                _MoveDire = ((DirecUnit.y > 0) ? DirectionEnum.RightUp : DirectionEnum.RightDown);
            }
            else
            {
                _MoveDire = ((DirecUnit.y > 0) ? DirectionEnum.LeftUp : DirectionEnum.LeftDown);
            }
        }

        protected virtual void ToNextPos() { }

        /// <summary>
        /// 是否到达终点
        /// </summary>
        /// <returns></returns>
        public virtual bool IsArriveEnd() { return false; }
    }

    /// <summary>
    /// 道路的移动信息
    /// </summary>
    public class RoadMoveInfo : MoveInfo
    {

        public RunGrid _NextGrid;
        public RunGrid NextGrid { get { return _NextGrid; } }    //接下来的格子

        private int toIndex;
        public int ToIndex { get { return toIndex; } }

        RunGrid _EndGrid;     //目标格子

        List<RunGrid> _LoadGrid;
        public List<RunGrid> LoadGrid { get { return _LoadGrid; } }

        public RoadMoveInfo(List<RunGrid> loadGrid, RunGrid end, Action onArrive, bool isMoveToStart)
        {
            _LoadGrid = loadGrid;
            _EndGrid = end;
            toIndex = (!isMoveToStart && loadGrid.Count > 1) ? (loadGrid.Count - 1) : loadGrid.Count;
            OnArrive = onArrive;
        }

        /// <summary>
        /// 更新到下个位置
        /// </summary>
        protected override void ToNextPos()
        {
            toIndex--;
            _NextGrid = _LoadGrid[toIndex];
            GloabPos = _NextGrid.WorldPosition;
        }

        public override bool IsArriveEnd()
        {
            return (_EndGrid == _NextGrid || (toIndex == 0));
        }

        /// <summary>
        /// 获取下一个格子的下一个格子
        /// </summary>
        /// <returns></returns>
        public RunGrid GetNextGrid2()
        {
            int index = toIndex - 1;
            if (index >= 0 && index < _LoadGrid.Count)
            {
                return _LoadGrid[toIndex];
            }
            return null;
        }
    }
}