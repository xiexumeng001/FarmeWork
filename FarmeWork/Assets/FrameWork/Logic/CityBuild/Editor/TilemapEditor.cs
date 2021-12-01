using LitJson;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ShipFarmeWork.Logic.CityMap.Editor
{
    /// <summary>
    /// tileMap地图编译器
    /// </summary>
    public class TilemapEditor : EditorWindow
    {
        //资源路径
        private static string _UserTileDataPath = "/StarShip/Res/default/TilemapData/";
        //资源后缀
        //private static string _TielDataSuffix = ".asset";
        private static string _TielDataSuffix = ".json";
        //所有地图资源
        private List<CityMapSerialize> _LoadMapList;
        private List<string> OffsetList;
        /// <summary>
        /// 新增物体
        /// </summary>
        private Grid _AddTileGridGame = null; //预制体
        private Tilemap _AddTileMapGame = null;
        private string _AddTileMapName = "";   //名称


        private string TileMapOffsetStr = "";

        /// <summary>
        /// 滑条的当前位置
        /// </summary>
        Vector2 scrollPos = Vector2.zero;

        //正在展示的地图
        private static CityMap ShowCityMap;

        #region ShowTileWSMMapEditor
        [MenuItem("Tools/Ship TileMap %t")]
        public static void ShowTileWSMMapEditor()
        {
            TilemapEditor window = (TilemapEditor)EditorWindow.GetWindow(typeof(TilemapEditor));
            window.titleContent = new GUIContent("WSM TileMap");
            window.Show();
        }
        #endregion

        private void OnDestroy()
        {
            ClearRes();
        }

        void OnGUI()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                ClearRes();
                return;
            }

            if (EditorApplication.isUpdating)
            {
                ClearRes();
            }
               
            if (_LoadMapList == null)
            {//加载地图资源
                UpdateMapListDataCache();
            }

            //读取资源的地址
            GUILayout.Label("当前资源目录: " + _UserTileDataPath);

            //新增地图
            ShowTitle("新增地图");
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Grid组件:", GUILayout.Width(60f));
                _AddTileGridGame = (Grid)EditorGUILayout.ObjectField("", _AddTileGridGame, typeof(Grid), true, GUILayout.Width(100f));
                GUILayout.Space(10);

                GUILayout.Label("TileMap组件:", GUILayout.Width(60f));
                _AddTileMapGame = (Tilemap)EditorGUILayout.ObjectField("", _AddTileMapGame, typeof(Tilemap), true, GUILayout.Width(100f));
                GUILayout.Space(10);

                GUILayout.Label("名称:", GUILayout.Width(30f));
                _AddTileMapName = EditorGUILayout.TextField("", _AddTileMapName, GUILayout.Width(100f));
                GUILayout.Space(10);
                if (GUILayout.Button("确认", GUILayout.Width(60)))
                {
                    if (_AddTileGridGame == null)
                    {
                        EditorUtility.DisplayDialog("提示", "新增Grid是空的", "好的");
                        return;
                    }
                    if (_AddTileMapName == null)
                    {
                        EditorUtility.DisplayDialog("提示", "新增TileMap是空的", "好的");
                        return;
                    }
                    if (_AddTileMapName == null || _AddTileMapName == "")
                    {
                        EditorUtility.DisplayDialog("提示", "新增物体名称是空的", "好的");
                        return;
                    }
                    CityMapSerialize map = AddNewCityMapData(_AddTileGridGame, _AddTileMapGame, _AddTileMapName);
                }
            }
            GUILayout.EndHorizontal();

            //展示地图列表
            ShowTitle("所有地图");
            GUILayout.BeginVertical("", "box");
            {
                scrollPos = GUILayout.BeginScrollView(scrollPos, false, true, GUILayout.Height(300));
                {//开始展示滑动条
                    if (_LoadMapList != null && _LoadMapList.Count != 0)
                    {
                        for (int i = 0; i < _LoadMapList.Count; i++)
                        {
                            //开始展示地图信息一行
                            GUILayout.BeginHorizontal();
                            {
                                CityMapSerialize mapInfo = _LoadMapList[i];
                                GUILayout.Label(mapInfo.CityName, GUILayout.Width(200));  //展示地图名称
                                //看正向地图信息
                                if (GUILayout.Button("正", GUILayout.Width(50)))
                                {
                                    ShowTileMapInfo(mapInfo, false);
                                }
                                GUILayout.Space(20);
                                //看反向信息
                                if (GUILayout.Button("反", GUILayout.Width(50)))
                                {
                                    ShowTileMapInfo(mapInfo, true);
                                }
                                GUILayout.Space(20);
                                //偏移
                                OffsetList[i] = EditorGUILayout.TextField("", OffsetList[i], GUILayout.Width(100f));
                                GUILayout.Space(20);
                                //偏移确定
                                if (GUILayout.Button("偏移", GUILayout.Width(50)))
                                {
                                    if (OffsetList[i] != "")
                                    {
                                        string[] strArr = OffsetList[i].Split(',');
                                        if (strArr.Length == 2)
                                        {
                                            OffsetMap(mapInfo, int.Parse(strArr[0]), int.Parse(strArr[1]));
                                        }
                                    }
                                }
                                GUILayout.Space(20);
                                //删除
                                if (GUILayout.Button("删除", GUILayout.Width(50)))
                                {
                                    if (EditorUtility.DisplayDialog("提示", "你确定删除 " + mapInfo.CityName + " 地图资源么", "确定", "点错了"))
                                    {
                                        TryRemoveMapData(mapInfo);
                                    }
                                }
                            }
                            GUILayout.EndHorizontal();
                        }
                    }
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();


            if (GUILayout.Button("导出Json", GUILayout.Width(position.width)))
            {
                MapDataToJson();
            }
            if (GUILayout.Button("检查错误", GUILayout.Width(position.width)))
            {
                //1、格子数据
                //2、
                EditorUtility.DisplayDialog("提示", "这个功能还没写,暂时没什么好检查的\n有需要的话提需求", "确定");
            }
            if (GUILayout.Button("刷新", GUILayout.Width(position.width))) {
                AssetDatabase.Refresh();
            }

            //偏移
            TileMapOffsetStr = EditorGUILayout.TextField("", TileMapOffsetStr, GUILayout.Width(100f));
            GUILayout.Space(20);
            //偏移确定
            if (GUILayout.Button("偏移", GUILayout.Width(position.width)))
            {
                if (_AddTileGridGame != null && _AddTileMapGame != null)
                {
                    if (_AddTileMapName == null || _AddTileMapName == "")
                    {
                        EditorUtility.DisplayDialog("提示", "新增物体名称是空的", "好的");
                        return;
                    }

                    string[] strArr = TileMapOffsetStr.Split(',');
                    int line = int.Parse(strArr[0]);
                    int row = int.Parse(strArr[1]);
                    OffsetTileMap(_AddTileGridGame, _AddTileMapGame, _AddTileMapName, line, row);
                }
            }


                return;
        }

        /// <summary>
        /// 展示标题
        /// </summary>
        private void ShowTitle(string tileStr)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(300);
            GUILayout.Label(tileStr, GUILayout.Width(position.width));
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 更新所有的地图资源的缓存
        /// </summary>
        private void UpdateMapListDataCache()
        {
            if (_LoadMapList == null) _LoadMapList = new List<CityMapSerialize>();
            if (OffsetList == null) OffsetList = new List<string>();
            _LoadMapList.Clear();
            OffsetList.Clear();
            DirectoryInfo direction = new DirectoryInfo(Application.dataPath + _UserTileDataPath);//获取文件夹，exportPath是文件夹的路径
            FileInfo[] files = direction.GetFiles("*" + _TielDataSuffix, SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                CityMapSerialize mapData = LoadDataFrolFile(files[i].Name);
                if (mapData != null)
                {
                    _LoadMapList.Add(mapData);
                    OffsetList.Add("");
                }
            }
        }

        /// <summary>
        /// 偏移tileMap
        /// </summary>
        private void OffsetTileMap(Grid grid, Tilemap tileMap, string cityName, int offsetLine, int offsetRow)
        {
            GameObject tempGridGame = GameObject.Instantiate(grid.gameObject);
            Grid tempGrid = tempGridGame.GetComponent<Grid>();
            Tilemap tempTile = tempGridGame.GetComponentInChildren<Tilemap>();
            CityMapSerialize mapData = new CityMapSerialize();
            mapData.LoadData(tempGrid, tempTile, "");

            //清空初始数据
            tileMap.ClearAllTiles();
            //生成新的数据
            for (int i = 0; i < mapData.GridDataList.Count; i++)
            {
                Vector2IntObj ve2Obj = mapData.GridDataList[i];
                Vector3Int pos = new Vector3Int(ve2Obj.x, ve2Obj.y, 0);
                Vector3Int newPos = new Vector3Int(pos.x + offsetLine, pos.y + offsetRow, 0);
                tileMap.SetTile(newPos, tempTile.GetTile(pos));
            }


            //移除原来的数据
            if (_LoadMapList != null)
            {
                for (int i = 0; i < _LoadMapList.Count; i++)
                {
                    if (_LoadMapList[i].CityName == cityName)
                    {
                        TryRemoveMapData(_LoadMapList[i]);
                        return;
                    }
                }
            }
            //添加新数据
            AddNewCityMapData(grid, tileMap, cityName);
        }

        /// <summary>
        /// 偏移地图
        /// </summary>
        private void OffsetMap(CityMapSerialize mapInfo, int line, int row)
        {
            List<Vector2IntObj> gridDataList = new List<Vector2IntObj>();
            for (int i = 0; i < mapInfo.GridDataList.Count; i++)
            {
                Vector2IntObj obj = mapInfo.GridDataList[i];
                Vector2IntObj newObj = new Vector2IntObj();
                newObj.x = obj.x + line;
                newObj.y = obj.y + row;
                gridDataList.Add(newObj);
            }
            mapInfo.GridDataList = gridDataList;
            WriteJsonToFile(mapInfo.CityName, mapInfo);

            //重新加载下缓存
            UpdateMapListDataCache();
            //展示
            ShowTileMapInfo(mapInfo, false);
        }

        /// <summary>
        /// 展示地图信息
        /// </summary>
        /// <param name="mapInfo"></param>
        /// <param name="isReserve"></param>
        private void ShowTileMapInfo(CityMapSerialize mapInfo, bool isReserve)
        {
            if (ShowCityMap != null) {
                ShowCityMap.RemoveMap();
                ShowCityMap = null;
            }
            CityMap.Init(() => { return new BuildGrid(); }, () => { return new RunGrid(); });
            ShowCityMap = new CityMap(mapInfo, Vector3.one, isReserve, true);
            return;
        }

        /// <summary>
        /// 添加新的地图数据
        /// </summary>
        /// <param name="mapGame"></param>
        /// <param name="tileMap"></param>
        /// <param name="mapName"></param>
        /// <returns></returns>
        private CityMapSerialize AddNewCityMapData(Grid mapGame, Tilemap tileMap, string mapName)
        {
            //是否有重名
            for (int i = 0; i < _LoadMapList.Count; i++)
            {
                if (_LoadMapList[i].CityName == mapName)
                {
                    EditorUtility.DisplayDialog("提示", "已经有这个名字的地图资源了", "好的");
                    return null;
                }
            }
            //是否在本地有这个文件
            CityMapSerialize loadMap = LoadDataFrolFile(mapName);
            if (loadMap != null) {
                EditorUtility.DisplayDialog("提示", "有些问题,本地已经有这个资源了", "好的");
                return null;
            }

            //生成资源
            CityMapSerialize mapData = new CityMapSerialize();
            mapData.LoadData(mapGame, tileMap, mapName);
            WriteJsonToFile(mapName, mapData);

            //重新加载下缓存
            UpdateMapListDataCache();
            return mapData;
        }

        /// <summary>
        /// 尝试移除地图数据
        /// </summary>
        /// <returns></returns>
        private bool TryRemoveMapData(CityMapSerialize mapInfo) 
        {
            string filePath = GetFileFullPath(mapInfo.CityName);
            if (File.Exists(filePath)) {
                File.Delete(filePath);
                AssetDatabase.Refresh();
                //重新加载下缓存
                UpdateMapListDataCache();
                return true;
            }
            else
            {
                EditorUtility.DisplayDialog("提示", "删除失败,本地没这个资源了", "好的");
            }
            return false;
        }

        /// <summary>
        /// 将数据转换为Json格式
        /// </summary>
        private void MapDataToJson()
        {
            var str = JsonMapper.ToJson(_LoadMapList);

            string filePath = Directory.GetCurrentDirectory() + "\\" + "ShipTileMapData.txt";
            //删除之前的
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            //创建并写入
            File.Create(filePath).Dispose();
            File.WriteAllText(filePath, str);

            Debug.LogWarning("导出Json完成 \n"+ str);
        }


        /// <summary>
        /// 清除临时资源
        /// </summary>
        private void ClearRes()
        {
            //若在展示城市,就关闭
            if (ShowCityMap != null)
            {
                ShowCityMap.RemoveMap();
                ShowCityMap = null;
            }
        }

        public string GetFileFullPath(string fileName)
        {
            if (!fileName.Contains(_TielDataSuffix))
            {
                fileName = fileName + _TielDataSuffix;
            }
            return Application.dataPath + _UserTileDataPath + fileName;
        }

        /// <summary>
        /// 写入json字符串
        /// </summary>
        /// <param name="jsonName"></param>
        /// <param name="mapData"></param>
        private void WriteJsonToFile(string fileName, CityMapSerialize mapData)
        {
            string filePath = GetFileFullPath(fileName);
            //删除之前的
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            var str = JsonMapper.ToJson(mapData);
            //创建并写入
            File.Create(filePath).Dispose();
            File.WriteAllText(filePath, str);

            AssetDatabase.Refresh();
        }


        /// <summary>
        /// 从文件中加载数据
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private CityMapSerialize LoadDataFrolFile(string fileName)
        {
            string filePath = GetFileFullPath(fileName);
            if (!File.Exists(filePath))
            {
                return null;
            }
            string jsonData = File.ReadAllText(filePath);
            CityMapSerialize cityData = JsonMapper.ToObject<CityMapSerialize>(jsonData);
            return cityData;
        }
    }
}

