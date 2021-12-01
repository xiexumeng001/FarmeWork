using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
//using LuaInterface;
using UnityEngine.UI;
using System;

namespace ShipFarmeWork.UI {

	public enum EventTriggerListenerType
    {
        /// <summary>
        /// 单机
        /// </summary>
		onClick,
        /// <summary>
        /// 按住
        /// </summary>
        onClickPass,
        /// <summary>
        /// 手指按下
        /// </summary>
        onDown,
        /// <summary>
        /// 进入
        /// </summary>
		onEnter,
        /// <summary>
        /// 离开
        /// </summary>
		onExit,
        /// <summary>
        /// 抬起手指
        /// </summary>
		onUp,
        /// <summary>
        /// 选中
        /// </summary>
		onSelect,
        /// <summary>
        /// 选中 中
        /// </summary>
		onUpdateSelect,
        /// <summary>
        /// 开始拖动
        /// </summary>
        onBeginDrag,
        /// <summary>
        /// 结束拖动
        /// </summary>
		onEndDrag,
        /// <summary>
        /// 拖动中
        /// </summary>
        onDrag,
        /// <summary>
        /// 长按
        /// </summary>
        onLongPress,
	}

	public class EventTriggerListener : UnityEngine.EventSystems.EventTrigger{
		public delegate void VoidDelegate (GameObject go, BaseEventData data);
		public VoidDelegate onClick;
        public VoidDelegate onClickPass;
        public VoidDelegate onDown;
		public VoidDelegate onEnter;
		public VoidDelegate onExit;
		public VoidDelegate onUp;
		public VoidDelegate onSelect;
		public VoidDelegate onUpdateSelect;
		public VoidDelegate onDrag;
		public VoidDelegate onEndDrag;
		public VoidDelegate onBeginDrag;
        public VoidDelegate onLongPress;

        public ScrollRect scrollRect;

		private bool isDrag = false;
        private float intervalTime = 0.2f;
        //设置为全局的变量
        private static float lastClickTime = 0;

		static public EventTriggerListener Get (GameObject go)
		{
			EventTriggerListener listener = go.GetComponent<EventTriggerListener>();
			if (listener == null) 
				listener = go.AddComponent<EventTriggerListener>();
			return listener;
		}

        /// <summary>
        /// 设置冷却时间
        /// </summary>
        public void SetIntervalTime(float time)
        {
            intervalTime = time;
        }


		public override void OnBeginDrag (PointerEventData eventData)
		{
            if (scrollRect)
            {
                scrollRect.OnBeginDrag(eventData);
                isDrag = true;
            }

            if ((scrollRect != null) || ((onBeginDrag != null)))
            {
                isDrag = true;
            }
			
			if(onBeginDrag != null) 	
				onBeginDrag(gameObject, eventData);
		}

		public override void OnEndDrag (PointerEventData eventData)
		{
            if (scrollRect)
            {
                scrollRect.OnEndDrag(eventData);
            }
            isDrag = false;

            if (onEndDrag != null) 	
				onEndDrag(gameObject, eventData);
		}

		public override void OnDrag (PointerEventData eventData)
		{
			if (scrollRect)
				scrollRect.OnDrag (eventData);
			if(onDrag != null) 	
				onDrag(gameObject, eventData);
		}


        public override void OnPointerClick(PointerEventData eventData)
        {
            if (intervalTime > 0 && Time.time < lastClickTime + intervalTime)
            {
                //Debug.LogError("冷却中");
                return;
            }
            // 是否响应拖动事件
            //bool responseDrag = (onBeginDrag != null) || (onDrag != null) || (onEndDrag != null);&& responseDrag
            // 如果拖动了 且需要响应拖动事件  则不响应点击事件
            if (isDrag)
            {
                return;
            }
            if (isLongPress)
            {
                return;
            }
            if (onClick != null)
            {
                UISound.OnClickBtnPlaySound(gameObject);
                onClick(gameObject, eventData);
                lastClickTime = Time.time;
            }
            if (onClickPass != null)
            {
                onClickPass(gameObject, eventData);
                PassEvent(eventData);
                lastClickTime = Time.time;
            }
        }

        private bool isDown;
        private float longPressTimer;
        private bool isLongPress;
		public override void OnPointerDown (PointerEventData eventData){
            isDown = true;
            longPressTimer = 0;
            isLongPress = false;
            StartCoroutine(LongPressCount(eventData));
            if (onDown != null)
				onDown(gameObject, eventData);
		}

