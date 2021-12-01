using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using SObj = System.Object;

using UnityEngine.UI;
using ShipFarmeWork.UI;

/// <summary>
/// 无限滚动脚本
/// 1、当List数据改变,增减了一两个,如何更新呢？？
/// 2、异步加载优化
/// </summary>
[RequireComponent(typeof(ScrollRect))]
public class ScrollViewHandle : MonoBehaviour
{
    //格子宽高与对应的间隔
    [Header("道具宽高")]
    public Vector2 CellSize;           //格子大小
    [Header("道具偏移")]
    public Vector2 CellOffSet;         //格子偏移
    [Header("道具间隔")]
    public Vector2 CellSpaceSize;      //格子间隔距离

    [SerializeField]
    [Header("适配信息")]
    public ScrollViewHandleAdaptInfo AdapterInfo;  //适配信息

    //格子占用大小
    private Vector2 CellOccuSize;

    [HideInInspector]
    public RectTransform Content;
    [HideInInspector]
    public RectTransform Mask;
    [HideInInspector]
    public ScrollRect ScrollRect;

    private IList _DataList;            //展示的数据
    private int _CurCellCount;          //数据数量
    private int _MaxShowCellCount = 0;  //同时展示的最大数量
    private float _ScrollViewLength;    //scrollview的显示长度

    //上一次更新的首行
    private int firstLine_ago = -1;
    //当前展示的开始下标
    private int _ShowFirstIndex;
    public int ShowFirstIndex { get { return _ShowFirstIndex; } }
    //当前展示的结束下标
    private int _ShowEndIndex;
    public int ShowEndIndex { get { return _ShowEndIndex; } }

    //Cell信息
    private string poolItemBundleName;
    private string poolItemAssetName;
    private string strLuaScripPath = string.Empty;
    private Type ItemComponentType;
    private object[] _InitItemParam;

    //池子
    private List<ScrollViewHandlerItem> m_privatePool = new List<ScrollViewHandlerItem>();
    //正在展示的,注意:这个集合里的item无序,和_DataList的顺序对应不上
    private List<ScrollViewHandlerItem> m_ShowItem = new List<ScrollViewHandlerItem>();
    public List<ScrollViewHandlerItem> ShowItemList { get { return m_ShowItem; } }

    public Action OnShowUpdateAction;   //当展示更新的回调

    private void OnDestroy()
    {
        foreach (var item in m_ShowItem)
        {
            item.Close();
            UIResLoad.UnLoadGameObject(item.gameObject);
        }
        foreach (var item in m_privatePool)
        {
            UIResLoad.UnLoadGameObject(item.gameObject);
        }
    }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="abName">资源bundle</param>
    /// <param name="assetName">资源名称</param>
    /// <param name="capacity">初始池子大小</param>
    /// <param name="_starIndex">开始位置</param>
    public void Init(string path, string assetName, Type componentType, params object[] param)
    {
        poolItemBundleName = path;
        poolItemAssetName = assetName;
        ItemComponentType = componentType;
        _InitItemParam = param;

        ScrollRect = GetComponent<ScrollRect>();
        Mask = (RectTransform)GetComponentInChildren<Mask>().transform;
        Content = ScrollRect.content;

        //看下有没有问题
        JudgeError();

        //设置content
        Content.anchoredPosition = new Vector2(0, 0);
        Content.anchorMin = new Vector2(0, 1);
        Content.anchorMax = new Vector2(0, 1);
        //Content.sizeDelta = new Vector2(0, 0);
        Content.pivot = new Vector2(0, 1);

        ScrollRect.onValueChanged.AddListener(OnValueChagned);

        //适配下
        AdapterInfo.Adapter(this);

        //计算展示长度与展示数量
        CellOccuSize = new Vector2(CellSize.x + CellSpaceSize.x, CellSize.y + CellSpaceSize.y);
        _ScrollViewLength = ScrollRect.horizontal ? Mask.rect.width : Mask.rect.height;
        float cellLength = ScrollRect.horizontal ? CellOccuSize.x : CellOccuSize.y;
        _MaxShowCellCount = Mathf.CeilToInt(_ScrollViewLength / cellLength + 1) * AdapterInfo.PerLineMaxCount;
    }

    /// <summary>
    /// 初始化（专门用于lua调用）
    /// </summary>
    /// <param name="abName">资源bundle</param>
    /// <param name="assetName">资源名称</param>
    /// <param name="componentType"></param>
    /// <param name="strLuaScriptPath"></param>
    /// <param name="param"></param>
    public void LuaInit(string abName, string assetName, Type componentType, string strLuaScriptPath, params object[] param)
    {
        strLuaScripPath = strLuaScriptPath;
        Init(abName, assetName, componentType, param);
    }


