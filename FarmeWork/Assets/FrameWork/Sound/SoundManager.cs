#define ENABLE_MULTI_SOUND_EFFECT

using ShipFarmeWork.Sound;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace ShipFarmeWork.Sound
{
    public class SoundManager:MonoBehaviour
    {
        //延迟播放
        private struct DelayPlay
        {
            public int group;
            public string path;
            public string name;
            public float playTime;
            public bool isLoop;
            public string tags;
        }
        private HashSet<DelayPlay> delayPlaySet = new HashSet<DelayPlay>();

        public AudioMixerGroup m_bgmGroup;      //背景
        public AudioMixerGroup m_effectGroup;   //音效

        public int maxPlayNum = 10;             //最大播放数量

        //Auto reset pool
        public float m_checkInterval = 3f;
        private float m_checkTimer = 0f;

        [Range(0, 100)]
        public int bgmVolume = 100;
        [Range(0, 100)]
        public int effVolume = 100;
        private float cureffVol = 1.0f;
        private float curbgmVol = 1.0f;


        //Only effect will be added in this list
        public List<AudioSource> m_audios = new List<AudioSource>();
        private AudioSource[] bgmaudList;
        private AudioSource[] effaudList;
        private Dictionary<AudioSource, float> BgmProgressDic = new Dictionary<AudioSource, float>();


        private Transform m_bgmNode = null;
        private Transform m_effectNode = null;

        public bool m_bgmMute = false;
        public bool m_effectMute = false;

        private static string NAME_SOUND_NODE = "SOUND_NODE";
        private static string NAME_BGM_NODE = "BGM_NODE";
        private static string NAME_EFFECT_NODE = "EFFECT_NODE";

        //BGM声音
        private string KEY_Volume_BGM { get { return "Volume_BGM"; } }
        //BGM开关
        private string KEY_MULT_BGM { get { return "MULT_BGM"; } }
        
        //音效声音值
        private string KEY_Volume_Effect { get { return "Volume_Effect"; } }
        //音效开关
        private string KEY_MULT_EFFECT { get { return "MULT_EFFECT"; } }

        private static SoundManager _Instance;
        public static SoundManager Instance { get { return _Instance; } }

        private void Awake()
        {
            var i = Init;
            _Instance = this;
        }

        private bool m_inited = false;
        private bool Init
        {
            get
            {
                if (!m_inited)
                {
                    //Init sound manager
                    GameObject gameMgrObj = gameObject;
                    if (gameMgrObj)
                    {
                        GameObject soundNode = new GameObject(NAME_SOUND_NODE);
                        soundNode.transform.localPosition = Vector3.zero;
                        soundNode.transform.SetParent(gameMgrObj.transform);

                        m_bgmNode = new GameObject(NAME_BGM_NODE).transform;
                        m_bgmNode.SetParent(soundNode.transform);
                        m_bgmNode.localPosition = Vector3.zero;

                        m_effectNode = new GameObject(NAME_EFFECT_NODE).transform;
                        m_effectNode.SetParent(soundNode.transform);
                        m_effectNode.localPosition = Vector3.zero;

                    }
                    else
                        Debug.LogError("Can not find Game Manager!!!");

                    m_checkTimer = 0f;

                    UpdateMuteState();

                    m_inited = true;
                }
                return m_inited;
            }
        }

        /// <summary>
        /// 设置最大播放数量
        /// </summary>
        public void SetMaxPlayNum(int num)
        {
            maxPlayNum = num;
        }


        private void GetAudio(string path, string name, bool fromPool, int group, bool isLoop, string tags, System.Action<UnityEngine.Object, int, bool, string> callback)
        {
            if (callback == null)
                return;

            AudioSource aSource = null;

            //允许播放多个相同的声音
#if ENABLE_MULTI_SOUND_EFFECT
            AudioSource lastOne = null;

            if (fromPool)
            {
                for (int i = 0; i < m_audios.Count; ++i)
                {
                    if (m_audios[i] != null && m_audios[i].name == name)
                    {
                        lastOne = m_audios[i];
                        if (!m_audios[i].isPlaying)
                        {
                            aSource = m_audios[i];
                            break;
                        }
                    }
                }
            }

            if (!aSource && lastOne)
            {
                aSource = Instantiate<AudioSource>(lastOne);
                aSource.transform.SetParent(m_effectNode);
                aSource.name = lastOne.name;
                m_audios.Add(aSource);
            }
#else
            if (fromPool)
            {
                for (int i = 0; i < m_audios.Count; ++i)
                {
                    if (m_audios[i] != null && m_audios[i].name.ToLower().Equals(name.ToLower()))
                    {
                        aSource = m_audios[i];
                        aSource.Stop();
                        break;
                    }
                }
            }

#endif

            if (aSource)
            {
                //Ready to Play
                callback(aSource, group, isLoop, tags);
                return;
            }
            else
            {
                AudioClip clip = SoundLoad.LoadRes(path, name);
                if (clip)
                {
                    callback(clip, group, isLoop, tags);
                }
            }
        }

        private void RemoveAllChildren(Transform parent)
        {
            if (parent == null)
                return;

            for (int i = 0; i < parent.childCount; ++i)
            {
                Destroy(parent.GetChild(i).gameObject);
            }
            BgmProgressDic.Clear();
        }
        
        public bool BgmMute
        {
            set
            {
                m_bgmMute = value;

                AudioSource[] aSources = m_bgmNode.GetComponentsInChildren<AudioSource>();
                for (int i = 0; i < aSources.Length; ++i)
                {
                    aSources[i].mute = m_bgmMute;
                }

                PlayerPrefs.SetInt(KEY_MULT_BGM, m_bgmMute ? 1 : 0);
            }
            get
            {
                return m_bgmMute;
            }
        }

        public bool EffectMute
        {
            set
            {
                m_effectMute = value;

                AudioSource[] aSources = m_effectNode.GetComponentsInChildren<AudioSource>();
                for (int i = 0; i < aSources.Length; ++i)
                {
                    aSources[i].mute = m_effectMute;
                }
                PlayerPrefs.SetInt(KEY_MULT_EFFECT, m_effectMute ? 1 : 0);
            }
            get
            {
                return m_effectMute;
            }
        }

        private AudioSource CreateAudioSource(AudioClip audio, string tags)
        {
            AudioSource aSource = null;
            GameObject newObj = new GameObject();
            if (newObj)
            {
                aSource = newObj.AddComponent<AudioSource>();
                aSource.Stop();
                aSource.clip = audio;
                aSource.name = audio.name;
                aSource.playOnAwake = false;
                //记录标记
                newObj.AddComponent<SoundTag>().Tags = tags;
            }
            return aSource;
        }

        /// <summary>
        /// 播放背景音乐
        /// </summary>
        /// <param name="audioObj"></param>
        private void PlayBGMSource(UnityEngine.Object audioObj, int group, bool isLoop,string tags)
        {
            if (audioObj == null)
                return;

            //Stop current bgm first.
            if (m_bgmNode.childCount > 0)
            {
                Transform cur = m_bgmNode.GetChild(0);
                if (cur)
                {
                    if (cur.name == name)
                    {
                        return;
                    }
                    else
                        StopBGM();
                }
            }

            AudioSource aSource = null;
            if (audioObj is AudioClip)
            {
                AudioClip clip = audioObj as AudioClip;
                aSource = CreateAudioSource(clip, tags);
                aSource.transform.SetParent(m_bgmNode);
            }
            else if (audioObj is AudioSource)
            {
                aSource = audioObj as AudioSource;
                aSource.GetComponent<SoundTag>().Tags = tags;
            }

            //Ready to play.
            if (aSource)
            {
                aSource.loop = isLoop;
                if (m_bgmGroup)
                    aSource.outputAudioMixerGroup = m_bgmGroup;
                aSource.mute = m_bgmMute;
                aSource.Play();
                BgmProgressDic[aSource] = 0;
            }
        }

        private void PlayerEffectSource(UnityEngine.Object audioObj, int group, bool isLoop, string tags)
        {
            if (audioObj == null)
                return;

            AudioSource aSource = null;
            if (audioObj is AudioClip)
            {
                AudioClip clip = audioObj as AudioClip;
                aSource = CreateAudioSource(clip, tags);
                aSource.transform.SetParent(m_effectNode);
                m_audios.Add(aSource);
            }
            else if (audioObj is AudioSource)
            {
                aSource = audioObj as AudioSource;
                aSource.GetComponent<SoundTag>().Tags = tags;
            }

            //Ready to play.
            if (aSource)
            {
                aSource.loop = isLoop;
                if (group == 1 && m_effectGroup)
                {
                    aSource.outputAudioMixerGroup = m_effectGroup;
                    aSource.pitch = Time.timeScale;
                }
                aSource.mute = m_effectMute;
                aSource.Play();
            }
        }

        public void PlayBGM(string path, string name, bool isLoop = true)
        {
            //if (BgmMute)
            //{
            //    return;
            //}
            GetAudio(path, name, false, 0, isLoop, "BGM", PlayBGMSource);
        }

        public void PlayEffect(string path, string name, float delay = 0, bool isLoop = false, int group = 1, string tags = "Effect")
        {
            //if (EffectMute)
            //{
            //    return;
            //}

            if (delay <= 0f)
            {
                //若没到最大播放数量再播放
                int currPlayNum = 0;
                foreach (var item in m_audios)
                {
                    if (item.isPlaying) { currPlayNum++; }
                }
                if (currPlayNum < maxPlayNum)
                {
                    GetAudio(path, name, true, group, isLoop, tags, PlayerEffectSource);
                }
            }
            else
            {
                DelayPlay delayPlay;
                delayPlay.path = path;
                delayPlay.name = name;
                delayPlay.group = group;
                delayPlay.playTime = Time.time + delay / Time.timeScale;
                delayPlay.isLoop = isLoop;
                delayPlay.tags = tags;

                if (delayPlaySet == null)
                    delayPlaySet = new HashSet<DelayPlay>();

                delayPlaySet.Add(delayPlay);
            }
        }

        public void PlayBGMFromRes(string name)
        {
            bool i = Init;

            AudioClip clip = Resources.Load<AudioClip>(name);
            if (clip)
            {
                PlayBGMSource(clip, 0, true, "BGM");
            }
        }

        public void StopSoundEffect(string name)
        {
            if (m_audios == null)
                return;

            for (int i = m_audios.Count - 1; i >= 0; --i)
            {
                if (m_audios[i].name.ToLower().Equals(name.ToLower()))
                {
                    m_audios[i].Stop();
                    break;
                }
            }

            delayPlaySet.RemoveWhere(delegate(DelayPlay delay)
            {
                return delay.name.ToLower().Equals(name.ToLower());
            });
        }

        public void StopSoundEffectWithTags(string name, string tags)
        {
            if (m_audios == null)
                return;

            for (int i = m_audios.Count - 1; i >= 0; --i)
            {
                if (m_audios[i].name.ToLower().Equals(name.ToLower()) && m_audios[i].GetComponent<SoundTag>().Tags == tags)
                {
                    m_audios[i].Stop();
                }
            }

            delayPlaySet.RemoveWhere(delegate (DelayPlay delay)
            {
                return delay.name.ToLower().Equals(name.ToLower()) && delay.tags == tags;
            });
        }

        public void StopBGM()
        {
            RemoveAllChildren(m_bgmNode);
        }

        public void StopAllEffects()
        {

        }

        public void UpdateMuteState()
        {
            //Attamp load mult state
            m_bgmMute = PlayerPrefs.GetInt(KEY_MULT_BGM, 0) == 1;
            m_effectMute = PlayerPrefs.GetInt(KEY_MULT_EFFECT, 0) == 1;

            bgmVolume = PlayerPrefs.GetInt(KEY_Volume_BGM, bgmVolume);
            effVolume = PlayerPrefs.GetInt(KEY_Volume_Effect, effVolume);
        }

        public void SeteffVolume(int value)
        {
            effVolume = value;
            // 控制音量
            cureffVol = (float)effVolume / 100;
            effaudList = m_effectNode.GetComponentsInChildren<AudioSource>();
            foreach (AudioSource v in effaudList)
            {
                v.volume = cureffVol;
            }
        }
        //保存音效高低
        public void SaveEffectVolume()
        {
            PlayerPrefs.SetInt(KEY_Volume_Effect, effVolume);
        }

        public void SetbgmVolume(int value)
        {
            bgmVolume = value;
            // 控制音量
            curbgmVol = (float)bgmVolume / 100;
            bgmaudList = m_bgmNode.GetComponentsInChildren<AudioSource>();
            foreach (AudioSource v in bgmaudList)
            {
                v.volume = curbgmVol;

                if (BgmProgressDic.ContainsKey(v))
                {
                    if (v.isPlaying)
                    {
                        BgmProgressDic[v] = v.time;
                    }
                    else
                    {
                        if (BgmProgressDic[v] + Time.deltaTime < v.clip.length - 3)
                        {//这儿是莫名原因停止了播放,可能是因为换耳机等,所以继续播放
                            v.time = BgmProgressDic[v];
                            v.Play();
                        }
                        else
                        {//正常的播放结束了,不用管了
                            BgmProgressDic.Remove(v);
                        }
                    }
                }
            }
        }

        public void SaveBgmVolume()
        {
            PlayerPrefs.SetInt(KEY_Volume_BGM, bgmVolume);
        }


        //Clear the pool
        private void ResetPool()
        {
            //Debug.Log("重置池子");
            HashSet<string> already = new HashSet<string>();
            for (int i = m_audios.Count - 1; i >= 0; --i)
            {
                AudioSource aSource = m_audios[i];
                if (aSource == null)
                {
                    m_audios.RemoveAt(i);
                    return;
                }
                if (already.Contains(aSource.clip.name) && !aSource.isPlaying)
                {
                    Destroy(aSource.gameObject);
                    m_audios.RemoveAt(i);
                }
                else
                    already.Add(aSource.clip.name);
            }
        }

        private void Update()
        {

            if (delayPlaySet != null && delayPlaySet.Count > 0)
            {
                delayPlaySet.RemoveWhere(delegate (DelayPlay delay)
                {
                    //播放并移除
                    if (Time.time >= delay.playTime)
                    {
                        if (m_audios.Count < maxPlayNum)
                        {
                            GetAudio(delay.path, delay.name, true, delay.group, delay.isLoop, delay.tags, PlayerEffectSource);
                        }
                        return true;
                    }
                    return false;
                });
            }

            if (m_audios.Count > 0)
            {
                m_checkTimer += Time.deltaTime;
                if (m_checkTimer >= m_checkInterval)
                {
                    ResetPool();
                    m_checkTimer = 0f;
                }
            }

            SetbgmVolume(bgmVolume);
            SeteffVolume(effVolume);

        }
        
        public void Renew()
        {
            if (delayPlaySet != null && delayPlaySet.Count > 0)
                delayPlaySet.Clear();

            for (int i = m_audios.Count - 1; i >= 0; --i)
            {
                if (m_audios[i] == null)
                    continue;

                if (m_audios[i].isPlaying)
                    m_audios[i].Stop();

                AudioSource aSource = m_audios[i];
                Destroy(aSource.gameObject);
            }
            m_audios.Clear();
            BgmProgressDic.Clear();
        }

        /// <summary>
        /// 清除所有声音
        /// </summary>
        public void ClearAllSound()
        {
            Renew();
            StopBGM();
            SoundLoad.ClearAllRes();
        }
    }
}