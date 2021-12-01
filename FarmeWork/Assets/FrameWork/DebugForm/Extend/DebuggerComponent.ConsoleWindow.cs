//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2020 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

namespace plugin.Debugger
{
    public sealed partial class DebuggerComponent
    {
        [Serializable]
        private sealed class ConsoleWindow : IDebuggerWindow
        {
            //输出信息队列
            private readonly Queue<LogNode> m_LogNodes = new Queue<LogNode>();
            //文本组件,用来做复制用的
            private readonly TextEditor m_TextEditor = new TextEditor();
            //输出池
            private List<LogNode> m_LogPool = new List<LogNode>();

            private Vector2 m_LogScrollPosition = Vector2.zero;
            private Vector2 m_StackScrollPosition = Vector2.zero;
            private int m_InfoCount = 0;
            private int m_WarningCount = 0;
            private int m_ErrorCount = 0;
            private int m_FatalCount = 0;
            private LogNode m_SelectedNode = null;

            [SerializeField]
            private bool m_LockScroll = true;

            [Tooltip("最大可展示的输出行数")]
            [SerializeField]
            private int m_MaxLine = 300;

            //-----各类型输出的开关------------------
            [SerializeField]
            private bool m_InfoFilter = true;

            [SerializeField]
            private bool m_WarningFilter = true;

            [SerializeField]
            private bool m_ErrorFilter = true;

            [SerializeField]
            private bool m_FatalFilter = true;
            //-----各类型输出的开关------------------

            //-----各类型输出的颜色------------------
            [SerializeField]
            private Color32 m_InfoColor = Color.white;

            [SerializeField]
            private Color32 m_WarningColor = Color.yellow;

            [SerializeField]
            private Color32 m_ErrorColor = Color.red;

            [SerializeField]
            private Color32 m_FatalColor = new Color(0.7f, 0.2f, 0.2f);
            //-----各类型输出的颜色------------------

            public int InfoCount { get { return m_InfoCount; } }

            public int WarningCount { get { return m_WarningCount; } }

            public int ErrorCount { get { return m_ErrorCount; } }

            public int FatalCount { get { return m_FatalCount; } }

            public Color32 InfoColor { get { return m_InfoColor; } }

            public Color32 WarningColor { get { return m_WarningColor; } }

            public Color32 ErrorColor { get { return m_ErrorColor; } }

            public Color32 FatalColor { get { return m_FatalColor; } }

            public void Initialize(params object[] args)
            {
                Application.logMessageReceived += OnLogMessageReceived;
            }

            public void Shutdown()
            {
                Application.logMessageReceived -= OnLogMessageReceived;
                Clear();
            }

            public void OnEnter()
            {
            }

            public void OnLeave()
            {
            }

            public void OnUpdate(float elapseSeconds, float realElapseSeconds)
            {
            }

            public void OnDraw()
            {
                RefreshCount();

                //展示各个开关
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Clear All", GUILayout.Width(100f)))
                    {
                        Clear();
                    }
                    m_LockScroll = GUILayout.Toggle(m_LockScroll, "Lock Scroll", GUILayout.Width(90f));
                    GUILayout.FlexibleSpace();
                    m_InfoFilter = GUILayout.Toggle(m_InfoFilter, string.Format("Info ({0})", m_InfoCount.ToString()), GUILayout.Width(90f));
                    m_WarningFilter = GUILayout.Toggle(m_WarningFilter, string.Format("Warning ({0})", m_WarningCount.ToString()), GUILayout.Width(90f));
                    m_ErrorFilter = GUILayout.Toggle(m_ErrorFilter, string.Format("Error ({0})", m_ErrorCount.ToString()), GUILayout.Width(90f));
                    m_FatalFilter = GUILayout.Toggle(m_FatalFilter, string.Format("Fatal ({0})", m_FatalCount.ToString()), GUILayout.Width(90f));
                }
                GUILayout.EndHorizontal();

