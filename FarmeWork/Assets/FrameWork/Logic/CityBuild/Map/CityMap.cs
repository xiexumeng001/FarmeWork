using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using LitJson;

using System;

namespace ShipFarmeWork.Logic.CityMap
{
    /// <summary>
    /// 城建地图类
    /// </summary>
    //hack【暂改】【tileMap】这儿的格子数据有点多,需要多切换下格子,加载很多飞船,看下是否有无清楚不干净的数据,导致内存泄露
    public class CityMap
    {
        //获取建造格子实例
        private static Func<BuildGrid> GetBuildGridInstance;
        //获取行走格子实例
        private static Func<RunGrid> GetRunGridInstance;

        //地图对象
        public IMap MapObj;
        /// <summary>
        /// 地图名称
        /// </summary>
        public string MapName;

        /// <summary>
        /// 人物行走格子集合
        /// </summary>
        private Dictionary<Vector2Int, RunGrid> _RunGridDic = new Dictionary<Vector2Int, RunGrid>();
        public Dictionary<Vector2Int, RunGrid> RunGridDic { get { return _RunGridDic; } }
        /// <summary>
        /// 数组集合,获取元素最好用这个,性能好
        /// </summary>
        public RunGrid[,] _RunGridArr;
        public RunGrid[,] RunGridArr { get { return _RunGridArr; } }

        /// <summary>
        /// 人物建造格子集合
        /// </summary>
        private Dictionary<Vector2Int, BuildGrid> _BuildGridDic = new Dictionary<Vector2Int, BuildGrid>();
        public Dictionary<Vector2Int, BuildGrid> BuildGridDic { get { return _BuildGridDic; } }
        /// <summary>
        /// 数组集合,获取元素最好用这个,性能好
        /// </summary>
        private BuildGrid[,] _BuildGridArr;
        public BuildGrid[,] BuildGridArr { get { return _BuildGridArr; } }


        /// <summary>
        /// Grid组件
        /// </summary>
        private Grid _Grid;

        /// <summary>
        /// 逻辑格的 Tilemap 对象
        /// </summary>
        private Tilemap _Tilemap;

        /// <summary>
        /// tileMap Z轴
        /// </summary>
        public float ZSort { get { return 0; } }
        //public float ZSort { get { return _Tilemap.transform.position.z; } }

        //hack【暂改】【战报】格子宽高,旋转角度 等 都做成static把
        //行走格子的宽高
        public static Vector2 RunGridWidHight = new Vector2(15.65248f, 15.65248f);
        //公共旋转角度
        public static float RotaAngleComm = 26.56505f;

        /// <summary>
        /// 是否翻转
        /// </summary>
        private bool _IsReverse;

        /// <summary>
        /// 行走格子之前的向量
        /// </summary>
        private Vector2 _RunLineVector;
        private Vector2 _RunRowVector;

        /// <summary>
        /// 建造格子之间的向量
        /// </summary
        private Vector2 _BuildLineVector;
        private Vector2 _BuildRowVector;

        //宽是垂直地图方向的长度、高是平行地图方向的长度
        public float RunGridHigh;
        public float RunGridWide;

        public float BuildGridHigh;
        public float BuildGridWidth;

        public int RunGridLineNum;        //行走格子行数
        public int RunGridRowNum;         //行走格子列数
        public int MinRunLine;            //行走格子最小行数
        public int MinRunRow;             //行走格子最小列数

        public float LineEquationK;          //行的直线方程的K
        public float RowEquationK;           //列的直线方程的K

        //参考格子
        public RunGrid RefreRunGrid = null;    //获取b的值时参考的格子
        public float ReferLineEquationB;     //参照行的直线方程的B
        public float ReferRowEquationB;      //参照列的直线方程的B

        public float RotaAngle;   //地图的偏转度数

        public int BuildMaxLine;   //最大行
        public int BuildMinLine;   //最小行
        public int BuildMaxRow;    //最大列
        public int BuildMinRow;    //最小列
        public int BuildGridLineNum;      //建造格子行数
        public int BuildGridRowNum;       //建造格子列数

