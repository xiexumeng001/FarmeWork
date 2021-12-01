using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShipFarmeWork.UI
{
    /// <summary>
    /// UI界面信息
    /// </summary>
    public class UIInfo
    {
        public string ResourcePath;
        public string ResourceName;
        public string LayerType;
        public PopMaskLucenyType PopMaskType; //遮罩类型
        public bool IsResident;               //是否常驻(用来在关闭所有界面的时候,只隐藏不销毁)

        public UIInfo(string resourcePath, string resourceName, string layerType)
        {
            InitCommon(resourcePath, resourceName, layerType, PopMaskLucenyType.None, false);
        }
        public UIInfo(string resourcePath, string resourceName, string layerType, PopMaskLucenyType maskType, bool isResident)
        {
            InitCommon(resourcePath, resourceName, layerType, maskType, isResident);
        }

        private void InitCommon(string resourcePath, string resourceName, string layerType, PopMaskLucenyType maskType, bool isResident)
        {
            ResourcePath = resourcePath;
            ResourceName = resourceName;
            LayerType = layerType;
            PopMaskType = maskType;
            IsResident = isResident;
        }
    }
}
