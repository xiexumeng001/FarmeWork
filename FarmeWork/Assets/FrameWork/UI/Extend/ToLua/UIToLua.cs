#if false
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LuaInterface;

public static class UIToLua
{
    /// <summary>
    /// 添加UILua组件
    /// </summary>
    public static LuaBaseUIForm AddUILuaComponent(GameObject go, string luaClassPath)
    {
        LuaBaseUIForm uiForm = go.AddComponent<LuaBaseUIForm>();
        //获取到Lua对应脚本的实例
        object[] param = { luaClassPath };
        object[] returnObj = Util.CallMethod("_G", "GetLuaInstance", param);
        LuaTable luaClassIntance = (LuaTable)returnObj[0];
        uiForm.BindLuaClass(luaClassIntance);
        return uiForm;
    }

}
#endif
