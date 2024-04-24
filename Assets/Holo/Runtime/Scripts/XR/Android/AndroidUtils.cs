using UnityEngine;

namespace Holo.XR.Android
{
    public class AndroidUtils
    {
        private AndroidJavaClass unityPlayer;
        private AndroidJavaObject currentActivity;
        private AndroidJavaClass toast;
        private AndroidJavaClass m_VibrateHelper;
        private static AndroidUtils instance = null;

        private AndroidUtils()
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                return;
            }
            unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            toast = new AndroidJavaClass("android.widget.Toast");
        }

        public static AndroidUtils GetInstance()
        {
            if (instance == null)
            {
                instance = new AndroidUtils();
            }
            return instance;
        }

        /// <summary>
        /// ��ʾ��׿Toast
        /// </summary>
        /// <param name="msg"></param>
        public static void Toast(string msg)
        {
            GetInstance().ShowToast(msg);
        }

        public void ShowToast(string msg)
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                Debug.Log(msg);
                return;
            }
            //Unity���ð�׿��Toast
            currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                toast.CallStatic<AndroidJavaObject>("makeText", currentActivity, msg, toast.GetStatic<int>("LENGTH_LONG")).Call("show");
            }));
            /*
             * ���������еڶ��������ǰ�׿�����Ķ��󣬳�����currentActivity�������ð�׿�е�GetApplicationContext()��������ġ�
            AndroidJavaObject context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");
            */
        }

        /// <summary>
        /// ʹ�豸��
        /// </summary>
        /// <param name="pattern">����ʽ</param>
        /// <param name="repeate">�𶯵Ĵ�����-1���ظ�����-1Ϊ��pattern��ָ���±꿪ʼ�ظ�</param>
        public static void vibrate(long[] pattern, int repeate)
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                return;
            }

            AndroidUtils androidUtils = GetInstance();
            if (androidUtils.m_VibrateHelper == null)
            {
                androidUtils.m_VibrateHelper = new AndroidJavaClass("com.eqgis.unity.utils.VibrateHelper");
            }
            androidUtils.m_VibrateHelper.CallStatic("vComplicated", 
                androidUtils.currentActivity, pattern, repeate);
        }

        /// <summary>
        /// ʹ�豸��
        /// </summary>
        /// <param name="millisecond">��ʱ��,��λ����</param>
        public static void vibrate(int millisecond)
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                return;
            }

            AndroidUtils androidUtils = GetInstance();
            if (androidUtils.m_VibrateHelper == null)
            {
                androidUtils.m_VibrateHelper = new AndroidJavaClass("com.eqgis.unity.utils.VibrateHelper");
            }
            androidUtils.m_VibrateHelper.CallStatic("vSimple",
                androidUtils.currentActivity, millisecond);
        }

        /// <summary>
        /// ����Ӧ��
        /// </summary>
        public static void RestartApplication()
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                return;
            }

            AndroidJavaClass androidToolKitClass = new AndroidJavaClass("com.eqgis.unity.AndroidToolkit");
            androidToolKitClass.CallStatic("restartApplication", GetInstance().currentActivity);
        }

        public static void Quit()
        {
            //killProcess
            if (Application.platform != RuntimePlatform.Android)
            {
                return;
            }

            AndroidJavaClass androidToolKitClass = new AndroidJavaClass("com.eqgis.unity.AndroidToolkit");
            androidToolKitClass.CallStatic("killProcess", GetInstance().currentActivity);
        }

        internal void Destroy()
        {
            toast.Dispose();
            currentActivity.Dispose();
            unityPlayer.Dispose();
        }
    }
}