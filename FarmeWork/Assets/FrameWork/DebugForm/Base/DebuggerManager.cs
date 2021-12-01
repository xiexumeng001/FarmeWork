//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2020 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

namespace plugin.Debugger
{
    /// <summary>
    /// 调试器管理器。
    /// </summary>
    internal sealed partial class DebuggerManager : IDebuggerManager
    {
        private readonly DebuggerWindowGroup m_DebuggerWindowRoot;

        /// <summary>
        /// 初始化调试器管理器的新实例。
        /// </summary>
        public DebuggerManager()
        {
            m_DebuggerWindowRoot = new DebuggerWindowGroup();
        }

        /// <summary>
        /// 获取游戏框架模块优先级。
        /// </summary>
        /// <remarks>优先级较高的模块会优先轮询，并且关闭操作会后进行。</remarks>
        //hack【暂改】框架优先级
        //internal override int Priority
        //{
        //    get
        //    {
        //        return -1;
        //    }
        //}

        ///// <summary>
        ///// 获取或设置调试器窗口是否激活。
        ///// </summary>
        //是否激活窗口由组件类来管理,管理类不再管了,管理类能运行就代表着激活了
        //public bool ActiveWindow
        //{
        //    get
        //    {
        //        return m_ActiveWindow;
        //    }
        //    set
        //    {
        //        m_ActiveWindow = value;
        //    }
        //}

        /// <summary>
        /// 调试器窗口根结点。
        /// </summary>
        public IDebuggerWindowGroup DebuggerWindowRoot
        {
            get
            {
                return m_DebuggerWindowRoot;
            }
        }

        /// <summary>
        /// 调试器管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        void Update(float elapseSeconds, float realElapseSeconds)
        {
            m_DebuggerWindowRoot.OnUpdate(elapseSeconds, realElapseSeconds);
        }

        /// <summary>
        /// 关闭并清理调试器管理器。
        /// </summary>
        public void Shutdown()
        {
            m_DebuggerWindowRoot.Shutdown();
        }

        /// <summary>
        /// 注册调试器窗口。
        /// </summary>
        /// <param name="path">调试器窗口路径。</param>
        /// <param name="debuggerWindow">要注册的调试器窗口。</param>
        /// <param name="args">初始化调试器窗口参数。</param>
        public void RegisterDebuggerWindow(string path, IDebuggerWindow debuggerWindow, params object[] args)
        {
            //hack【暂改】抛出异常
            //if (string.IsNullOrEmpty(path))
            //{
            //    throw new GameFrameworkException("Path is invalid.");
            //}

            //if (debuggerWindow == null)
            //{
            //    throw new GameFrameworkException("Debugger window is invalid.");
            //}

            m_DebuggerWindowRoot.RegisterDebuggerWindow(path, debuggerWindow);
            debuggerWindow.Initialize(args);
        }

        /// <summary>
        /// 获取调试器窗口。
        /// </summary>
        /// <param name="path">调试器窗口路径。</param>
        /// <returns>要获取的调试器窗口。</returns>
        public IDebuggerWindow GetDebuggerWindow(string path)
        {
            return m_DebuggerWindowRoot.GetDebuggerWindow(path);
        }

        /// <summary>
        /// 选中调试器窗口。
        /// </summary>
        /// <param name="path">调试器窗口路径。</param>
        /// <returns>是否成功选中调试器窗口。</returns>
        public bool SelectDebuggerWindow(string path)
        {
            return m_DebuggerWindowRoot.SelectDebuggerWindow(path);
        }
    }
}
