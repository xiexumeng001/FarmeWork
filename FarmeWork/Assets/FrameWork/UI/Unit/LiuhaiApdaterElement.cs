using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShipFarmeWork.UI
{
    /// <summary>
    /// 刘海范围
    /// </summary>
    public class LiuhaiRange
    {
        public float xMin;
        public float xMax;
        public float yMin;
        public float yMax;

        public float Width;
        public float Height;
    }

    /// <summary>
    /// 刘海的UI元素适配
    /// </summary>
    public class LiuhaiApdaterElement : MonoBehaviour
    {

        public static LiuhaiRange Range;
        // Start is called before the first frame update
        void Start()
        {
            Adpater();
        }

        // Update is called once per frame
        void Update()
        {
            Adpater();
        }

        private void Adpater()
        {
            RectTransform rectTrans = GetComponent<RectTransform>();
            float xMinNum = rectTrans.position.x - rectTrans.rect.width / 2;
            float xMaxNum = rectTrans.position.x + rectTrans.rect.width / 2;
            float yMinNum = rectTrans.position.y - rectTrans.rect.height / 2;
            float yMaxNum = rectTrans.position.y + rectTrans.rect.height / 2;

            if (xMinNum < Range.xMax && xMaxNum > Range.xMin)
            {
                if (yMinNum < Range.yMax && yMaxNum > Range.yMin)
                {
                    rectTrans.position = new Vector3(rectTrans.position.x + (Range.xMax - Range.xMin), rectTrans.position.y, rectTrans.position.z);
                }
            }
        }
    }
}
