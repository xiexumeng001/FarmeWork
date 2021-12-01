#if false
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OrangeEngine;

using XLua;
using System;

namespace ShipFarmeWork.UI
{
    [CSharpCallLua]
    public interface LuaFormClass
    {
        bool IsCanUpdate { get; set; }
        void SetBehaviour(XLuaBaseUIForm uiForm);
        void OnFirstEnter();
        void StartShow(params object[] param);
        void OnUpdate();
        void OnClose();
        void OnDestroy();
    }


    public class XLuaBaseUIForm : BaseUIForm
    {
        public LuaFormClass luaFormInstance;
        public bool IsCanUpdate;

        private HashSet<GameObject> triggerGameDic = new HashSet<GameObject>();
        public void BindLuaClass(LuaFormClass luaClassIntance)
        {
            luaFormInstance = luaClassIntance;
            luaFormInstance.SetBehaviour(this);
            IsCanUpdate = luaFormInstance.IsCanUpdate;
        }

        public void BindSetLuaTable(LuaTable luaIns)
        {
            LuaInstance = luaIns;
            //SetBehaviourFunc = LuaTable.Get<LuaFunction>("SetBehaviour");
            //SetBehaviourFunc.Call(LuaTable,this);
            SetBehaviourFunc = LuaInstance.Get<LuaFunction>("SetBehaviour");
            SetBehaviourFunc.Call(LuaInstance, this);

            OnFirstEnterFunc = LuaInstance.Get<LuaFunction>("OnFirstEnter");
            StartShowFunc = LuaInstance.Get<LuaFunction>("StartShow");
            UpdateFunc = LuaInstance.Get<LuaFunction>("OnUpdate");
            OnCloseFunc = LuaInstance.Get<LuaFunction>("OnClose");
            OnDestroyFunc = LuaInstance.Get<LuaFunction>("OnDestroy");
        }

        /// <summary>
        /// 当首次进入
        /// 用来发现所有要用到的子节点,做一些和数据无关的初始化的操作
        /// </summary>
        protected override void OnFirstEnter()
        {
            luaFormInstance.OnFirstEnter();
        }

        /// <summary>
        /// 开始展示
        /// 带着参数然后刷新展示
        /// </summary>
        protected override void StartShow(params object[] param)
        {
            luaFormInstance.StartShow(param);
        }

        //hack【暂改】Update需要在有的时候在调用,减少性能消耗
        protected override void OnUpdate()
        {
            if (IsCanUpdate)
            {
                luaFormInstance.OnUpdate();
            }
        }

        public void UpdateAA()
        {

        }

        /// <summary>
        /// 当界面关闭
        /// </summary>
        protected override void OnClose()
        {
            luaFormInstance.OnClose();
        }

        private void OnDestroy()
        {
            luaFormInstance.OnDestroy();

            luaFormInstance = null;
            foreach (var item in triggerGameDic)
            {
                EventTriggerListener.ClearAllListener(item);
            }
        }

        /// <summary>
        /// 添加单击事件
        /// </summary>
        public void AddTouchEvent(EventTriggerListenerType type, LuaFunction luafunc, GameObject go)
        {
            if (go == null || luafunc == null)
                return;

            if (!triggerGameDic.Contains(go))
            {
                triggerGameDic.Add(go);
            }
            EventTriggerListener.AddListenerLuaFunc(type, luaFormInstance, luafunc, go);
        }


        public void CloseSelf(bool isDestroy)
        {
            ShipFarmeWork.UI.UIManager.GetInstance().CloseUIForm(this, isDestroy);
        }
    }
}
#endif