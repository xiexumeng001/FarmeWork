using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;
using SObject = System.Object;

#if UNITY_EDITOR
using UnityEngine.UI;
#endif

namespace ShipFarmeWork.Resource
{
    public class CloneanbleAssetNode
    {
        private const string POOL_NODE = "POOL_NODE";
        private static Transform poolNode = null;
        private static Transform PoolNode
        {
            get
            {
                if (poolNode == null)
                {
                    GameObject mgrObj = ResourceManager.Instance.gameObject;
                    if (mgrObj)
                    {
                        poolNode = new GameObject(POOL_NODE).transform;
                        poolNode.SetParent(mgrObj.transform);
                        poolNode.localPosition = Vector3.zero;
                        poolNode.localScale = Vector3.one;
                        poolNode.gameObject.SetActive(false);
                    }
                    else
                    {
                        Debug.LogError("Create pool node failed! None Game manager object!");
                        return null;
                    }
                }
                return poolNode;
            }
        }

        private string m_assetName;                  //资源名称
        private GameObject m_original = null;           //原始资源
        private Transform m_tranParentNode = null;   //如果这个资源可以克隆，此为节点
        private List<AssetClone> m_clones = null;    //如果这个资源可以克隆，此为克隆池
        public List<AssetClone> Clones { get { return m_clones; } }
        private bool m_cached = false;              //original是否需要常驻，还是每次都unload?
        private int m_poolCapacity = 0;             //池子容量

        private CloneanbleAssetNode() { }

        public static CloneanbleAssetNode Create(GameObject original)
        {
            if (original == null)
            {
                Debug.LogError("Create AssetInfoNode failed. None original...");
                return null;
            }
            CloneanbleAssetNode node = new CloneanbleAssetNode();
            node.m_assetName = original.name;
            node.m_original = original;

#if UNITY_EDITOR
            RawImage[] imgs = original.GetComponentsInChildren<RawImage>();
            for (int i = 0; i < imgs.Length; ++i)
            {
                string shaderName = imgs[i].material.shader.name;
                imgs[i].material.shader = Shader.Find(shaderName);
            }
#endif

            return node;
        }

        public int PoolCapacity
        {
            set
            {
                m_poolCapacity = value;
                if (m_clones == null)
                    m_clones = new List<AssetClone>();

                //如果当前池里的数量已经比容量大
                if (m_clones.Count > m_poolCapacity)
                {
                    int count = m_clones.Count - m_poolCapacity;
                    for (int i = m_clones.Count - 1; i >= 0; --i)
                    {
                        if (!m_clones[i].IsActive)
                        {
                            if(m_clones[i].CloneObj)
                                GameObject.Destroy(m_clones[i].CloneObj);
                            //Debug.Log("成功删除了资源 \n  如果当前池里的数量已经比容量大");
                            m_clones.RemoveAt(i);
                            --count;
                            if (count <= 0)
                                break;
                        }
                    }
                }
                else if (m_clones.Count < m_poolCapacity)
                {
                    //分配足够多的
                    int count = m_poolCapacity - m_clones.Count;
                    while (count > 0)
                    {
                        --count;
                        Clone();
                    }

                    //Debug.Log("分配足够多的");
                }

                //如果已经没有clone了，删掉父节点
                if (m_clones.Count == 0)
                {
                    if (m_tranParentNode != null)
                    {
                        GameObject.Destroy(m_tranParentNode.gameObject);
                        //Debug.Log("成功删除了资源  \n   如果已经没有clone了，删掉父节点");
                        m_tranParentNode = null;
                    }
                }
            }
        }

        public bool Cached
        {
            set { m_cached = value; }
        }

        private Transform ParentNode
        {
            get
            {
                if (m_tranParentNode == null)
                {
                    if (!PoolNode)
                    {
                        Debug.LogError(string.Format("{0} clone failed. None parent node.", m_assetName));
                        return null;
                    }
                    else
                    {
                        m_tranParentNode = PoolNode.Find(m_assetName.ToUpper());
                    }
                    if (m_tranParentNode == null)
                    {
                        m_tranParentNode = new GameObject(m_assetName.ToUpper()).transform;
                        m_tranParentNode.SetParent(PoolNode);
                        m_tranParentNode.localPosition = Vector3.zero;
                        m_tranParentNode.localScale = Vector3.one;
                    }
                }
                return m_tranParentNode;
            }
        }