#region 废弃代码
#if false

            //_DataPath = EditorGUILayout.TextField("Data Path : ", _DataPath, GUILayout.Width(450));
            //if (GUILayout.Button("设置资源存放地址", GUILayout.Width(position.width)))
            //{
            //    if (_DataPath != null && _DataPath != "")
            //    {
            //        CityGridData.TargetDataPath = _DataPath;
            //    } 
            //}

        /// <summary>
        /// 单个窗口的预览
        /// </summary>
        /// <param name="cell">格子的大小</param>
        /// <returns></returns>
        private Tilemap CreateCurTileMap(CityMapSerialize mapSerialize)
        {
            GameObject grid = GameObject.Find("Grid");
            if (grid)
            {
                DestroyImmediate(grid);   //如果场景中有 就删除
            }
            grid = new GameObject("Grid");
            Grid gridScript = grid.AddComponent<Grid>();
            gridScript.cellLayout = GridLayout.CellLayout.Isometric;
            gridScript.cellSize = mapSerialize.CellSize;

            for (int i = 0; i < grid.transform.childCount; i++)
            {
                DestroyImmediate(grid.transform.GetChild(i).gameObject);   // 清空之前的数据
            }

            GameObject tilemap = new GameObject("Tilemap");
            tilemap.transform.SetParent(grid.transform);
            Tilemap map = tilemap.AddComponent<Tilemap>();
            TilemapRenderer render = tilemap.AddComponent<TilemapRenderer>();
            //render.sortOrder = (TilemapRenderer.SortOrder)tilemapData[tileMaptoolbarOption].SortOrderIndex;
            //render.sortingOrder = tilemapData[tileMaptoolbarOption].OrderInLayer;

            if (mapSerialize!=null)
            {
                List<GridTileInfo> tilemapData = mapSerialize.MapData;

                gridScript.cellSize = mapSerialize.CellSize;

                if (tilemapData != null && CityGridData.TargetData.tile != null)
                {
                    foreach (var tile in tilemapData)
                    {
                        //Vector3Int iv3 = CityGridData.ShiftCell(tile.ipos, mapSerialize.Direction) ;
                        //map.SetTile(iv3 + (Vector3Int)mapSerialize.GridCore, CityGridData.TargetData.tile_centre);


                        map.SetTile((Vector3Int)tile.ipos, CityGridData.TargetData.tile_centre);
                    }
                }
            }
            return map;
        }

