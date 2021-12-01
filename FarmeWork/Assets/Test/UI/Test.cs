using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Transform ImageTran;
    public Transform ParentTran;

    private void Awake()
    {
        //RectTransform rectTran = (RectTransform)ImageTran;
        //Debug.LogWarning("Awake当前宽高:" + rectTran.rect.width + " " + rectTran.rect.height);
    }

    // Start is called before the first frame update
    void Start()
    {
        GameObject imaGame = (GameObject)Resources.Load("Image");
        GameObject imaGameClone = Instantiate(imaGame/*, ParentTran*/);
        ImageTran = imaGameClone.transform;
        ImageTran.SetParent(ParentTran, false);

        RectTransform rectTran = (RectTransform)ImageTran;
        Debug.LogWarning("Start当前宽高:" + rectTran.rect.width + " " + rectTran.rect.height);
    }

    // Update is called once per frame
    void Update()
    {
        RectTransform rectTran = (RectTransform)ImageTran;
        Debug.LogWarning("Update当前宽高:" + rectTran.rect.width + " " + rectTran.rect.height);
    }
}