        //只是克隆一个新的对象放到池中
        private AssetClone Clone()
        {
            if(ParentNode == null)
            {
                Debug.LogError(string.Format("{0} clone failed. None parent node.", m_assetName));
                return null;
            }
            
            AssetClone newClone = null;
            if (m_original != null)
                newClone = AssetClone.Create((GameObject)m_original, ParentNode);
            else if (m_clones.Count > 0)
                newClone = new AssetClone(m_clones[0], ParentNode);

            if (newClone == null)
            {
                Debug.LogError(string.Format("{0} clone failed. None model", m_assetName));
                return null;
            }

            m_clones.Add(newClone);

            return newClone;
        }

        public GameObject Clone(SObject requester)
        {
            if (requester == null)
            {
                Debug.LogError(string.Format("{0} clone failed. None reqeuster.", m_assetName));
                return null;
            }
            if (ParentNode == null)
            {
                Debug.LogError(string.Format("{0} clone failed. None parent node.", m_assetName));
                return null;
            }

            if (m_clones == null)
                m_clones = new List<AssetClone>();

            if (m_clones.Count > 0)
            {
                for (int i = 0; i < m_clones.Count; ++i)
                {
                    if (!m_clones[i].IsActive && m_clones[i].CloneObj)
                    {
                        m_clones[i].SetActive(requester);
                        return m_clones[i].CloneObj;
                    }
                }
            }

            //当前池中没有符合需求的
            AssetClone newClone = Clone();

            newClone.SetActive(requester);

            return newClone.CloneObj;
        }

        public UObject Original
        {
            get { return m_original; }
        }

        public void ReturnByQuester(SObject requester)
        {
            if (requester == null
                || m_clones == null)
                return;

            for (int i = 0; i < m_clones.Count; ++i)
            {
                if (m_clones[i].Requester == requester)
                {
                    m_clones[i].Reset();
                }
            }

            //重置一下池子
            PoolCapacity = m_poolCapacity;
        }

        public void ReturnClones(GameObject[] clones)
        {
            if (clones == null || clones.Length == 0 || m_clones == null)
                return;

            for (int i = 0; i < m_clones.Count; ++i)
            {
                for (int j = 0; j < clones.Length; ++j)
                {
                    if (m_clones[i].CloneObj.GetInstanceID() == clones[j].GetInstanceID())
                    {
                        if (ResourceConfig.IsSafeCheck)
                        {
                            ResourceSafe.Instance.RemoveAssetRequestSafeRecord(m_clones[i].CloneObj);
                        }

                        m_clones[i].Reset();
                        break;
                    }
                }
            }

            //重置一下池子
            PoolCapacity = m_poolCapacity;
        }

        //归还单个克隆物体
        public void ReturnCloneOne(GameObject clone)
        {

            //Debug.LogError(clone + " 归还单个克隆物体");
            if (clone == null || m_clones == null)
                return;

            if (ResourceConfig.IsSafeCheck)
            {
                ResourceSafe.Instance.RemoveAssetRequestSafeRecord(clone);
            }

            for (int i = 0; i < m_clones.Count; ++i)
            {
                if (m_clones[i].CloneObj.GetInstanceID() == clone.GetInstanceID())
                {
                    m_clones[i].Reset();
                    //Debug.LogError(clone + "的ID是"+ clone.GetInstanceID() + "执行 Reset 方法");
                    break;
                }
            }

            //重置一下池子
            PoolCapacity = m_poolCapacity;
        }



        //检测是否可以卸载
        public bool CheckCanUnload()
        {
            //只要是设置了池的容量，就不能释放
            if (m_poolCapacity > 0)
                return false;

            //有一个节点被激活，就不能卸载
            for (int i = 0; i < m_clones.Count; ++i)
            {
                if (m_clones[i].IsActive)
                    return false;
            }
            return true;
        }

        public void Unload()
        {
            for (int i = 0; i < m_clones.Count; ++i)
            {
                if (ResourceConfig.IsSafeCheck)
                {
                    ResourceSafe.Instance.RemoveAssetRequestSafeRecord(m_clones[i].CloneObj);
                }
                m_clones[i].UnLoadSelf();
            }
        }
        
        public string Desc()
        {
            return string.Empty;
        }
    }
}