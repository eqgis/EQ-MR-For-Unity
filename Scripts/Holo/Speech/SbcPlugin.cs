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
        public bool callbackEnable = false;
        public UnityEvent unityEvent;

        private AndroidJavaClass duiLiteUtilsClass;

        private void Awake()
        {
            if (autoAuth)
            {
                //�Զ���ʼ��
                Init();
            }
        }

        /// <summary>
        /// ˼�س�SDK��ʼ��
        /// </summary>
        public void Init()
        {
            //��֧�ְ�׿
            if (Application.platform != RuntimePlatform.Android) return;

            duiLiteUtilsClass = new AndroidJavaClass("com.eqgis.speechPlugin.DuiLiteUtils");
            //ִ��init����
            duiLiteUtilsClass.CallStatic("initAsync");

            if (callbackEnable)
            {
                //���ü�Ȩ�ص�����ʱ��ȡ
                InvokeRepeating("CheckAuth", 0.1f, 0.1f);
            }
        }

        
        /// <summary>
        /// �����Ȩ
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