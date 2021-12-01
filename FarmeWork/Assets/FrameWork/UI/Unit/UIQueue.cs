using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShipFarmeWork.UI
{
    /// <summary>
    /// UI队列
    /// </summary>
    public class UIQueue
    {
        public class UIQueArgs
        {
            public string uiKey;
            public object[] param;
        }

        private List<UIQueArgs> _UIQue = new List<UIQueArgs>();
        private BaseUIForm CurrShowUi;

        /// <summary>
        /// 添加UI
        /// </summary>
        /// <param name="uiForm"></param>
        public void AddUI(string uiKey, params object[] param)
        {
            UIQueArgs args = new UIQueArgs();
            args.uiKey = uiKey;
            args.param = param;

            _UIQue.Add(args);
            if (_UIQue.Count == 1) { ShowUI(_UIQue[0]); }
        }

        /// <summary>
        /// 移除第一个UI
        /// </summary>
        public void RemoveShowUI()
        {
            CurrShowUi.RemoveCloseAction(RemoveShowUI);
            CurrShowUi = null;

            _UIQue.RemoveAt(0);
            if (_UIQue.Count > 0) { ShowUI(_UIQue[0]); }
        }

        /// <summary>
        /// 展示UI
        /// </summary>
        /// <param name="args"></param>
        private void ShowUI(UIQueArgs args)
        {
            CurrShowUi = UIManager.Instance.ShowUIForms(args.uiKey, args.param);
            CurrShowUi.AddCloseAction(RemoveShowUI);
        }
    }
}
