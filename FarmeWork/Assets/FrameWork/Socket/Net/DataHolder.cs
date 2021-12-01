using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ShipFarmeWork.Net
{
    /// <summary>
    /// 数据处理类,管理着从服务器获取的数据缓存,并对数据裁切,移除
    /// 这个拆包逻辑看起来更耗费内存,但是还好,可以用
    /// </summary>
    public class DataHolder
    {
        public static int LengthByteNum = 4;        //记录包体长度数字占几个字节

        //接收到的数据缓存
        public byte[] mRecvDataCache;//use array as buffer for efficiency consideration
                                     //接收到的字节数组(已经切割好之后的数组,是一个完整包)
        public byte[] mRecvData;

        private int mTail = -1;
        private int packLen;
        /// <summary>
        /// 将收到的数据放入缓存中
        /// </summary>
        /// <param name="data"></param>
        /// <param name="length"></param>
        public void PushData(byte[] data, int length)
        {
            if (mRecvDataCache == null)
                mRecvDataCache = new byte[length];

            if (this.Count + length > this.Capacity)//current capacity is not enough, enlarge the cache
            {
                byte[] newArr = new byte[this.Count + length];
                mRecvDataCache.CopyTo(newArr, 0);
                mRecvDataCache = newArr;
            }
            //从data的0下标开始复制length长度的数组到mRecvDataCache数组从mTail + 1下标之后的位置上
            Array.Copy(data, 0, mRecvDataCache, mTail + 1, length);
            mTail += length;
        }

        /// <summary>
        /// 是否可以生成一个完整的包了
        /// </summary>
        /// <returns></returns>
        public bool IsFinished()
        {
            if (this.Count == 0) { return false; }   //长度为0
            if (this.Count < LengthByteNum) { return false; }    //字节组数长度为4,连一个展示数组长度的数字都组不起来

            DataStream reader = new DataStream(mRecvDataCache);
            packLen = (int)reader.ReadInt32();
            if (packLen > 0)
            {
                if (this.Count - LengthByteNum >= packLen)
                {//若接收到的字节数组长度 大于 要接收的长度,则生成接收的字节数组
                    mRecvData = new byte[packLen];
                    Array.Copy(mRecvDataCache, LengthByteNum, mRecvData, 0, packLen);
                    return true;
                }
            }
            return false;
        }

        public void Reset()
        {
            mTail = -1;
            packLen = 0;
            mRecvDataCache = null;
            mRecvData = null;
        }

        /// <summary>
        /// 从缓存中移除数据
        /// </summary>
        public void RemoveFromHead()
        {
            int countToRemove = packLen + LengthByteNum;
            if (countToRemove > 0 && this.Count - countToRemove > 0)
            {
                Array.Copy(mRecvDataCache, countToRemove, mRecvDataCache, 0, this.Count - countToRemove);
            }
            mTail -= countToRemove;
        }

        //当前缓存池的容量
        public int Capacity
        {
            get
            {
                return mRecvDataCache != null ? mRecvDataCache.Length : 0;
            }
        }

        //当前缓存的字节数量
        public int Count
        {
            get
            {
                return mTail + 1;
            }
        }
    }
}