        //使用的寻路算法
        private static AStarArith SearchRoadArith = new AStarArith();

        //地图画线的脚本
        private MapDrawer _MapDrawer = null;

        /// <summary>
        /// 当更新位置
        /// </summary>
        public Action OnUpdateGridPos;

        /// <summary>
        /// 当参数错误时返回的坐标
        /// </summary>
        public Vector3 ErrorPos = Vector3.zero;

        /// <summary>
        /// 倍数
        /// </summary>
        public static int Multiple = 3;

        /// <summary>
        /// 是否能展示绘制
        /// </summary>
        public bool IsCanShowDraw = true;

        /// <summary>
        /// 初始化静态东西
        /// </summary>
        public static void Init(Func<BuildGrid> getBuildGridInstance, Func<RunGrid> getRunGridInstance)
        {
            GetBuildGridInstance = getBuildGridInstance;
            GetRunGridInstance = getRunGridInstance;
        }

        /// <summary>
        /// 初始化地形
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="mapName"></param>
        public CityMap(string mapPath, string mapName, IMap MapObj, Vector3 parentPos, bool isReverse, bool isCanShowDraw)
        {
            this.MapObj = MapObj;
            IsCanShowDraw = isCanShowDraw;

            TextAsset mapJson = MapLoad.LoadMapData(mapPath, mapName);
            CityMapSerialize citySerialize = JsonMapper.ToObject<CityMapSerialize>(mapJson.text);
            LoadCityMapData(citySerialize, parentPos, isReverse);
        }


        /// <summary>
        /// 初始化(这个是给在编译器下用的)
        /// </summary>
        /// <param name="citySerialize"></param>
        /// <param name="parentPos"></param>
        /// <param name="isReverse"></param>
        public CityMap(CityMapSerialize citySerialize, Vector3 parentPos, bool isReverse,bool isCanShowDraw)
        {
            IsCanShowDraw = isCanShowDraw;
            LoadCityMapData(citySerialize, parentPos, isReverse);
        }

