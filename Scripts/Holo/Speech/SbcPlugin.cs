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
        public UnityEvent<string> error;

        private AndroidJavaObject sbcAuthTool;
        private SbcAuthCallback sbcAuthCallback;

        private string cacheFolderPath;

        private void Awake()
        {
            //仅支持安卓
            if (Application.platform != RuntimePlatform.Android) return;

            sbcAuthTool = new AndroidJavaObject("com.eqgis.speech.sbc.SbcAuthTool", this.name);
            //回调事件
            sbcAuthCallback = new SbcAuthCallback(success,error);
        }


        /// <summary>
        /// 引擎初始化
        /// </summary>
        public void InitAsync()
        {
            if (sbcAuthTool == null) return;
            sbcAuthTool.Call("initAsyncFromUnity",sbcAuthCallback, apiKey,productID,productKey,productSecret);
        }

        /// <summary>
        /// 获取语音缓存文件夹目录
        /// </summary>
        /// <returns></returns>
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
        public void RequestPermiision()
        {
            if (sbcAuthTool == null) return;
            sbcAuthTool.Call("requestPermission");
        }

        /// <summary>
        /// 判断鉴权状态
        /// </summary>
        /// <returns>状态值</returns>
        public bool isAuthorized()
        {
            if (sbcAuthCallback == null) return false;
            return sbcAuthCallback.isAuthorized();
        }
    }

    class SbcAuthCallback : UnitySpeechCallback
    {
        public UnityEvent success;
        public UnityEvent<string> error;
        private bool authorized = false;

        /// <summary>
        /// 判断鉴权状态
        /// </summary>
        /// <returns>状态值</returns>
        public bool isAuthorized()
        {
            return authorized;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="success">成功时回调事件</param>
        /// <param name="error">失败时回调事件</param>
        public SbcAuthCallback(UnityEvent success, UnityEvent<string> error)
        {
            this.success = success;
            this.error = error;
        }

        /// <summary>
        /// 鉴权成功时回调
        /// </summary>
        public override void OnInitSuccess()
        {
            authorized = true;
            success.Invoke();
        }

        /// <summary>
        /// 鉴权失败时回调
        /// </summary>
        /// <param name="error">错误信息</param>
        public override void OnError(string errorMsg)
        {
            authorized = false;
            error.Invoke(errorMsg);
        }

        //以下方法暂不实现
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

        public override void OnResults(string var1)
        {
            throw new System.NotImplementedException();
        }

        public override void OnRmsChanged(float var1)
        {
            throw new System.NotImplementedException();
        }

        public override void OnSpeechFinish(string utteranceId)
        {
            throw new System.NotImplementedException();
        }

        public override void OnSpeechStart(string utteranceId)
        {
            throw new System.NotImplementedException();
        }

        public override void OnSynthesizeFinish(string utteranceId)
        {
            throw new System.NotImplementedException();
        }

        public override void OnSynthesizeStart(string utteranceId)
        {
            throw new System.NotImplementedException();
        }

        public override void OnWakeup(double confidence, string wakeupWord)
        {
            throw new System.NotImplementedException();
        }
    }
}