                //展示输出信息
                GUILayout.BeginVertical("box");
                {
                    if (m_LockScroll)
                    {
                        m_LogScrollPosition.y = float.MaxValue;
                    }

                    m_LogScrollPosition = GUILayout.BeginScrollView(m_LogScrollPosition);
                    {
                        bool selected = false;
                        foreach (LogNode logNode in m_LogNodes)
                        {
                            switch (logNode.LogType)
                            {
                                case LogType.Log:
                                    if (!m_InfoFilter) { continue; }
                                    break;

                                case LogType.Warning:
                                    if (!m_WarningFilter) { continue; }
                                    break;

                                case LogType.Error:
                                    if (!m_ErrorFilter) { continue; }
                                    break;

                                case LogType.Exception:
                                    if (!m_FatalFilter) { continue; }
                                    break;
                            }
                            if (GUILayout.Toggle(m_SelectedNode == logNode, GetLogString(logNode)))
                            {
                                selected = true;
                                if (m_SelectedNode != logNode)
                                {
                                    m_SelectedNode = logNode;
                                    m_StackScrollPosition = Vector2.zero;
                                }
                            }
                        }
                        if (!selected) { m_SelectedNode = null; }
                    }
                    GUILayout.EndScrollView();
                }
                GUILayout.EndVertical();

                //展示输出详细堆栈信息盒子
                GUILayout.BeginVertical("box");
                {
                    m_StackScrollPosition = GUILayout.BeginScrollView(m_StackScrollPosition, GUILayout.Height(100f));
                    {
                        if (m_SelectedNode != null)
                        {
                            GUILayout.BeginHorizontal();
                            Color32 color = GetLogStringColor(m_SelectedNode.LogType);
                            GUILayout.Label(string.Format("<color=#{0}{1}{2}{3}><b>{4}</b></color>", color.r.ToString("x2"), color.g.ToString("x2"), color.b.ToString("x2"), color.a.ToString("x2"), m_SelectedNode.LogMessage));
                            if (GUILayout.Button("COPY", GUILayout.Width(60f), GUILayout.Height(30f)))
                            {
                                m_TextEditor.text = string.Format("{0}{2}{2}{1}", m_SelectedNode.LogMessage, m_SelectedNode.StackTrack, Environment.NewLine);
                                m_TextEditor.OnFocus();
                                m_TextEditor.Copy();
                                m_TextEditor.text = null;
                            }
                            GUILayout.EndHorizontal();
                            if (m_SelectedNode.StackTrack == null || m_SelectedNode.StackTrack.Length == 0)
                            {
                                GUILayout.Label("null");
                            }
                            else {
                                GUILayout.Label(m_SelectedNode.StackTrack);
                            }
                        }
                        GUILayout.EndScrollView();
                    }
                }
                GUILayout.EndVertical();
            }

            private void Clear()
            {
                m_LogNodes.Clear();
            }

            /// <summary>
            /// 刷新输出信息的数量计算
            /// </summary>
            public void RefreshCount()
            {
                m_InfoCount = 0;
                m_WarningCount = 0;
                m_ErrorCount = 0;
                m_FatalCount = 0;
                foreach (LogNode logNode in m_LogNodes)
                {
                    switch (logNode.LogType)
                    {
                        case LogType.Log:
                            m_InfoCount++;
                            break;

                        case LogType.Warning:
                            m_WarningCount++;
                            break;

                        case LogType.Error:
                            m_ErrorCount++;
                            break;

                        case LogType.Exception:
                            m_FatalCount++;
                            break;
                    }
                }
            }

            public void GetRecentLogs(List<LogNode> results)
            {
                if (results == null)
                {
                    Debug.LogError("Results is invalid.");
                    return;
                }

                results.Clear();
                foreach (LogNode logNode in m_LogNodes)
                {
                    results.Add(logNode);
                }
            }

            public void GetRecentLogs(List<LogNode> results, int count)
            {
                if (results == null)
                {
                    Debug.LogError("Results is invalid.");
                    return;
                }

                if (count <= 0)
                {
                    Debug.LogError("Count is invalid.");
                    return;
                }

                int position = m_LogNodes.Count - count;
                if (position < 0)
                {
                    position = 0;
                }

                int index = 0;
                results.Clear();
                foreach (LogNode logNode in m_LogNodes)
                {
                    if (index++ < position)
                    {
                        continue;
                    }

                    results.Add(logNode);
                }
            }

            /// <summary>
            /// 当输出的回调
            /// </summary>
            /// <param name="logMessage"></param>
            /// <param name="stackTrace"></param>
            /// <param name="logType"></param>
            private void OnLogMessageReceived(string logMessage, string stackTrace, LogType logType)
            {
                if (logType == LogType.Assert)
                {
                    logType = LogType.Error;
                }

                m_LogNodes.Enqueue(GetNewLogNode(logType, logMessage, stackTrace));
                while (m_LogNodes.Count > m_MaxLine)
                {
                    LogNode node = m_LogNodes.Dequeue();
                    node.Clear();
                    m_LogPool.Add(node);
                }
            }