    /// <summary>
    /// 判断有没有问题
    /// </summary>
    private void JudgeError()
    {
        HorizontalOrVerticalLayoutGroup horVerCom = Content.GetComponent<HorizontalOrVerticalLayoutGroup>();
        GridLayoutGroup gridLayout = Content.GetComponent<GridLayoutGroup>();
        if (horVerCom != null)
        {
            if (horVerCom.enabled) throw new Exception("Content 不能挂在 排序组件,会出问题,物体是 " + gameObject.name);
            Debug.LogWarning("可以把 content的排序组件丢掉了,物体是 " + gameObject.name);
        }
        if (gridLayout != null)
        {
            if (gridLayout.enabled) throw new Exception("Content 不能挂在 排序组件,会出问题,物体是 " + gameObject.name);
            Debug.LogWarning("可以把 content的排序组件丢掉了,物体是 " + gameObject.name);
        }

        //检查适配
        AdapterInfo.CheckCanApdater(Mask);
    }

    /// <summary>
    /// 展示
    /// </summary>
    /// <param name="showData"></param>
    /// <param name="startIndex">开始下标</param>
    public void Show(IList showData, int startIndex)
    {
        _DataList = showData;
        _CurCellCount = showData.Count;

        RefreshAll(startIndex);
    }

    /// <summary>
    /// 展示
    /// </summary>
    /// <param name="showData"></param>
    /// <param name="isKeepPos">是否保持当前滑动的位置</param>
    public void Show(IList showData, bool isKeepPos)
    {
        _DataList = showData;
        _CurCellCount = showData.Count;

        int startIndex = isKeepPos ? ShowFirstIndex : 0;
        if (startIndex < 0) startIndex = 0;

        Vector3 oldPos = Content.anchoredPosition;
        RefreshAll(startIndex);
        ScrollToPosition(oldPos.y);
    }

    /// <summary>
    /// 刷新全部
    /// </summary>
    private void RefreshAll(int startIndex)
    {
        //更新大小
        UpdateContentSize();

        //关闭所有的展示道具
        for (int i = m_ShowItem.Count - 1; i >= 0; i--)
        {
            RemoveShowItem(i);
        }
        //开始展示新的道具
        startIndex = Mathf.Clamp(startIndex, 0, _CurCellCount - 1);
        int startLine = GetShowFirstLineFromIndex(startIndex);
        startIndex = startLine * AdapterInfo.PerLineMaxCount;
        int endIndex = Mathf.Clamp(startIndex + _MaxShowCellCount, 0, _CurCellCount);
        for (int i = startIndex; i < endIndex; i++)
        {
            GetItemFormPool(i);
        }
        //更新首行记录
        firstLine_ago = startLine;
        _ShowFirstIndex = startIndex;
        _ShowEndIndex = endIndex - 1;

        //移动至开始位置
        ScrollToIndex(startIndex, false);
    }

    //暂时不考虑动画直接跳转
    //跳转到索引
    /// <summary>
    /// 滑动至某个下标
    /// </summary>
    /// <param name="cellIdx"></param>
    /// <param name="anim">是否平滑的滑动过去</param>
    public void ScrollToIndex(int cellIdx, bool anim)
    {
        cellIdx = Mathf.Clamp(cellIdx, 0, _CurCellCount - 1);
        int line = cellIdx / AdapterInfo.PerLineMaxCount;
        if (ScrollRect.horizontal)
        {
            if (_ScrollViewLength >= Content.sizeDelta.x)
                Content.anchoredPosition = Vector2.zero;
            else
            {
                //最后加的间距只是好看些
                float px = -1 * ((CellOccuSize.x * line)) + AdapterInfo.ContentSpace.x;

                if (-px + _ScrollViewLength > Content.sizeDelta.x)
                {
                    px = -(Content.sizeDelta.x - _ScrollViewLength);
                }
                Content.anchoredPosition = new Vector2(px, 0);
            }
        }
        else
        {
            if (_ScrollViewLength >= Content.sizeDelta.y)
                Content.anchoredPosition = Vector2.zero;
            else
            {
                //最后减的间距只是好看些
                float py = (CellOccuSize.y * line) - AdapterInfo.ContentSpace.y;

                if (py + _ScrollViewLength > Content.sizeDelta.y)
                {
                    py = Content.sizeDelta.y - _ScrollViewLength;
                }
                Content.anchoredPosition = new Vector2(0f, py);
            }
        }
        UpdateShow();
    }

    /// <summary>
    /// 移动到指定位置
    /// </summary>
    /// <param name="pos"></param>
    public void ScrollToPosition(float pos)
    {
        if (ScrollRect.horizontal)
        {
            Content.anchoredPosition = new Vector2(pos, 0);
        }
        else
        {
            Content.anchoredPosition = new Vector2(0, pos);
        }
        UpdateShow();
    }