//GUILayout.BeginArea(new Rect(0, GridHeight, position.width, OldCreatePanel));

//            mapName = EditorGUILayout.TextField("Ship Name : ", mapName, GUILayout.Width(position.width));

//            CellSize = EditorGUILayout.Vector2Field("Cell Size : ", CellSize, GUILayout.Width(250));

//            Direction = EditorGUILayout.Popup(Direction, cityToward);


//            if (GUILayout.Button("添加", GUILayout.Width(350)))
//            {

//                if (mapName == "")
//                {
//                    if (EditorUtility.DisplayDialog("提示", "名称不能为空", "确定", "取消"))
//                    {
//                    }
//                    return;
//                }

//                for (int i = 0; i<CityGridData.TargetData.MapListData.Count; i++)
//                {
//                    if (CityGridData.TargetData.MapListData[i].cityName.Equals(mapName))
//                    {
//                        if (EditorUtility.DisplayDialog("提示", "已经有叫 " + mapName + " 的数据了", "确定", "取消"))
//                        {
//                        }
//                        return;
//                    }
//                }
//                CityMapSerialize serialize = CreateNewMap();
////serialize.Direction = (CityToward) Direction;

//tilemapData.Add(serialize);
//                EditorSceneManager.SaveOpenScenes();  // 主动保存场景
//                AssetDatabase.SaveAssets();
//            }


