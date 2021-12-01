using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ShipFarmeWork.UI
{
    /// <summary>
    /// UI界面基类
    /// </summary>
    public abstract class BaseUIForm : MonoBehaviour
    {
        //此窗体在所有窗体的集合中对应的下标名称
        private string _UIKey;
        public string UIKey { get { return _UIKey; } }

        //UI信息
        public UIInfo UiInfo;

        //这个对象是否是第一次展示(是这个对象,不是这个界面)
        private bool _IsFirstShow = true;

        //该界面是否正在使用中
        private bool _IsUsing = false;
        public bool IsUsing { get { return _IsUsing; } }

        /// <summary>
        /// 层级值
        /// </summary>
        private int _LayerNum;
        public int LayerNum { get { return _LayerNum; } }

        /// <summary>
        /// 当关闭的回调
        /// </summary>
        private Action OnCloseAction;

        /// <summary>
        /// 当首次进入
        /// 用来发现所有要用到的子节点,做一些和数据无关的初始化的操作
        /// 其功能相当于Awake
        /// </summary>
        protected virtual void OnFirstEnter() { }

        /// <summary>
        /// 开始展示
        /// 带着参数然后刷新展示
        /// </summary>
        protected virtual void StartShow(params object[] param) { }

        /// <summary>
        /// 当界面关闭
        /// </summary>
        protected virtual void OnClose() { }


        public void InitBaseInfo(string uiKey, UIInfo uiInfo)
        {
            _UIKey = uiKey;
            UiInfo = uiInfo;
        }

        /// <summary>
        /// 页面展示
        /// </summary>
        public void Display(int layerNum, params object[] param)
        {
            SetLayer(layerNum);

            //正在使用中
            _IsUsing = true;

            //开始显示
            gameObject.SetActive(true);
            if (_IsFirstShow)
            {
                OnFirstEnter();
                _IsFirstShow = false;
            }
            StartShow(param);

            //播放声音
            UISound.OnShowPlaySound(this);
        }

        /// <summary>
        /// 界面关闭
        /// </summary>
        public void Close()
        {
            try
            {
                OnClose();
            }
            catch (Exception e) { Debug.LogError(e); }
            OnCloseAction?.Invoke();

            //关闭,此时才可以从界面缓存中取出这个界面再次使用
            _IsUsing = false;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 关闭UI
        /// </summary>
        /// <param name="bDestroySelf">关闭时是否将自己销毁</param>
        public void CloseUISelf(bool bDestroySelf)
        {
           ShipFarmeWork.UI.UIManager.Instance.CloseUIForm(this, bDestroySelf);
        }

        public void AddCloseAction(Action onClose)
        {
            OnCloseAction += onClose;
        }

        public void RemoveCloseAction(Action onClose)
        {
            OnCloseAction -= onClose;
        }

        /// <summary>
        /// 设置图片资源
        /// </summary>
        /// <param name="img"></param>
        /// <param name="bundle"></param>
        /// <param name="asset"></param>
        public Sprite SetImage(Image img, string resPath, string resName)
        {
            Sprite spring = (Sprite)GetAsset(resPath, resName, typeof(UnityEngine.Sprite));
            img.sprite = spring;
            return spring;
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        public GameObject CloneRes(string resPath, string resName, Transform parent)
        {
            GameObject game = (GameObject)UIResLoad.LoadRes(resPath, resName, typeof(GameObject));
            if (game != null && parent != null)
            {
                game.transform.SetParent(parent);
            }
            return game;
        }

        /// <summary>
        /// 获取资源
        /// </summary>
        public UnityEngine.Object GetAsset(string resPath, string resName, System.Type assetType)
        {
            return UIResLoad.LoadRes(resPath, resName, assetType);
        }

        /// <summary>
        /// 设置层级
        /// </summary>
        private void SetLayer(int layerNum)
        {
            _LayerNum = layerNum;
            UIDepth uiDepth = GetComponent<UIDepth>();
            uiDepth.SetLayerNum(_LayerNum);
        }

    }

}