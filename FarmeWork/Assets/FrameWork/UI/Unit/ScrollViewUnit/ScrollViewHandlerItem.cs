using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SObj = System.Object;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using ShipFarmeWork.UI;

/// <summary>
/// 滑动条道具
/// </summary>
public abstract class ScrollViewHandlerItem : MonoBehaviour
{
    [HideInInspector]
    public int Index = -1;

    private bool isFirstShow = true;
    private bool isShowing;
    
    [HideInInspector]
    public ScrollViewHandle ScrollHandle;

    /// <summary>
    /// 当首次进入
    /// </summary>
    protected virtual void OnFirstEnter(params object[] param) { }

    /// <summary>
    /// 当展示
    /// </summary>
    /// <param name="data"></param>
    protected virtual void OnShow(SObj data) { }
 
    /// <summary>
    /// 当关闭
    /// </summary>
    protected virtual void OnClose() { }

    /// <summary>
    /// 获取需要传递拖拽物体的集合
    /// 如果无限滚动的格子添加了触发事件,那么就会阻挡拖拽ScrollView的拖拽事件,导致ScrollView无法拖拽
    /// 所以需要统计添加事件的节点, 当此节点接收到拖拽时间时, 就传递给ScrollView
    /// </summary>
    /// <returns></returns>
    protected abstract List<GameObject> GetTransDragEventGame();

    /// <summary>
    /// 展示
    /// </summary>
    public void Show(SObj data, params object[] initParam)
    {
        isShowing = true;

        if (isFirstShow)
        {
            isFirstShow = false;
            OnFirstEnter(initParam);

            List<GameObject> gameList = GetTransDragEventGame();
            if (gameList != null)
            {//添加拖动传递
                for (int i = 0; i < gameList.Count; i++)
                {
                    AddDragTranToGame(gameList[i]);
                }
            }
        }
        OnShow(data);
    }

    /// <summary>
    /// 关闭
    /// </summary>
    public void Close()
    {
        Index = -1;
        isShowing = false;

        try
        {
            OnClose();
        }
        catch (Exception e) { Debug.LogError(e); }

        if (gameObject != null)
        {
            gameObject.SetActive(false);
            gameObject.name = Index.ToString();
        }
    }

    private void AddDragTranToGame(GameObject game)
    {
        EventTriggerListener.AddListenerCSharp(EventTriggerListenerType.onBeginDrag, OnBeginDrag, game);
        EventTriggerListener.AddListenerCSharp(EventTriggerListenerType.onDrag, OnDrag, game);
        EventTriggerListener.AddListenerCSharp(EventTriggerListenerType.onEndDrag, OnEndDrag, game);
    }

    /// <summary>
    /// 当开始拖拽
    /// </summary>
    /// <param name="go"></param>
    /// <param name="data"></param>
    private void OnBeginDrag(GameObject go, BaseEventData data)
    {
        ExecuteEvents.Execute(ScrollHandle.ScrollRect.gameObject, data, ExecuteEvents.beginDragHandler);
    }

    /// <summary>
    /// 被拖拽
    /// </summary>
    /// <param name="go"></param>
    /// <param name="data"></param>
    private void OnDrag(GameObject go, BaseEventData data)
    {
        ExecuteEvents.Execute(ScrollHandle.ScrollRect.gameObject, data, ExecuteEvents.dragHandler);
    }

    /// <summary>
    /// 当结束拖拽
    /// </summary>
    /// <param name="go"></param>
    /// <param name="data"></param>
    private void OnEndDrag(GameObject go, BaseEventData data)
    {
        ExecuteEvents.Execute(ScrollHandle.ScrollRect.gameObject, data, ExecuteEvents.endDragHandler);
    }
}