//            if (GUILayout.Button("删除", GUILayout.Width(350)))
//            {
//                tilemapData.RemoveAt(indexMap);
//            }

//            GUILayout.EndArea();
//            #endregion

        private void ShowTileMapGame(CityMapSerialize mapInfo, bool isReserve)
        {
            //获取父级
            if (_ShowParentTran == null)
            {
                GameObject parentGame = new GameObject(_ShowParentName);
                _ShowParentTran = parentGame.transform;
            }

            //隐藏正在展示的
            if (_ShowingGame != null) { _ShowingGame.SetActive(false); }

            //删除该地图之前的展示
            Transform oldTran = _ShowParentTran.Find(mapInfo.cityName);
            if (oldTran != null) { DestroyImmediate(oldTran.gameObject); }

            //展示新的物体
            GameObject grid = new GameObject(mapInfo.cityName);
            grid.transform.SetParent(_ShowParentTran);
            _ShowingGame = grid;
            Grid gridScript = grid.AddComponent<Grid>();
            gridScript.cellLayout = GridLayout.CellLayout.Isometric;
            gridScript.cellSize = mapInfo.CellSize;

            //设置子物体
            GameObject tilemap = new GameObject("Tilemap");
            tilemap.transform.SetParent(grid.transform);

            Tilemap map = tilemap.AddComponent<Tilemap>();
            TilemapRenderer render = tilemap.AddComponent<TilemapRenderer>();
            List<GridTileInfo> tilemapData = mapInfo.MapData;
            GameObject gameParent = new GameObject();
            gameParent.name = "GameParent";
            //hack【暂改】  CityGridData.TargetData.tile 与 CityGridData.TargetData.tile_centre 这两个没看懂
            if (tilemapData != null && CityGridData.TargetData.tile != null)
            {
                foreach (var tile in tilemapData)
                {
                    map.SetTile((Vector3Int)tile.ipos, CityGridData.TargetData.tile_centre);

                    Vector3 pos = map.CellToWorld((Vector3Int)tile.ipos);
                    GameObject game = new GameObject();
                    game.transform.position = pos;
                    game.transform.parent = gameParent.transform;
                }
            }

            if (isReserve)
            {
                Vector3 rotate = tilemap.transform.localEulerAngles;
                tilemap.transform.localPosition = new Vector3(rotate.x, rotate.y, rotate.z - 180);
            }
        }


        /// <summary>
        /// 刷新数据
        /// </summary>
        private void ComputeMapInformation(CityMapSerialize serialize)
        {

            Tilemap map = CreateCurTileMap(serialize);

            map.CellToWorld(new Vector3Int(0, 0, 0));

            Vector3 grid = map.CellToWorld(new Vector3Int(0, 0, 0));
            Vector3 nextLineGrid = map.CellToWorld(new Vector3Int(1, 0, 0));
            Vector3 nextRowGrid = map.CellToWorld(new Vector3Int(0, 1, 0));

            //求格子宽高
            serialize.GridHigh = Vector3.Distance(grid, nextLineGrid);
            serialize.GridWide = Vector3.Distance(grid, nextRowGrid);

            //求飞船旋转角度
            Vector2 dirVec = nextLineGrid - grid;
            serialize.RotaAngle = Vector2.Angle(dirVec, new Vector2(1, 0));
            //hack【暂改】【城建机制】这儿到时重新捋一下
            if (dirVec.x < 0 && dirVec.y < 0)
            {//第三象限角度为负
                serialize.RotaAngle = serialize.RotaAngle * -1;
            }

            //第一行函数k,b
            serialize.RowEquationK = Mathf.Tan(serialize.RotaAngle / 180 * Mathf.PI);
            //serialize.FirstRowEquationB = nextLineGrid.y - serialize.RowEquationK * nextLineGrid.x;
            //第一列函数k,b
            serialize.LineEquationK = Mathf.Tan((180 - serialize.RotaAngle) / 180 * Mathf.PI);
            //serialize.FirstLineEquationB = nextRowGrid.y - serialize.LineEquationK * nextRowGrid.x;

            // 小个子的偏移量
            serialize.SamllGridLine = (nextLineGrid- grid).normalized * (serialize.GridHigh/3);// 默认是分成3份
            // 小个子的偏移量
            serialize.SamllGridRow = (grid- nextRowGrid ).normalized * (serialize.GridWide/3);// 默认是分成3份


        //foreach (var item in serialize.MapData)
        //{
        //    Vector2Int vec = CityGridData.WorldToCell(map, item.pos, serialize.Direction) - serialize.GridCore;
        //    item.ipos = vec - serialize.GridCore;
        //}

    }



            GridId = GUILayout.SelectionGrid(GridId, new[] { "根据Tilemap生成界面", "地图修正" }, 2);
            PanelSwitch((PanelType)GridId);

            GUILayout.Box(new GUIContent(""), new[] { GUILayout.Height(200), GUILayout.Width(position.width) });

            //刷新全部数据
            if (GUILayout.Button("刷新", GUILayout.Width(position.width)))
            {
                RefreshData();
            }
            if (GUILayout.Button("导出Json", GUILayout.Width(position.width)))
            {
                MapDataToJson();
            }

