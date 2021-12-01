using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using UnityEngine.UI;

/// <summary>
/// 适配类型
/// </summary>
public enum ScrollViewHandleAdapterType
{
    NumPerLine,          //每行数量
    WidthHight,          //宽高
    Scale,               //缩放
}

[Serializable]
public class ScrollViewHandleAdaptInfo
{
    [Header("适配类型")]
    public ScrollViewHandleAdapterType Type;  //适配类型

    [Header("道具距Content的间隔")]
    public Vector2 ContentSpace;       //距视图的间隔

    [Header("一行(列)几个")]
    [HideInInspector]
    public int PerLineMaxCount;        //每行的数量

    [HideInInspector]
    public Vector2 RealScaleRate = new Vector2(1, 1);      //实际的屏幕分辨率与参考屏幕分辨率的比值
    private ScrollViewHandle Handle;

    /// <summary>
    /// 检查是否可以适配
    /// </summary>
    public void CheckCanApdater(RectTransform mask)
    {
        //适配
        if (mask.anchorMin != new Vector2(0, 0) || mask.anchorMax != new Vector2(1, 1))
        {
            Debug.LogError("Viewport 的锚点设置不对,不可适配");
        }
    }

    //适配
    public void Adapter(ScrollViewHandle handle)
    {
        Handle = handle;
        switch (Type)
        {
            case ScrollViewHandleAdapterType.NumPerLine:
                NumApdat();
                break;
            case ScrollViewHandleAdapterType.Scale:
            case ScrollViewHandleAdapterType.WidthHight:
                ScaleOrWidHighAdapt();
                break;
        }
    }

    /// <summary>
    /// 数量适配
    /// </summary>
    private void NumApdat()
    {
        if (Handle.ScrollRect.horizontal)
        {
            //计算每行数量
            float realHeight = Handle.Mask.rect.height;
            float cellAllHeight = Handle.CellSize.y + Handle.CellSpaceSize.y;
            PerLineMaxCount = (int)((realHeight + Handle.CellSpaceSize.y) / cellAllHeight);
            if (PerLineMaxCount < 1) PerLineMaxCount = 1;

            //计算距离Content的间隔
            float occuWidth = PerLineMaxCount * Handle.CellSize.y + (PerLineMaxCount - 1) * Handle.CellSpaceSize.y;
            float space = (realHeight - occuWidth) / 2;
            if (ContentSpace.x < space)
            {
                ContentSpace.x = space;
            }
        }
        else
        {
            //计算每行数量
            float realWidth = Handle.Mask.rect.width;
            float cellAllWidth = Handle.CellSize.x + Handle.CellSpaceSize.x;
            PerLineMaxCount = (int)((realWidth + Handle.CellSpaceSize.x) / cellAllWidth);
            if (PerLineMaxCount < 1) PerLineMaxCount = 1;

            //计算距离Content的间隔
            float occuWidth = PerLineMaxCount * Handle.CellSize.x + (PerLineMaxCount - 1) * Handle.CellSpaceSize.x;
            float space = (realWidth - occuWidth) / 2;
            if (ContentSpace.x < space)
            {
                ContentSpace.x = space;
            }
        }
    }

    /// <summary>
    /// 缩放或者宽高适配
    /// </summary>
    /// <returns></returns>
    private void ScaleOrWidHighAdapt()
    {
        //若没有父级
        CanvasScaler canvaScale = Handle.GetComponentInParent<CanvasScaler>();
        if (canvaScale == null) return;

        //若是缩放适配,则一行一个,一个多个的缩放适配暂无规则,下面的计算也是默认1行1个来的
        //且不适配拖拽方向
        PerLineMaxCount = 1;
        Vector2 offsetRate = Handle.CellSize / Handle.CellOffSet;

        if (Handle.ScrollRect.horizontal)
        {
            float height = Handle.Mask.rect.height - ContentSpace.y * 2;
            RealScaleRate.y = height / Handle.CellSize.y;
            RealScaleRate.x = 1;

            Handle.CellSize = new Vector2(Handle.CellSize.x * RealScaleRate.x, height);
        }
        else
        {
            float width = Handle.Mask.rect.width - ContentSpace.x * 2;
            RealScaleRate.x = width / Handle.CellSize.x;
            RealScaleRate.y = 1;

            Handle.CellSize = new Vector2(width, Handle.CellSize.y * RealScaleRate.y);
        }

        Handle.CellOffSet = Handle.CellSize / offsetRate;
    }

}