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
        public UnityEvent success;
        public UnityEvent error;

        private AndroidJavaObject sbcAuthTool;
        private SbcAuthCallback sbcAuthCallback;

        private string cacheFolderPath;

        private void Awake()
        {
            //仅支持安卓
            if (Application.platform != RuntimePlatform.Android) return;

            sbcAuthTool = new AndroidJavaObject("com.eqgis.speech.sbc.SbcAuthTool", this.name);
            //回调事件
            sbcAuthCallback = new SbcAuthCallback();
            sbcAuthCallback.success = success;
            sbcAuthCallback.error = error;
            
            //自动鉴权的话，执行优先级要调高。
            if (autoAuth)
            {
                RequestPermission();
                InvokeRepeating("CheckAuth", 0.1F, 2F);
            }
        }

        /// <summary>
        /// 检查授权
        /// </summary>
        private void CheckAuth()
        {
            if (IsAuthorized())
            {
                CancelInvoke("CheckAuth");
            }
            else
            {
                InitAsync();
            }
        }

        private void OnDestroy()
        {
            sbcAuthCallback.javaInterface.Dispose();
            sbcAuthTool.Dispose();
            sbcAuthCallback = null;
            sbcAuthTool = null;
        }


        /// <summary>
        /// 引擎初始化
        /// </summary>
        public void InitAsync()
        {
            if (sbcAuthTool == null) return;
            //sbcAuthTool.Call("initAsync", apiKey,productID,productKey,productSecret);
            sbcAuthTool.Call("initAsyncFromUnity", sbcAuthCallback, apiKey, productID, productKey, productSecret);
        }

        /// <summary>
        /// 获取语音缓存文件夹目录
        /// </summary>
        /// <returns>语音缓存文件夹路径</returns>
        public string GetCacheFolderPath() {
            if (cacheFolderPath == null && sbcAuthTool != null)
            {
                cacheFolderPath = sbcAuthTool.Call<string>("getCacheFolderPath");
            }
            return cacheFolderPath;
        }

        /// <summary>
        /// 请求权限
        /// </summary>
        public void RequestPermission()
        {
            if (sbcAuthTool == null) return;
            sbcAuthTool.Call("requestPermission");
        }

        /// <summary>
        /// 判断鉴权状态
        /// </summary>
        /// <returns>状态值</returns>
        public bool IsAuthorized()
        {
            if (sbcAuthCallback == null) return false;
            return sbcAuthCallback.IsAuthorized();
        }

        /// <summary>
        /// 授权事件回调
        /// </summary>
        private class SbcAuthCallback : UnitySpeechCallback
        {
            public UnityEvent success { get; set; }
            public UnityEvent error { get; set; }
            private bool authorized = false;

            /// <summary>
            /// 判断鉴权状态
            /// </summary>
            /// <returns>状态值</returns>
            public bool IsAuthorized()
            {
                return authorized;
            }

            /// <summary>
            /// SDK初始化成功时回调
            /// </summary>
            public override void OnInitSuccess()
            {
#if DEBUG
                EqLog.d("SbcPlugin", "OnInitSuccess");
#endif
                authorized = true;
                success.Invoke();
            }

            /// <summary>
            /// 初始化失败时回调
            /// </summary>
            /// <param name="error">错误信息</param>
            public override void OnError(string errorMsg)
            {
#if DEBUG
                EqLog.e("SbcPlugin", "OnError\n" + errorMsg);
#endif
                authorized = false;
                error.Invoke();
            }

            public override void OnBeginningOfSpeech()
            {
                throw new System.NotImplementedException();
            }

            public override void OnEndOfSpeech()
            {
                throw new System.NotImplementedException();
            }

            public override void OnReadyForSpeech()
            {
                throw new System.NotImplementedException();
            }

            public override void OnRmsChanged(float var1)
            {
                throw new System.NotImplementedException();
            }

            public override void OnResults(string var1)
            {
                throw new System.NotImplementedException();
            }

            public override void OnWakeup(double confidence, string wakeupWord)
            {
                throw new System.NotImplementedException();
            }

            public override void OnSynthesizeStart(string utteranceId)
            {
                throw new System.NotImplementedException();
            }

            public override void OnSynthesizeFinish(string utteranceId)
            {
                throw new System.NotImplementedException();
            }

            public override void OnSpeechStart(string utteranceId)
            {
                throw new System.NotImplementedException();
            }

            public override void OnSpeechFinish(string utteranceId)
            {
                throw new System.NotImplementedException();
            }
        }
    }

}