
namespace ShipFarmeWork.Touch
{
    /// <summary>
    /// 触摸类型
    /// </summary>
    public enum TouchTriggerType
    {
        /// <summary>
        /// 无触摸
        /// </summary>
        None,
        /// <summary>
        /// 当点击
        /// </summary>
        OnTouchClick,
        /// <summary>
        /// 当按下
        /// </summary>
        OnTouchDown,
        /// <summary>
        /// 当抬起
        /// </summary>
        OnTouchUp,
        /// <summary>
        /// 当长按
        /// </summary>
        OnLongDown,
        /// <summary>
        /// 当拖拽开始
        /// </summary>
        OnDragStart,
        /// <summary>
        /// 当拖拽
        /// </summary>
        OnDrag,
        /// <summary>
        /// 当拖拽结束
        /// </summary>
        OnDragEnd,
        /// <summary>
        /// 当双指缩放
        /// </summary>
        OnDoubleTouch,
        /// <summary>
        /// 没有被选中
        /// </summary>
        WhenNotSelected,
    }

}