        /// <summary>
        /// 加载地图数据
        /// </summary>
        private void LoadCityMapData(CityMapSerialize citySerialize, Vector3 parentPos, bool isReverse)
        {
            MapName = citySerialize.CityName;

            List<Vector2IntObj> gridInfoList = citySerialize.GridDataList;
            //hack【暂改】【tileMap】临时先这样写,因为加载出来的预制体tilemap获取到的位置不对,展示新的物体
            string showMapParentName = "MapGameParent";
            GameObject mapParent = GameObject.Find(showMapParentName);
            if (mapParent == null)
            {
                mapParent = new GameObject(showMapParentName);
                //这个是因为 拓展的编译器也会去操作CityMap,但是在Editor下不能操作这个方法,所以需要判断下是否在播放游戏
                bool isDont = true;
#if UNITY_EDITOR
                isDont = UnityEditor.EditorApplication.isPlaying;
#endif
                if (isDont)
                {
                    GameObject.DontDestroyOnLoad(mapParent);
                }
            }

            GameObject grid = new GameObject(MapName);
            grid.transform.parent = mapParent.transform;
            grid.transform.position = parentPos;
            Grid gridCom = grid.AddComponent<Grid>();
            gridCom.cellLayout = GridLayout.CellLayout.Isometric;
            gridCom.cellSize = new Vector2((float)citySerialize.CellSize.x, (float)citySerialize.CellSize.y);

            //设置子物体
            GameObject tilemap = new GameObject("Tilemap");
            tilemap.transform.SetParent(grid.transform, false);
            if (isReverse)
            {
                Vector3 pos = tilemap.transform.localEulerAngles;
                tilemap.transform.localEulerAngles = new Vector3(pos.x, pos.y, pos.z - 180);
            }
            Tilemap tileMap = tilemap.AddComponent<Tilemap>();
            TilemapRenderer render = tilemap.AddComponent<TilemapRenderer>();

            _Grid = gridCom;
            _Tilemap = tileMap;

            //更新基础信息
            UpdateBaseInfo();

            List<Vector3> PosList = new List<Vector3>();
            for (int i = 0; i < gridInfoList.Count; i++)
            {
                Vector2IntObj gridInfo = gridInfoList[i];
                Vector2Int buildLineRow = new Vector2Int(gridInfo.x, gridInfo.y);
                Vector2 bigGridpos = GetBigGridCenterPos((Vector3Int)buildLineRow);
                //生成建造格子
                BuildGrid buildGrid = GetBuildGridInstance();
                buildGrid.Init(buildLineRow.x, buildLineRow.y, bigGridpos, this);
                _BuildGridDic.Add(buildLineRow, buildGrid);

                for (int line = -1; line <= 1; line++)
                {
                    for (int row = -1; row <= 1; row++)
                    {
                        Vector2Int lineRow = new Vector2Int(buildLineRow.x * Multiple + line, buildLineRow.y * Multiple + row);
                        Vector2 smallGridPos = bigGridpos + _RunLineVector * line + _RunRowVector * row;

                        //生成行走格子
                        RunGrid runGrid = GetRunGridInstance();
                        runGrid.Init(lineRow, smallGridPos, buildGrid, this);
                        _RunGridDic.Add(lineRow, runGrid);
                        buildGrid.RunGridList.Add(runGrid);
                        if (line == 1 && row == 1)
                        {
                            buildGrid.BaseRunGrid = runGrid;
                        }
                        if (RefreRunGrid == null) RefreRunGrid = runGrid;
                    }
                }

                if (i == 0)
                {
                    BuildMinLine = BuildMaxLine = buildLineRow.x;
                    BuildMinRow = BuildMaxRow = buildLineRow.y;
                }
                if (buildLineRow.x > BuildMaxLine) { BuildMaxLine = buildLineRow.x; }
                if (buildLineRow.x < BuildMinLine) { BuildMinLine = buildLineRow.x; }
                if (buildLineRow.y > BuildMaxRow) { BuildMaxRow = buildLineRow.y; }
                if (buildLineRow.y < BuildMinRow) { BuildMinRow = buildLineRow.y; }
            }

            //建造格子数组
            BuildGridLineNum = BuildMaxLine - BuildMinLine + 1;
            BuildGridRowNum = BuildMaxRow - BuildMinRow + 1;
            _BuildGridArr = new BuildGrid[BuildGridLineNum, BuildGridRowNum];
            foreach (var item in _BuildGridDic)
            {
                Vector2Int lineRow = item.Key;
                int lineIndex = lineRow.x - BuildMinLine;
                int rowIndex = lineRow.y - BuildMinRow;
                _BuildGridArr[lineIndex, rowIndex] = item.Value;
            }

            //行走格子数组
            MinRunLine = (BuildMinLine * Multiple - 1);
            MinRunRow = (BuildMinRow * Multiple - 1);
            RunGridLineNum = (BuildMaxLine * Multiple + 1) - MinRunLine + 1;
            RunGridRowNum = (BuildMaxRow * Multiple + 1) - MinRunRow + 1;
            _RunGridArr = new RunGrid[RunGridLineNum, RunGridRowNum];
            foreach (var item in _RunGridDic)
            {
                Vector2Int lineRow = item.Key;
                int lineIndex = lineRow.x - MinRunLine;
                int rowIndex = lineRow.y - MinRunRow;
                _RunGridArr[lineIndex, rowIndex] = item.Value;
            }

            ////获取参考的行走格子
            //BuildGrid baseBuildGrid = _BuildGridDic[(Vector2Int)baseGridLineRow];
            //RefreRunGrid = baseBuildGrid.BaseRunGrid;

            //计算变化数据
            ComputeParameter();
            //更新格子层级
            //UpdateGridLayer(isReverse);
            //展示辅助线
            StartShow();
        }

