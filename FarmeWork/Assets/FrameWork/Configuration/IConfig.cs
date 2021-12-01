using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

namespace ShipFarmeWork.Configuration
{
    /// <summary>
    /// 所有数据配置文件的接口
    /// </summary>
    public interface IConfig
    {
        /// <summary>
        /// 获取ID
        /// </summary>
        /// <returns></returns>
        int getID();

        /// <summary>
        /// 获取数据
        /// </summary>
        //void GetData(JsonData data);

    }
}
