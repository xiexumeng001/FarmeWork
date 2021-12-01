using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShipFarmeWork.Logic.CityMap
{
    public class AStarArith
    {
        //close代表已经搜寻过周围点的格子
        Dictionary<RunGrid, bool> CloseGrid = new Dictionary<RunGrid, bool>();

        //这里面的格子都是已发现,但是未搜寻过周围点
        //带着每条路径的耗费值
        Dictionary<SearchGrid, bool> OpenGrids = new Dictionary<SearchGrid, bool>();

        //二叉堆对象
        int heapMaxNum = 3000;    //堆的最大数量
        BinaryHeap<SearchGrid> binaryHeap;

        int searchIndex = 0;
        int searchGridMaxNum = 10000;
        int searchMaxNum = 10000;
        int roadMaxGridNum = 3000;
        List<SearchGrid> AllSearchGrid;

        /// <summary>
        /// 普通格子与搜寻格子的对应关系
        /// </summary>
        Dictionary<RunGrid, SearchGrid> relaexGrid = new Dictionary<RunGrid, SearchGrid>();


        DirGrid[] directionGrid = new DirGrid[4];

        int G_InsNum = 10;   //G的增量值

        public AStarArith() {
            //初始化搜寻格子
            AllSearchGrid = new List<SearchGrid>(searchGridMaxNum);
            for (int i = 0; i < searchGridMaxNum; i++)
            {
                AllSearchGrid.Add(new SearchGrid());
            }

            //初始化方向格子
            directionGrid[0] = new DirGrid(-1, 0);    //下方向
            directionGrid[1] = new DirGrid(0, -1);    //左方向
            directionGrid[2] = new DirGrid(1, 0);     //上方向
            directionGrid[3] = new DirGrid(0, 1);     //右方向
        }

        public List<RunGrid> SearchRoad(CityMap mapGrid, RunGrid startGrid, RunGrid endGrid)
        {
            //若相等
            if (startGrid == endGrid)
            {
                List<RunGrid> searchRoad = new List<RunGrid>();
                searchRoad.Add(startGrid);
                return searchRoad;
            }

            try
            {
                //Debug.LogWarningFormat("开始寻路:开始位置{0},{1} 结束位置{2},{3}", startGrid.Line, startGrid.Row, endGrid.Line, endGrid.Row);

                //如果终点格子不能行走
                if (!endGrid.IsWalk)
                {
                    Debug.LogWarning("thi endGrid is not walk");
                    return null;
                }

                CloseGrid.Clear();
                OpenGrids.Clear();
                relaexGrid.Clear();
                binaryHeap = new BinaryHeap<SearchGrid>(heapMaxNum);
                searchIndex = 0;

                SearchGrid startSearch = GetSearchGrid(startGrid);
                AddToOpenGrid(startSearch);

                int circule = 0;
                while (OpenGrids.Count > 0)
                {
                    //判断是否循环太多
                    circule++;
                    if (circule > searchMaxNum)
                    {
                        throw new System.Exception("search road is too long");
                    }

                    //获取当前最短路径的格子
                    SearchGrid fatherGrid = GetMinSearchGrid();

                    for (int i = 0; i < directionGrid.Length; i++)
                    {
                        int line = fatherGrid.Grid.Line + directionGrid[i].Line;
                        int row = fatherGrid.Grid.Row + directionGrid[i].Row;

                        RunGrid sonGrid = mapGrid.GetRunGrid(line, row);
                        if (sonGrid == null) continue;
                        //是否能走
                        if (!sonGrid.IsWalk) continue;
                        if (CloseGrid.ContainsKey(sonGrid)) continue;

                        //记录格子的搜寻值
                        int g = fatherGrid.G + G_InsNum;
                        SearchGrid searchGrid = GetSearchGrid(sonGrid);
                        if (OpenGrids.ContainsKey(searchGrid))
                        {
                            if (searchGrid.G > g) { searchGrid.SetG(g, fatherGrid); }
                        }
                        else
                        {
                            int h = (Mathf.Abs(endGrid.Line - sonGrid.Line) + Mathf.Abs(endGrid.Row - sonGrid.Row)) * G_InsNum;
                            searchGrid.SetNum(g, h, fatherGrid);
                            AddToOpenGrid(searchGrid);
                        }

                        //当找到了
                        if (sonGrid == endGrid)
                        {
                            return MakeRoadOnSearchEnd(startSearch, searchGrid);
                        }
                    }
                }
            }
            catch (System.Exception e) {
                Debug.LogError(e);
            }
            return null;

        }

        private void GetStr(RunGrid startGrid, RunGrid endGrid, double milliseconds, int num,bool isError)
        {
            string log = string.Format("寻路从{0}到{1},耗时{2}ms,找了{3}次", startGrid.Line + ";" + startGrid.Row, endGrid.Line + ";" + endGrid.Row, milliseconds, num);
            if (isError)
            {
                Debug.LogError(log);
            }
            else
            {
                Debug.LogWarning(log);
            }
        }

        /// <summary>
        /// 当搜寻完毕生成路径
        /// </summary>
        /// <param name="startGrid"></param>
        /// <param name="endGrid"></param>
        /// <returns></returns>
        List<RunGrid> MakeRoadOnSearchEnd(SearchGrid startGrid, SearchGrid endGrid)
        {
            List<RunGrid> searchRoad = new List<RunGrid>();
            searchRoad.Add(endGrid.Grid);

            SearchGrid roadGrid = endGrid;
            int critleNum = 0;
            while (roadGrid.FatherGrid != null)
            {
                critleNum++;
                if (critleNum > roadMaxGridNum) {
                    throw new System.Exception("the road is too long");
                }
                roadGrid = roadGrid.FatherGrid;
                searchRoad.Add(roadGrid.Grid);
            }

            if (roadGrid != startGrid)
            {
                throw new System.Exception("the road is breakoff");
            }
            return searchRoad;
        }

        /// <summary>
        /// 获取搜寻格子
        /// </summary>
        /// <param name="grid"></param>
        /// <returns></returns>
        private SearchGrid GetSearchGrid(RunGrid grid)
        {
            if (relaexGrid.TryGetValue(grid, out SearchGrid searchGrid))
            {
            }
            else
            {
                searchGrid = AllSearchGrid[searchIndex];
                searchGrid.Grid = grid;
                searchGrid.SetNum(0, 0, null);
                searchIndex++;
                relaexGrid.Add(grid, searchGrid);
            }
            return searchGrid;
        }

        /// <summary>
        /// 添加格子到开启列表中
        /// </summary>
        /// <param name="searchGrid"></param>
        private void AddToOpenGrid(SearchGrid searchGrid) {
            //添加开启的格子
            OpenGrids.Add(searchGrid, true);
            binaryHeap.Add(searchGrid);
        }

        /// <summary>
        /// 获取当前已知的最小路径的格子
        /// </summary>
        /// <returns></returns>
        private SearchGrid GetMinSearchGrid() {
            SearchGrid grid = binaryHeap.RemoveFirst();
            OpenGrids.Remove(grid);
            CloseGrid.Add(grid.Grid, true);
            return grid;
        }

        /// <summary>
        /// 搜寻类型格子
        /// </summary>
        class SearchGrid : IHeapItem<SearchGrid>
        {
            public int HeapIndex { get; set; }

            public int G;
            public int H;
            public int F;
            public SearchGrid FatherGrid;
            public RunGrid Grid;

            public void SetNum(int g, int h, SearchGrid father)
            {
                G = g;
                H = h;
                F = g + h;
                FatherGrid = father;
            }

            public void SetG(int g, SearchGrid father)
            {
                G = g;
                F = G + H;
                FatherGrid = father;
            }

            public int CompareTo(SearchGrid nodeToCompare)
            {
                //比较大小
                return F.CompareTo(nodeToCompare.F);
            }
        }

        /// <summary>
        /// 方向类型格子
        /// </summary>
        class DirGrid {
            public int Line;
            public int Row;

            public DirGrid(int line,int row) {
                Line = line;
                Row = row;
            }
        }

    }

}