        /// <summary>
        /// 地图位置变化时，更新地图信息
        /// </summary>
        public void UpdateMapInfo(Grid gridCom, Tilemap tileMapCom)
        {
            _Grid.transform.position = gridCom.transform.position;
            _Grid.transform.rotation = gridCom.transform.rotation;
            _Grid.transform.localScale = gridCom.transform.localScale;
            _Tilemap.transform.localPosition = tileMapCom.transform.localPosition;
            _Tilemap.transform.localRotation = tileMapCom.transform.localRotation;
            _Tilemap.transform.localScale = tileMapCom.transform.localScale;

            UpdateBaseInfo();

            foreach (KeyValuePair<Vector2Int, BuildGrid> kps in _BuildGridDic)
            {
                //更新建筑格子
                BuildGrid buildGrid = kps.Value;
                Vector2Int bigLineRow = kps.Key;
                buildGrid.SetMapWorldPos(GetBigGridCenterPos((Vector3Int)bigLineRow));

                //更新行走格子位置
                Vector2 bigGridpos = buildGrid.WorldPosition;
                for (int line = -1; line <= 1; line++)
                {
                    for (int row = -1; row <= 1; row++)
                    {
                        RunGrid runGridGrid = GetRunGrid(bigLineRow.x * Multiple + line, bigLineRow.y * Multiple + row);
                        Vector2 smallGridPos = bigGridpos + _RunLineVector * line + _RunRowVector * row;
                        runGridGrid.SetMapWorldPos(smallGridPos);
                    }
                }
            }

            ComputeParameter();
            StartShow();

            if (OnUpdateGridPos != null) OnUpdateGridPos();
        }


        /// <summary>
        /// 更新基础信息
        /// </summary>
        private void UpdateBaseInfo()
        {
            //计算格子向量
            Vector3Int baseGridLineRow = new Vector3Int(0, 0, 0);
            Vector3Int nextLineGridLineRow = new Vector3Int(1, 0, 0);
            Vector3Int nextRowGridLineRow = new Vector3Int(0, 1, 0);

            Vector3 basePos = _Tilemap.CellToWorld(baseGridLineRow);
            Vector3 nextLinePos = _Tilemap.CellToWorld(nextLineGridLineRow);
            Vector3 nextRowPos = _Tilemap.CellToWorld(nextRowGridLineRow);

            Vector3 nextLineVecDir = nextLinePos - basePos;    //下一行的方向向量
            Vector3 nextRowVecDir = nextRowPos - basePos;      //下一列的方向向量

            //格子宽高
            BuildGridHigh = nextLineVecDir.magnitude;
            BuildGridWidth = nextRowVecDir.magnitude;
            RunGridHigh = BuildGridHigh / Multiple;
            RunGridWide = BuildGridWidth / Multiple;

            //方向向量
            _RunLineVector = nextLineVecDir / 3;
            _RunRowVector = nextRowVecDir / 3;
            _BuildLineVector = nextLineVecDir;
            _BuildRowVector = nextRowVecDir;

            //偏转度数
            RotaAngle = Vector2.Angle(nextLineVecDir, new Vector2(1, 0));
            if (nextLineVecDir.y < 0)
            {//如果向量y小于0,求到的角度就是负的
                RotaAngle = RotaAngle * -1;
            }
        }


        /// <summary>
        /// 更具地形数据计算出必要的参数
        /// </summary>
        private void ComputeParameter()  //TODO  要根据地图的位置信息获取新的 方程系数
        {
            //计算基准线
            Vector2 pos = RefreRunGrid.WorldPosition;
            //下一行的向量求到的k是列的k
            LineEquationK = _RunRowVector.y / _RunRowVector.x;  /*Mathf.Tan(RotaAngle / 180 * Mathf.PI);       //行的直线方程的K*/
            ReferLineEquationB = pos.y - LineEquationK * pos.x;  //基准行的直线方程的B
            RowEquationK = _RunLineVector.y / _RunLineVector.x;/* Mathf.Tan((180 - RotaAngle) / 180 * Mathf.PI);//列的直线方程的K*/
            ReferRowEquationB = pos.y - RowEquationK * pos.x;    //基准列的直线方程的B
        }

