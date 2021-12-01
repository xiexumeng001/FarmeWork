using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf;

namespace ShipFarmeWork.Net
{
    /// <summary>
    /// 包体
    /// </summary>
    public class Packet
    {
        public int m_packetType;     //包类型
        public IMessage m_message;            //包体(protobuff的内容)

        public byte[] GetData()
        {
            byte[] pbdata = m_message.ToByteArray();
            DataStream writer = new DataStream();
            writer.WriteSInt32((int)m_packetType);
            writer.WriteRaw(pbdata);

            return writer.ToByteArray();
        }


    }
}


#region 废弃的逻辑
//public class Packet
//{

//    public PacketOpcode m_packetType;
//    public IMessage m_object;

//    public byte[] GetData()
//    {

//        byte[] pbdata = m_object.ToByteArray();
//        DataStream writer = new DataStream(true);
//        writer.WriteSInt32((int)m_packetType);
//        writer.WriteRaw(pbdata);

//        return writer.ToByteArray();
//    }


//    public static Packet GetPacket(int type, byte[] bs)
//    {
//        Packet pack = new Packet();

//        IExtensible IExten = null;
//        PacketOpcode typeCode = (PacketOpcode)type;
//        if (typeCode == PacketOpcode.GetRoleResultType)
//        {
//            IExten = PBSerialize.Deserialize<CSLoginRes>(bs);
//        }
//        if (IExten == null)
//        {
//            Debug.LogError("没有找到对应的类型");
//            return null;
//        }
//        pack.m_packetType = typeCode;
//        pack.m_object = IExten;
//        return pack;
//    }
//}
#endregion