//hack【UI】【XLua】
#if false
using OrangeEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XLua;

namespace ShipFarmeWork.UI
{
    public class XLuaScrollViewHandlerItem : ScrollViewHandlerItem, IXLuaCompoent
    {
        private ScrollRect scrollRect;

        public XLuaComponentObj ComponentObj { get; set; }

        public LuaFunction OnFirstEnterFunc;
        public LuaFunction OnShowFunc;
        public LuaFunction UpdateFunc;
        public LuaFunction OnCloseFunc;
        public LuaFunction OnDestroyFunc;

        public void Bind()
        {
            LuaTable luaInstan = ComponentObj.LuaInstance;
            OnFirstEnterFunc = luaInstan.Get<LuaFunction>("OnFirstEnter");
            OnShowFunc = luaInstan.Get<LuaFunction>("OnShow");
            UpdateFunc = luaInstan.Get<LuaFunction>("OnUpdate");
            OnCloseFunc = luaInstan.Get<LuaFunction>("OnClose");
            OnDestroyFunc = luaInstan.Get<LuaFunction>("OnDestroy");
        }

        protected override void OnFirstEnter(params object[] param)
        {
            if (OnFirstEnterFunc != null)
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
                OnFirstEnterFunc.Call(newParam);
            }
        }

        /// <summary>
        /// 显示界面
        /// </summary>
        /// <param name="data"></param>
        protected override void OnShow(object data)
        {
            if (OnShowFunc != null)
            {
                object[] newParam = new object[2];
                newParam[0] = ComponentObj.LuaInstance;
                newParam[1] = data;
                OnShowFunc.Call(newParam);
            }
        }

        private void Update()
        {
            if (UpdateFunc != null)
            {
                UpdateFunc.Call(ComponentObj.LuaInstance);
            }
        }

        protected override void OnClose()
        {
            if (OnCloseFunc != null)
            {
                OnCloseFunc.Call(ComponentObj.LuaInstance);
            }
        }

        protected override void OnDestroySelf()
        {
            if (OnDestroyFunc != null)
            {
                OnDestroyFunc.Call(ComponentObj.LuaInstance);
            }

            if (OnFirstEnterFunc != null) { OnFirstEnterFunc.Dispose(); OnFirstEnterFunc = null; }
            if (OnShowFunc != null) { OnShowFunc.Dispose(); OnShowFunc = null; }
            if (UpdateFunc != null) { UpdateFunc.Dispose(); UpdateFunc = null; }
            if (OnCloseFunc != null) { OnCloseFunc.Dispose(); OnCloseFunc = null; }
            if (OnDestroyFunc != null) { OnDestroyFunc.Dispose(); OnDestroyFunc = null; }

            ComponentObj.OnDestroy();
        }

        public void ExecuteDragEvents(GameObject goBtn, ScrollRect scrollRect)
        {
            this.scrollRect = scrollRect;
            EventTriggerListener.AddListenerCSharp(EventTriggerListenerType.onBeginDrag, OnBeginDrag, goBtn);
            EventTriggerListener.AddListenerCSharp(EventTriggerListenerType.onDrag, OnItemDrag, goBtn);
            EventTriggerListener.AddListenerCSharp(EventTriggerListenerType.onEndDrag, OnEndDrag, goBtn);
        }


        private void OnBeginDrag(GameObject go, BaseEventData data)
        {
            if (scrollRect != null)
            {
                ExecuteEvents.Execute(scrollRect.gameObject, data, ExecuteEvents.beginDragHandler);
            }
        }

        private void OnItemDrag(GameObject go, BaseEventData data)
        {
            if (scrollRect != null)
            {
                ExecuteEvents.Execute(scrollRect.gameObject, data, ExecuteEvents.dragHandler);
            }
        }

        private void OnEndDrag(GameObject go, BaseEventData data)
        {
            if (scrollRect != null)
            {
                ExecuteEvents.Execute(scrollRect.gameObject, data, ExecuteEvents.endDragHandler);
            }
        }
    }
}
#endif