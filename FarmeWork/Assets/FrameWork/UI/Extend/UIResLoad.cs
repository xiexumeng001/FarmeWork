using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShipFarmeWork.UI
{
    /// <summary>
    /// UI资源加载
    /// </summary>
    public static class UIResLoad
    {
        /// <summary>
        /// 加载资源
        /// </summary>
        /// <returns></returns>
        public static UnityEngine.Object LoadRes(string resPath, string resName, Type type)
        {
#if UNITY_EDITOR
            string res = resPath + "/" + resName;
            var obj = UnityEditor.AssetDatabase.LoadAssetAtPath(res, type);
            obj = GameObject.Instantiate(obj);
            return obj;
#endif
            return null;
        }

        /// <summary>
        /// 卸载资源
        /// </summary>
        /// <param name="game"></param>
        public static void UnLoadGameObject(GameObject game)
        {
#if UNITY_EDITOR
            GameObject.Destroy(game);
#endif
        }
    }
}
