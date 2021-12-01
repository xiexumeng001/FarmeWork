using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShipFarmeWork.UI
{
    /// <summary>
    /// UI层级
    /// </summary>
    public class UIDepth : MonoBehaviour
    {
        Canvas Canvas;
        int LayerNum;

        // Start is called before the first frame update
        void Start()
        {
            Canvas = GetComponent<Canvas>();
            SetLayerNum(LayerNum);
        }

        public void SetLayerNum(int layerNum)
        {
            LayerNum = layerNum;
            if (Canvas == null) return;

            Canvas.overrideSorting = true;
            Canvas.sortingOrder = LayerNum;
        }
    }
}
