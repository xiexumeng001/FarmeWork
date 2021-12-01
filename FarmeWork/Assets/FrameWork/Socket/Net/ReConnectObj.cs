using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

namespace ShipFarmeWork.Net
{
    /// <summary>
    /// 重连对象
    /// </summary>
    /// hack【暂改】【重连机制】如何在后台的时候重连
    public class ReConnectObj
    {
        public bool IsOpenReConnet = false;

        public float ResetLimitTime;         //重置连接的时间
        public float CurrentConnectNum;      //当前连接次数
        private bool ConnectIsOverLimit = false;       //连接是否超限

        //hack【暂改】【重连】 重连时间先不用
        private float ReConnectDelayTime = 1;   //当断开时延迟个一秒,在重连
        private float ReConnectTime;            //重连时间
        private int ReConnectNum = 0;           //重连次数
        public SocketHelper Socket;

        public Action OnConnectOverLImit = null;

        public void Init(SocketHelper socket)
        {
            Socket = socket;
            Reset();
        }


        public void Update()
        {
            ResetLimitTime += Time.deltaTime;
            if (ResetLimitTime >= SocketHelper.ConnectLimitTime)
            {
                ResetLimitTime = 0;
                CurrentConnectNum = 0;
            }
        }

        /// <summary>
        /// 更新重连开关
        /// </summary>
        public void UpdateReConnectSwitch(bool isOpen)
        {
            IsOpenReConnet = isOpen;
        }

        public void Reset()
        {
            ReConnectNum = 0;
            ReConnectTime = 0;
        }

        /// <summary>
        /// 是否能重连
        /// </summary>
        public bool IsCanConnect()
        {
            return IsOpenReConnet && !ConnectIsOverLimit && (ReConnectNum < SocketHelper.ReconnectMaxNum);
        }

        /// <summary>
        /// 重连
        /// </summary>
        public void ReConnect()
        {
            ReConnectTime = 0;
            ReConnectNum++;
            Debug.LogWarningFormat("开始第{0}次重连", ReConnectNum);
            Socket.StartCoroutine(Socket.Connect(OnReConnectSuccess, OnReConnectFail));

#if false
            //若正在连接中,就等等
            if (State == ConnectState.Connecting) { return; }
            if (reconnectNum >= ReconnectMaxNum) { return; }
            if (!isAllowConnect) { return; }
            ReConnectTime = 0;

            CloseConnect(true);//关闭之前的连接

            reconnectNum++;
            Debug.LogWarningFormat("开始第{0}次重连", reconnectNum);
            Socket.StartCoroutine(Socket.Connect(OnReConnectSuccess, OnReConnectFail));
#endif
        }

        /// <summary>
        /// 当重连成功
        /// </summary>
        private void OnReConnectSuccess()
        {
            ReConnectTime = 0;
            ReConnectNum = 0;
            Debug.LogWarning("重连成功");
        }

        /// <summary>
        /// 当重连失败
        /// </summary>
        private void OnReConnectFail()
        {
            if (ReConnectNum >= SocketHelper.ReconnectMaxNum)
            {
                Debug.LogWarning("超过最大重连次数");
            }
        }

        public void AddConnectNum()
        {
            CurrentConnectNum++;
            if (CurrentConnectNum > SocketHelper.ReconnectMaxNum)
            {//当连接超过限制
                Debug.LogErrorFormat("{0}s时间内连接次数超过了{1}次数", SocketHelper.ConnectLimitTime, SocketHelper.ConnectLimitMaxTime);
                ConnectIsOverLimit = true;
                if (OnConnectOverLImit != null) OnConnectOverLImit();
            }
        }

    }


}