#region 操作
            GUILayout.BeginArea(new Rect(0, GridHeight, position.width, SetUpPanel));

            if (GUILayout.Button("复制当前舱室", GUILayout.Width(200)))
            {
                //CityMapSerialize serialize = CityGridData.TargetData.MapListData[indexMap].Clone();
                //tilemapData.Add(serialize);
            }

            if (GUILayout.Button("翻转当前舱室", GUILayout.Width(200)))
            {
                CityMapSerialize city = tilemapData[indexMap];

                Tilemap _map = CreateCurTileMap(city);




                for (int i = 0; i < city.MapData.Count; i++)
                {
                    //Vector2Int v2 = new Vector2Int(-city.MapData[i].ipos.x, -city.MapData[i].ipos.y) - city.GridCore;
                    //city.MapData[i].ipos = v2;
                }
                //city.MapData[i].pos = CityGridData.CellToWorld(_map, v2, city.Direction);

                ComputeMapInformation(city);
                //CreateCurTileMap(city.CellSize , city.Direction); // 刷新显示
            }

            CenterGrid_1 = EditorGUILayout.Vector2Field("中心点  : ", CenterGrid_1, GUILayout.Width(250));
            CenterGrid.x = (int)CenterGrid_1.x;
            CenterGrid.y = (int)CenterGrid_1.y;


            if (GUILayout.Button("中心点刷新", GUILayout.Width(350)))
            {
                CityMapSerialize city = tilemapData[indexMap];
                //UpdateCenter(city);
                UpdateCenter_2(CenterGrid);
                CreateCurTileMap(city);
            }


            GUILayout.EndArea();

