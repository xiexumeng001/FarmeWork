using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace ShipFarmeWork.UI
{
    /// <summary>
    /// ui拖拽组件,挂上即可拖拽
    /// </summary>
    public class DragUI : MonoBehaviour
    {
        [Header("所在界面的Canvas")]
        public Canvas UICanvas;

        private RectTransform rectTran;
        private Vector2 Pos_Ori;

        private Vector2 offSet;
        private Action StartDragAction;
        private Action OnDragAction;
        private Action EndDragAction;
        private Func<bool> IsCanDragAction;

        private bool isDraging;

        // Start is called before the first frame update
        void Start()
        {
            if (transform is RectTransform)
            {
                rectTran = (RectTransform)transform;
            }
            else {
                Debug.LogError("该物体不是UI物体");
                return;
            }
            Pos_Ori = transform.localPosition;

            //添加拖拽时间
            EventTriggerListener.AddListenerCSharp(EventTriggerListenerType.onBeginDrag, StartDrag, gameObject);
            EventTriggerListener.AddListenerCSharp(EventTriggerListenerType.onDrag, OnDrag, gameObject);
            EventTriggerListener.AddListenerCSharp(EventTriggerListenerType.onEndDrag, EndDrag, gameObject);
        }

        public void AddAction(Action startDrag, Action onDrag, Action endDrag, Func<bool> isCanDrag)
        {
            StartDragAction = startDrag;
            OnDragAction = onDrag;
            EndDragAction = endDrag;
            IsCanDragAction = isCanDrag;
        }

        /// <summary>
        /// 开始拖拽
        /// </summary>
        private void StartDrag(GameObject go, BaseEventData data)
        {
            if (IsCanDragAction != null && !IsCanDragAction()) return;
            PointerEventData p_data = (PointerEventData)data;
            if (p_data != null)
            {
                offSet = GetUIOffSetByScreenPos(p_data.position);
                if (StartDragAction != null) {
                    StartDragAction();
                }
                isDraging = true;
            }
        }

        /// <summary>
        /// 当拖拽
        /// </summary>
        private void OnDrag(GameObject go, BaseEventData data)
        {
            if (IsCanDragAction != null && !IsCanDragAction()) return;
            PointerEventData p_data = (PointerEventData)data;
            if (p_data != null)
            {
                SetUIByScreenPos(p_data.position);
                if (OnDragAction != null)
                {
                    OnDragAction();
                }
                isDraging = true;
            }
        }

        /// <summary>
        /// 当拖住结束
        /// </summary>
        private void EndDrag(GameObject go, BaseEventData data)
        {
            if (!isDraging) return;
            transform.localPosition = Pos_Ori;
            offSet = Vector2.zero;
            if (EndDragAction != null) { EndDragAction(); }
            isDraging = false;
        }


        public Vector2 GetUIOffSetByScreenPos(Vector3 point)
        {
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(UICanvas.transform as RectTransform, point, UICanvas.worldCamera, out pos);
            return rectTran.anchoredPosition - pos;
        }


        /// <summary>
        /// 通过屏幕坐标设置UI坐标
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="point"></param>
        /// <param name="rect"></param>
        public void SetUIByScreenPos(Vector3 point)
        {
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(UICanvas.transform as RectTransform, point, UICanvas.worldCamera, out pos);
            rectTran.anchoredPosition = pos + offSet;
        }
    }
}
