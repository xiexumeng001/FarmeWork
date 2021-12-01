using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ShipFarmeWork.UI
{
    /// <summary>
    /// 遮罩界面
    /// </summary>
    public class UIMask : MonoBehaviour
    {
        public Canvas MaskCanvas;
        public Image MaskImage;

        private BaseUIForm MaskForm;

        private int reduceLayer = 1;

        /// <summary>
        /// 当UI界面展示
        /// </summary>
        public void OnUIShow(BaseUIForm uiForm)
        {
            bool isCanUpdate = false;
            if (uiForm.UiInfo.PopMaskType != PopMaskLucenyType.None)
            {
                if (MaskForm != null)
                {
                    isCanUpdate = (MaskForm.LayerNum < uiForm.LayerNum);
                }
                else { isCanUpdate = true; }
            }

            if (isCanUpdate) { ShowMask(uiForm); }
        }

        /// <summary>
        /// 当UI界面关闭
        /// </summary>
        public void OnUIClose(BaseUIForm uiForm)
        {
            if (MaskForm == null || MaskForm != uiForm) { return; }

            //找到最大层级的需要遮罩的界面,并且更新上
            BaseUIForm maxUiForm = null;
            foreach (var item in UIManager.Instance.ShowUIs)
            {
                if (item.Value.Count > 0)
                {
                    BaseUIForm showUiForm = item.Value[item.Value.Count - 1];
                    if (showUiForm.UiInfo.PopMaskType != PopMaskLucenyType.None)
                    {
                        if (maxUiForm == null || maxUiForm.LayerNum < showUiForm.LayerNum) { maxUiForm = showUiForm; }
                    }
                }
            }
            if (maxUiForm != null)
            {
                ShowMask(maxUiForm);
            }
            else { CloseMask(); }
        }

        /// <summary>
        /// 设置遮罩状态
        /// </summary>
        /// <param name="uiForm">要显示的UI层</param>
        /// <param name="lucenyType"></param>
        private void ShowMask(BaseUIForm uiForm)
        {
            MaskForm = uiForm;
            //颜色
            Color newColor = new Color(0 / 255F, 0 / 255F, 0 / 255F, 80 / 255F);
            //启动遮罩,并且设置透明度
            switch (uiForm.UiInfo.PopMaskType)
            {
                case PopMaskLucenyType.Lucency:
                    newColor = new Color(255 / 255F, 255 / 255F, 255 / 255F, 0F / 255F);
                    break;
                case PopMaskLucenyType.Translucence:
                    break;
                case PopMaskLucenyType.ImPenetrable:
                    newColor = new Color(50 / 255F, 50 / 255F, 50 / 255F, 200F / 255F);
                    break;
                default:
                    break;
            }
            MaskImage.color = newColor;

            //层级
            MaskCanvas.overrideSorting = true;
            MaskCanvas.sortingOrder = MaskForm.LayerNum - reduceLayer;

            gameObject.SetActive(true);
        }

        private void CloseMask()
        {
            MaskForm = null;
            gameObject.SetActive(false);
        }

    }
}
