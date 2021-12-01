using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace ShipFarmeWork.UI
{
    /// <summary>
    /// 界面层级信息
    /// </summary>
    [Serializable]
    public class UILayerInfo
    {
        public string Name;
        public int Depth;

        [HideInInspector]
        public RectTransform Tran;
    }

    /// <summary>
    /// UI管理类
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("UI摄像机")]
        public Camera UICamera;
        [Header("根Canvas")]
        public Canvas RootCanvas;
        [Header("根CanvasScaler")]
        public CanvasScaler RootCanvasScaler;
        [Header("层级信息")]
        public UILayerInfo[] UILayerInfos;
        [Header("遮罩界面")]
        public UIMask UIMask;

        //UI窗体预设路径
        private Dictionary<string, UIInfo> _DicFormsPaths;
        //----------所有窗体的集合---------------
        //缓存所有UI窗体(键:界面键值)
        private Dictionary<string, List<BaseUIForm>> _DicALLUIForms;
        //展示中的UI窗体(键:界面层级)
        public Dictionary<string, List<BaseUIForm>> _ShowUIs;
        public Dictionary<string, List<BaseUIForm>> ShowUIs { get { return _ShowUIs; } }

        //界面层级间隔
        private int FormLayerInter = 100;
        //实际的屏幕分辨率与参考屏幕分辨率的比值
        public Vector2 RealScaleRate { get; set; }
        //单例
        private static UIManager _Instance = null;
        public static UIManager Instance { get { return _Instance; } }

        void Awake()
        {
            //字段初始化
            _Instance = this;
            _DicALLUIForms = new Dictionary<string, List<BaseUIForm>>();
            _ShowUIs = new Dictionary<string, List<BaseUIForm>>();
            _DicFormsPaths = new Dictionary<string, UIInfo>();

            //生成父级
            RectTransform roonRectTram = RootCanvas.transform as RectTransform;
            foreach (var item in UILayerInfos)
            {
                GameObject game = new GameObject(item.Name, typeof(RectTransform));
                RectTransform rectTran = (RectTransform)game.transform;

                rectTran.SetParent(RootCanvas.transform, false);
                rectTran.anchorMin = new Vector2(0, 0);
                rectTran.anchorMax = new Vector2(1, 1);
                rectTran.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, roonRectTram.rect.width);
                rectTran.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, roonRectTram.rect.height);
                item.Tran = rectTran;

                _ShowUIs.Add(item.Name, new List<BaseUIForm>());
            }

            RectTransform canvasTran = (RectTransform)transform;
            RealScaleRate = canvasTran.rect.size / RootCanvasScaler.referenceResolution;
            //"根UI窗体"在场景转换的时候,不允许销毁
            DontDestroyOnLoad(transform);
        }

        /// <summary>
        /// 添加窗体信息
        /// </summary>
        /// <param name="key"></param>
        /// <param name="info"></param>
        public void AddUIFormInfo(string key, UIInfo info)
        {
            _DicFormsPaths.Add(key, info);
        }

        /// <summary>
        /// 展示UI窗体
        /// 注意:暂不支持对同一个名称进行多个界面的展示
        /// </summary>
        /// <param name="uiFormName">ui界面的名称</param>
        /// <param name="layerType">ui界面打开在哪个层级</param>
        /// <param name="param">传给UI界面的参数</param>
        public BaseUIForm ShowUIForms(string uiKey, params object[] param)
        {
            BaseUIForm baseUIForm = null;
            try
            {
                if (string.IsNullOrEmpty(uiKey)) { Debug.LogError("传入键值是空的"); return baseUIForm; }
                if (!_DicFormsPaths.ContainsKey(uiKey)) { Debug.LogErrorFormat("没有获取到{0}界面的信息", uiKey); return baseUIForm; }

                UILayerInfo layerInfo = null;
                UIInfo uiInfo = _DicFormsPaths[uiKey];
                foreach (var item in UILayerInfos)
                {
                    if (item.Name == uiInfo.LayerType)
                    {
                        layerInfo = item;
                        break;
                    }
                }
                if (layerInfo == null) { Debug.LogErrorFormat("没有获取到{0}界面对应的层级信息", uiKey); return baseUIForm; }

                //加载
                baseUIForm = LoadFormsToAllUIFormsCache(uiKey);
                if (baseUIForm == null) { return baseUIForm; }
                baseUIForm.transform.SetParent(layerInfo.Tran, false);
                //展示
                LoadUIFormToShowCache(baseUIForm, layerInfo, param);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            return baseUIForm;
        }

        /// <summary>
        /// 关闭UI窗体
        /// </summary>
        public void CloseUIForm(BaseUIForm uiForm, bool isDestroy)
        {
            if (uiForm == null) { return; }
            bool isSuccess = false;
            bool isErrorLog = true;
            if (!uiForm.IsUsing)
            {
                Debug.LogWarningFormat("界面{0}已经关闭了", uiForm.UIKey);
                isSuccess = true;
                isErrorLog = false;
            }
            else
            {
                isSuccess = ExitUIFormFromShowCache(uiForm, isDestroy);
            }

            if (isDestroy && isSuccess)
            {//销毁了,就从缓存移除了
                RemoveUICache(uiForm, isErrorLog);
            }
        }

        /// <summary>
        /// 关闭所有UI界面
        /// </summary>
        public void CloseAllForm()
        {
            foreach (var item in _ShowUIs)
            {
                //所有正常展示的界面
                List<BaseUIForm> values = new List<BaseUIForm>(item.Value);
                for (int i = 0; i < values.Count; i++)
                {
                    BaseUIForm uiForm = values[i];
                    // 若常驻,就不销毁
                    CloseUIForm(uiForm, !uiForm.UiInfo.IsResident);
                }
            }
        }

        /// <summary>
        /// 获得键值对应的UI界面集合
        /// </summary>
        public List<BaseUIForm> GetShowUIList(string uiKey)
        {
            List<BaseUIForm> uiList = new List<BaseUIForm>();
            _DicFormsPaths.TryGetValue(uiKey, out UIInfo uiInfo);
            foreach (var item in _ShowUIs[uiInfo.LayerType])
            {
                if (item.UIKey == uiKey)
                {
                    uiList.Add(item);
                }
            }
            return uiList;
        }

        /// <summary>
        /// 根据UI窗体的名称,加载到"所有UI窗体"缓存集合中
        /// 功能:检查"所有UI窗体"集合中,是否已经加载过,否则才加载
        /// </summary>
        /// <param name="uiFormName"></param>
        /// <returns></returns>
        private BaseUIForm LoadFormsToAllUIFormsCache(string uiFormName)
        {
            //查看缓存是否有对应的资源
            List<BaseUIForm> baseUIformList = GetUIListCache(uiFormName);
            if (baseUIformList.Count > 0)
            {
                for (int i = 0; i < baseUIformList.Count; i++)
                {
                    if (!baseUIformList[i].IsUsing)
                    {
                        return baseUIformList[i];
                    }
                }
            }
            //没找到合适的资源,那么再加载一个
            return LoadUIForm(uiFormName);
        }

        /// <summary>
        /// 将该界面从缓存中移除
        /// </summary>
        /// <param name="uiForm"></param>
        /// <returns></returns>
        private bool RemoveUICache(BaseUIForm uiForm, bool errorLog)
        {
            List<BaseUIForm> baseUIList;
            _DicALLUIForms.TryGetValue(uiForm.UIKey, out baseUIList);
            bool isRemove = baseUIList.Remove(uiForm);
            if (!isRemove && errorLog)
            {
                Debug.LogErrorFormat("没找到{0}界面的缓存资源", uiForm.UIKey);
            }
            return isRemove;
        }

        /// <summary>
        /// 获取ui名称对应的资源集合,若没有的话,在缓存中申请一下
        /// </summary>
        /// <param name="uiFormName"></param>
        /// <returns></returns>
        private List<BaseUIForm> GetUIListCache(string uiFormName)
        {
            List<BaseUIForm> baseUIformList = null;
            if (_DicALLUIForms.ContainsKey(uiFormName))
            {
                baseUIformList = _DicALLUIForms[uiFormName];
            }
            else
            {
                _DicALLUIForms[uiFormName] = baseUIformList = new List<BaseUIForm>();
            }
            return baseUIformList;
        }

        private BaseUIForm LoadUIForm(string uiFormName)
        {
            BaseUIForm baseUiForm = null;           //窗体基类

            UIInfo info = _DicFormsPaths[uiFormName];

            //加载预制体
            GameObject goCloneUIPrefabs = LoadResou(info.ResourcePath, info.ResourceName);
            if (goCloneUIPrefabs.GetComponent<Canvas>() == null) { goCloneUIPrefabs.AddComponent<Canvas>(); }  //加上Canvas，每个UI界面都至少挂个Canvas,否则要不断的大量生成Mesh
            if (goCloneUIPrefabs.GetComponent<GraphicRaycaster>() == null) { goCloneUIPrefabs.AddComponent<GraphicRaycaster>(); } //加上射线检测组件
            if (goCloneUIPrefabs.GetComponent<UIDepth>() == null) { goCloneUIPrefabs.AddComponent<UIDepth>(); } //加上深度组件

            //挂载脚本
            if (goCloneUIPrefabs != null)
            {
                baseUiForm = goCloneUIPrefabs.GetComponent<BaseUIForm>();
                if (baseUiForm == null)
                {
                    Debug.LogErrorFormat("界面{0}未挂载对应脚本", uiFormName);
                }
                //把克隆体,加入到"所有UI 窗体"(缓存)集合中
                GetUIListCache(uiFormName).Add(baseUiForm);
                baseUiForm.InitBaseInfo(uiFormName, info);

                goCloneUIPrefabs.SetActive(false);
                return baseUiForm;
            }
            else
            {
                Debug.LogError("未加载到UI预制体  " + uiFormName);
            }
            return null;

        }

        /// <summary>
        /// 把UI窗体加载到当前正在展示的UI窗体集合中
        /// </summary>
        /// <param name="uiFormName"></param>
        private void LoadUIFormToShowCache(BaseUIForm baseUiForm, UILayerInfo layerInfo, params object[] param)
        {
            List<BaseUIForm> layerUiList = _ShowUIs[baseUiForm.UiInfo.LayerType];

            //最大层级
            int useLayerNum = layerInfo.Depth;
            if (layerUiList.Count > 0)
            {
                useLayerNum = layerUiList[layerUiList.Count - 1].LayerNum + FormLayerInter;
            }

            layerUiList.Add(baseUiForm);
            baseUiForm.Display(useLayerNum, param);
            //遮罩更新
            UIMask.OnUIShow(baseUiForm);
        }

        /// <summary>
        /// 结束UI界面的展示
        /// </summary>
        /// <param name="strUIFormName">UI界面名称</param>
        /// <param name="isDestroy">是否删除</param>
        private bool ExitUIFormFromShowCache(BaseUIForm baseUiForm, bool isDestroy)
        {
            List<BaseUIForm> layerUiList = _ShowUIs[baseUiForm.UiInfo.LayerType];

            if (layerUiList.Remove(baseUiForm))
            {
                //隐藏界面
                baseUiForm.Close();
                //遮罩更新
                UIMask.OnUIClose(baseUiForm);

                if (isDestroy) { UnLoadUIRes(baseUiForm); }
                return true;
            }
            else
            {
                Debug.LogError("要移除的界面不再展示队列");
                return false;
            }
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns
        private GameObject LoadResou(string resPath, string resName)
        {
            return (GameObject)UIResLoad.LoadRes(resPath, resName, typeof(GameObject));
        }

        /// <summary>
        /// 卸载界面
        /// </summary>
        private void UnLoadUIRes(BaseUIForm form)
        {
            UIResLoad.UnLoadGameObject(form.gameObject);
        }
    }
}