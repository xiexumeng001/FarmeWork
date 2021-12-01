using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ShipFarmeWork.UI;
using ui;
using ShipFarmeWork.Resource;
using UnityEditor;
using System.IO;

public class UISceneMgr : MonoBehaviour
{
    GameObject game = null;

    public GameObject ResTestGame;
    // Start is called before the first frame update
    void Start()
    {
        ResourceManager.Instance.InitRes(OnResEnd);
    }

    private void OnResEnd()
    {
        UIManager.Instance.AddUIFormInfo("UILogin", new UIInfo("Assets/Test/UI", "ui_Login.prefab", "Main"));
        UIManager.Instance.AddUIFormInfo("UIBag", new UIInfo("Assets/Test/UI", "ui_bag.prefab", "Tip"));
        UIManager.Instance.AddUIFormInfo("UITips", new UIInfo("Assets/Test/UI", "ui_tips.prefab", "Tip", PopMaskLucenyType.Translucence, false));
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.W))
        {
            UILogin login = (UILogin)UIManager.Instance.ShowUIForms("UILogin", 10);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            //LoadSpriteRes();

            AssetBundle ab = AssetBundle.LoadFromFile(ResDefine.GetDataPath("prefabs/new sprite.unity3d"));
            AssetBundle ab_1 = AssetBundle.LoadFromFile(ResDefine.GetDataPath("atlas/bg.unity3d"));
            //GameObject game = ab.LoadAsset<GameObject>("New Sprite");
            //game = GameObject.Instantiate(game);
            //game.transform.SetParent(ResTestGame.transform);
            //game.SetActive(true);

            Sprite sp = ab_1.LoadAsset<Sprite>("ui_com_Popup_bg_new_1");
            GameObject spGame = new GameObject();
            SpriteRenderer spRender = spGame.AddComponent<SpriteRenderer>();
            spRender.sprite = sp;

            //game = ResourceManager.Instance.CloneAsset("Prefabs", "New Sprite", this);
            //game.transform.SetParent(ResTestGame.transform);
            //game.SetActive(true);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            //ResourceManager.instance.ReturnCloneOne("Prefabs", "New Sprite", game);

            ResourceManager.Instance.ReturnAllByRequester(this);
        }

    }


    private void LoadSpriteRes()
    {
        //SINGLEFOLDER
        Sprite sp = (Sprite)ResourceManager.Instance.GetAsset("Atlas/KuangBg/NormalKuang", "ui_kuang_2", typeof(Sprite), this);
        GameObject game = new GameObject();
        SpriteRenderer spRender = game.AddComponent<SpriteRenderer>();
        spRender.sprite = sp;

        sp = (Sprite)ResourceManager.Instance.GetAsset("Atlas/KuangBg/NormalKuang/Nromal_1", "ui_com_Popup_bg", typeof(Sprite), this);
        sp = (Sprite)ResourceManager.Instance.GetAsset("Atlas/KuangBg", "ui_tips_bg_lines", typeof(Sprite), this);

        //Folder
        sp = (Sprite)ResourceManager.Instance.GetAsset("Atlas/Bg", "ui_com_bg2", typeof(Sprite), this);
        sp = (Sprite)ResourceManager.Instance.GetAsset("Atlas/Bg/Bg_1", "ui_com_Popup_bg_new", typeof(Sprite), this);
        //ONLYFOLDER
        sp = (Sprite)ResourceManager.Instance.GetAsset("Atlas/topBg", "icon_chat_press_new", typeof(Sprite), this); //有值
        sp = (Sprite)ResourceManager.Instance.GetAsset("Atlas/topBg/subBg", "icon_mail", typeof(Sprite), this);   //应该为空
    }
}
