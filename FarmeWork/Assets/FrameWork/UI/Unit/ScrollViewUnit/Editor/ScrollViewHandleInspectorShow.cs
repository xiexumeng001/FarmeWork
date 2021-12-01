using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// 无限滚动的编辑器扩展
/// 选择不同的适配类型,展示不同的选项
/// </summary>
[CustomEditor(typeof(ScrollViewHandle))]
public class ScrollViewHandleInspectorShow : Editor
{
    private SerializedObject comPro;//序列化
    private SerializedProperty adapterInfo;                //适配信息
    private SerializedProperty adapterType;                //适配类型

    //private SerializedProperty adapterIsAdapteScrollDir;  //是否适配拖拽方向
    //private SerializedProperty adapterPerLineMaxCount;
    //private SerializedProperty adapterContentSpace;

    void OnEnable()
    {
        comPro = new SerializedObject(target);
        adapterInfo = comPro.FindProperty("AdapterInfo");
        adapterType = adapterInfo.FindPropertyRelative("Type");

        //adapterIsAdapteScrollDir = adapterInfo.FindPropertyRelative("IsAdapteScrollDir");
        //adapterPerLineMaxCount = adapterInfo.FindPropertyRelative("PerLineMaxCount");
        //adapterContentSpace = adapterInfo.FindPropertyRelative("ContentSpace");
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        comPro.Update();//更新test
        ScrollViewHandleAdapterType type = (ScrollViewHandleAdapterType)adapterType.enumValueIndex;
        switch (type)
        {
            case ScrollViewHandleAdapterType.NumPerLine:
                break;
            case ScrollViewHandleAdapterType.WidthHight:
            case ScrollViewHandleAdapterType.Scale:
                //EditorGUILayout.PropertyField(adapterIsAdapteScrollDir);
                //EditorGUILayout.PropertyField(adapterPerLineMaxCount);
                //EditorGUILayout.PropertyField(adapterContentSpace);
                break;
        }
        EditorGUILayout.LabelField("缩放或者宽高类型的适配,默认1行1个道具,且不适配拖拽方向");

        comPro.ApplyModifiedProperties();//应用
    }
}