        /// <summary>
        /// 获取大格子的中心位置
        /// </summary>
        /// <param name="bigGridLineRow"></param>
        /// <returns></returns>
        private Vector3 GetBigGridCenterPos(Vector3Int bigGridLineRow)
        {
            Vector2 pos = (Vector2)_Tilemap.CellToWorld((Vector3Int)bigGridLineRow) + _BuildLineVector / 2 + _BuildRowVector / 2;
            return pos;
        }
        
        /// <summary>
        /// 绘制编辑模式的线
        /// </summary>
        private void StartShow()
        {
            //如果不能展示绘制
            if (!IsCanShowDraw) return;

            //如果没有绘制类,就先生成绘制类
            if (_MapDrawer == null)
            {
                _MapDrawer = new MapDrawer();
            }
            //绘制行走格子
            foreach (var item in _RunGridDic)
            {
                _MapDrawer.AddDrawPos(item.Value, RunGridHigh, RotaAngle);
            }
            //绘制建筑格子
            foreach (var item in _BuildGridDic)
            {
                _MapDrawer.AddDrawPos(item.Value, RunGridHigh * Multiple, RotaAngle);
            }
            _MapDrawer.AddStandardLine(LineEquationK, ReferLineEquationB, 1);
            _MapDrawer.AddStandardLine(RowEquationK, ReferRowEquationB, 2);
        }

        /// <summary>
        /// 更新格子展示颜色
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="colorEnum"></param>
        public void UpdateDrawGridColor(BaseGrid grid, GridColorEnum colorEnum, bool isAdd)
        {
            if (_MapDrawer != null)
            {
                _MapDrawer.UpdateDrawGridColor(grid, colorEnum, isAdd);
            }
        }

