//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2020 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace plugin.Debugger
{
    /// <summary>
    /// 调试器组件。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed partial class DebuggerComponent : MonoBehaviour
    {
        /// <summary>
        /// 默认调试器漂浮框大小。
        /// </summary>
        internal static readonly Rect DefaultIconRect = new Rect(10f, 10f, 60f, 60f);

        /// <summary>
        /// 默认调试器窗口大小。
        /// </summary>
        internal static readonly Rect DefaultWindowRect = new Rect(10f, 10f, 640f, 480f);

        /// <summary>
        /// 默认调试器窗口缩放比例。
        /// </summary>
        internal static readonly float DefaultWindowScale = 1f;

        /// <summary>
        /// 默认的屏幕分辨率
        /// </summary>
        public static float desiginWidth = 1280f;
        public static float desiginHeight = 720f;

        private DebuggerManager m_DebuggerManager = null;
        private Rect m_DragRect = new Rect(0f, 0f, float.MaxValue, 25f);
        private Rect m_IconRect = DefaultIconRect;
        private Rect m_WindowRect = DefaultWindowRect;
        private float m_WindowScale = DefaultWindowScale;
        private bool m_ActiveWindow;
        private FpsCounter m_FpsCounter = null;


        [SerializeField]
        private GUISkin m_Skin = null;

        [SerializeField]
        private DebuggerActiveWindowType m_ActiveWindowType = DebuggerActiveWindowType.AlwaysOpen;

        [SerializeField]
        private bool m_ShowFullWindow = false;

        [SerializeField]
        private ConsoleWindow m_ConsoleWindow = new ConsoleWindow();

        /// <summary>
        /// 获取或设置调试器窗口是否激活。
        /// </summary>
        public bool ActiveWindow { get { return m_ActiveWindow; } }

        /// <summary>
        /// 获取或设置是否显示完整调试器界面。
        /// </summary>
        public bool ShowFullWindow { get { return m_ShowFullWindow; } }

        /// <summary>
        /// 获取或设置调试器漂浮框大小。
        /// </summary>
        public Rect IconRect { get { return m_IconRect; } }

        /// <summary>
        /// 获取或设置调试器窗口大小。
        /// </summary>
        public Rect WindowRect { get { return m_WindowRect; } }

        /// <summary>
        /// 获取或设置调试器窗口缩放比例。
        /// </summary>
        public float WindowScale { get { return m_WindowScale; } }

        /// <summary>
        /// 游戏框架组件初始化。
        /// </summary>
        void Awake()
        {
            //hack【暂改】 获取Debug管理器
            m_DebuggerManager = new DebuggerManager();
            //hack【暂改】这儿应该还带一个debug变量整体调控,也调控其它的一些调试的地方
            switch (m_ActiveWindowType)
            {
                case DebuggerActiveWindowType.AlwaysOpen:
                    m_ActiveWindow = true;
                    break;

                case DebuggerActiveWindowType.OnlyOpenWhenDevelopment:
                    m_ActiveWindow = Debug.isDebugBuild;
                    break;

                case DebuggerActiveWindowType.OnlyOpenInEditor:
                    m_ActiveWindow = Application.isEditor;
                    break;

                default:
                    m_ActiveWindow = false;
                    break;
            }

            //FPS的计算
            m_FpsCounter = new FpsCounter(0.5f);


            //按照宽度比来拉伸调试框
            float widthRate = Screen.width / desiginWidth;
            float hightRate = Screen.height / desiginHeight;
            m_WindowScale = widthRate;
        }

        private void Start()
        {
            //注册各种窗口
            RegisterDebuggerWindow("Console", m_ConsoleWindow);
        }

        private void Update()
        {
            //FPS的更新
            m_FpsCounter.Update(Time.deltaTime, Time.unscaledDeltaTime);
        }

        private void OnGUI()
        {
            //若不展示,则关闭
            if (!ActiveWindow) { return; }

            //开始显示
            GUISkin cachedGuiSkin = GUI.skin;
            Matrix4x4 cachedMatrix = GUI.matrix;

            GUI.skin = m_Skin;
            GUI.matrix = Matrix4x4.Scale(new Vector3(m_WindowScale, m_WindowScale, 1f));

            if (m_ShowFullWindow)
            {
                m_WindowRect = GUILayout.Window(0, m_WindowRect, DrawWindow, "<b>DEBUGGER</b>");
            }
            else
            {
                m_IconRect = GUILayout.Window(0, m_IconRect, DrawDebuggerWindowIcon, "<b>DEBUGGER</b>");
            }

            GUI.matrix = cachedMatrix;
            GUI.skin = cachedGuiSkin;
        }

        private void OnDestroy()
        {
            m_DebuggerManager.Shutdown();
        }

        /// <summary>
        /// 注册调试器窗口。
        /// </summary>
        /// <param name="path">调试器窗口路径。</param>
        /// <param name="debuggerWindow">要注册的调试器窗口。</param>
        /// <param name="args">初始化调试器窗口参数。</param>
        public void RegisterDebuggerWindow(string path, IDebuggerWindow debuggerWindow, params object[] args)
        {
            m_DebuggerManager.RegisterDebuggerWindow(path, debuggerWindow, args);
        }

        /// <summary>
        /// 获取调试器窗口。
        /// </summary>
        /// <param name="path">调试器窗口路径。</param>
        /// <returns>要获取的调试器窗口。</returns>
        public IDebuggerWindow GetDebuggerWindow(string path)
        {
            return m_DebuggerManager.GetDebuggerWindow(path);
        }

        /// <summary>
        /// 选中调试器窗口。
        /// </summary>
        /// <param name="path">调试器窗口路径。</param>
        /// <returns>是否成功选中调试器窗口。</returns>
        public bool SelectDebuggerWindow(string path)
        {
            return m_DebuggerManager.SelectDebuggerWindow(path);
        }

        /// <summary>
        /// 还原调试器窗口布局。
        /// </summary>
        public void ResetLayout()
        {
            m_IconRect = DefaultIconRect;
            m_WindowRect = DefaultWindowRect;
            m_WindowScale = DefaultWindowScale;
        }

        private void DrawWindow(int windowId)
        {
            //添加拖拽功能
            GUI.DragWindow(m_DragRect);
            DrawDebuggerWindowGroup(m_DebuggerManager.DebuggerWindowRoot);
        }

        /// <summary>
        /// 主要是绘制窗口,并且对跟窗口下的各个子窗口进行绘制
        /// </summary>
        /// <param name="debuggerWindowGroup"></param>
        private void DrawDebuggerWindowGroup(IDebuggerWindowGroup debuggerWindowGroup)
        {
            if (debuggerWindowGroup == null)
            {
                return;
            }

            List<string> names = new List<string>();
            string[] debuggerWindowNames = debuggerWindowGroup.GetDebuggerWindowNames();
            for (int i = 0; i < debuggerWindowNames.Length; i++)
            {
                names.Add(string.Format("<b>{0}</b>", debuggerWindowNames[i]));
            }

            if (debuggerWindowGroup == m_DebuggerManager.DebuggerWindowRoot)
            {
                names.Add("<b>Close</b>");
            }
            //参数 1， 选中的项， 参数2 每项的图片， 参数3 整个Toolbar的宽度， 参数4 Toolbar的高度  
            //返回选中的项
            int toolbarIndex = GUILayout.Toolbar(debuggerWindowGroup.SelectedIndex, names.ToArray(), GUILayout.Height(30f), GUILayout.MaxWidth(Screen.width));
            if (toolbarIndex >= debuggerWindowGroup.DebuggerWindowCount)
            {
                m_ShowFullWindow = false;
                return;
            }

            if (debuggerWindowGroup.SelectedWindow == null)
            {
                return;
            }

            if (debuggerWindowGroup.SelectedIndex != toolbarIndex)
            {
                debuggerWindowGroup.SelectedWindow.OnLeave();
                debuggerWindowGroup.SelectedIndex = toolbarIndex;
                debuggerWindowGroup.SelectedWindow.OnEnter();
            }

            IDebuggerWindowGroup subDebuggerWindowGroup = debuggerWindowGroup.SelectedWindow as IDebuggerWindowGroup;
            if (subDebuggerWindowGroup != null)
            {
                DrawDebuggerWindowGroup(subDebuggerWindowGroup);
            }

            debuggerWindowGroup.SelectedWindow.OnDraw();
        }

        private void DrawDebuggerWindowIcon(int windowId)
        {
            GUI.DragWindow(m_DragRect);
            //插入一段空白格
            GUILayout.Space(5);
            Color32 color = Color.white;
            m_ConsoleWindow.RefreshCount();
            if (m_ConsoleWindow.FatalCount > 0)
            {
                color = m_ConsoleWindow.GetLogStringColor(LogType.Exception);
            }
            else if (m_ConsoleWindow.ErrorCount > 0)
            {
                color = m_ConsoleWindow.GetLogStringColor(LogType.Error);
            }
            else if (m_ConsoleWindow.WarningCount > 0)
            {
                color = m_ConsoleWindow.GetLogStringColor(LogType.Warning);
            }
            else
            {
                color = m_ConsoleWindow.GetLogStringColor(LogType.Log);
            }

            string title = string.Format("<color=#{0}{1}{2}{3}><b>FPS: {4}</b></color>", color.r.ToString("x2"), color.g.ToString("x2"), color.b.ToString("x2"), color.a.ToString("x2"), m_FpsCounter.CurrentFps.ToString("F2"));
            if (GUILayout.Button(title, GUILayout.Width(100f), GUILayout.Height(40f)))
            {
                m_ShowFullWindow = true;
            }
        }

        /// <summary>
        /// 获取记录的全部日志。
        /// </summary>
        /// <param name="results">要获取的日志。</param>
        public void GetRecentLogs(List<LogNode> results)
        {
            m_ConsoleWindow.GetRecentLogs(results);
        }

        /// <summary>
        /// 获取记录的最近日志。
        /// </summary>
        /// <param name="results">要获取的日志。</param>
        /// <param name="count">要获取最近日志的数量。</param>
        public void GetRecentLogs(List<LogNode> results, int count)
        {
            m_ConsoleWindow.GetRecentLogs(results, count);
        }

        /// <summary>
        /// 帧率计算类
        /// </summary>
        private sealed class FpsCounter
        {
            private float m_UpdateInterval;
            private float m_CurrentFps;
            private int m_Frames;
            private float m_Accumulator;
            private float m_TimeLeft;

            public FpsCounter(float updateInterval)
            {
                if (updateInterval <= 0f)
                {
                    Debug.LogError("Update interval is invalid.");
                    return;
                }

                m_UpdateInterval = updateInterval;
                Reset();
            }

            public float UpdateInterval
            {
                get
                {
                    return m_UpdateInterval;
                }
                set
                {
                    if (value <= 0f)
                    {
                        Debug.LogError("Update interval is invalid.");
                        return;
                    }

                    m_UpdateInterval = value;
                    Reset();
                }
            }

            public float CurrentFps
            {
                get
                {
                    return m_CurrentFps;
                }
            }

            public void Update(float elapseSeconds, float realElapseSeconds)
            {
                m_Frames++;
                m_Accumulator += realElapseSeconds;
                m_TimeLeft -= realElapseSeconds;

                if (m_TimeLeft <= 0f)
                {
                    m_CurrentFps = m_Accumulator > 0f ? m_Frames / m_Accumulator : 0f;
                    m_Frames = 0;
                    m_Accumulator = 0f;
                    m_TimeLeft += m_UpdateInterval;
                }
            }

            private void Reset()
            {
                m_CurrentFps = 0f;
                m_Frames = 0;
                m_Accumulator = 0f;
                m_TimeLeft = 0f;
            }
        }
    }

}
