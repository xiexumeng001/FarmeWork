using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using ShipFarmeWork.UI;
using ui;
using UnityEngine.EventSystems;

public class UITips : BaseUIForm
{
    [Header("提示文本")]
    public Text TipsText;
    [Header("确定按钮")]
    public GameObject OkGame;

    protected override void OnFirstEnter()
    {
        EventTriggerListener.AddListenerCSharp(EventTriggerListenerType.onClick, OnClickOk, OkGame);
    }

    protected override void StartShow(params object[] param)
    {
        string tips = (string)param[0];
        TipsText.text = tips;
    }


    private void OnClickOk(GameObject go, BaseEventData data)
    {
        CloseUISelf(true);
    }

}
