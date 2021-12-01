using System.Collections.Generic;
using SObject = System.Object;
using System.Text;
using UnityEngine;

namespace ShipFarmeWork.Resource
{
    public class AssetRequest
    {
        private const char loadedSign = '|';
        private static string CombineKey(string assetName, System.Type assetType)
        {
            return string.Format("{0}{1}{2}", assetName, loadedSign, assetType.ToString());
        }
        //类型转换器
        private static Dictionary<string, System.Type> typeConvert = new Dictionary<string, System.Type>();
        //每次都尝试记住一种类型名称对应的类型，方便后面取出
        private static void SaveType(System.Type _type)
        {
            //保存type
            if (!typeConvert.ContainsKey(_type.ToString()))
                typeConvert.Add(_type.ToString(), _type);
        }
        private static System.Type LoadType(string typeName)
        {
            if (typeConvert.ContainsKey(typeName))
                return typeConvert[typeName];

            return null;
        }

        private string m_bundleName;
        private List<string> m_assetsRequest = null;
        private System.Action<string, string[], System.Type[]> m_assetsCallback = null;
        private System.Action<string> m_bundleCallback = null;

        private SObject m_requester;

        private AssetRequest() { }

        //建立一个资源请求
        public static AssetRequest Create(
            string bundleName,
            string[] assets,
            System.Type[] types,
            System.Object quester,
            System.Action<string> bundleCallback,
            System.Action<string, string[], System.Type[]> assetCallback)
        {
            if (assets != null
                && types != null
                && assets.Length != types.Length)
            {
                Debug.LogError("请求资源时发生了错误，名称、类型数量不符！");
                return null;
            }
            AssetRequest request = new AssetRequest();
            //保存资源请求
            if (assets == null || assets.Length == 0)
                request.m_assetsRequest = null;
            else
            {
                request.m_assetsRequest = new List<string>();
                for (int i = 0; i < assets.Length; ++i)
                {
                    //尝试记住一种类型名称和类型的映射
                    AssetRequest.SaveType(types[i]);
                    request.m_assetsRequest.Add(CombineKey(assets[i], types[i]));
                }
            }

            request.m_assetsCallback = assetCallback;
            request.m_bundleCallback = bundleCallback;
            request.m_bundleName = bundleName;
            request.m_requester = quester;
            return request;
        }

        //资源准备后的通知
        /// <summary>
        /// 部分资源准备完毕
        /// </summary>
        /// <returns><c>true</c>, 是否可以移除此次请求, <c>false</c> otherwise.</returns>
        /// <param name="assets">Assets.</param>
        /// <param name="types">Types.</param>
        public bool AssetsReady(string[] assets, System.Type[] types)
        {
            if (assets != null
                && types != null
                && assets.Length == types.Length
                && m_assetsCallback != null
                && m_assetsRequest != null
                && m_assetsRequest.Count > 0)
            {
                //尝试获取一下是否有需求内的资源
                List<string> assetsLoadedName = null;
                List<System.Type> assetsLoadedType = null;
                for (int i = 0; i < assets.Length; ++i)
                {
                    string key = CombineKey(assets[i], types[i]);
                    for (int j = m_assetsRequest.Count - 1; j >= 0; --j)
                    {
                        if (m_assetsRequest[j] == key)
                        {
                            if (assetsLoadedName == null)
                                assetsLoadedName = new List<string>();
                            if (assetsLoadedType == null)
                                assetsLoadedType = new List<System.Type>();

                            assetsLoadedName.Add(assets[i]);
                            assetsLoadedType.Add(types[i]);
                            //通知后就移除
                            m_assetsRequest.RemoveAt(j);
                            break;
                        }
                    }
                }

                //通知一下
                if (assetsLoadedName != null)
                {
                    if (m_assetsCallback != null)
                        m_assetsCallback(m_bundleName, assetsLoadedName.ToArray(), assetsLoadedType.ToArray());
                }

                //判断是否需要移除
                if (m_assetsRequest.Count == 0)
                {
                    // Debug.Log(string.Format("一个关于{0}的多个资源请求已经全部准备就绪，可以移除了～", m_bundleName));
                    return true;
                }
            }

            return false;
        }

        //全部资源准别后的通知
        public void AllAssetsReady()
        {
            //本来就是一个全部资源加载的请求
            if (m_bundleCallback != null)
                m_bundleCallback(m_bundleName);

            //不是一个全部资源加载的请求，但是肯定也完成了
            if (m_assetsCallback != null
               && m_assetsRequest != null
               && m_assetsRequest.Count > 0)
            {

                List<string> assetsLoadedName = new List<string>();
                List<System.Type> assetsLoadedType = new List<System.Type>();
                for (int i = 0; i < m_assetsRequest.Count; ++i)
                {
                    string[] args = m_assetsRequest[i].Split(AssetRequest.loadedSign);
                    if (args != null && args.Length == 2)
                    {
                        //资源名称
                        string assetName = args[0];
                        System.Type assetType = AssetRequest.LoadType(args[1]);
                        if (assetType != null)
                        {
                            assetsLoadedName.Add(assetName);
                            assetsLoadedType.Add(assetType);
                        }
                    }
                }
                //通知一下
                if (assetsLoadedName.Count > 0)
                {
                    m_assetsCallback(m_bundleName, assetsLoadedName.ToArray(), assetsLoadedType.ToArray());
                }
            }
        }

        public SObject Requester
        {
            get
            {
                return m_requester;
            }
        }

        public string Desc()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("{0}的资源请求情况：", m_bundleName);
            if (m_assetsRequest != null && m_assetsCallback != null)
            {
                for (int i = 0; i < m_assetsRequest.Count; ++i)
                {
                    builder.AppendFormat("{0}: {1}", i, m_assetsRequest[i]);
                }
            }
            else if (m_bundleCallback != null)
                builder.Append("被请求bundle下的全部资源...");
            else
                builder.Append("异常！！没有任何callback!");

            return builder.ToString();
        }
    }
}