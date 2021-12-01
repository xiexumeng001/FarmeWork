using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using SObject = System.Object;
using UObject = UnityEngine.Object;

namespace ShipFarmeWork.Notify
{
    /// <summary>
    /// 消息中心,管理者消息的添加移除与派发
    /// </summary>
    public class EventCenter
    {
        /// <summary>
        /// 全局事件,没有被监听者
        /// </summary>
        private static Dictionary<NotifyType, Delegate> eventListeners
            = new Dictionary<NotifyType, Delegate>();

        /// <summary>
        /// 点对点的事件集合
        /// </summary>
        private static Dictionary<SObject, Dictionary<NotifyType, Delegate>> objectLists
            = new Dictionary<SObject, Dictionary<NotifyType, Delegate>>();

        /// <summary>
        /// 时间数量记录
        /// </summary>
        private static Dictionary<NotifyType, int> eventNumDic = new Dictionary<NotifyType, int>();
        private static Dictionary<NotifyType, Dictionary<SObject, int>> eventNumDicObject = new Dictionary<NotifyType, Dictionary<SObject, int>>();


        /// <summary>
        /// 是否有监听者
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <param name="beListener">被监听者</param>
        /// <returns></returns>
        private static bool HasListener(NotifyType eventType, SObject beListener = null)
        {
            if (beListener == null)
            {
                return eventListeners.ContainsKey(eventType);
            }
            else
            {
                return objectLists.ContainsKey(beListener) && objectLists[beListener].ContainsKey(eventType);
            }
        }

        /// <summary>
        /// 获取到监听
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <param name="beListener">被监听者</param>
        /// <returns></returns>
        public static Delegate GetListener(NotifyType eventType, SObject beListener = null)
        {
            if (HasListener(eventType, beListener))
            {
                if (beListener == null)
                    return eventListeners[eventType];
                else
                    return objectLists[beListener][eventType];
            }
            return null;

        }

        /// <summary>
        /// 当添加消息监听
        /// </summary>
        /// <param name="eventType">消息类型</param>
        /// <param name="callBack">对应的方法</param>
        /// <param name="beListener">被监听者</param>
        private static void OnListenerAdding(NotifyType eventType, Delegate callBack, SObject beListener = null, bool isIgronRecord = false)
        {
            if (!isIgronRecord)
            {
                AddEventNumRecord(eventType, beListener);
            }

            if (beListener == null)
            {
                if (!eventListeners.ContainsKey(eventType))
                {
                    eventListeners.Add(eventType, null);
                }
                Delegate d = eventListeners[eventType];
                if (d != null && d.GetType() != callBack.GetType())
                {
                    throw new Exception(string.Format("尝试为事件{0}添加不同类型的委托，当前事件所对应的委托是{1}，要添加的委托类型为{2}", eventType, d.GetType(), callBack.GetType()));
                }
            }
            else
            {
                Dictionary<NotifyType, Delegate> info = null;
                if (!objectLists.ContainsKey(beListener))
                {
                    info = new Dictionary<NotifyType, Delegate>();
                    objectLists[beListener] = info;// 给委托变量赋值
                }
                else
                {
                    info = objectLists[beListener];
                }
                if (!info.ContainsKey(eventType))
                {
                    info.Add(eventType, null);// 给委托变量赋值
                }
                Delegate d = info[eventType];
                if (d != null && d.GetType() != callBack.GetType())
                {
                    throw new Exception(string.Format("尝试为对象事件{0}添加不同类型的委托，当前事件所对应的委托是{1}，要添加的委托类型为{2}", eventType, d.GetType(), callBack.GetType()));
                }
            }
        }

        private static void AddEventNumRecord(NotifyType eventType, SObject beListener = null)
        {
            if (beListener == null)
            {
                if (eventNumDic.ContainsKey(eventType))
                {
                    eventNumDic[eventType] = eventNumDic[eventType] + 1;
                    if (eventNumDic[eventType] > 30)
                    {
                        string trackStr = new System.Diagnostics.StackTrace().ToString();
                        string msg = "事件 " + eventType.ToString() + " 监听超过30,没移除或者循环监听了??";
                        Debug.LogError(msg + "\n" + trackStr);
                    }
                }
                else { eventNumDic.Add(eventType, 1); }
            }
            else
            {
                if (eventNumDicObject.ContainsKey(eventType))
                {
                    Dictionary<SObject, int> dic = eventNumDicObject[eventType];
                    if (dic.ContainsKey(beListener))
                    {
                        dic[beListener] = dic[beListener] + 1;
                        if (dic[beListener] > 10)
                        {
                            string trackStr = new System.Diagnostics.StackTrace().ToString();
                            string msg = "事件 " + eventType.ToString() + " 监听超过10,没移除或者循环监听了??";
                            Debug.LogError(msg + "\n" + trackStr);
                        }
                    }
                    else { dic.Add(beListener, 1); }
                }
                else
                {
                    Dictionary<SObject, int> dic = new Dictionary<SObject, int>();
                    dic.Add(beListener, 1);
                    eventNumDicObject.Add(eventType, dic);
                }
            }
        }