#endregion

#region 添加Tilemap到地图

            GUILayout.BeginArea(new Rect(0, GridHeight, position.width, TilemapCreatePanel));

            mapName = EditorGUILayout.TextField("地图名称 : ", mapName, GUILayout.Width(position.width));
            tilemapName = EditorGUILayout.TextField("tilemap名称 : ", tilemapName, GUILayout.Width(position.width));
            try
            {
                ScanningRadius = int.Parse(EditorGUILayout.TextField("tilemap大小 : ", ScanningRadius.ToString(), GUILayout.Width(position.width)));
            }
            catch (System.Exception)
            {
                ScanningRadius = 999;
            }

            //Direction = EditorGUILayout.Popup(Direction, cityToward);

            if (GUILayout.Button("添加场景中的地图", GUILayout.Width(position.width)))
            {
                if (mapName == "")
                {
                    if (EditorUtility.DisplayDialog("提示", "名称不能为空", "确定", "取消"))
                    {
                    }
                    return;
                }

                for (int i = 0; i < CityGridData.TargetData.MapListData.Count; i++)
                {
                    if (CityGridData.TargetData.MapListData[i].cityName.Equals(mapName))
                    {
                        if (EditorUtility.DisplayDialog("提示", "已经有叫 " + mapName + " 的数据了", "确定", "取消"))
                        {
                        }
                        return;
                    }
                }
                //CityMapSerialize serialize = CreateNewMap();
                //serialize.Direction = (CityToward) Direction;
                CityMapSerialize serialize = AddTilempaByScene(mapName, tilemapName);
                tilemapData.Add(serialize);
                EditorSceneManager.SaveOpenScenes();  // 主动保存场景
                AssetDatabase.SaveAssets();
                CreateCurTileMap(serialize);
            }

            GUILayout.Label("注释：tilemap名称  是在场景中物品名称，务必要保证没有重名");
            GUILayout.Label("注释：tilemap大小  扫描格子的半径");

            GUILayout.EndArea();

