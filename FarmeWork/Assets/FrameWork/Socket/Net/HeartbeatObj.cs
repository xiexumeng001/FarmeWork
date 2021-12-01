using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

using System;


namespace ShipFarmeWork.Net
{
    /// <summary>
    /// 心跳对象
    /// </summary>
    //hack【暂改】【心跳】目前的心跳在后台暂停时不能接到包,所以当从后台出来,即使断了,可能也需要一段时间发现
    public class HeartbeatObj
    {
        public Packet HeartPack;
        public SocketHelper Socket;

        private static DateTime StartTimeStamp = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        public int SendHeartBeatInterval;   //发送心跳间隔
        public long SendTime;               //发送时间点
        public int MaxHeartBeatLiveTime;    //最大心跳存活时间
        public float NoHeartBeatTime;         //未接收心跳包的时间

        public bool IsCanHeatbeat = false;   //是否能心跳
        private object IsCanHeartBeatObj = new object();

        public bool IsKeepLiveOnPause = false;   //当在后台是否保活
        private object IsKeepLiveOnPauseObj = new object();

        public Thread thread;

        public void Init(SocketHelper socket, Packet heartPack, int sendInterval)
        {
            Socket = socket;
            HeartPack = heartPack;
            SendHeartBeatInterval = sendInterval * 1000;
            MaxHeartBeatLiveTime = SendHeartBeatInterval * 3;
        }


        public void Start()
        {
            UpdatePauseKeepLiveStaus(IsKeepLiveOnPause);
            NoHeartBeatTime = 0;
            SendTime = GetCurrentTimeStampToSecond() + (SendHeartBeatInterval / 1000);
        }

        public void Update()
        {
            if (!Socket.IsConnect) return;
            if (!IsCanHeatbeat) return;

            //发送
            if (!IsKeepLiveOnPause)
            {
                if (GetCurrentTimeStampToSecond() >= SendTime)
                {
                    if (HeartPack != null) Socket.SendPacket(HeartPack);
                    SendTime = GetCurrentTimeStampToSecond() + (SendHeartBeatInterval / 1000);
                }
            }

            //接收
            NoHeartBeatTime += Time.deltaTime;
            if (NoHeartBeatTime >= (MaxHeartBeatLiveTime / 1000))
            {
                Socket.State.Value = ConnectState.ConnectException;
                Socket.ShowWarn("心跳接收超时");
            }
        }

        public void StopThread()
        {
            if (thread != null)
            {
                thread.Abort();
                thread = null;
                Socket.ShowWarn("心跳线程退出");
            }
        }

        public void UpdatePauseKeepLiveStaus(bool isOpen)
        {
            if (isOpen)
            {
                if (thread == null)
                {
                    thread = new Thread(Heartbeat);
                    thread.IsBackground = true;
                    thread.Start();
                }
            }
            lock (IsKeepLiveOnPauseObj)
            {
                IsKeepLiveOnPause = isOpen;
            }
        }

        public void UpdateSwitch(bool isCan)
        {
            lock (IsCanHeartBeatObj)
            {
                IsCanHeatbeat = isCan;
            }
        }

        /// <summary>
        /// 当接收到心跳包
        /// </summary>
        public void OnReceiveHeart()
        {
            NoHeartBeatTime = 0;
        }

        /// <summary>
        /// 心跳(注意是在多线程中,U3D的变量不可用)
        /// </summary>
        public void Heartbeat()
        {
            while (true)
            {
                Thread.Sleep(SendHeartBeatInterval);
                try
                {
                    lock (IsKeepLiveOnPauseObj)
                    {
                        if (!IsKeepLiveOnPause) { continue; }
                    }
                    if (!Socket.IsConnect) { continue; }
                    lock (IsCanHeartBeatObj)
                    {
                        if (!IsCanHeatbeat) { continue; }
                    }
                    if (HeartPack != null) Socket.SendPacket(HeartPack);
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        /// <summary>
        /// 获取当前到秒的时间戳
        /// </summary>
        /// <returns></returns>
        private long GetCurrentTimeStampToSecond()
        {
            TimeSpan ts = DateTime.UtcNow - StartTimeStamp;
            return Convert.ToInt64(ts.TotalSeconds);
        }
    }

}
#if false

        public float HeartBeatTime;
        public float NextSendHeartBeatTime;   //下一次发送心跳的时间点
public void Update()
{
    HeartBeatTime += Time.deltaTime;
    if (HeartBeatTime >= SendHeartBeatInterval)
    {
        Socket.SendPacket(HeartPack);
        HeartBeatTime = 0;
    }
}

        public HeatbeatObj HearObj;     //心跳对象

            HearObj = new HeatbeatObj();
            Packet pack = new Packet();
            pack.m_packetType = (int)PacketOpcode.BATTLE_HEARTBEAT_MODULE;
            pack.m_message = new BattleHeartBeatModule();
            HearObj.Init(BattleSocketHelper.GetInstance(), pack, 5 * 1000);


            HearObj.Update(TimeControl.CurrTimeStampMilliSecond);
#endif