            /// <summary>
            /// 获取新的日志记录节点
            /// </summary>
            /// <param name="logType"></param>
            /// <param name="logMessage"></param>
            /// <param name="stackTrack"></param>
            /// <returns></returns>
            private LogNode GetNewLogNode(LogType logType, string logMessage, string stackTrack) {
                LogNode node = null;
                if (m_LogPool.Count > 0)
                {
                    int index = m_LogPool.Count - 1;
                    node = m_LogPool[index];
                    m_LogPool.RemoveAt(index);
                }
                else {
                    node = new LogNode();
                }
                node.Init(logType, logMessage, stackTrack);
                return node;
            }

            private string GetLogString(LogNode logNode)
            {
                Color32 color = GetLogStringColor(logNode.LogType);
                return string.Format("<color=#{0}{1}{2}{3}>[{4}][{5}] {6}</color>",
                    color.r.ToString("x2"), color.g.ToString("x2"), color.b.ToString("x2"), color.a.ToString("x2"),
                    logNode.LogTime.ToString("HH:mm:ss.fff"), logNode.LogFrameCount.ToString(), logNode.LogMessage);
            }

            /// <summary>
            /// 获取输出类型的颜色
            /// </summary>
            /// <param name="logType"></param>
            /// <returns></returns>
            internal Color32 GetLogStringColor(LogType logType)
            {
                Color32 color = Color.white;
                switch (logType)
                {
                    case LogType.Log:
                        color = m_InfoColor;
                        break;

                    case LogType.Warning:
                        color = m_WarningColor;
                        break;

                    case LogType.Error:
                        color = m_ErrorColor;
                        break;

                    case LogType.Exception:
                        color = m_FatalColor;
                        break;
                }

                return color;
            }
        }


        /// <summary>
        /// 日志记录结点。
        /// </summary>
        public sealed class LogNode
        {
            private DateTime m_LogTime;
            private int m_LogFrameCount;
            private LogType m_LogType;
            private string m_LogMessage;
            private string m_StackTrack;

            /// <summary>
            /// 初始化日志记录结点的新实例。
            /// </summary>
            public LogNode()
            {
                m_LogTime = default(DateTime);
                m_LogFrameCount = 0;
                m_LogType = LogType.Error;
                m_LogMessage = null;
                m_StackTrack = null;
            }

            /// <summary>
            /// 获取日志时间。
            /// </summary>
            public DateTime LogTime
            {
                get
                {
                    return m_LogTime;
                }
            }

            /// <summary>
            /// 获取日志帧计数。
            /// </summary>
            public int LogFrameCount
            {
                get
                {
                    return m_LogFrameCount;
                }
            }

            /// <summary>
            /// 获取日志类型。
            /// </summary>
            public LogType LogType
            {
                get
                {
                    return m_LogType;
                }
            }

            /// <summary>
            /// 获取日志内容。
            /// </summary>
            public string LogMessage
            {
                get
                {
                    return m_LogMessage;
                }
            }

            /// <summary>
            /// 获取日志堆栈信息。
            /// </summary>
            public string StackTrack
            {
                get
                {
                    return m_StackTrack;
                }
            }


            /// <summary>
            /// 初始化日志记录结点。
            /// </summary>
            /// <param name="logType">日志类型。</param>
            /// <param name="logMessage">日志内容。</param>
            /// <param name="stackTrack">日志堆栈信息。</param>
            /// <returns>创建的日志记录结点。</returns>
            public void Init(LogType logType, string logMessage, string stackTrack) {
                m_LogTime = DateTime.Now;
                m_LogFrameCount = Time.frameCount;
                m_LogType = logType;
                m_LogMessage = logMessage;
                m_StackTrack = stackTrack;
            }

            /// <summary>
            /// 清理日志记录结点。
            /// </summary>
            public void Clear()
            {
                m_LogTime = default(DateTime);
                m_LogFrameCount = 0;
                m_LogType = LogType.Error;
                m_LogMessage = null;
                m_StackTrack = null;
            }
        }
    }
}
