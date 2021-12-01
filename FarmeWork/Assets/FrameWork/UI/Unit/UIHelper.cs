using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ShipFarmeWork.UI
{
    public class UIHelper
    {

        /// <summary>
        /// 在UI界面上展示非UI物体
        /// </summary>
        /// <param name="tran">展示的物体</param>
        /// <param name="tagLayer">层级,和标签在一块的那个层级</param>
        /// <param name="sortingLayerName">图片展示优先层级</param>
        /// <param name="baseSortingLayer">图片展示优先层级数值</param>
        public static void ShowNoUIGame(Transform tran, int tagLayer, string sortingLayerName, int baseSortingLayer)
        {
            Transform[] tranArr = tran.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < tranArr.Length; i++)
            {
                Transform childTran = tranArr[i];
                //更新摄像机照射的那个层次
                childTran.gameObject.layer = tagLayer;
                //更新图片层级
                SpriteRenderer sprite = childTran.GetComponent<SpriteRenderer>();
                if (sprite != null)
                {
                    sprite.sortingLayerName = sortingLayerName;
                    sprite.sortingOrder = baseSortingLayer + sprite.sortingOrder;
                    //遮罩剔除,先不改
                    sprite.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
                }
                //更新特效层级
                ParticleSystem particle = childTran.GetComponent<ParticleSystem>();
                if (particle != null)
                {
                    Renderer render = particle.GetComponent<Renderer>();
                    if (render != null)
                    {
                        render.sortingLayerName = sortingLayerName;
                        render.sortingOrder = baseSortingLayer + render.sortingOrder;
                    }
                }
            }

            //物体的适配,暂定标准宽高是1280*720
            //感觉貌似不用适配,再看
            //float hightRate = Screen.height / 720.0f;
            //float widthRate = Screen.width / 1280.0f;
            //float useRate = (hightRate < widthRate) ? hightRate : widthRate;
            //tran.localScale = tran.localScale * useRate;
        }

        /// <summary>
        /// 缓存的数组
        /// </summary>
        private static Vector3[] _CachedCorners;
        /// <summary>
        /// 把ui限制在屏幕范围内，保证ui完全显示
        /// </summary>
        public static void ClampPositionInCanvas(RectTransform rootCanvasTran, RectTransform rect, float margin)
        {
            if (_CachedCorners == null) { _CachedCorners = new Vector3[4]; }

            //获取UI在画布中四个顶点的世界坐标
            float scale = rootCanvasTran.transform.localScale.x;
            rect.GetWorldCorners(_CachedCorners);
            for (int i = 0; i < _CachedCorners.Length; ++i)
            {
                _CachedCorners[i] /= scale;
            }

            float halfWidth = rootCanvasTran.sizeDelta.x * 0.5f - margin;
            float halfHeight = rootCanvasTran.sizeDelta.y * 0.5f - margin;
            Vector2 offset = Vector2.zero;
            //检查四个方向，如果有超出，反向偏移回来
            for (int i = 0; i < _CachedCorners.Length; ++i)
            {
                Vector3 p = _CachedCorners[i];
                if (p.x < -halfWidth)
                {
                    float value = -halfWidth - p.x;
                    if (Mathf.Abs(value) > Mathf.Abs(offset.x))
                    {
                        offset.x = value;
                    }
                }
                else if (p.x > halfWidth)
                {
                    float value = halfWidth - p.x;
                    if (Mathf.Abs(value) > Mathf.Abs(offset.x))
                    {
                        offset.x = value;
                    }
                }

                if (p.y < -halfHeight)
                {
                    float value = -halfHeight - p.y;
                    if (Mathf.Abs(value) > Mathf.Abs(offset.y))
                    {
                        offset.y = value;
                    }
                }
                else if (p.y > halfHeight)
                {
                    float value = halfHeight - p.y;
                    if (Mathf.Abs(value) > Mathf.Abs(offset.y))
                    {
                        offset.y = value;
                    }
                }
            }

            rect.anchoredPosition += offset;
        }

        /// <summary>
        /// 传递滑动事件
        /// </summary>
        public static void TranScrollEvent(GameObject game, ScrollRect sroll)
        {
            EventTriggerListener.AddListenerCSharp(EventTriggerListenerType.onBeginDrag, (GameObject go, BaseEventData data) =>
            {
                ExecuteEvents.Execute(sroll.gameObject, data, ExecuteEvents.beginDragHandler);
            }, game);
            EventTriggerListener.AddListenerCSharp(EventTriggerListenerType.onDrag, (GameObject go, BaseEventData data) =>
            {
                ExecuteEvents.Execute(sroll.gameObject, data, ExecuteEvents.dragHandler);
            }, game);
            EventTriggerListener.AddListenerCSharp(EventTriggerListenerType.onEndDrag, (GameObject go, BaseEventData data) =>
            {
                ExecuteEvents.Execute(sroll.gameObject, data, ExecuteEvents.endDragHandler);
            }, game);
        }
    }

}