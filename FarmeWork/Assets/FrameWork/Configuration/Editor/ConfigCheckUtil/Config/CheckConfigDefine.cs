using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShipFarmeWork.Configuration.Editor
{
    /// <summary>
    /// 检查配置
    /// </summary>
    public class CheckConfigDefine
    {
        //配表路径
        public static string ConfigPath = "Assets\\StarShip\\Res\\default\\Config\\";
        //配表后缀
        public static string ConfigFix = ".json";
        //检查配表的配置文件
        public static string CheckConfigPath = "Assets\\Editor\\ConfigCheckUtil\\Config\\CheckConfig.json";
        //加载表对象
        public static LoadTable LoadTableObj = new LoadTable_LitJson();
    }

}
