using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShipFarmeWork.Notify
{
    //使用泛型的逆变，使泛型具有继承性
    public delegate void NotifyCallback();
    public delegate void NotifyCallback<in T>(T arg);
    public delegate void NotifyCallback<in T, in T1>(T arg1, T1 arg2);
    public delegate void NotifyCallback<in T, in T2, in T3>(T arg1, T2 arg2, T3 arg3);
    public delegate void NotifyCallback<in T, in T2, in T3, in T4>(T arg1, T2 arg2, T3 arg3, T4 arg4);
    public delegate void NotifyCallback<in T, in T2, in T3, in T4, in T5>(T arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
}
