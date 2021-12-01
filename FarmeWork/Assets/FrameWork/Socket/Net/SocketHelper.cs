using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace ShipFarmeWork.Net
{

    public enum ConnectState
    {
        NoConnect,         //无连接
        Connected,         //已连接
        Connecting,        //连接中
        ConnectFail,       //连接失败
        ConnectException,  //连接异常
    }
    /// <summary>
    /// Socket管理类
    /// </summary>
    /// hack【暂改】服务器有半秒之内同时受到两个相同协议的包,会过滤后面包的逻辑,需要看下
    /// hack【暂改】如果有时间的话,需要优化下重连机制 与 连接 成功失败的回调
    public abstract class SocketHelper : MonoBehaviour
    {

        //添加收到包的委托
        public static bool IsAddResp;
        public static Action AddRespDelegate;
        //反序列化包的委托
        public static Func<int, byte[], Packet> DeserializePack;

        private static float ConnectTimeout = 5f;     //连接超时时间

        public static int ReconnectMaxNum = 3;        //重连的最大次数
        public static int ConnectLimitMaxTime = 3;    //连接限制最大次数
        public static int ConnectLimitTime = 10;      //连接限制时间,规定一段时间内的最大连接次数,如果超过了.就不允许连接了
        public static int ThreadSleepTime = 10;       //线程沉睡时间(ms)


        private static int SystemBuffSize = 32000;        //系统缓冲区大小
        private static int BuffSize = 1024;               //用户缓冲区大小

        private Socket socket;
        private IPEndPoint Endpoint;            //连接的终端信息
        private float ConnectingTime;           //当前连接时间
        public SocketThreadSafeObject<ConnectState> State = new SocketThreadSafeObject<ConnectState>(ConnectState.NoConnect);


        public int MaxPackNumPerFarme = 5;            //每帧处理包的最大数量
        //这三个个队列,会在线程中被改变,所以更改此对象时,使用锁锁住
        DataHolder mDataHolder = new DataHolder();
        Queue<byte[]> dataQueue = new Queue<byte[]>();
        List<Packet> sendPacketQueue = new List<Packet>();

        SocketThreadSafeObject<bool> isStopThread = new SocketThreadSafeObject<bool>(true);
        Thread thread_receive;
        Thread thread_send;

        public delegate void ConnectCallback();
        public Action OnConnectSuccess = null;
        public Action OnConnectFail = null;
        public Action<bool, bool> OnDicConnectDelegate = null;

        //心跳对象
        public bool IsOpenHeart = false;
        //发送心跳间隔
        public int SendHeartbeatInterval;
        HeartbeatObj heartbeatObj = null;
        public Func<Packet> GetHeatbeatPack = null;

        //重连对象
        public bool IsOpenReConnect = true;
        public ReConnectObj reconnectObj = null;

        public string Tags = "socketHelper";

        //是否连接上了
        public bool IsConnect
        {
            get { return socket != null && socket.Connected && State.Value == ConnectState.Connected; }
        }

        //public static SocketHelper GetInstance()
        //{
        //    return socketHelper;
        //}

        protected abstract void SetInstance();

        void Awake()
        {
            SetInstance();

            if (IsOpenReConnect)
            {
                reconnectObj = new ReConnectObj();
                reconnectObj.Init(this);
            }
        }


        //接收到数据放入数据队列，按顺序取出
        void Update()
        {
            //心跳机制
            if (heartbeatObj != null) heartbeatObj.Update();
            //重连机制
            if (reconnectObj != null) reconnectObj.Update();

            //接收到包的逻辑回调
            int num = 0;
            while (dataQueue.Count > 0 && num <= MaxPackNumPerFarme)
            {
                num++;
                byte[] receiveBs = null;
                lock (dataQueue) { receiveBs = dataQueue.Dequeue(); }
                Packet packet = ProtoManager.Instance.TryDeserialize(receiveBs);
            }
            //if (dataQueue.Count > 0)
            //{
            //    byte[] receiveBs = null;
            //    lock (dataQueue) { receiveBs = dataQueue.Dequeue(); }
            //    Packet packet = ProtoManager.Instance.TryDeserialize(receiveBs);
            //}

            //重连机制
            if (State.Value == ConnectState.ConnectFail || State.Value == ConnectState.ConnectException)
            {
                bool isCanRe = (reconnectObj != null && reconnectObj.IsCanConnect());
                CloseConnect(true, isCanRe, (State.Value == ConnectState.ConnectFail));
                if (isCanRe)
                {
                    reconnectObj.ReConnect();
                }
            }
        }

        void OnDestroy()
        {
            AllClose();
        }

        public void ShowError(string msg)
        {
            Debug.LogErrorFormat("tags:{0}, msg:{1}", Tags, msg);
        }

        public void ShowWarn(string msg)
        {
            Debug.LogWarningFormat("tags:{0}, msg:{1}", Tags, msg);
        }

        /// <summary>
        /// 初始化IP信息
        /// </summary>
        public void InitIpInfo(string serverIp, int serverPort)
        {
            IPAddress address = IPAddress.Parse(serverIp);
            Endpoint = new IPEndPoint(address, serverPort);
        }


        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <returns>The connect.</returns>
        /// <param name="serverIp">Server ip.</param>
        /// <param name="serverPort">Server port.</param>
        /// <param name="connectCallback">Connect callback.</param>
        /// <param name="connectFailedCallback">Connect failed callback.</param>
        public IEnumerator Connect(Action connectCallback, Action connectFailedCallback)
        {
            //若在连接中
            if (IsConnect)
            {
                ShowWarn("已经连接上了,不可在连接");
                yield break;
            }
            if (State.Value == ConnectState.Connecting)
            {
                ShowWarn("正在连接中,不可 在连接");
                yield break;
            }

            ConnectingTime = 0;
            State.Value = ConnectState.Connecting;

            if (reconnectObj != null) reconnectObj.AddConnectNum();

            //采用TCP方式连接  
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //对TCP进行一些设置
            socket.NoDelay = true;         //禁用延迟,即即使传出去的数据很少,也应该立即发送
            socket.Ttl = 64;               //存活时间,这个设置为多长时间合适呢,大了会有影响么
            socket.SendBufferSize = SystemBuffSize;      //设置发送缓冲区的大小
            socket.ReceiveBufferSize = SystemBuffSize;   //设置接收缓冲区的大小

            bool isfail = false;
            try
            {
                IAsyncResult result = socket.BeginConnect(Endpoint, new AsyncCallback(ConnectedCallback), socket);
            }
            catch (Exception e)
            {
                isfail = true;
                ShowError(string.Format("socket连接失败 地址:{0},端口:{1},网络状态{2} \n {3}", Endpoint.Address.ToString(), Endpoint.Port, Application.internetReachability, e));
            }
            //判断超时
            while (!isfail && !socket.Connected && ConnectingTime <= ConnectTimeout)
            {
                ConnectingTime += Time.deltaTime;
                yield return null;
            }
            //连接结束
            ConnectingTime = 0;
            if (!socket.Connected)
            {//连接失败
                State.Value = ConnectState.ConnectFail;
                ShowError(string.Format("socket连接超时 地址:{0},端口:{1},网络状态{2}", Endpoint.Address.ToString(), Endpoint.Port, Application.internetReachability));
                if (connectFailedCallback != null) { connectFailedCallback(); }
                if (OnConnectFail != null) { OnConnectFail(); }
            }
            else
            {
                State.Value = ConnectState.Connected;
                ShowWarn("连接成功 " + Application.internetReachability);

                isStopThread.Value = false;
                //开始接受数据线程
                if (thread_receive == null)
                {
                    thread_receive = new Thread(new ThreadStart(ReceiveSocket));
                    thread_receive.IsBackground = true;
                    thread_receive.Start();
                }
                //开始发送数据线程
                if (thread_send == null)
                {
                    thread_send = new Thread(new ThreadStart(SendSocket));
                    thread_send.IsBackground = true;
                    thread_send.Start();
                }
                //开始心跳
                if (IsOpenHeart)
                {
                    if (heartbeatObj == null)
                    {
                        heartbeatObj = new HeartbeatObj();
                        heartbeatObj.Init(this, GetHeatbeatPack(), SendHeartbeatInterval);
                    }
                    heartbeatObj.Start();
                }

                //重连
                if (reconnectObj != null) reconnectObj.Reset();

                //与socket建立连接成功，开启线程接受服务端数据
                if (connectCallback != null) { connectCallback(); }
                if (OnConnectSuccess != null) { OnConnectSuccess(); }
            }

            if (AddRespDelegate != null && !IsAddResp)
            {
                AddRespDelegate();
                IsAddResp = true;
            }
        }

        public void SendPacket(Packet packet)
        {
            lock (sendPacketQueue)
            {
                ShowWarn(string.Format("发送 {0} 包", packet.m_packetType + "\n " + packet.m_message.ToString()));
                sendPacketQueue.Add(packet);
            }
        }

        private void ConnectedCallback(IAsyncResult asyncConnect)
        {//结束异步操作
            try
            {
                Socket _socket = (Socket)asyncConnect.AsyncState;
                _socket.EndConnect(asyncConnect);
            }
            catch (Exception e)
            {
                ShowError(e.ToString());
            }
        }

        private void ReceiveSocket()
        {
            mDataHolder.Reset();
            try
            {
                while (!isStopThread.Value)
                {
                    Thread.Sleep(ThreadSleepTime);     //隔10毫秒再下去
                    //与服务器断开连接,就等等
                    try
                    {
                        if (!IsConnect) { continue; }
                    }
                    catch (Exception e) { Debug.LogWarning(e); }

                    try
                    {
                        if (socket.Poll(1, SelectMode.SelectRead))
                        {//卡住1微妙,看是否有可读的数据,有的话立马下来,不要使用-1,-1会无限卡住线程,直到有可写入的内容
                         //Receive方法中会一直等待服务端回发消息,如果没有回发会一直在这里等着
                            byte[] bytes = new byte[BuffSize];   //接受数据保存至bytes当中,每次从缓存中最多读取1024个字节
                            int i = socket.Receive(bytes);

                            //返回0,一般就是服务器断开了
                            if (i <= 0)
                            {
                                if (!isStopThread.Value)
                                {
                                    State.Value = ConnectState.ConnectException;
                                    ShowWarn("接收到数据小于0,断开");
                                }
                                continue;
                            }

                            lock (mDataHolder)
                            {
                                mDataHolder.PushData(bytes, i);

                                while (mDataHolder.IsFinished())
                                {
                                    lock (dataQueue)
                                    {
                                        dataQueue.Enqueue(mDataHolder.mRecvData);
                                    }
                                    mDataHolder.RemoveFromHead();
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (!isStopThread.Value)
                        {
                            State.Value = ConnectState.ConnectException;
                            ShowError("接收包异常\n" + e);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (!isStopThread.Value) { State.Value = ConnectState.ConnectException; ShowError("接收循环出了问题\n" + e); }
            }
        }
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <returns></returns>
        private void SendSocket()
        {
            try
            {
                while (!isStopThread.Value)
                {
                    Thread.Sleep(ThreadSleepTime);   //隔10毫秒再下去
                    try
                    {
                        //这儿有时会在关闭服务器连接时候报错, 不过问题不大
                        if (!IsConnect) { continue; }
                    }
                    catch (Exception e) { Debug.LogError(e); }

                    if (sendPacketQueue.Count == 0) { continue; }
                    if (socket.Poll(1, SelectMode.SelectWrite))
                    {//卡住1微妙,看是否可以写入,如果可写,立马下来
                        Packet packet = null;
                        lock (sendPacketQueue)
                        {
                            packet = sendPacketQueue[0];
                            sendPacketQueue.RemoveAt(0);
                        }

                        try
                        {

                            //byte[] msg = packet.GetData();
                            //byte[] buffer = new byte[4];
                            //DataStream writer = new DataStream(buffer, true);
                            //writer.WriteInt32((uint)msg.Length);//增加数据长度

                            byte[] msg = packet.GetData();
                            byte[] buffer = new byte[msg.Length + 4];
                            DataStream writer = new DataStream(buffer);

                            writer.WriteInt32((uint)msg.Length);//增加数据长度
                            writer.WriteRaw(msg);

                            byte[] data = writer.ToByteArray();

                            if (msg.Length + 4 != data.Length)
                            {
                                string errorStr = string.Format("发送的长度 与 实际长度不对,发送长度:{0},实际长度是:{1},包类型是{2}", msg.Length + 4, data.Length, packet.m_packetType);
                                throw new Exception(errorStr);
                            }

                            //hack【暂改】这儿有可能发送给无效的socket,那么要咋办呢
                            int sendLength = socket.Send(data);
                        }
                        catch (Exception e)
                        {
                            if (!isStopThread.Value)
                            {
                                State.Value = ConnectState.ConnectException;
                                ShowError("发送其他异常\n" + e.ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (!isStopThread.Value) { State.Value = ConnectState.ConnectException; ShowError("发送循环异常\n" + e); }
            }
        }

        /// <summary>
        /// 当接到心跳包
        /// </summary>
        public void OnReceiveHeartBeat()
        {
            if (heartbeatObj != null) { heartbeatObj.OnReceiveHeart(); }
        }

        public void UpdateHeatbeatSwitch(bool isOpen)
        {
            if (heartbeatObj != null) { heartbeatObj.UpdateSwitch(isOpen); }
        }

        /// <summary>
        /// 更新心跳的暂停保活状态
        /// </summary>
        public void UpdatePauseKeepLiveStaus(bool isOpen)
        {
            if (heartbeatObj != null) { heartbeatObj.UpdatePauseKeepLiveStaus(isOpen); }
        }

        /// <summary>
        /// 更新重连开关
        /// </summary>
        public void UpdateReConnectSwitch(bool isOpen)
        {
            if (reconnectObj != null) reconnectObj.UpdateReConnectSwitch(isOpen);
        }

        /// <summary>
        /// 全部关闭
        /// </summary>
        public void AllClose()
        {
            ShowWarn("AllClosesocket");

            //关闭线程
            StopThread();
            //关闭连接清除输出
            CloseConnectAndClearData(false, false, false);
        }

        //关闭连接清除输出
        public void CloseConnectAndClearData(bool isNeedDicDel, bool isCanAutoReConnect, bool isConnectFail)
        {
            //关闭连接
            CloseConnect(isNeedDicDel, isCanAutoReConnect, isConnectFail);
            //清除输出
            ClearData();
        }

        /// <summary>
        /// 线程停止
        /// </summary>
        private void StopThread()
        {
            isStopThread.Value = true;
            if (thread_receive != null)
            {//接收线程退出
                thread_receive.Abort();
                thread_receive = null;
            }
            if (thread_send != null)
            {//发送线程退出
                thread_send.Abort();
                thread_send = null;
            }
            if (heartbeatObj != null)
            {//心跳线程退出
                heartbeatObj.StopThread();
            }
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        private void CloseConnect(bool isNeedDicDel, bool isCanAutoReConnect, bool isConnectFail)
        {
            //这儿有概率和收发线程同步进行,导致可能把连接改成未连接了,又被改回异常了
            //可以有个逻辑,关闭连接时,把线程等待,直到重新连接

            //hack【暂改】【网络通信】如果正在连接中,调用了关闭连接怎么办?


            //关闭连接
            if ((socket != null && socket.Connected))
            {//这儿就用 socket.Connected 来判断,用State判断不对,因为它先改成了不在连接的值,在触发到这儿
                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
                catch (Exception e)
                {
                    ShowWarn(e.ToString());
                }
            }
            State.Value = ConnectState.NoConnect;
            socket = null;
            ShowWarn("断开网络连接 " + Application.internetReachability);

            //关闭心跳
            UpdateHeatbeatSwitch(false);
            //清除接收队列的数据,因为一个socket一个缓冲区,再次连接上,就又是一个新的缓冲区,那么mDataHolder里面的数据就接不上了
            lock (mDataHolder) mDataHolder.Reset();

            //回调
            if (isNeedDicDel)
            {
                if (OnDicConnectDelegate != null) OnDicConnectDelegate(isCanAutoReConnect, isConnectFail);
            }
        }

        //清除数据
        private void ClearData()
        {
            lock (dataQueue) { dataQueue.Clear(); }
            lock (sendPacketQueue) { sendPacketQueue.Clear(); }
            lock (mDataHolder) mDataHolder.Reset();
        }
    }

}


#if false
        private bool isFirstDis = true;
        private ConnectState State_Old = ConnectState.NoConnect;

            if (State ==ConnectState.ConnectFail || State == ConnectState.ConnectException)
            {//如果连接失败或者连接异常,就重连看下
                if (isCanReConnect)
                {
                    ReConnectTime += Time.deltaTime;
                    if (ReConnectTime > ReConnectDelayTime)
                    {
                        ReConnect();
                    }
                }
                else
                {
                    if (isFirstDis)
                    {
                        if (OnDicConnectDelegate != null) OnDicConnectDelegate();
                        isFirstDis = false;
                    }
                }
            }


                isFirstDis = true;


        private float ReConnectDelayTime = 1;   //当断开时延迟个一秒,在重连
        private float ReConnectTime;            //重连时间

        /// <summary>
        /// 重连
        /// </summary>
        public void ReConnect()
        {
            if (!isCanReConnect) { return; }
            //若正在连接中,就等等
            if (State == ConnectState.Connecting) { return; }
            if (reconnectNum >= ReconnectMaxNum) { return; }
            if (!isAllowConnect) { return; }
            ReConnectTime = 0;

            CloseConnect(true);//关闭之前的连接

            reconnectNum++;
            Debug.LogWarningFormat("开始第{0}次重连", reconnectNum);
            StartCoroutine(Connect(OnReConnectSuccess, OnReConnectFail));
        }
        /// <summary>
        /// 当重连成功
        /// </summary>
        private void OnReConnectSuccess()
        {
            reconnectNum = 0;
            Debug.LogWarning("重连成功");
            if (reconnectDelegate != null) reconnectDelegate();
        }
        /// <summary>
        /// 当重连失败
        /// </summary>
        private void OnReConnectFail()
        {
            if (reconnectNum >= ReconnectMaxNum)
            {//超过最大重连次数,就告诉业务层
                Debug.LogWarning("超过最大连接次数,不连接");
                //hack【暂改】展示UI
                if (reconnectFailedDelegate != null) reconnectFailedDelegate();
            }
        }

        int reconnectNum = 0;
        bool isCanReConnect = true;          //是否能重连
        /// <summary>
        /// 关闭重连机制
        /// </summary>
        public void CloseReconnect()
        {
            isCanReConnect = false;
        }
        /// <summary>
        /// 打开重连机制
        /// </summary>
        public void OpenReconnect() {
            isCanReConnect = true;
            reconnectNum = 0;
        }


        ConnectLimit connectLimit;
        bool isAllowConnect = true;           //是否允许连接

            connectLimit = new ConnectLimit();
            connectLimit.onConnectOverLImit = OnConnetOverLimit;

            //连接次数的限制
            connectLimit.Update();

            if (!isAllowConnect)
            {
                if (connectFailedCallback != null) connectFailedCallback();
                yield break;
            }

            connectLimit.AddConnectNum();

        /// <summary>
        /// 当连接超限
        /// </summary>
        private void OnConnetOverLimit()
        {
            isAllowConnect = false;
            Debug.LogError("快报警,规定时间内超过最大连接数了");
            //hack【暂改】展示UI
        }

        /// <summary>
        /// 连接限制类
        /// 限定一定时间内的链接次数
        /// </summary>
        class ConnectLimit
        {
            public float currentTime;
            public float connectNum;
            public Action onConnectOverLImit;

            public void Update()
            {
                currentTime += Time.deltaTime;
                if (currentTime >= ConnectLimitTime)
                {
                    currentTime = 0;
                    connectNum = 0;
                }
            }

            public void AddConnectNum()
            {
                connectNum++;
                if (connectNum > ReconnectMaxNum)
                {//当连接超过限制
                    onConnectOverLImit();
                }
            }
        }
#endif

#region 测试逻辑
//byte[] bs = new byte[10];
//byte[] buffer = new byte[bs.Length + 4];
//                    for (int i = 0; i<bs.Length; i++)
//                    {
//                        bs[i] = 10;
//                    }
//                    DataStream writer = new DataStream(buffer, true);
//writer.WriteInt32((uint) bs.Length);//增加数据长度
//                    writer.WriteRaw(bs);
#endregion

#region 一些新知
//在阻塞模式下, send函数的过程是将应用程序请求发送的数据拷贝到发送缓存中发送就返回.但由于发送缓存的存在,表现为:如果发送缓存大小比请求发送的大小要大,那么send函数立即返回,同时向网络中发送数据;否则,send会等待接收端对之前发送数据的确认,以便腾出缓存空间容纳新的待发送数据,再返回(接收端协议栈只要将数据收到接收缓存中, 就会确认, 并不一定要等待应用程序调用recv),如果一直没有空间能容纳待发送的数据,则一直阻塞;

//在非阻塞模式下,send函数的过程仅仅是将数据拷贝到协议栈的缓存区而已,如果缓存区可用空间不够,则尽能力的拷贝,立即返回成功拷贝的大小;如缓存区可用空间为0,则返回-1,同时设置errno为EAGAIN.
#endregion


//1、服务器主动踢出人物的逻辑,不应该去重连
//2、玩家重复登录的逻辑,不应该重连
