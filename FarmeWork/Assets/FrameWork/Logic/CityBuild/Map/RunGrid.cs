using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ShipFarmeWork.Logic.CityMap
{
    /// <summary>
    /// 行走格子
    /// </summary>
    public class RunGrid : BaseGrid
    {
        /// <summary>
        /// 建筑格子
        /// </summary>
        private BuildGrid _BuildGrid;
        public BuildGrid BuildGrid { get { return _BuildGrid; } }

        private bool _IsWalk;
        /// <summary>
        /// 是否可行走
        /// </summary>
        public bool IsWalk { get { return _IsWalk; } }

        private bool _IsDoor;
        /// <summary>
        /// 是否是们上的格子
        /// </summary>
        public bool IsDoor { get { return _IsDoor; } }


        public ICanBuild Builder { get { return _BuildGrid.Builder; } }


        /// <summary>
        /// 所在的地图
        /// </summary>
        private CityMap _Map;
        public CityMap Map { get { return _Map; } }

        public void Init(Vector2Int lineRow, Vector3 pos, BuildGrid buildGrid, CityMap map)
        {
            Line = lineRow.x;
            Row = lineRow.y;
            _BuildGrid = buildGrid;
            _Map = map;
            _IsWalk = true;

            SetMapWorldPos(pos);
        }

        //hack【暂改】【tileMap】这儿可以记录一个格子点到建筑格子位置的偏移量,到时求得建筑格子位置,加上偏移量乘以缩放
        public void SetMapWorldPos(Vector3 pos)
        {
            WorldPosition = new Vector3(pos.x, pos.y, _Map.ZSort);
        }

        /// <summary>
        /// 更新格子行走信息
        /// </summary>
        public void OnAddBuild()
        {
            //是否是舱室内可行走的格子
            bool isBulildRunGrid = IsBuilderCanRunGrid();

            //是否是门口的格子
            _IsDoor = IsDoorGrid();

            //是否可行走
            _IsWalk = isBulildRunGrid || _IsDoor;

            //添加到可行走格子范围
            if (_IsWalk)
            {
                if (Builder.CanRunGridList == null) Builder.CanRunGridList = new List<RunGrid>();
                Builder.CanRunGridList.Add(this);
            }

            //更新格子颜色
            GridColorEnum colorEnum = GridColorEnum.BeOccu;
            if (_IsWalk) {
                colorEnum = _IsDoor ? GridColorEnum.Door : GridColorEnum.CanRun;
            }
            _Map.UpdateDrawGridColor(this, colorEnum, true);
        }

        /// <summary>
        /// 移除格子信息
        /// </summary>
        public void OnRemoveBuild()
        {
            try
            {
                GridColorEnum colorEnum = GridColorEnum.BeOccu;
                if (_IsWalk)
                {
                    colorEnum = _IsDoor ? GridColorEnum.Door : GridColorEnum.CanRun;
                }
                _Map.UpdateDrawGridColor(this, colorEnum, false);
            }
            catch (Exception e) { Debug.LogError(e); }

            _IsWalk = true;
            _IsDoor = false;
        }

        /// <summary>
        /// 是否在格子内部
        /// </summary>
        /// <returns></returns>
        public bool IsIn(Vector2 pos)
        {
            Vector2 dir = pos - (Vector2)WorldPosition;
            float dis = dir.magnitude;
            if ((dir.x > 0) == (dir.y > 0))
            {
                //一三象限 判断 高
                return dis <= Map.RunGridHigh;
            }
            else
            {
                //二四象限 判断 宽
                return dis <= Map.RunGridWide;
            }
        }


        private bool IsBuilderCanRunGrid()
        {
            if (Builder == null) return false;
            List<Vector2Int> canRunGrids = Builder.CanRunGridPos;
            RunGrid runBaseGrid = Builder.ClickGrid.BaseRunGrid;
            for (int i = 0; i < canRunGrids.Count; i++)
            {
                int line = runBaseGrid.Line + canRunGrids[i].x;
                int row = runBaseGrid.Row + canRunGrids[i].y;
                if (Line == line && Row == row)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 是否是门上的格子
        /// </summary>
        /// <returns></returns>
        private bool IsDoorGrid()
        {
            if (Builder == null) return false;
            if (Builder.DoorGridPos == null) return false;
            List<Vector2Int> doorGrids = Builder.DoorGridPos;
            RunGrid runBaseGrid = Builder.ClickGrid.BaseRunGrid;
            for (int i = 0; i < doorGrids.Count; i++)
            {
                int line = runBaseGrid.Line + doorGrids[i].x;
                int row = runBaseGrid.Row + doorGrids[i].y;
                if (Line == line && Row == row)
                {
                    return true;
                }
            }
            return false;
        }

        public override string ToString()
        {
            return string.Format(" ({0},{1}) ", Line, Row);
        }
    }
}