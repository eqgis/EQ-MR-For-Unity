using Holo.XR.Android;
using UnityEngine;
using UnityEngine.Events;

namespace Holo.Speech
{
    public class SbcPlugin : MonoBehaviour
    {
        [Header("自动鉴权")]
        public bool autoAuth = true;

        [Header("思必驰授权设置")]
        public string apiKey;
        public string productID;
        public string productKey;
        public string productSecret;

        [Header("鉴权回调")]
        public bool callbackEnable = false;
        public UnityEvent unityEvent;

        private AndroidJavaClass duiLiteUtilsClass;

        private void Awake()
        {
            if (autoAuth)
            {
                //自动初始化
                Init();
            }
        }

        /// <summary>
        /// 思必驰SDK初始化
        /// </summary>
        public void Init()
        {
            //仅支持安卓
            if (Application.platform != RuntimePlatform.Android) return;

            duiLiteUtilsClass = new AndroidJavaClass("com.eqgis.speechPlugin.DuiLiteUtils");
            //执行init方法
            duiLiteUtilsClass.CallStatic("initAsync");

            if (callbackEnable)
            {
                //启用鉴权回调，定时获取
                InvokeRepeating("CheckAuth", 0.1f, 0.1f);
            }
        }

        
        /// <summary>
        /// 检查授权
        /// </summary>
        private void CheckAuth()
        {
            if (duiLiteUtilsClass != null)
            {
                bool status = duiLiteUtilsClass.CallStatic<bool>("isAuthorized");
                if (status)
                {
#if DEBUG
                    EqLog.i("SbcPlugin", "CheckAuth return true.");
#endif
                    unityEvent.Invoke();
                    CancelInvoke("CheckAuth");
                }
            }
        }
    }
}