#endregion

#region 滑条 展示所有地图
            GUILayout.BeginArea(new Rect(0, 350, position.width, position.height));
            scrollPos = GUILayout.BeginScrollView(scrollPos, false, true, GUILayout.Height(300));

            if (tilemapData != null && tilemapData.Count != 0)
            {
                foreach (var item in tilemapData)
                {
                    if (GUILayout.Button(item.cityName, GUILayout.Width(200)))
                    {
                        indexMap = tilemapData.IndexOf(item);
                        mapName = item.cityName;
                        CityGridData.index = indexMap;
                        CellSize = item.CellSize;

                        // 刷新显示
                        CreateCurTileMap(item);
                    }
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
#endregion


        /// <summary>
        /// 通过场景中的物品获得一个Tilemap
        /// </summary>
        private CityMapSerialize AddTilempaByScene(string name , string gameObjectName)
        {
            Grid _grid = GameObject.Find(gameObjectName).GetComponent<Grid>();
            Tilemap _tilemap = _grid.transform.GetComponentInChildren<Tilemap>();

            List<GridTileInfo> MapData = new List<GridTileInfo>();


            Vector3 grid = _tilemap.CellToWorld(new Vector3Int(0, 0, 0));
            Vector3 nextLineGrid = _tilemap.CellToWorld(new Vector3Int(1, 0, 0));
            Vector3 nextRowGrid = _tilemap.CellToWorld(new Vector3Int(0, 1, 0));
            //求格子宽高
            float gridHigh = Vector3.Distance(grid, nextLineGrid);
            float gridWide = Vector3.Distance(grid, nextRowGrid);


            Vector3 gridComponentPos = _grid.transform.position;
            Quaternion gridComponentRotation = _grid.transform.rotation;
            Vector3 tilemapComponentLocalPosition = _tilemap.transform.localPosition;
            Quaternion tilempaRotation = _tilemap.transform.localRotation;
            Vector2Int ipos;
            for (int i = -ScanningRadius; i < ScanningRadius; i++)
            {
                for (int j = -ScanningRadius; j < ScanningRadius; j++)
                {
                    ipos = new Vector2Int(i, j);
                    TileBase tileBase = _tilemap.GetTile((Vector3Int)ipos);
                    if (tileBase != null)
                    {
                        GridTileInfo _data = new GridTileInfo
                        {
                            //tile的中心点为四个顶点的其中一个点，默认左下角
                            pos = _tilemap.GetCellCenterWorld((Vector3Int)ipos) + new Vector3(gridHigh * 0.5f, gridWide * 0.5f),
                            ipos = ipos
                        };
                        MapData.Add(_data);


                        Debug.LogWarning(MapData.Count + "   " + _data.pos + "   " + _tilemap.GetCellCenterWorld((Vector3Int)ipos) + "  " + gridHigh);
                    }
                }
            }





            CityMapSerialize citymap = new CityMapSerialize();
            citymap.cityName = name;
            citymap.CellSize = _tilemap.cellSize;
            citymap.MapData = MapData;

            ComputeMapInformation(citymap);
            return citymap;
        }


        /// <summary>
        /// 更新中心点
        /// </summary>
        /// <param name="serialize"></param>
        private void UpdateCenter(CityMapSerialize serialize)
        {


            serialize.GridCore = CenterGrid;


            Tilemap map = CreateCurTileMap(serialize);

            map.CellToWorld(new Vector3Int(0, 0, 0));

            Vector3 grid = map.CellToWorld(new Vector3Int(0, 0, 0));
            Vector3 nextLineGrid = map.CellToWorld(new Vector3Int(1, 0, 0));
            Vector3 nextRowGrid = map.CellToWorld(new Vector3Int(0, 1, 0));

            foreach (var item in serialize.MapData)
            {
                Vector2Int vec = CityGridData.WorldToCell(map, item.pos, serialize.Direction);
                Vector2Int sub = (vec + CenterGrid);

                Vector3 worldPos = CityGridData.CellToWorld(map, sub, serialize.Direction);
                item.ipos = (Vector2Int)sub;
                item.pos = worldPos;
            }

            CenterGrid = Vector2Int.zero;
        }


        /// <summary>
        /// Tilemap创建地形方式
        /// </summary>
        /// <returns></returns>
        private int TilemapCreatePanel = 150;
        /// <summary>
        /// 设置面板
        /// </summary>
        /// <returns></returns>
        private int SetUpPanel = 150;

        /// <summary>
        /// 保存读取
        /// </summary>
        /// <returns></returns>
        private int SeveLond = 150;


        /// <summary>
        /// 面板开关
        /// </summary>
        private void PanelSwitch(PanelType openPanel)
        {
            switch (openPanel)
            {
                case PanelType.TilemapCreatePanel:
                    TilemapCreatePanel = 150;
                    SetUpPanel = 0;
                    SeveLond = 0;
                    break;
                case PanelType.SetUpPanel:
                    TilemapCreatePanel = 0;
                    SetUpPanel = 150;
                    SeveLond = 0;
                    break;
                case PanelType.SeveLond:
                    TilemapCreatePanel = 0;
                    SetUpPanel = 0;
                    SeveLond = 150;
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        ///  页面类型
        /// </summary>
        public enum PanelType
        {
            TilemapCreatePanel = 0,
            SetUpPanel = 1,
            SeveLond = 2,
        }

#endif
#endregion