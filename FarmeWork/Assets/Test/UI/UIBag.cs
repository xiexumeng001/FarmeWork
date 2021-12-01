using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ShipFarmeWork.UI;
using UnityEngine.EventSystems;

namespace ui
{
    /// <summary>
    /// 背包界面
    /// </summary>
    public class UIBag : BaseUIForm
    {
        [Header("背包物体所在的滑动条")]
        public ScrollViewHandle ScrollViewHandle;
        [Header("关闭按钮")]
        public GameObject CloseBtn;
        [Header("添加道具按钮")]
        public GameObject AddItemBtn;
        [Header("移除道具按钮")]
        public GameObject RemoveItemBtn;

        List<string> bagList = new List<string>();
        protected override void OnFirstEnter()
        {
            ScrollViewHandle.Init("Assets/Test/UI", "bagItem.prefab", typeof(bagItem), this);

            EventTriggerListener.AddListenerCSharp(EventTriggerListenerType.onClick, OnClickCloseBtn, CloseBtn);
            EventTriggerListener.AddListenerCSharp(EventTriggerListenerType.onClick, OnClickAddItem, AddItemBtn);
            EventTriggerListener.AddListenerCSharp(EventTriggerListenerType.onClick, OnClickRemoveItem, RemoveItemBtn);
        }

        protected override void StartShow(params object[] param)
        {
            for (int i = 0; i < 15; i++)
            {
                bagList.Add("物品" + i);
            }
            ScrollViewHandle.Show(bagList, 0);

            RectTransform rectTran = (RectTransform)ScrollViewHandle.gameObject.transform;
            Debug.LogWarning("StartShow宽,高:" + rectTran.rect.width + "  " + rectTran.rect.height);
        }

        private void Update()
        {
            RectTransform rectTran = (RectTransform)ScrollViewHandle.gameObject.transform;
            Debug.LogWarning("宽,高:" + rectTran.rect.width + "  " + rectTran.rect.height);
        }

        /// <summary>
        /// 添加道具
        /// </summary>
        private void AddItem()
        {
            bagList.Add("物品" + bagList.Count);
            ScrollViewHandle.Show(bagList, true);
        }

        private void RemoveItem()
        {
            bagList.RemoveAt(bagList.Count - 1);
            ScrollViewHandle.Show(bagList, true);
        }

        /// <summary>
        /// 当点击登录按钮
        /// </summary>
        private void OnClickCloseBtn(GameObject go, BaseEventData data)
        {
            CloseUISelf(true);
        }

        private void OnClickAddItem(GameObject go, BaseEventData data)
        {
            AddItem();
        }

        private void OnClickRemoveItem(GameObject go, BaseEventData data)
        {
            RemoveItem();
        }
    }
}
