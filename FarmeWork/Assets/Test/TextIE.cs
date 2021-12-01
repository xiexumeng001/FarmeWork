using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextIE : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //try
        //{
        //    TextTry();
        //}
        //catch (Exception e)
        //{
        //    Debug.LogError("捕获到异常" + e);
        //}
        StartCoroutine(test());

        //StartCoroutine(test1());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void TextTry()
    {
        StartCoroutine(test());
    }

    IEnumerator test()
    {
        int num = 0;
        while (num < 2)
        {
            yield return test1();

            yield return new WaitForSeconds(1);
            num++;
            Debug.LogWarning("test");
        }
    }

    IEnumerator test1()
    {
        int num = 0;
        while (num < 2)
        {
            yield return new WaitForSeconds(1);
            num++;
            Debug.LogWarning("test1");
            //yield break;
            //string str = null;
            //str.ToString();
        }
    }
}