        private IEnumerator LongPressCount(PointerEventData eventData)
        {
            while (true)
            {
                if (!isDown)
                {
                    break;
                }
                longPressTimer += Time.deltaTime;
                if(longPressTimer > 0.5f)
                {
                    isLongPress = true;
                    if (onLongPress == null)
                    {
                        break;
                    }
                    onLongPress(gameObject, eventData);
                    break;
                }
                yield return null;
            }
        }
		public override void OnPointerEnter (PointerEventData eventData){
			if(onEnter != null)
				onEnter(gameObject, eventData);
		}

		public override void OnPointerExit (PointerEventData eventData){
            isDown = false;
			if(onExit != null) 
				onExit(gameObject, eventData);
		}

		public override void OnPointerUp (PointerEventData eventData){
            isDown = false;
			if(onUp != null) 
				onUp(gameObject, eventData);
		}

		public override void OnSelect (BaseEventData eventData){
			if(onSelect != null) 
				onSelect(gameObject, eventData);
		}

		public override void OnUpdateSelected (BaseEventData eventData){
			if(onUpdateSelect != null)
				onUpdateSelect(gameObject, eventData);
		}

        //hack【UI】【Lua】
		//public static void AddListenerLuaFunc(EventTriggerListenerType type, LuaTable luaClassIntance, string funcName, GameObject go){
		//	object func = luaClassIntance [funcName];
		//	if(func == null)
		//		return;
		//	LuaFunction luafunc = func as LuaFunction;
		//	AddListenerLuaFunc (type, luaClassIntance, luafunc, go);
		//}

        
        public void PassEvent(PointerEventData data)
        {
            int count = 0;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(data, results);
            GameObject current = data.pointerCurrentRaycast.gameObject;

            for (int i = 0; i < results.Count; i++)
            {
                if (current != results[i].gameObject)
                {
                    if(ExecuteEvents.Execute(results[i].gameObject, data, ExecuteEvents.pointerClickHandler))
                    {
                        count++;
                    }
                    if (count>= 2)
                    {
                        break;
                    }
                }

            }
        }

        //hack【UI】【Lua】
  //      public static void AddListenerLuaFunc(EventTriggerListenerType type, LuaTable luaClassIntance, LuaFunction luafunc, GameObject go){
		//	EventTriggerListener listener = EventTriggerListener.Get (go);
		//	switch (type) {
		//	case EventTriggerListenerType.onClick:
		//		{
		//			listener.onClick += delegate(GameObject g, BaseEventData data) {
		//				luafunc.Call (luaClassIntance, g, data);
		//			};
		//		}
		//		break;
  //          case EventTriggerListenerType.onLongPress:
  //              {
  //                  listener.onLongPress += delegate (GameObject g, BaseEventData data) {
  //                      luafunc.Call(luaClassIntance, g, data);
  //                  };
  //              }
  //              break;
  //          case EventTriggerListenerType.onClickPass:
  //          {
  //              listener.onClickPass += delegate (GameObject g, BaseEventData data) {
  //                  luafunc.Call(luaClassIntance, g, data);
  //              };
  //          }
  //          break;
  //          case EventTriggerListenerType.onDown:
		//	{
		//		listener.onDown += delegate(GameObject g, BaseEventData data) {
		//			luafunc.Call (luaClassIntance, g, data);
		//		};
		//	}
		//	break;
		//	case EventTriggerListenerType.onEnter:
		//		{
		//			listener.onClick += delegate(GameObject g, BaseEventData data) {
		//				luafunc.Call (luaClassIntance, g, data);
		//			};
		//		}
		//		break;
		//	case EventTriggerListenerType.onExit:
		//		{
		//			listener.onExit += delegate(GameObject g, BaseEventData data) {
		//				luafunc.Call (luaClassIntance, g, data);
		//			};
		//		}
		//		break;
		//	case EventTriggerListenerType.onSelect:
		//		{
		//			listener.onSelect += delegate(GameObject g, BaseEventData data) {
		//				luafunc.Call (luaClassIntance, g, data);
		//			};
		//		}
		//		break;
		//	case EventTriggerListenerType.onUp:
		//		{
		//			listener.onUp += delegate(GameObject g, BaseEventData data) {
		//				luafunc.Call (luaClassIntance, g, data);
		//			};
		//		}
		//		break;
		//	case EventTriggerListenerType.onUpdateSelect:
		//		{
		//			listener.onUpdateSelect += delegate(GameObject g, BaseEventData data) {
		//				luafunc.Call (luaClassIntance, g, data);
		//			};
		//		}
		//		break;
		//	case EventTriggerListenerType.onDrag:
		//		{
		//			listener.onDrag += delegate(GameObject g, BaseEventData data) {
		//				luafunc.Call (luaClassIntance, g, data);
		//			};
		//		}
		//		break;
		//	case EventTriggerListenerType.onEndDrag:
		//		{
		//			listener.onEndDrag += delegate(GameObject g, BaseEventData data) {
		//				luafunc.Call (luaClassIntance, g, data);
		//			};
		//		}
		//		break;
		//	case EventTriggerListenerType.onBeginDrag:
		//		{
		//			listener.onBeginDrag += delegate(GameObject g, BaseEventData data) {
		//				luafunc.Call (luaClassIntance, g, data);
		//			};
		//		}
		//		break;
		//	}
		//}


