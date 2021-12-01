using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using ShipFarmeWork.Resource;
using System.Collections.Generic;

public class FirstPage : MonoBehaviour
{

    [SerializeField]
    private Text m_textVersion;

    private static object m_holder = new object();
    private static FirstPage _instance;
    private Action m_clickOKCallback;                //当点击提示面板OK的回调
    private Action m_clickCancelCallback;            //当点击提示面板取消的回调

    [SerializeField]
    private GameObject m_objUpgradePanel;            //更新提示面板
    [SerializeField]
    private Text m_textTitle;
    [SerializeField]
    private Text m_textUpgradePrompt;
    [SerializeField]
    private Button m_btnCancel;
    [SerializeField]
    private Button m_btnOK;

    [SerializeField]
    private string m_minPoint = "";
    [SerializeField]
    private string m_maxPoint = "....";

    [SerializeField]
    private GameObject m_objSlider;
    [SerializeField]
    private Image m_imgSlider;
    [SerializeField]
    private Text m_textSliderRate;
    [SerializeField]
    private Text m_textPrompt;

    [SerializeField]
    private Text m_textUID;

    [SerializeField]
    private RectTransform m_effectLight;
    private float m_barLength;

    private bool m_showAnim = false;
    private string m_strPrompt = string.Empty;
    private string m_strAnim = string.Empty;
    private string m_downLoadSpeed = string.Empty;

    //更新时的语言数据
    private Dictionary<string, string> UpdateLangDic = new Dictionary<string, string>();

    void Awake()
    {
        m_barLength = m_imgSlider.rectTransform.sizeDelta.x;
    }

    public void Start()
    {
        var versionShow = PlayerPrefs.GetString(ShipFarmeWork.Resource.ResourceConfig.LocalVersionKey);

        string versionStr = GetStartUpLangID("10046");
        if (versionShow == string.Empty)
            m_textVersion.text = string.Format("{0}:{1}", versionStr, Application.version);
        else
        {
            //判断一下大小
            if (string.Compare(Application.version, versionShow) > 0)
                m_textVersion.text = string.Format("{0}:{1}", versionStr, Application.version);
            else
                m_textVersion.text = string.Format("{0}:{1}", versionStr, versionShow);
        }

        m_textUID.text = PlayerPrefs.GetString("USER_ID", string.Empty);

        StartCoroutine(PrmoptStrAnimation());
    }

    public static FirstPage Instance
    {
        get
        {
            return _instance;
        }
    }

    private void InitPanel()
    {
        //更新提示面板隐藏
        m_objUpgradePanel.SetActive(false);
        //按钮回调
        m_btnOK.onClick.AddListener(OnClickOK);
        m_btnCancel.onClick.AddListener(OnClickCancel);
        m_strAnim = m_minPoint;
    }

    private void OnClickOK()
    {
        if (m_clickOKCallback != null)
            m_clickOKCallback();
    }

    private void OnClickCancel()
    {
        if (m_clickCancelCallback != null)
            m_clickCancelCallback();
    }

    private static void SetupPage(GameObject objPage)
    {
        _instance = objPage.GetComponent<FirstPage>();
        _instance.InitPanel();
        //设置在UI Root下
        objPage.transform.SetParent(GameObject.Find("UIRoot").transform);
        objPage.transform.localPosition = Vector3.zero;
        objPage.transform.localScale = Vector3.one;
        RectTransform rect = objPage.GetComponent<RectTransform>();
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        objPage.SetActive(true);
    }

    private static void CreateFromResFold()
    {
        //从Resources下获取
        GameObject model = Resources.Load<GameObject>("FirstPage");
        if (model)
        {
            GameObject clone = GameObject.Instantiate(model);
            SetupPage(clone);
        }
        else
        {
            Debug.LogError("我曹，读取FirstPage失败了居然！不能够啊思密达！");
        }
    }

    public static void Create()
    {
        //暂定只是从Res文件夹里提取prefab
        CreateFromResFold();

        //先加载上语言表
        _instance.LoadLanguage();
    }

    public void Close()
    {
        GameObject.DestroyImmediate(this.gameObject);
        //AppFacade.Instance.GetManager<ResourceManager>(ManagerName.Resource).ResourcesUnloadUnusedAssets();

        Debug.Log("close ..........");
    }

    /// <summary>
    /// 显示loading
    /// </summary>
    public void ShowLoading()
    {
        GameObject.Find("UI Root").transform.Find("FirstPage").gameObject.SetActive(true);
        m_imgSlider.gameObject.SetActive(true);
    }


