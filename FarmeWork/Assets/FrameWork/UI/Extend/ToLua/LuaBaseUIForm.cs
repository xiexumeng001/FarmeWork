//hack【UI】【ToLua】
#if false
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LuaInterface;
using OrangeEngine;

namespace ShipFarmeWork.UI
{
    public class LuaBaseUIForm : BaseUIForm
    {
        protected LuaTable luaClassIntance;

        private Dictionary<int, HashSet<LuaFunction>> luaFuncs = new Dictionary<int, HashSet<LuaFunction>>();
        public void BindLuaClass(LuaTable luaClassIntance)
        {
            this.luaClassIntance = luaClassIntance;
            Util.CallMethod(luaClassIntance["SetGameObject"], luaClassIntance, gameObject);
            Util.CallMethod(luaClassIntance["SetBehaviour"], luaClassIntance, this);
        }

        /// <summary>
        /// 当首次进入
        /// 用来发现所有要用到的子节点,做一些和数据无关的初始化的操作
        /// </summary>
        protected override void OnFirstEnter()
        {
            Util.CallMethod(luaClassIntance["OnFirstEnter"], luaClassIntance);
        }

        /// <summary>
        /// 开始展示
        /// 带着参数然后刷新展示
        /// </summary>
        protected override void StartShow(params object[] param)
        {
            //hack【待定】这样写不太好,再看看
            int length = param.Length;
            object[] param_ = new object[length + 1];
            param_[0] = luaClassIntance;
            for (int i = 0; i < length; i++) {
                param_[i + 1] = param[i];
            }

            Util.CallMethod(luaClassIntance["StartShow"], param_);
        }


        /// <summary>
        /// 添加单击事件
        /// </summary>
        public void AddTouchEvent(EventTriggerListenerType type, GameObject go, LuaFunction luafunc)
        {
            if (go == null || luafunc == null)
                return;

            HashSet<LuaFunction> luafuncList = null;

            luaFuncs.TryGetValue(go.GetInstanceID(), out luafuncList);

            if (luafuncList == null)
            {
                luafuncList = new HashSet<LuaFunction>();
                luaFuncs.Add(go.GetInstanceID(), luafuncList);
                luafuncList.Add(luafunc);
                EventTriggerListener.AddListenerLuaFunc(type, luaClassIntance, luafunc, go);
            }
            else
            {
                if (!luafuncList.Contains(luafunc))
                {
                    luafuncList.Add(luafunc);
                    EventTriggerListener.AddListenerLuaFunc(type, luaClassIntance, luafunc, go);
                }
            }
        }

    }


}
#endif