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
        public UnityEvent<string> error;

        private AndroidJavaObject sbcAuthTool;
        private SbcAuthCallback sbcAuthCallback;

        private string cacheFolderPath;

        private void Awake()
        {
            //��֧�ְ�׿
            if (Application.platform != RuntimePlatform.Android) return;

            sbcAuthTool = new AndroidJavaObject("com.eqgis.speech.sbc.SbcAuthTool", this.name);
            //�ص��¼�
            sbcAuthCallback = new SbcAuthCallback(success,error);
        }


        /// <summary>
        /// �����ʼ��
        /// </summary>
        public void InitAsync()
        {
            if (sbcAuthTool == null) return;
            sbcAuthTool.Call("initAsyncFromUnity",sbcAuthCallback, apiKey,productID,productKey,productSecret);
        }

        /// <summary>
        /// ��ȡ���������ļ���Ŀ¼
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
        /// ����Ȩ��
        /// </summary>
        public void RequestPermiision()
        {
            if (sbcAuthTool == null) return;
            sbcAuthTool.Call("requestPermission");
        }

        /// <summary>
        /// �жϼ�Ȩ״̬
        /// </summary>
        /// <returns>״ֵ̬</returns>
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
        /// �жϼ�Ȩ״̬
        /// </summary>
        /// <returns>״ֵ̬</returns>
        public bool isAuthorized()
        {
            return authorized;
        }

        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="success">�ɹ�ʱ�ص��¼�</param>
        /// <param name="error">ʧ��ʱ�ص��¼�</param>
        public SbcAuthCallback(UnityEvent success, UnityEvent<string> error)
        {
            this.success = success;
            this.error = error;
        }

        /// <summary>
        /// ��Ȩ�ɹ�ʱ�ص�
        /// </summary>
        public override void OnInitSuccess()
        {
            authorized = true;
            success.Invoke();
        }

        /// <summary>
        /// ��Ȩʧ��ʱ�ص�
        /// </summary>
        /// <param name="error">������Ϣ</param>
        public override void OnError(string errorMsg)
        {
            authorized = false;
            error.Invoke(errorMsg);
        }

        //���·����ݲ�ʵ��
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