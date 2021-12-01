using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class CreateUI
{

    [MenuItem("GameObject/UI/Image")]
    static void CreatImage()
    {
        GameObject go = new GameObject("Image", typeof(Image));
        go.GetComponent<Image>().raycastTarget = false;
        SetParent(go);
    }


    [MenuItem("GameObject/UI/Text")]
    static void CreatText()
    {
        GameObject go = new GameObject("Text", typeof(Text));
        go.GetComponent<Text>().raycastTarget = false;
        SetTextCompont(go.GetComponent<Text>(), "Text");
        SetParent(go);
    }


    [MenuItem("GameObject/UI/Button")]
    static void CreatButton()
    {
        GameObject go = new GameObject("Button", typeof(Image), typeof(Button));
        RectTransform buttonTran = go.GetComponent<RectTransform>();
        SetParent(go);

        //生成text
        GameObject textGo = new GameObject("Text", typeof(Text));
        //设置位置
        RectTransform rectTran = textGo.GetComponent<RectTransform>();
        rectTran.SetParent(go.transform, false);
        rectTran.anchorMin = new Vector2(0, 0);
        rectTran.anchorMax = new Vector2(1, 1);
        rectTran.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, buttonTran.rect.width);
        rectTran.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, buttonTran.rect.height);

        textGo.GetComponent<Text>().raycastTarget = false;
        SetTextCompont(textGo.GetComponent<Text>(), "Button");
    }


    /// <summary>
    /// 设置父级
    /// </summary>
    /// <param name="gameObj"></param>
    static void SetParent(GameObject gameObj)
    {
        if (Selection.activeTransform)
        {
            if (Selection.activeTransform.GetComponentInParent<Canvas>())
            {
                gameObj.transform.SetParent(Selection.activeTransform, false);
            }
        }
        else
        {
            Transform p = GameObject.FindObjectOfType<Canvas>().transform;
            if (p != null)
            {
                gameObj.transform.SetParent(p, false);
            }
        }
    }

    static void SetTextCompont(Text text,string showText)
    {
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.black;
        text.text = showText;
    }


}