        //liao 20180116
        public static void ClearAllListener(GameObject go)
        {
            //Check has componnet first
            EventTriggerListener listener = go.GetComponent<EventTriggerListener>();
            if (listener == null)
                return;
            else
            {
                
                listener.scrollRect = null;
                listener.onClickPass = null;
                listener.onClick = null;
                listener.onDown = null;
                listener.onLongPress = null;
                listener.onEnter = null;
                listener.onExit = null;
                listener.onUp = null;
                listener.onSelect = null;
                listener.onUpdateSelect = null;
                listener.onDrag = null;
                listener.onBeginDrag = null;
                listener.onBeginDrag = null;
                

                //Destroy(listener);
            }

        }

        public static void ClearListenerByType(EventTriggerListenerType type, GameObject go){
            EventTriggerListener listener = EventTriggerListener.Get(go);
            switch (type)
            {
                case EventTriggerListenerType.onClick:
                    {
                        listener.onClick = null;
                    }
                    break;
                case EventTriggerListenerType.onLongPress:
                    {
                        listener.onLongPress = null;
                    }
                    break;
                case EventTriggerListenerType.onClickPass:
                    {
                        listener.onClickPass = null;
                    }
                    break;
                case EventTriggerListenerType.onDown:
                    {
                        listener.onDown = null;
                    }
                    break;
                case EventTriggerListenerType.onEnter:
                    {
                        listener.onClick = null;
                    }
                    break;
                case EventTriggerListenerType.onExit:
                    {
                        listener.onExit = null;
                    }
                    break;
                case EventTriggerListenerType.onSelect:
                    {
                        listener.onSelect = null;
                    }
                    break;
                case EventTriggerListenerType.onUp:
                    {
                        listener.onUp = null;
                    }
                    break;
                case EventTriggerListenerType.onUpdateSelect:
                    {
                        listener.onUpdateSelect = null;
                    }
                    break;
                case EventTriggerListenerType.onDrag:
                    {
                        listener.onDrag = null;
                    }
                    break;
                case EventTriggerListenerType.onEndDrag:
                    {
                        listener.onEndDrag = null;
                    }
                    break;
                case EventTriggerListenerType.onBeginDrag:
                    {
                        listener.onBeginDrag = null;
                    }
                    break;
            }
        }



