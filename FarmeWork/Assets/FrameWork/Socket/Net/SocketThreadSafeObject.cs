using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShipFarmeWork.Net
{
    /// <summary>
    /// net的线程安全的对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SocketThreadSafeObject<T>
    {
        object lockObj = new object();
        private T _value;
        public T Value
        {
            get
            {
                lock (lockObj)
                {
                    return _value;
                }
            }

            set
            {
                lock (lockObj)
                {
                    _value = value;
                }
            }
        }

        public SocketThreadSafeObject()
        {
        }

        public SocketThreadSafeObject(T value)
        {
            _value = value;
        }
    }
}

