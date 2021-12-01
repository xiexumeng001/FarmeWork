using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ShipFarmeWork.Logic.CityMap
{
    /// <summary>
    /// 建筑相关的格子
    /// </summary>
    public class BuildGrid : BaseGrid
    {
        /// <summary>
        /// 是否可建造
        /// </summary>
        public bool IsCanBuild { get { return _Builder == null; } }

        /// <summary>
        /// 放置的建筑物
        /// </summary>
        private ICanBuild _Builder;
        public ICanBuild Builder { get { return _Builder; } }

        /// <summary>
        /// 基准格子
        /// </summary>
        public RunGrid BaseRunGrid;

        /// <summary>
        /// 所在的地图
        /// </summary>
        private CityMap _Map;
        public CityMap Map { get { return _Map; } }

        /// <summary>
        /// 格子的基准层级
        /// </summary>
        public int Layer = 0;

        public List<RunGrid> RunGridList = new List<RunGrid>();


        protected virtual void OnSetBuild(ICanBuild builer) { }
        //当取消建筑
        protected virtual void OnCancelBuild_Child() { }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="line"></param>
        /// <param name="row"></param>
        /// <param name="pos"></param>
        /// <param name="map"></param>
        public void Init(int line, int row, Vector3 pos, CityMap map)
        {
            Line = line;
            Row = row;
            _Map = map;

            SetMapWorldPos(pos);
        }

        public void SetMapWorldPos(Vector3 pos)
        {
            WorldPosition = new Vector3(pos.x, pos.y, _Map.ZSort);
        }
        
        /// <summary>
        /// 得到建筑物
        /// </summary>
        public void SetBuild(ICanBuild builer)
        {
            _Builder = builer;
            for (int i = 0; i < RunGridList.Count; i++)
            {
                RunGridList[i].OnAddBuild();
            }
            OnSetBuild(builer);

            _Map.UpdateDrawGridColor(this, GridColorEnum.BeOccu, true);
        }

        /// <summary>
        /// 当取消建筑
        /// </summary>
        public void OnCanCelBuild()
        {
            OnCancelBuild_Child();

            _Builder = null;
            for (int i = 0; i < RunGridList.Count; i++)
            {
                RunGridList[i].OnRemoveBuild();
            }
            _Map.UpdateDrawGridColor(this, GridColorEnum.BeOccu, false);
        }
    }
}