    /// <summary>
    /// 通过数据获取item
    /// </summary>
    public ScrollViewHandlerItem GetHandleItem(SObj data)
    {
        if (Content == null) return null;
        int index = _DataList.IndexOf(data);
        Transform itemTran = Content.Find(index.ToString());
        if (itemTran == null)
        {
            return null;
        }
        return itemTran.GetComponent<ScrollViewHandlerItem>();
    }

    //当滑动
    private void OnValueChagned(Vector2 data)
    {
        UpdateShow();
    }

    /// <summary>
    /// 更新content大小
    /// </summary>
    private void UpdateContentSize()
    {
        int lineCount = Mathf.Max(Mathf.CeilToInt((float)_CurCellCount / AdapterInfo.PerLineMaxCount), 0);

        float cellAllHeight = CellSize.y + CellSpaceSize.y;
        float cellAllWidth = CellSize.x + CellSpaceSize.x;
        if (ScrollRect.horizontal)
        {
            Content.sizeDelta = new Vector2(
               cellAllWidth * lineCount - CellSpaceSize.x + AdapterInfo.ContentSpace.x * 2,
               Mask.rect.height
                //cellAllHeight * perLineMaxCount - CellSpaceSize.y + ContentSpace.y * 2
                );
        }
        else
        {
            Content.sizeDelta = new Vector2(
                //cellAllWidth * perLineMaxCount - CellSpaceSize.x + ContentSpace.x * 2,
                Mask.rect.width,
                cellAllHeight * lineCount - CellSpaceSize.y + AdapterInfo.ContentSpace.y * 2
                );
        }
    }

    /// <summary>
    /// 更新展示
    /// </summary>
    private void UpdateShow()
    {
        if (_CurCellCount == 0) return;
        int firstLine = GetCurScrollPerLine();
        if (firstLine == firstLine_ago) return;

        //现在的区间
        int startIndex = firstLine * AdapterInfo.PerLineMaxCount;   //起始id
        int endIndex = Mathf.Min(startIndex + _MaxShowCellCount - 1, _CurCellCount - 1);      //结束id

        //之前的区间
        int startIndex_ago = firstLine_ago * AdapterInfo.PerLineMaxCount;   //起始id
        int endIndex_ago = Mathf.Min(startIndex_ago + _MaxShowCellCount - 1, _CurCellCount - 1);      //结束id

        //获取展示与关闭的区间
        int close_startIndex;
        int close_endIndex;
        int show_startIndex;
        int show_endIndex;
        if (startIndex > startIndex_ago)
        {
            close_startIndex = startIndex_ago;
            close_endIndex = startIndex - 1;
            //这儿防止一帧内拖的长度比整个展示区间都长的情况
            show_startIndex = Mathf.Max(endIndex_ago + 1, startIndex);
            show_endIndex = endIndex;
        }
        else
        {
            close_startIndex = endIndex + 1;
            close_endIndex = endIndex_ago;

            show_startIndex = startIndex;
            //这儿防止一帧内拖的长度比整个展示区间都长的情况
            show_endIndex = Mathf.Min(startIndex_ago - 1, endIndex);
        }

        //先关闭
        for (int i = m_ShowItem.Count - 1; i >= 0; i--)
        {
            ScrollViewHandlerItem item = m_ShowItem[i];
            int index = item.Index;
            if ((index >= close_startIndex) && (index <= close_endIndex))
            {
                RemoveShowItem(i);
            }
        }

        //在展示
        for (int i = show_startIndex; i <= show_endIndex; i++)
        {
            GetItemFormPool(i);
        }
        if (m_ShowItem.Count > _MaxShowCellCount)
        {
            //Debug.LogWarning(str + closeStr + showStr);
            //Debug.LogWarningFormat("当前数量是,{0}当前第一行是{1},展示区间{2}-{3},结束区间 {4}-{5} ", m_ShowItem.Count, firstLine, show_startIndex, show_endIndex, close_startIndex, close_endIndex);
        }

        firstLine_ago = firstLine;
        _ShowFirstIndex = startIndex;
        _ShowEndIndex = endIndex;

        //当展示更新的回调
        if (OnShowUpdateAction != null) { OnShowUpdateAction(); }
    }