        /// <summary>
        /// 删除地图
        /// </summary>
        public void RemoveMap()
        {
            try
            {
                //这儿也是因为Editor下要调用,所以用了这一帧的删除
                if (_Grid != null) GameObject.DestroyImmediate(_Grid.gameObject);

                if (_MapDrawer != null)
                {
                    _MapDrawer.Close();
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

#region 对外接口

        /// <summary>
        /// 获取大格子的行列数
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        //hack【暂改】【tileMap】这儿没用到么？
        public Vector3Int GetBigGricPos(Vector3 pos)
        {
            return _Tilemap.WorldToCell(pos);
        }

        /// <summary>
        /// 获取建筑格子
        /// </summary>
        /// <returns></returns>
        public BuildGrid GetBuildGrid(int line, int row)
        {
            int lineIndex = line - BuildMinLine;
            int rowIndex = row - BuildMinRow;
            if (lineIndex >= 0 && lineIndex < BuildGridLineNum && rowIndex >= 0 && rowIndex < BuildGridRowNum)
            {
                try
                {
                    return _BuildGridArr[lineIndex, rowIndex];
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat("获取建造格子错误,行列:{0}{1},地图:{2}\n错误{3}", line, row, MapName, e);
                }
            }
            return null;
        }

        /// <summary>
        /// 获取大格子
        /// </summary>
        /// <returns></returns>
        public BuildGrid GetBuildGridByPos(Vector3 pos)
        {
            Vector2Int lineRow = (Vector2Int)_Tilemap.WorldToCell(pos);
            return GetBuildGrid(lineRow.x, lineRow.y);
        }


        /// <summary>
        /// 获取行走格子
        /// </summary>
        /// <param name="line"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public RunGrid GetRunGrid(int line, int row)
        {
            int lineIndex = line - MinRunLine;
            int rowIndex = row - MinRunRow;
            if (lineIndex >= 0 && lineIndex < RunGridLineNum && rowIndex >= 0 && rowIndex < RunGridRowNum)
            {
                try
                {
                    return _RunGridArr[lineIndex, rowIndex];
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat("获取行走格子错误,行列:{0}{1},地图:{2}\n错误{3}", line, row, MapName, e);
                }
            }
            return null;
        }

        /// <summary>
        /// 获取行走格子
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public RunGrid GetRunGridByPos(Vector3 pos)
        {
            Vector2Int ranks = GetGridRanks(pos);
            return GetRunGrid(ranks.x, ranks.y);
        }

        /// <summary>
        /// 获取周围可站立的格子
        /// </summary>
        /// <param name="baseGrid"></param>
        /// <returns></returns>
        public RunGrid GetAroundCanIdleRunGrid(RunGrid baseGrid, bool isSameRoom)
        {
            RunGrid occGrid = baseGrid;
            ICanBuild occuBuild = occGrid.Builder;
            for (int line = -1; line <= 1; line++)
            {
                for (int row = -1; row <= 1; row++)
                {
                    if (line == 0 && row == 0) continue;
                    Vector2Int lineRow = new Vector2Int(occGrid.Line + line, occGrid.Row + row);
                    RunGrid runGrid = GetRunGrid(lineRow.x, lineRow.y);
                    if (runGrid != null && runGrid.IsWalk && !runGrid.IsDoor && occuBuild == runGrid.Builder)
                    {//这儿注意用break只跳出一个循环,一不小心就错了
                        return runGrid;
                    }
                }
            }
            return occGrid;
        }

        /// <summary>
        /// 获得格子的行列数
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public Vector2Int GetGridRanks(Vector3 pos)
        {
            //求pos点所在行的一元一次方程 与 第一列的方程的交点
            float lineB = pos.y - LineEquationK * pos.x;  // 当前点的B值
            float nodeX = (lineB - ReferRowEquationB) / (RowEquationK - LineEquationK);
            float nodeY = LineEquationK * nodeX + lineB;
            Vector2 node_width = new Vector2(nodeX, nodeY);  //交点
            float dicWidth = Vector2.Distance(node_width, pos);
            //求正反向
            Vector2 widthDir = node_width - (Vector2)pos;
            float widthAngle = Vector2.Angle(widthDir, _RunLineVector);
            if (widthAngle > 90)
            {//大于90度,就是反向
                dicWidth = -dicWidth;
            }

            //求pos点所在列的一元一次方程 与 第一行的方程的交点
            float rowB = pos.y - RowEquationK * pos.x;
            float nodeX_ = (rowB - ReferLineEquationB) / (LineEquationK - RowEquationK);
            float nodeY_ = RowEquationK * nodeX_ + rowB;
            Vector2 node_hight = new Vector2(nodeX_, nodeY_);  //交点
            float dicHight = Vector2.Distance(node_hight, pos);
            //求正反向
            Vector2 hightDir = node_hight - (Vector2)pos;
            float hightAngle = Vector2.Angle(hightDir, _RunRowVector);
            if (hightAngle > 90)
            {
                dicHight = -dicHight;
            }

            int row = RefreRunGrid.Row + System.Convert.ToInt32(dicWidth / RunGridWide);
            int line = RefreRunGrid.Line + System.Convert.ToInt32(dicHight / RunGridHigh);
            return new Vector2Int(line, row);
        }

        /// <summary>
        /// 获得建筑格子的位置
        /// </summary>
        /// <param name="line"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public Vector3 GetBulidGridPos(int line,int row)
        {
            return _Tilemap.CellToWorld(new Vector3Int(line,row,0));
        }

        /// <summary>
        /// 获得建筑格子的行数
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public Vector2Int GetBulidGridRanks(Vector3 pos)
        {
            return (Vector2Int)_Tilemap.WorldToCell(pos);
        }

#region  AI 寻路相关

        /// <summary>
        /// 寻路格子
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public List<RunGrid> GetRoad(RunGrid start, RunGrid end)
        {
            List<RunGrid> runList = SearchRoadArith.SearchRoad(this, start, end);
            if (runList != null && runList.Count > 0)
            {
                UpdateDrawGridColor(start, GridColorEnum.SearchStart, true);
                UpdateDrawGridColor(end, GridColorEnum.SearchEnd, true);
            }
            return runList;
        }

        /// <summary>
        /// 格子是否可以移动
        /// </summary>
        /// <returns></returns>
        public bool GridCanMove(int line, int row, ICanBuild build)
        {
            RunGrid runGrid = GetRunGrid(line, row);

            if (runGrid != null)
            {
                if (runGrid.Builder == build)  // 如果格子自身
                {
                    return true;
                }
                return runGrid.IsWalk;
            }
            return false;
        }

#endregion

#region 建筑相关

        /// <summary>
        /// 尝试建造(用于解析网络传递回来的建造信息用)
        /// </summary>
        /// <param name="builer"></param>
        /// <param name="line"></param>
        /// <param name="row"></param>
        /// <param name="isNeedBuild">是否需要走建造流程</param>
        /// <returns></returns>
        public bool TryBuild(ICanBuild builer, int line, int row)
        {
            List<BuildGrid> buildGridList = TryGetCanBuildGrids(line, row, builer.Width, builer.Height, builer);

            BuildGrid clickGrid = GetBuildGrid(line, row);
            if (clickGrid == null)
            {
                throw new Exception(string.Format("点击的格子是空的 {0},{1},地图名称:{2}", line, row, MapName));
            }

            if (buildGridList != null && buildGridList.Count > 0)
            {
                //移除格子信息
                TryRemoveGridInfo(builer);

                //添加格子信息
                TryAddGridInfo(builer, clickGrid, buildGridList);

                //当建造成功
                MapObj.OnBeBuild(builer);
                builer.OnBePlaceToMap(MapObj);

                return true;
            }
            else
            {
                Debug.LogErrorFormat("No  Can  Build ,点击格子{0},{1},guid:{2}", line, row, builer.GUID);
            }
            return false;
        }


        /// <summary>
        /// 尝试移除建筑物
        /// </summary>
        public bool TryRemoveBuilder(ICanBuild builer)
        {
            //先判断是否存在
            //if (_AllBuilder.Contains(builer))
            if (builer.ClickGrid != null)
            {
                //当取消成功
                builer.OnRemoveSuccess();

                TryRemoveGridInfo(builer);
                return true;
            }
            else
            {
                Debug.LogError("the builer is NO build!!");
            }
            return false;
        }

        /// <summary>
        /// 尝试添加格子信息
        /// </summary>
        private void TryAddGridInfo(ICanBuild builer, BuildGrid clickGrid, List<BuildGrid> buildGridList)
        {
            //添加点击格子
            builer.ClickGrid = clickGrid;
            //添加建筑物的占用
            builer.OccuGrid = buildGridList;
            for (int i = 0; i < buildGridList.Count; i++)
            {
                buildGridList[i].SetBuild(builer);
            }
        }


        /// <summary>
        /// 尝试清除格子信息
        /// </summary>
        /// <param name="builer"></param>
        /// <returns>返回之前时候被建造</returns>
        private void TryRemoveGridInfo(ICanBuild builer)
        {
            //取消占用的格子
            if (builer.OccuGrid != null)
            {
                builer.OnRemoveGridInfo();

                for (int i = 0; i < builer.OccuGrid.Count; i++)
                {
                    builer.OccuGrid[i].OnCanCelBuild();
                }
                builer.OccuGrid = null;
                builer.CanRunGridList = null;
                //点击格子清除
                builer.ClickGrid = null;
            }
        }

#region  格子是否可以使用判断
        public bool IsCanBuild(int line, int row, int width, int height, ICanBuild IgornBuild)
        {
            List<BuildGrid> cityGridList = TryGetCanBuildGrids(line, row, width, height, IgornBuild);
            if (cityGridList == null || cityGridList.Count <= 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 获取可以建造的空间
        /// </summary>
        /// <param name="line"></param>
        /// <param name="row"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="IgornBuild"></param>
        /// <returns></returns>
        protected List<BuildGrid> TryGetCanBuildGrids(int line, int row, int width, int height, ICanBuild IgornBuild)
        {
            List<BuildGrid> canBuildGridList = null;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    BuildGrid grid = GetBuildGrid(line - j, row - i);
                    if (grid != null && (grid.IsCanBuild || grid.Builder == IgornBuild))
                    {
                        if (canBuildGridList == null) canBuildGridList = new List<BuildGrid>();
                        canBuildGridList.Add(grid);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            return canBuildGridList;
        }
#endregion




#region 格子上物品的坐标


        /// <summary>
        /// 获得建筑所在位置
        /// </summary>
        /// <param name="cityGrids"></param>
        /// <returns></returns>
        public Vector3 GetGridsPos(List<BuildGrid> cityGrids)
        {
            if (cityGrids != null && cityGrids.Count > 0)
            {

                BuildGrid LeftPoint = cityGrids[0];
                BuildGrid RightPoint = cityGrids[0];

                for (int i = 0; i < cityGrids.Count; i++)
                {
                    if (cityGrids[i].WorldPosition.x < LeftPoint.WorldPosition.x)
                    {
                        LeftPoint = cityGrids[i];
                    }
                    if (cityGrids[i].WorldPosition.x > RightPoint.WorldPosition.x)
                    {
                        RightPoint = cityGrids[i];
                    }
                }
                return (RightPoint.WorldPosition + LeftPoint.WorldPosition) * 0.5f;
            }
            return ErrorPos;
        }

        /// <summary>
        /// 获取建筑位置
        /// </summary>
        /// <returns></returns>
        public Vector3 GetBuilderPos(Vector3 pos, int width, int height, out BuildGrid baseBuildGrid)
        {
            Vector3 basePos = pos;
            //有格子的话,就更新下基础位置
            baseBuildGrid = GetBuildGridByPos(pos);
            if (baseBuildGrid != null)
            {
                basePos = baseBuildGrid.WorldPosition;
            }
            return CountCenterPos_Build(basePos, width, height);
        }

        /// <summary>
        /// 得到建筑位置
        /// </summary>
        /// <param name="line"></param>
        /// <param name="row"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public Vector3 GetBuilderPos(int line, int row, int width, int height)
        {
            //根据格子格子的点击坐标，和坐标长宽，加载坐标长宽距离获取实际中心位置
            BuildGrid grid = GetBuildGrid(line, row);
            if (grid != null)
            {
                return CountCenterPos_Build(grid.WorldPosition, width, height);
            }
            return ErrorPos;
        }

        /// <summary>
        /// 计算中心位置
        /// </summary>
        /// <param name="vasePos"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private Vector3 CountCenterPos_Build(Vector3 pos, int width, int height)
        {
            //计算中心点
            pos = pos + -1 * (Vector3)_BuildLineVector / 2 * (height - 1);
            pos = pos + -1 * (Vector3)_BuildRowVector / 2 * (width - 1);

            return pos;
        }




        /// <summary>
        /// 得到建筑位置
        /// </summary>
        /// <param name="line"></param>
        /// <param name="row"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public Vector3 GetBuilderPosByRunGrid(int line, int row, int width, int height)
        {
            //根据格子格子的点击坐标，和坐标长宽，加载坐标长宽距离获取实际中心位置
            RunGrid grid = GetRunGrid(line, row);
            if (grid != null)
            {
                return CountCenterPos_Run(grid.WorldPosition, width, height);
            }
            return ErrorPos;
        }

        /// <summary>
        /// 计算中心位置
        /// </summary>
        /// <param name="vasePos"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private Vector3 CountCenterPos_Run(Vector3 pos, int width, int height)
        {
            //计算中心点
            pos = pos + -1 * (Vector3)_RunLineVector / 2 * (height - 1);
            pos = pos + -1 * (Vector3)_RunRowVector / 2 * (width - 1);

            return pos;
        }

        #endregion
        #endregion
        #endregion
    }
}