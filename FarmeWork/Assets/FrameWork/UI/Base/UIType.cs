using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ShipFarmeWork.UI
{
    /// <summary>
    /// UI窗体层级类型
    /// 表示了UI窗体的所有层级,不同层级有不同层级的特点
    /// 后面的int数字是各层级的基本层级数
    /// </summary>
    public enum UILayerType
    {
        //场景的主界面层级-----整个场景的主界面,层级最低
        MainLayer = 1000,
        //普通层级-------------普通界面,如背包,任务等界面
        NormalLayer = 2000,
        //Loading层级----------等待界面,遮住普通界面,不遮住栈界面
        LoadingLayer = 3000,
        //栈层级---------------栈界面,可展示各种提示界面
        PopLayer = 4000,
        //顶层层级-------------最顶层界面
        TopLayer = 5000,
    }


    /// <summary>
    /// 栈层级的遮罩透明度
    /// 定义弹出"模态窗体"不同透明度的类型,对于“弹出窗体”往往因为需要玩家优先处理弹出小窗体，则要求玩家不能(无法)点击“父窗体”
    /// 实现原理:在弹出窗体的后面增加一层“UI遮罩窗体”，当需要弹出特定模态窗体时，脚本自动控制“UI遮罩窗体”的“层级”，
    ///          把弹出模特窗体与普通窗体之间进行隔离，起到突出显示与遮挡用户点击其他窗体的作用
    /// </summary>
    public enum PopMaskLucenyType
    {
        //无遮罩
        None,
        //完全透明
        Lucency,
        //半透明
        Translucence,
        //低透明度
        ImPenetrable,
    }
}