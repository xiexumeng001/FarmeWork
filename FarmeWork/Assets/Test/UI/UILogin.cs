using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using ShipFarmeWork.UI;
using UnityEngine.EventSystems;

namespace ui
{
    /// <summary>
    /// 登录界面
    /// </summary>
    public class UILogin : BaseUIForm
    {
        [Header("背景物体")]
        public GameObject BgGame;
        [Header("账号输入")]
        public InputField IdInput;
        [Header("密码输入")]
        public InputField PassInput;
        [Header("登录按钮")]
        public GameObject LoginBtn;
        [Header("关闭且销毁按钮")]
        public GameObject CloseAndDesBtn;
        [Header("关闭不销毁按钮")]
        public GameObject CloseNoDesBtn;

        protected override void OnFirstEnter()
        {
            Debug.Log("首次打开界面");
            //添加点击事件
            EventTriggerListener.AddListenerCSharp(EventTriggerListenerType.onClick, OnClickLoginBtn, LoginBtn);
            EventTriggerListener.AddListenerCSharp(EventTriggerListenerType.onClick, OnClickCloseAndDes, CloseAndDesBtn);
            EventTriggerListener.AddListenerCSharp(EventTriggerListenerType.onClick, OnClickCloseNoDesBtn, CloseNoDesBtn);
        }

        protected override void StartShow(params object[] param)
        {
            Debug.Log("开始展示界面");

            int num = (int)param[0];
            Debug.LogFormat("传入参数是:{0}", num);

            RectTransform bgTran = (RectTransform)BgGame.transform;
            Debug.LogFormat("背景宽高:{0},{1}", bgTran.rect.width, bgTran.rect.height);
        }

        protected override void OnClose()
        {
            Debug.Log("当关闭界面");
        }

        /// <summary>
        /// 当点击登录按钮
        /// </summary>
        private void OnClickLoginBtn(GameObject go, BaseEventData data)
        {
            string id = IdInput.text;
            string password = PassInput.text;
            //判断是否正确

            //Debug.Log("开始登录");
            //UIManager.Instance.ShowUIForms("UITips", "点击了登录按钮");

            //UIQueue uiQue = new UIQueue();
            //uiQue.AddUI("UITips", "测试下遮罩界面");
            //uiQue.AddUI("UITips", "测试下队列");

            ShipFarmeWork.UI.UIManager.Instance.ShowUIForms("UIBag");
            //ShipFarmeWork.UI.UIManager.Instance.ShowUIForms("UIBag");
        }

        private void OnClickCloseAndDes(GameObject go, BaseEventData data)
        {
            Debug.Log("关闭自己");
            CloseUISelf(true);
        }

        private void OnClickCloseNoDesBtn(GameObject go, BaseEventData data)
        {
            Debug.Log("关闭不销毁自己");
            CloseUISelf(false);
        }

    }
}