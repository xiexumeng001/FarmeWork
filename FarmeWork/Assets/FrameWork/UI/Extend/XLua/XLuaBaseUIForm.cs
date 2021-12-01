//hack【UI】【XLua】
#if false
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OrangeEngine;

using XLua;
using System;

namespace ShipFarmeWork.UI
{
    public class XLuaBaseUIForm : BaseUIForm, IXLuaCompoent
    {
        public XLuaComponentObj ComponentObj { get; set; }

        public LuaFunction OnFirstEnterFunc;
        public LuaFunction StartShowFunc;
        public LuaFunction UpdateFunc;
        public LuaFunction OnCloseFunc;
        public LuaFunction OnDestroyFunc;
        public LuaFunction ExecuteLuaFunction;

        public void Bind()
        {
            LuaTable luaInstan = ComponentObj.LuaInstance;
            OnFirstEnterFunc = luaInstan.Get<LuaFunction>("OnFirstEnter");
            StartShowFunc = luaInstan.Get<LuaFunction>("StartShow");
            UpdateFunc = luaInstan.Get<LuaFunction>("OnUpdate");
            OnCloseFunc = luaInstan.Get<LuaFunction>("OnClose");
            OnDestroyFunc = luaInstan.Get<LuaFunction>("OnDestroy");
            ExecuteLuaFunction = luaInstan.Get<LuaFunction>("ExecuteLuaFunction");
        }

        /// <summary>
        /// 当首次进入
        /// 用来发现所有要用到的子节点,做一些和数据无关的初始化的操作
        /// </summary>
        protected override void OnFirstEnter()
        {
            if (OnFirstEnterFunc != null)
            {
                OnFirstEnterFunc.Call(ComponentObj.LuaInstance);
            }
        }

        /// <summary>
        /// 开始展示
        /// 带着参数然后刷新展示
        /// </summary>
        protected override void StartShow(params object[] param)
        {
            if (StartShowFunc != null)
            {
                object[] newParam = new object[1 + (param == null ? 0 : param.Length)];
                newParam[0] = ComponentObj.LuaInstance;
                if (param != null)
                {
                    for (int i = 0; i < param.Length; i++)
                    {
                        newParam[i + 1] = param[i];
                    }
                }
                StartShowFunc.Call(newParam);
            }
        }

        protected override void OnUpdate()
        {
            if (UpdateFunc != null)
            {
                UpdateFunc.Call(ComponentObj.LuaInstance);
            }

            //System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            //stopwatch.Start(); //  开始监视代码运行时间

            ////luaFormInstance.OnUpdate();

            ////updateFunc = LuaTable.Get<LuaFunction>("OnUpdate");
            //UpdateFunc.Call(LuaInstance);
            ////UpdateAA();
            ////if (IsCanUpdate)
            ////{
            ////    luaFormInstance.OnUpdate();
            ////}
            //stopwatch.Stop(); //  停止监视
            //TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间
            //double milliseconds = timespan.TotalMilliseconds;  //  总毫秒数

            //Debug.LogError("毫秒数是 " + milliseconds);
        }

        /// <summary>
        /// 当界面关闭
        /// </summary>
        protected override void OnClose()
        {
            if (OnCloseFunc != null)
            {
                OnCloseFunc.Call(ComponentObj.LuaInstance);
            }
        }
        
        /// <summary>
        /// 通过给定lua方法名称执行相对应的lua方法
        /// </summary>
        /// <param name="strLuaFuncName"></param>
        /// <param name="param"></param>
        public void ExecuteLuaFunctionByGivenName(string strLuaFuncName, params object[] param)
        {
            if (ExecuteLuaFunction != null)
            {
                object[] newParam = new object[2 + (param == null ? 0 : param.Length)];
                newParam[0] = ComponentObj.LuaInstance;
                newParam[1] = strLuaFuncName;
                if (param != null)
                {
                    for (int i = 0; i < param.Length; i++)
                    {
                        newParam[i + 2] = param[i];
                    }
                }
                ExecuteLuaFunction.Call(newParam);
            }
        }

        private void OnDestroy()
        {
            if (OnDestroyFunc != null)
            {
                OnDestroyFunc.Call(ComponentObj.LuaInstance);
            }
            //foreach (var item in triggerGameDic)
            //{
            //    EventTriggerListener.ClearAllListener(item);
            //}

            if (OnFirstEnterFunc != null) { OnFirstEnterFunc.Dispose(); OnFirstEnterFunc = null; }
            if (StartShowFunc != null) { StartShowFunc.Dispose(); StartShowFunc = null; }
            if (UpdateFunc != null) { UpdateFunc.Dispose(); UpdateFunc = null; }
            if (OnCloseFunc != null) { OnCloseFunc.Dispose(); OnCloseFunc = null; }
            if (OnDestroyFunc != null) { OnDestroyFunc.Dispose(); OnDestroyFunc = null; }
            if (ExecuteLuaFunction != null) { ExecuteLuaFunction.Dispose(); ExecuteLuaFunction = null; }

            ComponentObj.OnDestroy();
        }

        public void CloseSelf(bool isDestroy)
        {
            UIManager.GetInstance().CloseUIForm(this, isDestroy);
        }
    }
}
#endif