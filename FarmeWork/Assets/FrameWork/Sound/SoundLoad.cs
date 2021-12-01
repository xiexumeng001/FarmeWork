using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ShipFarmeWork.Resource;

namespace ShipFarmeWork.Sound
{
    public static class SoundLoad
    {
        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static AudioClip LoadRes(string path, string name)
        {
            return ResHelp.GetAsset<AudioClip>(path, name, SoundManager.Instance.gameObject);
        }

        /// <summary>
        /// 清除所有资源
        /// </summary>
        public static void ClearAllRes()
        {
            ResHelp.ReturnAllByRequester(SoundManager.Instance.gameObject);
        }

    }
}