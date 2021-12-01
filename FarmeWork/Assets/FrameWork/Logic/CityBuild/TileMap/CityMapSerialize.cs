using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ShipFarmeWork.Logic.CityMap
{
    /// <summary>
    /// 每艘船的数据
    /// </summary>
    [Serializable]
    public class CityMapSerialize
    {
        [SerializeField]
        public string CityName;         //地图名称
        [SerializeField]
        public Vector2DoubleObj CellSize;        // 格子的大小
        [SerializeField]
        public List<Vector2IntObj> GridDataList = new List<Vector2IntObj>();    //格子信息

        /// <summary>
        /// 加载数据
        /// </summary>
        /// <param name="gridCom"></param>
        /// <param name="tileMap"></param>
        /// <param name="mapName"></param>
        public void LoadData(Grid gridCom, Tilemap tileMap, string mapName)
        {
            //名称
            CityName = mapName;
            //格子大小
            CellSize = new Vector2DoubleObj();
            CellSize.x = gridCom.cellSize.x;
            CellSize.y = gridCom.cellSize.y;
            //地图格子信息
            BoundsInt cellBounds = tileMap.cellBounds;
            for (int line = cellBounds.xMin; line < cellBounds.xMax; line++)
            {
                for (int row = cellBounds.yMin; row < cellBounds.yMax; row++)
                {
                    TileBase tile = tileMap.GetTile(new Vector3Int(line, row, 0));
                    if (tile != null)
                    {
                        Vector2IntObj lineRow = new Vector2IntObj();
                        lineRow.x = line;
                        lineRow.y = row;
                        GridDataList.Add(lineRow);

                        //GridTileInfo _data = new GridTileInfo();
                        //_data.LineRow = new Vector2Int(line, row);
                        //GridDataList.Add(_data);
                    }
                }
            }
            if (GridDataList.Count == 0)
            {
                throw new Exception(mapName + "  地图数据不对 ");
            }
        }
    }


    [Serializable]
    public class Vector2IntObj
    {
        public int x;
        public int y;
    }


    [Serializable]
    public class Vector2DoubleObj
    {
        public double x;
        public double y;
    }
}


#region 废弃逻辑
#if false
    /// <summary>
    /// 克隆
    /// </summary>
    /// <returns></returns>
    //public CityMapSerialize Clone()
    //{
    //    CityMapSerialize clone = new CityMapSerialize();
    //    clone.cityName = this.cityName;
    //    clone.CellSize = this.CellSize;
    //    clone.GridHigh = this.GridHigh;
    //    clone.GridWide = this.GridWide;
    //    clone.LineEquationK = this.LineEquationK;
    //    clone.FirstLineEquationB = this.FirstLineEquationB;
    //    clone.RowEquationK = this.RowEquationK;
    //    clone.FirstRowEquationB = this.FirstRowEquationB;
    //    clone.RotaAngle = this.RotaAngle;

    //    clone.MapData = new List<GridTileInfo>();

    //    for (int i = 0; i < MapData.Count; i++)
    //    {
    //        GridTileInfo info = new GridTileInfo();

    //        //info.tile = MapData[i].tile;
    //        info.pos = MapData[i].pos;
    //        info.ipos = MapData[i].ipos;
    //        clone.MapData.Add(info);
    //    }

    //    return clone;
    //}



    /// <summary>
    /// 每艘船的数据
    /// </summary>
    [Serializable]
    public class CityMapSerialize
    {
        [SerializeField]
        public string cityName;
        [SerializeField]
        public Vector2 CellSize;        // 格子的大小
        [SerializeField]
        public float GridHigh;          // 格子的高度
        [SerializeField]
        public float GridWide;          // 格子的宽度
        [SerializeField]
        public float LineEquationK;     //行的直线方程的K
        [SerializeField]
        public float FirstLineEquationB;//第一行的直线方程的B
        [SerializeField]
        public float RowEquationK;      //列的直线方程的K
        [SerializeField]
        public float FirstRowEquationB; //第一列的直线方程的B
        [SerializeField]
        public float RotaAngle;         //地图的偏转度数
        [SerializeField]
        public Vector2 SamllGridLine;   //小格子的行
        [SerializeField]
        public Vector2 SamllGridRow;    //小格子的宽
        [SerializeField]
        public Vector3Int BaseGridPos;  //基础格子的行列数

        [SerializeField]
        public List<GridTileInfo> MapData = new List<GridTileInfo>();



        public CityMapSerialize(Grid gridCom, Tilemap tileMap, string mapName)
        {
            //名称
            cityName = mapName;
            //格子大小
            CellSize = gridCom.cellSize;
            //地图格子信息
            BoundsInt cellBounds = tileMap.cellBounds;
            for (int line = cellBounds.xMin; line < cellBounds.xMax; line++)
            {
                for (int row = cellBounds.yMin; row < cellBounds.yMax; row++)
                {
                    TileBase tile = tileMap.GetTile(new Vector3Int(line, row, 0));
                    if (tile != null)
                    {
                        GridTileInfo _data = new GridTileInfo();
                        _data.ipos = new Vector2Int(line, row);
                        MapData.Add(_data);
                    }
                }
            }
            if (MapData.Count == 0)
            {
                throw new Exception(mapName + "  地图数据不对 ");
            }

            //先取0,0点的位置
            BaseGridPos = new Vector3Int(0, 0, 0);

            Vector3Int nextLinePos = new Vector3Int(BaseGridPos.x + 1, BaseGridPos.y, BaseGridPos.z);
            Vector3Int nextRowPos = new Vector3Int(BaseGridPos.x, BaseGridPos.y + 1, BaseGridPos.z);
            if (!tileMap.HasTile(BaseGridPos) || !tileMap.HasTile(nextLinePos) || !tileMap.HasTile(nextRowPos))
            {
                throw new Exception(mapName + "  原点信息不对 ");
            }




            Vector3 baseGrid = tileMap.CellToWorld(new Vector3Int(0, 0, 0));
            Vector3 nextLineGrid = tileMap.CellToWorld(new Vector3Int(1, 0, 0));
            Vector3 nextRowGrid = tileMap.CellToWorld(new Vector3Int(0, 1, 0));

            //求格子宽高
            //GridHigh = Vector3.Distance(baseGrid, nextLineGrid);
            //GridWide = Vector3.Distance(baseGrid, nextRowGrid);

            ////求飞船旋转角度
            //Vector2 dirVec = nextLineGrid - baseGrid;
            //RotaAngle = Vector2.Angle(dirVec, new Vector2(1, 0));
            ////hack【暂改】【城建机制】这儿到时重新捋一下
            //if (dirVec.x < 0 && dirVec.y < 0)
            //{//第三象限角度为负
            //    serialize.RotaAngle = serialize.RotaAngle * -1;
            //}

            ////第一行函数k,b
            //serialize.RowEquationK = Mathf.Tan(serialize.RotaAngle / 180 * Mathf.PI);
            ////serialize.FirstRowEquationB = nextLineGrid.y - serialize.RowEquationK * nextLineGrid.x;
            ////第一列函数k,b
            //serialize.LineEquationK = Mathf.Tan((180 - serialize.RotaAngle) / 180 * Mathf.PI);
            ////serialize.FirstLineEquationB = nextRowGrid.y - serialize.LineEquationK * nextRowGrid.x;

            // 小个子的偏移量
            //SamllGridLine = (nextLinePos - baseGrid).normalized * (GridHigh / 3);// 默认是分成3份
            //// 小个子的偏移量
            //SamllGridRow = (baseGrid - nextRowGrid).normalized * (GridWide / 3);// 默认是分成3份
        }
    }
#endif
#endregion