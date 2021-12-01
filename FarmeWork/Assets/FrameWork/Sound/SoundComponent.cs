using ShipFarmeWork.Touch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ShipFarmeWork.Sound
{
    /// <summary>
    /// 声音组件
    /// </summary>
    public class SoundComponent : MonoBehaviour
    {
        /// <summary>
        /// 播放类型
        /// </summary>
        public enum PlayTime
        {
            OnLoad,   //当加载出来
            OnShow,   //当展示
            OnClickUI,  //当点击UI
            OnClickNoUI,//当点击非UI
        }

        [Header("播放时机")]
        public PlayTime Time;
        [Header("路径")]
        public string Path;
        [Header("声音名称")]
        public string SoundName;

        private void Start()
        {
            if (Time == PlayTime.OnLoad)
            {
                SoundHelper.PlayEffect(Path, SoundName);
            }
            else if (Time == PlayTime.OnClickUI)
            {
                ShipFarmeWork.UI.EventTriggerListener.AddListenerCSharp(UI.EventTriggerListenerType.onClick, OnClick, gameObject);
            }
            else if (Time == PlayTime.OnClickNoUI)
            {
                ShipFarmeWork.Touch.TouchTrigger.AddListenerFunc(Touch.TouchTriggerType.OnTouchClick, OnClick, gameObject);
            }
        }


        private void OnEnable()
        {
            if (Time == PlayTime.OnShow)
            {
                SoundHelper.PlayEffect(Path, SoundName);
            }
        }


        private void OnClick(GameObject go, TouchData data)
        {
            SoundHelper.PlayEffect(Path, SoundName);
        }
        private void OnClick(GameObject go, BaseEventData data)
        {
            SoundHelper.PlayEffect(Path, SoundName);
        }

    }
}
