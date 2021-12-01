using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShipFarmeWork.Sound
{
    /// <summary>
    /// 声音辅助器
    /// </summary>
    public static class SoundHelper
    {
        /// <summary>
        /// 播放音乐
        /// </summary>
        public static void PlayBGM(string path, string assetName, bool isLoop = true)
        {
            SoundManager.Instance.PlayBGM(path, assetName, isLoop);
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        public static void PlayEffect(string path, string name)
        {
            SoundManager.Instance.PlayEffect(path, name);
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        public static void PlayEffect(string path, string name, float delay, bool isLoop, string tags = "Effect", int group = 1)
        {
            SoundManager.Instance.PlayEffect(path, name, delay, isLoop, group, tags);
        }

        /// <summary>
        /// 停止声音(包含bgm与音效)
        /// </summary>
        /// <param name="name"></param>
        public static void StopSoundEffect(string name)
        {
            SoundManager.Instance.StopSoundEffect(name);
        }

        /// <summary>
        /// 停止声音,带着标签查找
        /// </summary>
        public static void StopSoundEffectWithTags(string name, string tags)
        {
            SoundManager.Instance.StopSoundEffectWithTags(name, tags);
        }

        /// <summary>
        /// 停止所有bgm
        /// </summary>
        public static void StopBGM()
        {
            SoundManager.Instance.StopBGM();
        }

        /// <summary>
        /// 停止所有音效
        /// </summary>
        public static void StopAllEffects()
        {
            SoundManager.Instance.StopAllEffects();
        }

        /// <summary>
        /// 播放音效的回调
        /// </summary>
        private static void PlayEffectCallback(string path, string name)
        {
            SoundManager.Instance.PlayEffect(path, name);
        }


        /// <summary>
        /// 设置音效开关
        /// </summary>
        public static void SeteffMute(bool isOpen)
        {
            SoundManager.Instance.EffectMute = !isOpen;
        }

        /// <summary>
        /// 设置BGM开关
        /// </summary>
        public static void SetBGMMute(bool isOpen)
        {
            SoundManager.Instance.BgmMute = !isOpen;
        }

        /// <summary>
        /// 设置音效音量
        /// </summary>
        /// <param name="value"></param>
        public static void SeteffVolume(int value)
        {
            SoundManager.Instance.SeteffVolume(value);
            SoundManager.Instance.SaveEffectVolume();
        }

        /// <summary>
        /// 设置BGM音量
        /// </summary>
        /// <param name="value"></param>
        public static void SetbgmVolume(int value)
        {
            SoundManager.Instance.SetbgmVolume(value);
            SoundManager.Instance.SaveBgmVolume();
        }

        /// <summary>
        /// 清除所有声音
        /// </summary>
        public static void CleadAllSound()
        {
            SoundManager.Instance.ClearAllSound();
        }

    }
}