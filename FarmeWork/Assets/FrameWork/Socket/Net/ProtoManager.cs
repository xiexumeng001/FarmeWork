using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShipFarmeWork.Net
{
    /// <summary>
    /// 协议管理类
    /// </summary>
    public class ProtoManager
    {
        private static ProtoManager mInstance;

        public delegate void responseDelegate(Packet resp);
        private Dictionary<int, List<responseDelegate>> mDelegateMapping;

        private ProtoManager()
        {
            mDelegateMapping = new Dictionary<int, List<responseDelegate>>();
        }


        /// <summary>
        /// 添加代理，在接受到服务器数据时会下发数据
        /// </summary>
        /// <param name="protocol">Protocol.</param>
        /// <param name="d">D.</param>
        public void AddRespDelegate(int protocol, responseDelegate d)
        {
            List<responseDelegate> dels;
            if (mDelegateMapping.ContainsKey(protocol))
            {
                dels = mDelegateMapping[protocol];
                for (int i = 0; i < dels.Count; i++)
                {
                    if (dels[i] == d)
                    {
                        return;
                    }
                }
            }
            else
            {
                dels = new List<responseDelegate>();
                mDelegateMapping.Add(protocol, dels);
            }
            dels.Add(d);

        }
        public void DelRespDelegate(int protocol, responseDelegate d)
        {
            if (mDelegateMapping.ContainsKey(protocol))
            {
                List<responseDelegate> dels = mDelegateMapping[protocol];
                dels.Remove(d);
            }
        }

        public Packet TryDeserialize(byte[] buffer)
        {
            Packet pack = null;
            DataStream stream = new DataStream(buffer);

            int protocol = stream.ReadSInt32();
            if (mDelegateMapping.ContainsKey(protocol))
            {
                pack = SocketHelper.DeserializePack(protocol, stream.ReadRemainBytes());
                List<responseDelegate> dels = mDelegateMapping[protocol];
                for (int i = 0; i < dels.Count; i++)
                {
                    try
                    {
                        dels[i](pack);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
            }
            else
            {
                Debug.LogError("没有注册协议包 : " + protocol + "!please reg to RegisterResp.");
            }

            return pack;
        }

        public static ProtoManager Instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = new ProtoManager();
                }
                return mInstance;
            }
        }

    }

}