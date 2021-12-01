using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System;

namespace ShipFarmeWork.Logic.CityMap
{
    /// <summary>
    /// 地图绘制类
    /// </summary>
    public class MapDrawer
    {
        RunGridDraw _RunGridDraw;
        BuildGridDraw _BuildGridDraw;

        public MapDrawer()
        {
            _BuildGridDraw = (BuildGridDraw)GridDrawBase.StartDrawSelf("BuildGridDraw", typeof(BuildGridDraw));
            _BuildGridDraw.gameObject.SetActive(false);

            _RunGridDraw = (RunGridDraw)GridDrawBase.StartDrawSelf("RunGridDraw", typeof(RunGridDraw));
            _RunGridDraw.gameObject.SetActive(false);
        }

        /// <summary>
        /// 添加基准线的辅助线
        /// </summary>
        /// <param name="k"></param>
        /// <param name="b"></param>
        public void AddStandardLine(float k, float b, int symbol)
        {
            float start_x = -2000;
            float end_x = 2800;
            float start_y = k * start_x + b;
            float end_y = k * end_x + b;
            Vector2 startPos = new Vector3(start_x, start_y);
            Vector2 endPos = new Vector3(end_x, end_y);
            //先设定为行走地图展示基准线
            _RunGridDraw.AddStandardLine(startPos, endPos, symbol);
        }

        /// <summary>
        /// 添加行走格子逻辑
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="lineLength"></param>
        /// <param name="angle"></param>
        public void AddDrawPos(RunGrid grid, float lineLength, float angle)
        {
            _RunGridDraw.AddGridShow(grid, lineLength, angle);
        }

        /// <summary>
        /// 添加建造格子逻辑
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="lineLength"></param>
        /// <param name="angle"></param>
        public void AddDrawPos(BuildGrid grid, float lineLength, float angle)
        {
            _BuildGridDraw.AddGridShow(grid, lineLength, angle);
        }

        /// <summary>
        /// 更新格子展示颜色
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="colorEnum"></param>
        public void UpdateDrawGridColor(BaseGrid grid, GridColorEnum colorEnum,bool isAdd)
        {
            if (grid is RunGrid)
            {
                _RunGridDraw.UpdateDrawGridColor(grid, colorEnum, isAdd);
            }
            else
            {
                _BuildGridDraw.UpdateDrawGridColor(grid, colorEnum, isAdd);
            }
        }

        /// <summary>
        /// 关闭,清除
        /// </summary>
        public void Close()
        {
            if (_RunGridDraw != null) _RunGridDraw.ClearDraw();
            if (_BuildGridDraw != null) _BuildGridDraw.ClearDraw();
        }
    }

    /// <summary>
    /// 格子绘制基础类
    /// </summary>
    public class GridDrawBase : MonoBehaviour
    {
        //所有的格子信息
        protected Dictionary<BaseGrid, DrawGridInfo> allGridInfo = new Dictionary<BaseGrid, DrawGridInfo>();
        protected Dictionary<int, StandardLine> allLine = new Dictionary<int, StandardLine>();

        public GameObject TextRes;
        public Transform TestShowParent;    //文本展示父级
        public Transform GridPosTran;       //标记Tran

        /// <summary>
        /// 开始绘制自己
        /// </summary>
        public static GridDrawBase StartDrawSelf(string showName, Type type)
        {

            //获取绘制类父节点
            string mapDrawName = "MapDrawer";
            GameObject mapDrawer = GameObject.Find(mapDrawName);
            if (mapDrawer == null)
            {
                mapDrawer = new GameObject(mapDrawName);
                //这个是因为 拓展的编译器也会去操作CityMap,但是在Editor下不能操作这个方法,所以需要判断下是否在播放游戏
                bool isDont = true;
#if UNITY_EDITOR
                isDont = UnityEditor.EditorApplication.isPlaying;
#endif
                if (isDont)
                {
                    GameObject.DontDestroyOnLoad(mapDrawer);
                }
            }

            //生成自己的绘制节点
            GameObject selfGame = new GameObject(showName);
            GridDrawBase gridDraw = (GridDrawBase)selfGame.AddComponent(type);
            selfGame.transform.parent = mapDrawer.transform;
            //生成标记节点
            GameObject gridPosGame = new GameObject("GridPosTran");
            gridPosGame.transform.parent = selfGame.transform;

            //添加公共的展示text文本节点
            GameObject textShowGame = new GameObject("ShowText");
            textShowGame.transform.parent = selfGame.transform;
            Canvas canvas = textShowGame.AddComponent<Canvas>();
            //canvas.sortingLayerName = ShipVar.UILayer;
            textShowGame.AddComponent<CanvasScaler>();
            textShowGame.AddComponent<GraphicRaycaster>();

            gridDraw.TestShowParent = textShowGame.transform;
            gridDraw.TextRes = (GameObject)Resources.Load("GridShowText");
            gridDraw.GridPosTran = gridPosGame.transform;

            return gridDraw;
        }

