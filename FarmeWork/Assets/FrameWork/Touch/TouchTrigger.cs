using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ShipFarmeWork.Touch
{
    public class TouchTrigger : MonoBehaviour
    {
        public delegate void VoidDelegate(GameObject go, TouchData data);
        //当点击(暂时按照当抬起算)
        private VoidDelegate onTouchClick;
        //当按下
        private VoidDelegate onTouchDown;
        //当抬起
        private VoidDelegate onTouchUp;
        //当拖拽开始
        private VoidDelegate onDragStart;
        //当拖拽
        private VoidDelegate onDrag;
        //当拖拽结束
        private VoidDelegate onDragEnd;
        //当长按
        private VoidDelegate onLongPress;
        //当缩放
        private VoidDelegate onDoubleTouch;
        //当没有被选中
        private VoidDelegate whenNotSelected;
        //当点击(暂时按照当抬起算)
        public VoidDelegate TouchClick { get { return onTouchClick; } }
        //当按下
        public VoidDelegate TouchDown { get { return onTouchDown; } }
        //当抬起
        public VoidDelegate TouchUp { get { return onTouchUp; } }
        //当拖拽开始
        public VoidDelegate DragStart { get { return onDragStart; } }
        //当拖拽
        public VoidDelegate Drag { get { return onDrag; } }
        //当拖拽结束
        public VoidDelegate DragEnd { get { return onDragEnd; } }
        //当长按
        public VoidDelegate LongPress { get { return onLongPress; } }
        //当缩放
        public VoidDelegate DoubleTouch { get { return onDoubleTouch; } }
        //当没有被选中
        public VoidDelegate OhenNotSelected { get { return whenNotSelected; } }

        //绑定数据
        public System.Object BindData { get; set; }

        public static Action<GameObject> OnTriggerClick;  //当触发点击


        static public TouchTrigger Get(GameObject go)
        {
            TouchTrigger listener = go.GetComponent<TouchTrigger>();
            if (listener == null)
                listener = go.AddComponent<TouchTrigger>();
            return listener;
        }


        public static void AddListenerFunc(TouchTriggerType type, VoidDelegate func, GameObject go)
        {
            TouchTrigger listen = TouchTrigger.Get(go);
            switch (type)
            {
                case TouchTriggerType.OnTouchClick:
                    listen.onTouchClick += func;
                    break;
                case TouchTriggerType.OnTouchDown:
                    listen.onTouchDown += func;
                    break;
                case TouchTriggerType.OnTouchUp:
                    listen.onTouchUp += func;
                    break;
                case TouchTriggerType.OnDragStart:
                    listen.onDragStart += func;
                    break;
                case TouchTriggerType.OnDrag:
                    listen.onDrag += func;
                    break;
                case TouchTriggerType.OnDragEnd:
                    listen.onDragEnd += func;
                    break;
                case TouchTriggerType.OnDoubleTouch:
                    listen.onDoubleTouch += func;
                    break;
                case TouchTriggerType.WhenNotSelected:
                    listen.whenNotSelected += func;
                    break;

            }
        }

        public static void RemoveListenerFunc(TouchTriggerType type, VoidDelegate func, GameObject go)
        {
            TouchTrigger listen = TouchTrigger.Get(go);
            switch (type)
            {
                case TouchTriggerType.OnTouchClick:
                    listen.onTouchClick -= func;
                    break;
                case TouchTriggerType.OnTouchDown:
                    listen.onTouchDown -= func;
                    break;
                case TouchTriggerType.OnTouchUp:
                    listen.onTouchUp -= func;
                    break;
                case TouchTriggerType.OnDragStart:
                    listen.onDragStart -= func;
                    break;
                case TouchTriggerType.OnDrag:
                    listen.onDrag -= func;
                    break;
                case TouchTriggerType.OnDragEnd:
                    listen.onDragEnd -= func;
                    break;
                case TouchTriggerType.OnDoubleTouch:
                    listen.onDoubleTouch -= func;
                    break;
                case TouchTriggerType.WhenNotSelected:
                    listen.whenNotSelected -= func;
                    break;

            }
        }

        /// <summary>
        /// 传递事件给触发者
        /// </summary>
        public static void TransEventToTrigger(TouchTrigger trigger, TouchTriggerType type, GameObject game, TouchData data)
        {
            if (trigger == null)
            {
                Debug.LogError("没有触发者要传递");
                return;
            }
            switch (type)
            {
                case TouchTriggerType.OnTouchClick:
                    trigger.OnTouchClick(game, (OnTouchUpData)data);
                    break;
                case TouchTriggerType.OnTouchDown:
                    trigger.OnTouchDown(game, (OnTouchDownData)data);
                    break;
                case TouchTriggerType.OnTouchUp:
                    trigger.OnTouchUp(game, (OnTouchUpData)data);
                    break;
                case TouchTriggerType.OnDragStart:
                    trigger.OnDragStart(game, (OnDragData)data);
                    break;
                case TouchTriggerType.OnDrag:
                    trigger.OnDrag(game, (OnDragData)data);
                    break;
                case TouchTriggerType.OnDragEnd:
                    trigger.OnDragEnd(game, (OnDragEndData)data);
                    break;
                case TouchTriggerType.OnDoubleTouch:
                    trigger.OnDoubleTouch(game, (OnDoubleTouchData)data);
                    break;
                case TouchTriggerType.WhenNotSelected:
                    trigger.WhenNotSelected(game, (OnTouchUpData)data);
                    break;
            }
        }


        /// <summary>
        /// 设置绑定数据
        /// </summary>
        public static void SetBindData(System.Object obj, GameObject go)
        {
            TouchTrigger listen = TouchTrigger.Get(go);
            listen.BindData = obj;
        }


        public void OnTouchClick(GameObject gam, OnTouchUpData data)
        {
            try
            {
                if (onTouchClick != null)
                    onTouchClick(gam, data);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public void OnTouchDown(GameObject gam, OnTouchDownData data)
        {
            try
            {
                if (onTouchDown != null)
                    onTouchDown(gam, data);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public void OnTouchUp(GameObject game, OnTouchUpData data)
        {
            try
            {
                if (onTouchUp != null)
                    onTouchUp(game, data);
                if (OnTriggerClick != null)
                {
                    OnTriggerClick(game);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }


        public void OnDragStart(GameObject game, OnDragData data)
        {
            try
            {
                if (onDragStart != null)
                    onDragStart(game, data);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public void OnDrag(GameObject gam, OnDragData data)
        {
            try
            {
                if (onDrag != null)
                    onDrag(gam, data);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public void OnDragEnd(GameObject gam, OnDragEndData data)
        {
            try
            {
                if (onDragEnd != null)
                    onDragEnd(gameObject, data);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }


        //public void OnLongPress()
        //{
        //    if (onLongPress != null)
        //        onLongPress(gameObject, eventData);
        //}
        /// <summary>
        /// 当双指缩放
        /// </summary>
        /// <param name="data"></param>
        /// <returns>返回当前物体是否捕获到缩放了</returns>
        public bool OnDoubleTouch(GameObject game, OnDoubleTouchData data)
        {
            try
            {
                if (onDoubleTouch != null)
                    onDoubleTouch(game, data);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            return (onDoubleTouch != null);
        }



        // yada
        public void WhenNotSelected(GameObject game, OnTouchUpData data)
        {
            try
            {
                if (whenNotSelected != null)
                    whenNotSelected(game, data);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

    }
}
