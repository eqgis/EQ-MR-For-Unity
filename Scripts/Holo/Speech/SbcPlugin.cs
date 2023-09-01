using Holo.XR.Android;
using UnityEngine;
using UnityEngine.Events;

namespace Holo.Speech
{
    public class SbcPlugin : MonoBehaviour
    {
        [Header("�Զ���Ȩ")]
        public bool autoAuth = true;

        [Header("˼�س���Ȩ����")]
        public string apiKey;
        public string productID;
        public string productKey;
        public string productSecret;

        [Header("��Ȩ�ص�")]
        public UnityEvent success;
        public UnityEvent error;

        private AndroidJavaObject sbcAuthTool;
        private SbcAuthCallback sbcAuthCallback;

        private string cacheFolderPath;

        private void Awake()
        {
            //��֧�ְ�׿
            if (Application.platform != RuntimePlatform.Android) return;

            sbcAuthTool = new AndroidJavaObject("com.eqgis.speech.sbc.SbcAuthTool", this.name);
            //�ص��¼�
            sbcAuthCallback = new SbcAuthCallback();
            sbcAuthCallback.success = success;
            sbcAuthCallback.error = error;
            
            //�Զ���Ȩ�Ļ���ִ�����ȼ�Ҫ���ߡ�
            if (autoAuth)
            {
                RequestPermission();
                InvokeRepeating("CheckAuth", 0.1F, 2F);
            }
        }

        /// <summary>
        /// �����Ȩ
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
        /// �����ʼ��
        /// </summary>
        public void InitAsync()
        {
            if (sbcAuthTool == null) return;
            //sbcAuthTool.Call("initAsync", apiKey,productID,productKey,productSecret);
            sbcAuthTool.Call("initAsyncFromUnity", sbcAuthCallback, apiKey, productID, productKey, productSecret);
        }

        /// <summary>
        /// ��ȡ���������ļ���Ŀ¼
        /// </summary>
        /// <returns>���������ļ���·��</returns>
        public string GetCacheFolderPath() {
            if (cacheFolderPath == null && sbcAuthTool != null)
            {
                cacheFolderPath = sbcAuthTool.Call<string>("getCacheFolderPath");
            }
            return cacheFolderPath;
        }

        /// <summary>
        /// ����Ȩ��
        /// </summary>
        public void RequestPermission()
        {
            if (sbcAuthTool == null) return;
            sbcAuthTool.Call("requestPermission");
        }

        /// <summary>
        /// �жϼ�Ȩ״̬
        /// </summary>
        /// <returns>״ֵ̬</returns>
        public bool IsAuthorized()
        {
            if (sbcAuthCallback == null) return false;
            return sbcAuthCallback.IsAuthorized();
        }

        /// <summary>
        /// ��Ȩ�¼��ص�
        /// </summary>
        private class SbcAuthCallback : UnitySpeechCallback
        {
            public UnityEvent success { get; set; }
            public UnityEvent error { get; set; }
            private bool authorized = false;

            /// <summary>
            /// �жϼ�Ȩ״̬
            /// </summary>
            /// <returns>״ֵ̬</returns>
            public bool IsAuthorized()
            {
                return authorized;
            }

            /// <summary>
            /// SDK��ʼ���ɹ�ʱ�ص�
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
            /// ��ʼ��ʧ��ʱ�ص�
            /// </summary>
            /// <param name="error">������Ϣ</param>
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