        private static void RemoveEventNumRecord(NotifyType eventType, SObject beListener = null)
        {
            if (beListener == null)
            {
                if (eventNumDic.ContainsKey(eventType))
                {
                    eventNumDic[eventType] = eventNumDic[eventType] - 1;
                    if (eventNumDic[eventType] <= 0)
                    {
                        eventNumDic.Remove(eventType);
                    }
                }
            }
            else
            {
                if (eventNumDicObject.ContainsKey(eventType))
                {
                    Dictionary<SObject, int> dic = eventNumDicObject[eventType];
                    if (dic.ContainsKey(beListener))
                    {
                        dic[beListener] = dic[beListener] - 1;
                        if (dic[beListener] <= 0)
                        {
                            dic.Remove(beListener);
                            eventNumDicObject.Remove(eventType);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 当移除结束
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <param name="beListener">被监听者</param>
        private static void OnListenerRemoved(NotifyType eventType, SObject beListener = null)
        {
            RemoveEventNumRecord(eventType, beListener);

            if (beListener == null)
            {
                if (eventListeners[eventType] == null)
                {
                    eventListeners.Remove(eventType);
                }
            }
            else
            {
                if (objectLists[beListener][eventType] == null)
                {
                    objectLists[beListener].Remove(eventType);
                }
                if (objectLists[beListener].Count == 0)
                {
                    objectLists.Remove(beListener);
                }
            }
        }

        /// <summary>
        /// 添加消息监听
        /// </summary>
        /// <param name="eventType">消息类型</param>
        /// <param name="callBack">对应的方法</param>
        /// <param name="beListener">被监听者</param>
        //no parameters
        public static void AddListener(NotifyType eventType, NotifyCallback callBack, SObject beListener = null, bool isIgronRecord = false)
        {
            OnListenerAdding(eventType, callBack, beListener, isIgronRecord);
            if (beListener == null)
            {
                eventListeners[eventType] = (NotifyCallback)eventListeners[eventType] + callBack;
            }
            else
            {
                objectLists[beListener][eventType] = (NotifyCallback)objectLists[beListener][eventType] + callBack;
            }
        }
        //Single parameters
        public static void AddListener<T>(NotifyType eventType, NotifyCallback<T> callBack, SObject beListener = null, bool isIgronRecord = false)
        {
            OnListenerAdding(eventType, callBack, beListener, isIgronRecord);
            if (beListener == null)
            {
                eventListeners[eventType] = (NotifyCallback<T>)eventListeners[eventType] + callBack;
            }
            else
            {
                objectLists[beListener][eventType] = (NotifyCallback<T>)objectLists[beListener][eventType] + callBack;
            }
        }
        //two parameters
        public static void AddListener<T, T1>(NotifyType eventType, NotifyCallback<T, T1> callBack, SObject beListener = null, bool isIgronRecord = false)
        {
            OnListenerAdding(eventType, callBack, beListener, isIgronRecord);
            if (beListener == null)
            {
                eventListeners[eventType] = (NotifyCallback<T, T1>)eventListeners[eventType] + callBack;
            }
            else
            {
                objectLists[beListener][eventType] = (NotifyCallback<T, T1>)objectLists[beListener][eventType] + callBack;
            }
        }
        //three parameters
        public static void AddListener<T, T1, T2>(NotifyType eventType, NotifyCallback<T, T1, T2> callBack, SObject beListener = null, bool isIgronRecord = false)
        {
            OnListenerAdding(eventType, callBack, beListener, isIgronRecord);
            if (beListener == null)
            {
                eventListeners[eventType] = (NotifyCallback<T, T1, T2>)eventListeners[eventType] + callBack;
            }
            else
            {
                objectLists[beListener][eventType] = (NotifyCallback<T, T1, T2>)objectLists[beListener][eventType] + callBack;
            }
        }
        //four parameters
        public static void AddListener<T, T1, T2, T3>(NotifyType eventType, NotifyCallback<T, T1, T2, T3> callBack, SObject beListener = null, bool isIgronRecord = false)
        {
            OnListenerAdding(eventType, callBack, beListener, isIgronRecord);
            if (beListener == null)
            {
                eventListeners[eventType] = (NotifyCallback<T, T1, T2, T3>)eventListeners[eventType] + callBack;
            }
            else
            {
                objectLists[beListener][eventType] = (NotifyCallback<T, T1, T2, T3>)objectLists[beListener][eventType] + callBack;
            }
        }
        //five parameters
        public static void AddListener<T, T1, T2, T3, T4>(NotifyType eventType, NotifyCallback<T, T1, T2, T3, T4> callBack, SObject beListener = null, bool isIgronRecord = false)
        {
            OnListenerAdding(eventType, callBack, beListener, isIgronRecord);
            if (beListener == null)
            {
                eventListeners[eventType] = (NotifyCallback<T, T1, T2, T3, T4>)eventListeners[eventType] + callBack;
            }
            else
            {
                objectLists[beListener][eventType] = (NotifyCallback<T, T1, T2, T3, T4>)objectLists[beListener][eventType] + callBack;
            }
        }

        /// <summary>
        /// 移除消息监听
        /// </summary>
        /// <param name="eventType">消息类型</param>
        /// <param name="callBack">移除的方法</param>
        /// <param name="beListener">被监听者</param>
        //no parameters
        public static void RemoveListener(NotifyType eventType, NotifyCallback callBack, SObject beListener = null)
        {
            try
            {
                if (!HasListener(eventType, beListener)) return;
                if (beListener == null)
                {
                    if (eventListeners[eventType] is NotifyCallback eventCallback)
                    {
                        eventListeners[eventType] = eventCallback - callBack;
                    }
                    else
                    {
                        throw new Exception(string.Format("移除事件错误：事件{0}对应委托具有不同的类型", eventType));
                    }
                }
                else
                {
                    if (objectLists[beListener][eventType] is NotifyCallback eventCallback)
                    {
                        objectLists[beListener][eventType] = eventCallback - callBack;
                    }
                    else
                    {
                        throw new Exception(string.Format("移除事件错误：事件{0}对应委托具有不同的类型", eventType));
                    }
                }
                OnListenerRemoved(eventType, beListener);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        //single parameters
        public static void RemoveListener<T>(NotifyType eventType, NotifyCallback<T> callBack, SObject beListener = null)
        {

            try
            {
                if (!HasListener(eventType, beListener)) return;
                if (beListener == null)
                {
                    if (eventListeners[eventType] is NotifyCallback<T> eventCallback)
                    {
                        eventListeners[eventType] = eventCallback - callBack;
                    }
                    else
                    {
                        throw new Exception(string.Format("移除事件错误：事件{0}对应委托具有不同的类型", eventType));
                    }
                }
                else
                {
                    if (objectLists[beListener][eventType] is NotifyCallback<T> eventCallback)
                    {
                        objectLists[beListener][eventType] = eventCallback - callBack;
                    }
                    else
                    {
                        throw new Exception(string.Format("移除事件错误：事件{0}对应委托具有不同的类型", eventType));
                    }
                }
                OnListenerRemoved(eventType, beListener);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        //two parameters
        public static void RemoveListener<T, T1>(NotifyType eventType, NotifyCallback<T, T1> callBack, SObject beListener = null)
        {
            try
            {
                if (!HasListener(eventType, beListener)) return;
                if (beListener == null)
                {
                    if (eventListeners[eventType] is NotifyCallback<T, T1> eventCallback)
                    {
                        eventListeners[eventType] = eventCallback - callBack;
                    }
                    else
                    {
                        throw new Exception(string.Format("移除事件错误：事件{0}对应委托具有不同的类型", eventType));
                    }
                }
                else
                {
                    if (objectLists[beListener][eventType] is NotifyCallback<T, T1> eventCallback)
                    {
                        objectLists[beListener][eventType] = eventCallback - callBack;
                    }
                    else
                    {
                        throw new Exception(string.Format("移除事件错误：事件{0}对应委托具有不同的类型", eventType));
                    }
                }
                OnListenerRemoved(eventType, beListener);
            }
            catch (Exception e) {
                Debug.LogError(e);
            }
        }
        //three parameters
        public static void RemoveListener<T, T1, T2>(NotifyType eventType, NotifyCallback<T, T1, T2> callBack, SObject beListener = null)
        {
            try
            {
                if (!HasListener(eventType, beListener)) return;
                if (beListener == null)
                {
                    if (eventListeners[eventType] is NotifyCallback<T, T1, T2> eventCallback)
                    {
                        eventListeners[eventType] = eventCallback - callBack;
                    }
                    else
                    {
                        throw new Exception(string.Format("移除事件错误：事件{0}对应委托具有不同的类型", eventType));
                    }
                }
                else
                {
                    if (objectLists[beListener][eventType] is NotifyCallback<T, T1, T2> eventCallback)
                    {
                        objectLists[beListener][eventType] = eventCallback - callBack;
                    }
                    else
                    {
                        throw new Exception(string.Format("移除事件错误：事件{0}对应委托具有不同的类型", eventType));
                    }
                }
                OnListenerRemoved(eventType, beListener);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        //four parameters
        public static void RemoveListener<T, T1, T2, T3>(NotifyType eventType, NotifyCallback<T, T1, T2, T3> callBack, SObject beListener = null)
        {
            try
            {
                if (!HasListener(eventType, beListener)) return;
                if (beListener == null)
                {
                    if (eventListeners[eventType] is NotifyCallback<T, T1, T2, T3> eventCallback)
                    {
                        eventListeners[eventType] = eventCallback - callBack;
                    }
                    else
                    {
                        throw new Exception(string.Format("移除事件错误：事件{0}对应委托具有不同的类型", eventType));
                    }
                }
                else
                {
                    if (objectLists[beListener][eventType] is NotifyCallback<T, T1, T2, T3> eventCallback)
                    {
                        objectLists[beListener][eventType] = eventCallback - callBack;
                    }
                    else
                    {
                        throw new Exception(string.Format("移除事件错误：事件{0}对应委托具有不同的类型", eventType));
                    }
                }
                OnListenerRemoved(eventType, beListener);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        //five parameters
        public static void RemoveListener<T, T1, T2, T3, T4>(NotifyType eventType, NotifyCallback<T, T1, T2, T3, T4> callBack, SObject beListener = null)
        {
            try
            {
                if (!HasListener(eventType, beListener)) return;
                if (beListener == null)
                {
                    if (eventListeners[eventType] is NotifyCallback<T, T1, T2, T3, T4> eventCallback)
                    {
                        eventListeners[eventType] = eventCallback - callBack;
                    }
                    else
                    {
                        throw new Exception(string.Format("移除事件错误：事件{0}对应委托具有不同的类型", eventType));
                    }
                }
                else
                {
                    if (objectLists[beListener][eventType] is NotifyCallback<T, T1, T2, T3, T4> eventCallback)
                    {
                        objectLists[beListener][eventType] = eventCallback - callBack;
                    }
                    else
                    {
                        throw new Exception(string.Format("移除事件错误：事件{0}对应委托具有不同的类型", eventType));
                    }
                }
                OnListenerRemoved(eventType, beListener);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        /// <summary>
        /// 派发事件(多对一的派发)
        /// </summary>
        ///<param name="eventType">事件类型
        public static void Broadcast(NotifyType eventType)
        {
            try
            {
                Delegate d = GetListener(eventType);
                if (d != null)
                {
                    NotifyCallback callBack = d as NotifyCallback;
                    if (callBack != null)
                    {
                        callBack();
                    }
                    else
                    {
                        throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventType));
                    }
                }

            }
            catch (Exception e) {
                Debug.LogError(e);
            }
        }

        //single parameters
        public static void Broadcast<T>(NotifyType eventType, T arg)
        {
            try
            {
                Delegate d = GetListener(eventType);
                if (d != null)
                {
                    NotifyCallback<T> callBack = d as NotifyCallback<T>;
                    if (callBack != null)
                    {
                        callBack(arg);
                    }
                    else
                    {
                        throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventType));
                    }
                }

            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        //two parameters
        public static void Broadcast<T, T1>(NotifyType eventType, T arg1, T1 arg2)
        {
            try
            {
                Delegate d = GetListener(eventType);
                if (d != null)
                {
                    NotifyCallback<T, T1> callBack = d as NotifyCallback<T, T1>;
                    if (callBack != null)
                    {
                        callBack(arg1, arg2);
                    }
                    else
                    {
                        throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventType));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        //three parameters
        public static void Broadcast<T, T1, T2>(NotifyType eventType, T arg1, T1 arg2, T2 arg3)
        {
            try
            {
                Delegate d = GetListener(eventType);
                if (d != null)
                {
                    if (d is NotifyCallback<T, T1, T2> callBack)
                    {
                        callBack(arg1, arg2, arg3);
                    }
                    else
                    {
                        throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventType));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        //four parameters
        public static void Broadcast<T, T1, T2, T3>(NotifyType eventType, T arg1, T1 arg2, T2 arg3, T3 arg4)
        {
            try
            {
                Delegate d = GetListener(eventType);
                if (d != null)
                {
                    if (d is NotifyCallback<T, T1, T2, T3> callBack)
                    {
                        callBack(arg1, arg2, arg3, arg4);
                    }
                    else
                    {
                        throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventType));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        //five parameters
        public static void Broadcast<T, T1, T2, T3, T4>(NotifyType eventType, T arg1, T1 arg2, T2 arg3, T3 arg4, T4 arg5)
        {
            try
            {
                Delegate d = GetListener(eventType);
                if (d != null)
                {
                    if (d is NotifyCallback<T, T1, T2, T3, T4> callBack)
                    {
                        callBack(arg1, arg2, arg3, arg4, arg5);
                    }
                    else
                    {
                        throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventType));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        /// <summary>
        /// 被监听者派发事件，需要知道发送者，具体消息的情况
        /// </summary>
        ///<param name="beListener">发送者
        ///<param name="eventType">事件类型
        //no parameters
        public static void Broadcast(SObject beListener, NotifyType eventType)
        {
            try
            {
                Delegate d = GetListener(eventType, beListener);
                if (d != null)
                {
                    if (d is NotifyCallback callBack)
                    {
                        
                        callBack();
                    }
                    else
                    {
                        throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventType));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        //single parameters
        public static void Broadcast<T>(SObject beListener, NotifyType eventType, T arg)
        {
            try
            {
                Delegate d = GetListener(eventType, beListener);
                if (d != null)
                {
                    if (d is NotifyCallback<T> callBack)
                    {
                        callBack(arg);
                    }
                    else
                    {
                        throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventType));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        //two parameters
        public static void Broadcast<T, T1>(SObject beListener, NotifyType eventType, T arg1, T1 arg2)
        {
            try
            {
                Delegate d = GetListener(eventType, beListener);
                if (d != null)
                {
                    if (d is NotifyCallback<T, T1> callBack)
                    {
                        callBack(arg1, arg2);
                    }
                    else
                    {
                        throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventType));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        //three parameters
        public static void Broadcast<T, T1, T2>(SObject beListener, NotifyType eventType, T arg1, T1 arg2, T2 arg3)
        {
            try
            {
                Delegate d = GetListener(eventType, beListener);
                if (d != null)
                {
                    if (d is NotifyCallback<T, T1, T2> callBack)
                    {
                        callBack(arg1, arg2, arg3);
                    }
                    else
                    {
                        throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventType));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        //four parameters
        public static void Broadcast<T, T1, T2, T3>(SObject beListener, NotifyType eventType, T arg1, T1 arg2, T2 arg3, T3 arg4)
        {
            try
            {
                Delegate d = GetListener(eventType, beListener);
                if (d != null)
                {
                    NotifyCallback<T, T1, T2, T3> callBack = d as NotifyCallback<T, T1, T2, T3>;
                    if (callBack != null)
                    {
                        callBack(arg1, arg2, arg3, arg4);
                    }
                    else
                    {
                        throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventType));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        //five parameters
        public static void Broadcast<T, T1, T2, T3, T4>(SObject beListener, NotifyType eventType, T arg1, T1 arg2, T2 arg3, T3 arg4, T4 arg5)
        {
            try
            {
                Delegate d = GetListener(eventType, beListener);
                if (d != null)
                {
                    NotifyCallback<T, T1, T2, T3, T4> callBack = d as NotifyCallback<T, T1, T2, T3, T4>;
                    if (callBack != null)
                    {
                        callBack(arg1, arg2, arg3, arg4, arg5);
                    }
                    else
                    {
                        throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventType));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}