    [SerializeField]
    private RectTransform m_rectUpdateTipBG = null;
    [SerializeField]
    private RectTransform m_rectUpdatePrompt = null;
    [SerializeField]
    private RectTransform m_rectUpdatePromptText = null;
    private bool lastLongState = false;
    /// <summary>
    /// 设置展示的提示信息
    /// </summary>
    /// <param name="title">标题</param>
    /// <param name="detail">细节</param>
    /// <param name="clickOKCallback">ok的回调</param>
    /// <param name="clickCancelCallback">取消的回调</param>
    /// <param name="okText">确认按钮的文本</param>
    /// <param name="cancelText">取消按钮的文本</param>
    /// <param name="isLong"></param>
    public void SetUpgradeTips(string title, string detail, Action clickOKCallback, Action clickCancelCallback, string okText, string cancelText, bool isLong = false)
    {
        m_clickOKCallback = clickOKCallback;
        m_clickCancelCallback = clickCancelCallback;
        m_btnCancel.gameObject.SetActive(m_clickCancelCallback != null);
        m_btnOK.gameObject.SetActive(m_clickOKCallback != null);
        m_btnOK.transform.GetChild(0).GetComponent<Text>().text = GetStartUpLangID(okText);
        if (string.IsNullOrEmpty(cancelText))
            m_btnCancel.gameObject.SetActive(false);
        else
        {
            m_btnCancel.transform.GetChild(0).GetComponent<Text>().text = GetStartUpLangID(cancelText);
            m_btnCancel.gameObject.SetActive(true);
        }
        if (lastLongState != isLong)
        {
            if (!isLong)
            {
                m_rectUpdateTipBG.sizeDelta = new Vector2(930, 510);
                m_rectUpdatePrompt.sizeDelta = new Vector2(885, 320);
                m_rectUpdatePromptText.sizeDelta = new Vector2(866, m_rectUpdatePromptText.sizeDelta.y);
                //866
            }
            else
            {
                m_rectUpdateTipBG.sizeDelta = new Vector2(1180, 830);
                m_rectUpdatePrompt.sizeDelta = new Vector2(1130, 633);
                m_rectUpdatePromptText.sizeDelta = new Vector2(1100, m_rectUpdatePromptText.sizeDelta.y);
                //1100
            }
            lastLongState = isLong;
        }
        m_objUpgradePanel.SetActive(true);
        m_textUpgradePrompt.text = GetStartUpLangID(detail);
        m_textTitle.text = GetStartUpLangID(title);
    }

    public void HideUpgradeTips()
    {
        m_objUpgradePanel.SetActive(false);
    }

    public void UpdatePromptText(string str, bool anim)
    {
        string promptStr = GetStartUpLangID(str);
        if (promptStr != m_strPrompt)
            m_strAnim = m_minPoint;

        m_strPrompt = promptStr;
        m_showAnim = anim;
        UpdatePromptStr();
    }

    private IEnumerator PrmoptStrAnimation()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);

            m_strAnim = m_strAnim + ".";

            if (m_strAnim == m_maxPoint)
                m_strAnim = m_minPoint;

            UpdatePromptStr();
        }
    }

    private void UpdatePromptStr()
    {
        if (m_showAnim)
            m_textPrompt.text = string.Format("{0}{1}{2}", m_strPrompt, m_downLoadSpeed, m_strAnim);
        else
            m_textPrompt.text = string.Format("{0}{1}", m_strPrompt, m_downLoadSpeed);
    }

    //更新进度条
    public void UpdateProgress(float rate)
    {
        rate = Mathf.Clamp(rate, 0f, 1f);
        m_imgSlider.fillAmount = rate;
        m_textSliderRate.text = string.Format("{0:0.0}%", rate * 100f);
        //更新光的位置
        m_effectLight.anchoredPosition = new Vector2(m_barLength * rate, 0f);
    }

    //是否激活进度条文字
    public void ActiveProgressText(bool active)
    {
        m_objSlider.SetActive(true);
        if (active)
        {
            m_textSliderRate.gameObject.SetActive(true);
        }
        else
        {
            m_textSliderRate.gameObject.SetActive(false);
            UpdateProgress(0);
        }
    }
    public void ActiveProgress(bool active)
    {
        m_objSlider.SetActive(active);
    }

    public void UpdateDownLoadSpeed(string speed)
    {
        m_downLoadSpeed = speed;
        UpdatePromptStr();
    }


    /// <summary>
    /// 加载语言
    /// </summary>
    private void LoadLanguage()
    {
        try
        {
            //先加载上语言表
            TextAsset file = Resources.Load<TextAsset>(ResDefine.UpdateLanguageFileName);
            if (file != null)
            {
                string content = file.text;
                string[] lines = content.Split('\n');
                for (int i = 1; i < lines.Length; ++i)
                {
                    string[] args = lines[i].Split('|');
                    if (args.Length >= 2)
                    {
                        if (!UpdateLangDic.ContainsKey(args[0]))
                        {
                            UpdateLangDic.Add(args[0], args[1]);
                        }
                        else { Debug.LogError("语言表键值有两个:" + args[0]); }
                    }
                }
            }
            else
            {
                Debug.LogError("错误，读取start up langID失败~");
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public string GetStartUpLangID(string key)
    {
        if (!UpdateLangDic.ContainsKey(key))
        {
            return key;
        }
        return UpdateLangDic[key];
    }
}