        /// <summary>
        /// 获取绘制颜色
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="gridGridInfo"></param>
        /// <returns></returns>
        public virtual Color GetDrawColor(BaseGrid grid, DrawGridInfo gridGridInfo)
        {
            return Color.white;
        }

        /// <summary>
        /// 绘制
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!gameObject.activeSelf) return;

            //格子的绘制
            foreach (KeyValuePair<BaseGrid, DrawGridInfo> kvs in allGridInfo)
            {
                BaseGrid grid = kvs.Key;
                DrawGridInfo drawGridInfo = kvs.Value;

                Gizmos.color = GetDrawColor(grid, drawGridInfo);
                drawGridInfo.Peak.Draw();
            }

            //线的绘制
            foreach (KeyValuePair<int, StandardLine> kps in allLine)
            {
                StandardLine line = kps.Value;
                Gizmos.color = Color.red;
                Gizmos.DrawLine(line.StartPos, line.EndPos);
            }
        }

        /// <summary>
        /// 添加格子
        /// </summary>
        public void AddGridShow(BaseGrid grid, float lineLength, float angle)
        {
            angle += 10;

            //获取对应的展示信息
            DrawGridInfo gridInfo;
            if (allGridInfo.TryGetValue(grid, out gridInfo))
            {

            }
            else
            {
                gridInfo = new DrawGridInfo();
                gridInfo.Peak = new PeakPos();
                GameObject game = GameObject.Instantiate(TextRes, TestShowParent);
                gridInfo.ShowText = game.GetComponent<Text>();
                allGridInfo.Add(grid, gridInfo);
            }

            //更新范围与行列展示
            gridInfo.Peak.Init(grid.WorldPosition, lineLength, angle);
            gridInfo.ShowText.transform.position = grid.WorldPosition;
            gridInfo.ShowText.text = string.Format("({0},{1})", grid.Line, grid.Row);
        }

        /// <summary>
        /// 添加基准线的辅助线
        /// </summary>
        /// <param name="k"></param>
        /// <param name="b"></param>
        public void AddStandardLine(Vector3 startPos, Vector3 endPos, int symbol)
        {
            StandardLine line;
            if (allLine.TryGetValue(symbol, out line))
            {

            }
            else
            {
                line = new StandardLine();
                allLine.Add(symbol, line);
            }
            line.Init(startPos, endPos);
        }

        /// <summary>
        /// 更新格子展示颜色
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="colorEnum"></param>
        public void UpdateDrawGridColor(BaseGrid grid, GridColorEnum colorEnum, bool isAdd)
        {
            if (allGridInfo.ContainsKey(grid))
            {
                DrawGridInfo gridInfo = allGridInfo[grid];
                gridInfo.Peak.UpdateColor(colorEnum, isAdd);
            }
        }

        /// <summary>
        /// 关闭,清除
        /// </summary>
        public void ClearDraw()
        {
            foreach (var itme in allGridInfo)
            {
                DestroyImmediate(itme.Value.ShowText.gameObject);
            }
            allGridInfo.Clear();

            DestroyImmediate(gameObject);
        }
    }

    /// <summary>
    /// 运转格子绘制
    /// </summary>
    public class RunGridDraw : GridDrawBase
    {

        public override Color GetDrawColor(BaseGrid grid, DrawGridInfo gridGridInfo)
        {
            Color color = Color.white;

            RunGrid runGrid = (RunGrid)grid;
            bool isChoose = false;
            if (Vector2.Distance(runGrid.WorldPosition, GridPosTran.position) < 100)
            {
                RunGrid gloabGrid = runGrid.Map.GetRunGridByPos(GridPosTran.position);
                isChoose = (gloabGrid == runGrid);
            }
            if (isChoose)
            {
                color = Color.red;
            }
            else { color = gridGridInfo.Peak.GetColor(); }
            return color;
        }
    }

    /// <summary>
    /// 建造格子绘制
    /// </summary>
    public class BuildGridDraw:GridDrawBase
    {
        public override Color GetDrawColor(BaseGrid grid, DrawGridInfo gridGridInfo)
        {
            Color color = Color.white;

            BuildGrid buildGrid = (BuildGrid)grid;
            bool isChoose = false;
            if (Vector2.Distance(buildGrid.WorldPosition, GridPosTran.position) < 100)
            {
                BuildGrid gloabGrid = buildGrid.Map.GetBuildGridByPos(GridPosTran.position);
                isChoose = (gloabGrid == buildGrid);
            }

            if (isChoose)
            {
                color = Color.red;
            }
            else { color = gridGridInfo.Peak.GetColor(); }

            return color;
        }
    }


    /// <summary>
    /// 展示格子信息
    /// </summary>
    public struct DrawGridInfo
    {
        public PeakPos Peak;   //展示的顶点等
        public Text ShowText;  //展示的文本等
    }

    /// <summary>
    /// 顶点位置类
    /// </summary>
    public class PeakPos
    {
        public Vector3 UpPos;
        public Vector3 RightPos;
        public Vector3 DownPos;
        public Vector3 LeftPos;

        public List<GridColorEnum> ColorList = new List<GridColorEnum>();

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init(Vector3 pos, float lineLength, float angle)
        {
            float length = lineLength * Mathf.Cos(angle / 180 * Mathf.PI);

            UpPos = new Vector3(pos.x, pos.y + length / 2);
            RightPos = new Vector3(pos.x + length, pos.y);
            DownPos = new Vector3(pos.x, pos.y - length / 2);
            LeftPos = new Vector3(pos.x - length, pos.y);
        }

        public void UpdateColor(GridColorEnum color, bool isAdd)
        {
            if (isAdd)
            {
                ColorList.Add(color);
            }
            else
            {
                ColorList.Remove(color);
            }
        }

        /// <summary>
        /// 获取颜色
        /// </summary>
        /// <returns></returns>
        public Color GetColor()
        {
            GridColorEnum showColor = GridColorEnum.Default;
            //获取权重最大的类型
            for (int i = 0; i < ColorList.Count; i++)
            {
                if (ColorList[i] > showColor)
                {
                    showColor = ColorList[i];
                }
            }

            Color color = Color.white;
            switch (showColor)
            {
                case GridColorEnum.BeOccu:
                    color = Color.red;
                    break;
                case GridColorEnum.Door:
                    color = Color.yellow;
                    break;
                case GridColorEnum.SearchStart:
                    color = Color.green;
                    break;
                case GridColorEnum.SearchEnd:
                    color = Color.green;
                    break;
                case GridColorEnum.CanRun:
                    color = Color.blue;
                    break;
                case GridColorEnum.Walk:
                    color = Color.black;
                    break;
            }
            return color;
        }

        /// <summary>
        /// 绘制
        /// </summary>
        public void Draw()
        {
            Gizmos.DrawLine(UpPos, RightPos);
            Gizmos.DrawLine(RightPos, DownPos);
            Gizmos.DrawLine(DownPos, LeftPos);
            Gizmos.DrawLine(LeftPos, UpPos);
        }
    }

    /// <summary>
    /// 一元一次方程基准线的绘制类
    /// </summary>
    public class StandardLine
    {
        public Vector2 StartPos;   //开始点
        public Vector2 EndPos;     //结束点

        public void Init(Vector2 startPos, Vector2 endPos)
        {
            StartPos = startPos;
            EndPos = endPos;
        }
    }

    /// <summary>
    /// 格子颜色枚举(值代表展示优先级)
    /// </summary>
    public enum GridColorEnum
    {
        Default = 1,     //默认颜色
        BeOccu = 2,      //被占用格子
        CanRun = 3,      //可行走的格子
        Door = 4,        //门的格子
        SearchStart = 5, //寻路起点
        SearchEnd = 6,   //寻路终点
        Walk = 7,        //行走颜色
    }

}
