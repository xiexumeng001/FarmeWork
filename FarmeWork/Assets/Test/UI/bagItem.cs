using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ui
{
    /// <summary>
    /// 背包物品展示
    /// </summary>
    public class bagItem : ScrollViewHandlerItem
    {
        [Header("展示文本")]
        public Text ShowText;

        UIBag uiBag;
        [HideInInspector]
        public string itemName;
        protected override void OnFirstEnter(params object[] param)
        {
            uiBag = (UIBag)param[0];
        }

        protected override void OnShow(object data)
        {
            itemName = (string)data;
            ShowText.text = itemName;
        }

        protected override List<GameObject> GetTransDragEventGame()
        {
            return null;
        }
    }
}