		public static void AddListenerCSharp(EventTriggerListenerType type, VoidDelegate func, GameObject go){
			EventTriggerListener listener = EventTriggerListener.Get (go);
            switch (type) {
			case EventTriggerListenerType.onClick:
				{
					listener.onClick += func;
				}
				break;
            case EventTriggerListenerType.onLongPress:
                {
                    listener.onLongPress += func;
                }
                break;
            case EventTriggerListenerType.onClickPass:
            {
                listener.onClickPass += func;
            }
            break;
            case EventTriggerListenerType.onDown:
			{
				listener.onDown += func;
			}
			break;
			case EventTriggerListenerType.onEnter:
				{
					listener.onClick += func;
				}
				break;
			case EventTriggerListenerType.onExit:
				{
					listener.onExit += func;
				}
				break;
			case EventTriggerListenerType.onSelect:
				{
					listener.onSelect +=func;
				}
				break;
			case EventTriggerListenerType.onUp:
				{
					listener.onUp += func;
				}
				break;
			case EventTriggerListenerType.onUpdateSelect:
				{
					listener.onUpdateSelect += func;
				}
				break;
			case EventTriggerListenerType.onDrag:
				{
					listener.onDrag += func;
				}
				break;
			case EventTriggerListenerType.onEndDrag:
				{
					listener.onEndDrag += func;
				}
				break;
			case EventTriggerListenerType.onBeginDrag:
				{
					listener.onBeginDrag += func;
				}
				break;
			}


		}

#if HOTFIX_ENABLE
        public static void AddListenerLuaFunc(EventTriggerListenerType type, IXLuaCompoent luaClassIntance, XLuaHelper.OnEventTriggerDel luafunc, GameObject go)
        {
            EventTriggerListener listener = EventTriggerListener.Get(go);

            if (!luaClassIntance.ComponentObj.TriggerGameList.Contains(go))
            {
                luaClassIntance.ComponentObj.TriggerGameList.Add(go);
            }

            switch (type)
            {
                case EventTriggerListenerType.onClick:
                    {
                        listener.onClick += delegate (GameObject g, BaseEventData data)
                        {
                            luafunc(luaClassIntance.ComponentObj.LuaInstance, g, data);
                        };
                    }
                    break;
                case EventTriggerListenerType.onLongPress:
                    {
                        listener.onLongPress += delegate (GameObject g, BaseEventData data)
                        {
                            luafunc(luaClassIntance.ComponentObj.LuaInstance, g, data);
                        };
                    }
                    break;
                case EventTriggerListenerType.onClickPass:
                    {
                        listener.onClickPass += delegate (GameObject g, BaseEventData data)
                        {
                            luafunc(luaClassIntance.ComponentObj.LuaInstance, g, data);
                        };
                    }
                    break;
                case EventTriggerListenerType.onDown:
                    {
                        listener.onDown += delegate (GameObject g, BaseEventData data)
                        {
                            luafunc(luaClassIntance.ComponentObj.LuaInstance, g, data);
                        };
                    }
                    break;
                case EventTriggerListenerType.onEnter:
                    {
                        listener.onClick += delegate (GameObject g, BaseEventData data)
                        {
                            luafunc(luaClassIntance.ComponentObj.LuaInstance, g, data);
                        };
                    }
                    break;
                case EventTriggerListenerType.onExit:
                    {
                        listener.onExit += delegate (GameObject g, BaseEventData data)
                        {
                            luafunc(luaClassIntance.ComponentObj.LuaInstance, g, data);
                        };
                    }
                    break;
                case EventTriggerListenerType.onSelect:
                    {
                        listener.onSelect += delegate (GameObject g, BaseEventData data)
                        {
                            luafunc(luaClassIntance.ComponentObj.LuaInstance, g, data);
                        };
                    }
                    break;
                case EventTriggerListenerType.onUp:
                    {
                        listener.onUp += delegate (GameObject g, BaseEventData data)
                        {
                            luafunc(luaClassIntance.ComponentObj.LuaInstance, g, data);
                        };
                    }
                    break;
                case EventTriggerListenerType.onUpdateSelect:
                    {
                        listener.onUpdateSelect += delegate (GameObject g, BaseEventData data)
                        {
                            luafunc(luaClassIntance.ComponentObj.LuaInstance, g, data);
                        };
                    }
                    break;
                case EventTriggerListenerType.onDrag:
                    {
                        listener.onDrag += delegate (GameObject g, BaseEventData data)
                        {
                            luafunc(luaClassIntance.ComponentObj.LuaInstance, g, data);
                        };
                    }
                    break;
                case EventTriggerListenerType.onEndDrag:
                    {
                        listener.onEndDrag += delegate (GameObject g, BaseEventData data)
                        {
                            luafunc(luaClassIntance.ComponentObj.LuaInstance, g, data);
                        };
                    }
                    break;
                case EventTriggerListenerType.onBeginDrag:
                    {
                        listener.onBeginDrag += delegate (GameObject g, BaseEventData data)
                        {
                            luafunc(luaClassIntance.ComponentObj.LuaInstance, g, data);
                        };
                    }
                    break;
            }
        }
#endif
    }
}