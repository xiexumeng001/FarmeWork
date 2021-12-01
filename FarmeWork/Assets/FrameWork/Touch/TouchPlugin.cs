using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace ShipFarmeWork.Touch
{
    public class TouchPlugin : MonoBehaviour
    {
        /// <summary>
        /// 主摄像机(默认是主摄像机)
        /// </summary>
        public Camera MainCamera;

        /// <summary>
        /// 当前触发的物体
        /// </summary>
        public TouchTrigger _TriggerGam = null;

        /// <summary>
        /// 基础的缩放物体,用来在没有物体捕获到缩放时调用
        /// 当前选中的物体没有捕获到缩放,就传递给基础物体
        /// </summary>
        public TouchTrigger _BaseScaleGame = null;

        public Vector2 DefaultPos = new Vector2(-999, -999);

        /// <summary>
        /// 上一帧的点击位置
        /// </summary>
        public Vector2 PosScreenAgo_Click;

        /// <summary>
        /// 是否正在拖拽中
        /// </summary>
        public bool isDrag = false;

        /// <summary>
        /// 当按下时是否按在了UI上
        /// </summary>
        public bool IsInUiOnDown = false;
        public Vector2 DragPos_Last;  //上一帧的拖拽位置,给手机用的

        /// <summary>
        /// 当前被选中的物体
        /// 用于实现全局取消
        /// </summary>
        public TouchTrigger _trigger_cancel;
        /// <summary>
        /// 上一个被点击的物体
        /// </summary>
        public TouchTrigger ClickTriggerBefore;

        public Action OnFingerDown;    //当手指按下瞬间的回调


        public bool IsShowLog = false;


        /// <summary>
        /// 上一帧的手指点击数
        /// </summary>
        private int lastTouches;

        //单例
        private static TouchPlugin _Instance;
        public static TouchPlugin Instance
        {
            get { return _Instance; }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="baseScaleGame">基础的缩放物体</param>
        public void Init(TouchTrigger baseScaleGame, Camera camera, bool isShowLog)
        {
            _BaseScaleGame = baseScaleGame;
            MainCamera = camera;
            IsShowLog = isShowLog;
        }

        void Start()
        {
            _Instance = this;
            PosScreenAgo_Click = DefaultPos;
            DragPos_Last = DefaultPos;
        }

        // Update is called once per frame
        void Update()
        {
#if UNITY_EDITOR
            //手指操作的回调
            if (Input.GetMouseButtonDown(0))
            {
                if (OnFingerDown != null) OnFingerDown();
            }
#elif (UNITY_IOS || UNITY_ANDROID)
            Touch[] touches_an = Input.touches;
            //当单指
            if (touches_an.Length == 1)
            {
                if (touches_an[0].phase == TouchPhase.Began)
                {
                    if (OnFingerDown != null) OnFingerDown();
                }
            }
#endif

            if (!UpdateIsCanTrigger()) return;

#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0))
            {//当按下
                PosScreenAgo_Click = Input.mousePosition;
                OnClickDown();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                PosScreenAgo_Click = DefaultPos;
                if (isDrag)
                {//当拖拽结束
                    isDrag = false;
                    OnDragEnd();
                }
                else
                {//当抬起
                    OnClickUp();
                }
                _TriggerGam = null;
            }
            else if (Input.GetMouseButton(0) && (_TriggerGam != null) && PosScreenAgo_Click != DefaultPos)
            {//若手势处于正常按下状态
                Vector2 vec = PosScreenAgo_Click - (Vector2)Input.mousePosition;
                if (vec.magnitude > 0.1f)
                {
                    if (!isDrag)
                    {
                        OnDragStart(vec);
                        isDrag = true;
                    }
                    else
                    {
                        //当拖拽
                        OnDrag(vec);
                    }
                }
                else
                {//当长按

                }
            }
            else if (Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                //当缩放
                float scale = Input.GetAxis("Mouse ScrollWheel");
                OnDoubleTouch(scale);
            }
#elif (UNITY_IOS || UNITY_ANDROID)

            Touch[] touches = Input.touches;
            //-----------为了让缩放结束时不影响拖动-----------
            //hack【暂改】这儿有个问题,就是放上两只手,一只离开,另一只不能拖动
            if(touches.Length !=0 && lastTouches>touches.Length)
            {
               return;
            }
            lastTouches = touches.Length;

            //--------------------------------------------

            if (touches.Length == 0)
            {//hack【暂改】当拖拽结束,暂时先用这种逻辑,因为坑爹的是 拖拽结束不会触发 Ended事件
                if (isDrag)
                {//拖拽结束
                    isDrag = false;
                    OnDragEnd();
                    _TriggerGam = null;
                }
                else {
                    if (DragPos_Last != DefaultPos) { DragPos_Last = DefaultPos; }
                }
            }
            else if (touches.Length == 1)  //当单指
            {
                switch (touches[0].phase)
                {
                    case TouchPhase.Began:   //按下
                        OnClickDown();

                        break;
                    case TouchPhase.Moved:    //拖动
                        Vector2 deltaNum = touches[0].position - touches[0].deltaPosition;
                        if (!isDrag)
                        {
                            OnDragStart(deltaNum);
                            isDrag = true;
                        }
                        else
                        {
                            OnDrag(deltaNum);
                        }
                        DragPos_Last = GetFingerPos();
                        break;
                    case TouchPhase.Ended:
                        if (!isDrag)
                        {//抬起
                            OnClickUp();
                            _TriggerGam = null;
                        }
                        break;
                    case TouchPhase.Stationary:   //长按
                        break;
                }
            }
            else if (touches.Length > 1)
            {
                //没有按下,就先走按下的逻辑
                if (_TriggerGam == null)
                {
                    OnClickDown();
                }
                if (IsShowLog) {
                    Debug.LogWarning(" 开始缩放++++++  ");
                }

                //多值判定为缩放
                Touch touchZero = touches[0];
                Touch touchOne = touches[1];
                //记录前一帧触摸位置
                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
                // 记录两指间长度
                float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;
                // 记录缩放长度
                float num_ = prevTouchDeltaMag - touchDeltaMag;
                if (IsShowLog)
                {
                    Debug.LogWarning(" 开始缩放++++++ 222222   " + num_);
                }

                if (num_ != 0) OnDoubleTouch(num_);

            }
#endif

        }

        /// <summary>
        /// 更新是否能触发值
        /// </summary>
        /// <returns>是否能触发</returns>
        private bool UpdateIsCanTrigger()
        {
#if UNITY_EDITOR
            if (IsInUiOnDown)
            {//若当按下时在UI上,那么之后的逻辑都不会触发,包括当抬起
                if (Input.GetMouseButtonUp(0)) { IsInUiOnDown = false; }
                return false;
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {//判断按下去的时候是否按在了UI上,如果按在了UI上,那么就不能触发,直到抬起复原值
                    IsInUiOnDown = IsOverUI(Input.mousePosition);
                }
            }

#elif (UNITY_IOS || UNITY_ANDROID)
            Touch[] touches = Input.touches;
            if (touches.Length == 0) {
                if (IsInUiOnDown)
                {//若不能触发,那么即使抬起动作改为了可以触发,这一帧也不能触发
                    IsInUiOnDown = false;
                    return false;
                }
            }
            else {
                if (touches[0].phase == TouchPhase.Began) {
                    IsInUiOnDown = IsOverUI(touches[0].position);
                    //IsInUiOnDown = IsOverUI(Input.mousePosition);
                }
            }
#endif
            return !IsInUiOnDown;
        }

        /// <summary>
        /// 是否操作在UI上
        /// </summary>
        /// <returns></returns>
        private bool IsOverUI(Vector3 pos)
        {
            if (IsPointerOverGameObject(pos))
                return true;
            if (EventSystem.current.IsPointerOverGameObject())
                return true;
            if (GUIUtility.hotControl != 0)   //防止OnGUI界面穿透
                return true;
            if (GUIUtility.keyboardControl != 0) //防止OnGUI界面穿透
                return true;
            return false;
        }

        /// <summary>
        /// 更改点击物体
        /// </summary>
        public void ChangeClickTriggerGame(TouchTrigger trigger)
        {
            _trigger_cancel = trigger;
        }


        /// <summary>
        /// 当按下
        /// </summary>
        public void OnClickDown()
        {
            _TriggerGam = FindFingerTrigger();

            if (IsShowLog) Debug.LogWarning("当前 按下 物体是 " + (_TriggerGam == null ? "" : _TriggerGam.name));

            if (_TriggerGam != null)
            {
                //生成参数
                OnTouchDownData data = new OnTouchDownData();
                data.PosScreen = GetFingerPos();


                //发送消息
                _TriggerGam.OnTouchDown(_TriggerGam.gameObject, data);
            }
        }


        /// <summary>
        /// 当抬起
        /// </summary>
        public void OnClickUp()
        {
            if (IsShowLog)
            {
                Debug.LogWarning("当前 抬起 物体是 " + (_TriggerGam == null ? "" : _TriggerGam.name));
            }

            if (_TriggerGam == null) return;
            //生成参数
            OnTouchUpData data = new OnTouchUpData();
            data.PosScreen = GetFingerPos();
            data.TouchOnTouchUpData = _TriggerGam.gameObject;


            //WSM 如果有之前选中过物品，且不是自己，就执行这个物品的取消选中功能
            if (_trigger_cancel != null)
            {
                if (_TriggerGam != _trigger_cancel)
                {
                    _trigger_cancel.WhenNotSelected(_TriggerGam.gameObject, data);
                }
            }
            ChangeClickTriggerGame(_TriggerGam);


            //发送消息
            _TriggerGam.OnTouchUp(_TriggerGam.gameObject, data);
            _TriggerGam.OnTouchClick(_TriggerGam.gameObject, data);
        }

        /// <summary>
        /// 当拖拽开始
        /// </summary>
        /// <param name="dragNum"></param>
        public void OnDragStart(Vector2 dragNum)
        {
            if (IsShowLog)
            {
                Debug.LogWarning("当前 拖拽 物体是 " + (_TriggerGam == null ? "" : _TriggerGam.name));
            }

            if (_TriggerGam == null) return;

            OnDragData data = new OnDragData();
            data.PosScreen = GetFingerPos();
            data.DragVector = dragNum;

            _TriggerGam.OnDragStart(_TriggerGam.gameObject, data);
        }


        /// <summary>
        /// 当拖拽
        /// </summary>
        public void OnDrag(Vector2 dragNum)
        {
            if (_TriggerGam == null) return;

            OnDragData data = new OnDragData();
            data.PosScreen = GetFingerPos();
            data.DragVector = dragNum;

            _TriggerGam.OnDrag(_TriggerGam.gameObject, data);
        }

        /// <summary>
        /// 当拖拽结束
        /// </summary>
        /// <param name="data"></param>
        public void OnDragEnd()
        {
            if (_TriggerGam == null) return;

            OnDragEndData data = new OnDragEndData();
            data.PosScreen = GetFingerPos();

            _TriggerGam.OnDragEnd(_TriggerGam.gameObject, data);
        }


        /// <summary>
        /// 当双指缩放
        /// </summary>
        public void OnDoubleTouch(float scaleNum)
        {
            TouchTrigger triggerGam = null;
            //在电脑上应该是缩放上一次选中的物体,在手机上因为缩放需要手指点击,所以就是当前点击的物体
#if UNITY_EDITOR
            triggerGam = _trigger_cancel;
#elif (UNITY_IOS || UNITY_ANDROID)
            triggerGam = _TriggerGam;
#endif
            if (IsShowLog)
            {
                Debug.LogWarning("当前 缩放 物体是 " + (triggerGam == null ? "" : triggerGam.name));
            }

            if (triggerGam == null) return;

            OnDoubleTouchData data = new OnDoubleTouchData();
            data.ScaleNum = scaleNum;

            bool isGetScale = triggerGam.OnDoubleTouch(triggerGam.gameObject, data);
            if (!isGetScale && _BaseScaleGame != null)
            {//如果没捕获到缩放 并且 有基础缩放物体,就调用基础缩放物体的当缩放
                _BaseScaleGame.OnDoubleTouch(triggerGam.gameObject, data);

            }
        }


        /// <summary>
        /// 获取当前手指对应的触发器
        /// </summary>
        /// <param name="StartVector"></param>
        /// <param name="EndVector"></param>
        /// <returns></returns>
        public TouchTrigger FindFingerTrigger()
        {//射线检测
            TouchTrigger chooseTrigger = null;
            GameObject chooseGam = FindFingerGameObject();
            if (chooseGam != null)
            {
                chooseTrigger = chooseGam.GetComponent<TouchTrigger>();
                if (chooseTrigger == null)
                {
                    chooseTrigger = chooseGam.GetComponentInParent<TouchTrigger>();
                }
            }
            return chooseTrigger;
        }


        /// <summary>
        /// 获取手指对应的物体
        /// </summary>
        /// <param name="StartVector"></param>
        /// <param name="EndVector"></param>
        /// <returns></returns>
        public GameObject FindFingerGameObject()
        {//射线检测      
            GameObject chooseGam = null;
            RaycastHit hit;
            Ray ray = MainCamera.ScreenPointToRay(GetFingerPos());
            //tast.Add(MainCamera .name+ "点击位置" + GetFingerPos() + " 转换后的位置"+ ray);
            //Debug.DrawLine(ray.origin, ray.origin + ray.direction * 1000, Color.red);
            if (Physics.Raycast(ray, out hit, 1000))
            //if (Physics.Raycast(startVector, endVector, out hit))
            {
                chooseGam = hit.transform.gameObject;
            }
            return chooseGam;
        }


        /// <summary>
        /// 获取手指对应的某个层级的物体
        /// </summary>
        /// <param name="StartVector"></param>
        /// <param name="EndVector"></param>
        /// <returns></returns>
        public GameObject FindFingerGameObject(string layerName)
        {//射线检测      
            GameObject chooseGam = null;
            RaycastHit hit;
            Ray ray = MainCamera.ScreenPointToRay(GetFingerPos());
            int layer = LayerMask.GetMask(layerName);
            if (Physics.Raycast(ray, out hit, 1000, layer))
            //if (Physics.Raycast(startVector, endVector, out hit))
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                    chooseGam = hit.transform.gameObject;
            }
            return chooseGam;
        }

        /// <summary>
        /// 获取手指点在世界上的坐标
        /// </summary>
        /// <returns></returns>
        public Vector3 GetFingerInWorldPos()
        {
            return MainCamera.ScreenToWorldPoint(GetFingerPos());
        }

        /// <summary>
        /// 获取当前手势位置
        /// </summary>
        /// <returns></returns>
        //hack【暂改】【手势插件】手机端获取手指位置的逻辑先这样写,因为当移动结束的位置获取不了,应该不会有大问题
        private Vector3 GetFingerPos()
        {
#if UNITY_EDITOR

#elif (UNITY_IOS || UNITY_ANDROID)
            Touch[] touches = Input.touches;
            if (touches.Length == 0 && DragPos_Last != DefaultPos)
            {
                return DragPos_Last;
            }
            return touches[0].position;
#endif
            return Input.mousePosition;
        }


        /// <summary>
        /// 判断是否点到了UI上
        /// 这种写法可以保证在手机上也可以起作用
        /// </summary>
        /// <param name="screenPosition"></param>
        /// <returns></returns>
        public static bool IsPointerOverGameObject(Vector2 screenPosition)
        {
            //实例化点击事件
            PointerEventData eventDataCurrentPosition = new PointerEventData(UnityEngine.EventSystems.EventSystem.current);
            //将点击位置的屏幕坐标赋值给点击事件
            eventDataCurrentPosition.position = new Vector2(screenPosition.x, screenPosition.y);

            List<RaycastResult> results = new List<RaycastResult>();
            //向点击处发射射线
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

            return results.Count > 0;
        }

        /// <summary>
        /// 添加点击回调
        /// </summary>
        public void AddFingerDownAction(Action action)
        {
            OnFingerDown += action;
        }

        /// <summary>
        /// 取消点击回调
        /// </summary>
        public void RemoveFingerDownAction(Action action)
        {
            OnFingerDown -= action;
        }

        ///  Test
        //List<string> tast = new List<string>();

        //private void OnGUI()
        //{
        //    GUIStyle style = new GUIStyle
        //    {
        //        border = new RectOffset(10, 10, 10, 10),
        //        fontSize = 30,
        //        fontStyle = FontStyle.BoldAndItalic,
        //    };
        //    // normal:Rendering settings for when the component is displayed normally.
        //    int i = 0;
        //    foreach (var item in tast)
        //    {
        //        style.normal.textColor = new Color(200 / 255f, 180 / 255f, 150 / 255f);    // 需要除以255，因为范围是0-1
        //        GUI.Label(new Rect(100 , 100 + i, 200, 80), item, style);
        //        i += 50;
        //    }

        //    if (tast.Count>10)
        //    {
        //        tast.Clear();
        //    }
        //}

    }

}

#region 废弃逻辑
//if (!isDrag)
//{//若正处于拖拽过程中,就不判断是否在界面上
//    if (IsPointerOverGameObject(Input.mousePosition))
//        return;
//    if (EventSystem.current.IsPointerOverGameObject())
//        return;
//    if (GUIUtility.hotControl != 0)   //防止OnGUI界面穿透
//        return;
//    if (GUIUtility.keyboardControl != 0) //防止OnGUI界面穿透
//        return;
//}
#endregion