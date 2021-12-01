using UnityEngine;
using SObject = System.Object;

namespace ShipFarmeWork.Resource
{
    public class AssetClone
    {
        private string m_assetName;
        public string AssetName { get { return m_assetName; } }
        private SObject m_requester;
        private GameObject m_objClone;
        private Transform m_parentNode;

        private AssetClone() { }

        public static AssetClone Create(GameObject original, Transform parentNode)
        {
            if (original == null || parentNode == null)
                return null;

            AssetClone clone = new AssetClone();
            clone.m_assetName = original.name;
            clone.m_objClone = GameObject.Instantiate<GameObject>(original, parentNode);
            clone.m_requester = null;
            clone.m_parentNode = parentNode;
            clone.Reset();
            return clone;
        }

        public AssetClone(AssetClone cloned, Transform parentNode)
        {
            m_assetName = cloned.m_assetName;
            m_parentNode = parentNode;
            m_requester = null;
            m_objClone = GameObject.Instantiate<GameObject>(cloned.m_objClone, parentNode);
            Reset();
        }

        public bool IsActive
        {
            get
            {
                if (m_objClone)
                {
                    return m_requester != null || m_objClone.activeSelf;
                }
                else {
                    return false;
                }
            }
        }

        public GameObject CloneObj
        {
            get { return m_objClone; }
        }

        public SObject Requester
        {
            get { return m_requester; }
        }

        public void Reset()
        {
            if (m_objClone == null)
            {
                //Debug.LogWarning(string.Format("恭喜你：尝试重置名为 {0} 的asset，失败，此obj为空，推测已被Destroy！", m_assetName));
                m_requester = null;
                return;
            }

            m_objClone.transform.SetParent(m_parentNode);
            m_objClone.transform.localPosition = Vector3.zero;
            //关闭默认缩放为0
            //m_objClone.transform.localScale = Vector3.one;
            m_objClone.transform.localRotation = Quaternion.identity;
            m_objClone.name = m_assetName;
            m_requester = null;
            m_objClone.SetActive(false);
        }

        public void SetActive(SObject requester)
        {
            m_requester = requester;
        }

        public void UnLoadSelf()
        {
            m_requester = null;
            GameObject.Destroy(m_objClone);
        }
        
        public string Desc()
        {
            return string.Empty;
        }
    }
}