    /// <summary>
    /// 获取当前展示的第一行
    /// </summary>
    /// <returns></returns>
    private int GetCurScrollPerLine()
    {
        int firstLine = 0;
        if (ScrollRect.vertical)
        {
            float py = Mathf.Max(Content.anchoredPosition.y, 0f);
            firstLine = Mathf.FloorToInt(py / (CellSize.y + CellSpaceSize.y));
        }
        else if (ScrollRect.horizontal)
        {
            float px = Mathf.Abs(Mathf.Min(Content.anchoredPosition.x, 0f));
            firstLine = Mathf.FloorToInt(px / (CellSize.x + CellSpaceSize.x));
        }

        //判断是否到了最低端了,如果是,那么把首行反推下
        int startIndex = firstLine * AdapterInfo.PerLineMaxCount;   //起始id
        int endIndex = startIndex + _MaxShowCellCount - 1;
        if (endIndex > _CurCellCount - 1)
        {//如果结果下标 超过了数据最大值,那么从结束下表反推出开始的下标
            endIndex = _CurCellCount - 1;
            firstLine = GetShowFirstLineFromIndex(endIndex);
        }

        return firstLine;
    }


    /// <summary>
    /// 获取下标对应的能展示的首行
    /// </summary>
    /// <returns></returns>
    private int GetShowFirstLineFromIndex(int index)
    {
        int maxStartIndex = _CurCellCount - _MaxShowCellCount + 1;
        maxStartIndex = Mathf.Clamp(maxStartIndex, 0, _CurCellCount - 1);
        if (index < 0) index = 0;
        index = Mathf.Min(index, maxStartIndex);
        int line = Mathf.CeilToInt((float)index / AdapterInfo.PerLineMaxCount);
        return line;
    }

    //创建新的道具对象
    private ScrollViewHandlerItem CreateNewItem()
    {
        GameObject game = game = (GameObject)UIResLoad.LoadRes(poolItemBundleName, poolItemAssetName, typeof(GameObject));
        game.transform.SetParent(Content, false);

        if (AdapterInfo.Type == ScrollViewHandleAdapterType.Scale)
        {
            float saleX = game.transform.localScale.x * AdapterInfo.RealScaleRate.x;
            float saleY = game.transform.localScale.y * AdapterInfo.RealScaleRate.y;
            game.transform.localScale = new Vector3(saleX, saleY, game.transform.localScale.z);
        }
        else if (AdapterInfo.Type == ScrollViewHandleAdapterType.WidthHight)
        {
            RectTransform rectTransform = (RectTransform)game.transform;
            rectTransform.sizeDelta = CellSize;
        }

        ScrollViewHandlerItem item = null;
        if (string.IsNullOrEmpty(strLuaScripPath) == false)
        {
            //hack【UI】【XLua】
            //item = XLuaHelper.AddUIComponent<XLuaScrollViewHandlerItem>(game, strLuaScripPath);
        }
        else
        {
            item = (ScrollViewHandlerItem)game.GetComponent(ItemComponentType);
            if (item == null)
            {
                item = (ScrollViewHandlerItem)(game.AddComponent(ItemComponentType));
            }
        }
        
        game.SetActive(false);
        //装入池子
        m_privatePool.Add(item);
        return item;
    }

    /// <summary>
    /// 从池子中获取一个物体
    /// </summary>
    /// <returns></returns>
    private ScrollViewHandlerItem GetItemFormPool(int index)
    {
        if (m_privatePool.Count == 0)
        {
            CreateNewItem();
        }

        ScrollViewHandlerItem item = m_privatePool[m_privatePool.Count - 1];
        m_privatePool.RemoveAt(m_privatePool.Count - 1);
        m_ShowItem.Add(item);

        //设置名称、位置、缩放等
        item.name = index.ToString();
        item.transform.localPosition = GetPosByIndex(index);
        item.gameObject.SetActive(true);

        //开始展示
        item.Index = index;
        item.ScrollHandle = this;
        item.Show(_DataList[index], _InitItemParam);
        return item;
    }


    /// <summary>
    /// 通过下标获取位置
    /// </summary>
    public Vector2 GetPosByIndex(int index)
    {
        int row = Mathf.FloorToInt(index * 1f / AdapterInfo.PerLineMaxCount);
        int column = index % AdapterInfo.PerLineMaxCount;
        if (ScrollRect.horizontal)
        {
            return new Vector2(
                CellOffSet.x + row * CellOccuSize.x + AdapterInfo.ContentSpace.x,
                -CellOffSet.y - column * CellOccuSize.y - AdapterInfo.ContentSpace.y);
        }
        else
        {
            return new Vector2(
                CellOffSet.x + column * CellOccuSize.x + AdapterInfo.ContentSpace.x,
                -CellOffSet.y - row * CellOccuSize.y - AdapterInfo.ContentSpace.y);
        }
    }


    /// <summary>
    /// 移除展示的道具
    /// </summary>
    private void RemoveShowItem(int index)
    {
        ScrollViewHandlerItem item = m_ShowItem[index];
        item.Close();
        m_ShowItem.RemoveAt(index);
        m_privatePool.Add(item);
    }
}