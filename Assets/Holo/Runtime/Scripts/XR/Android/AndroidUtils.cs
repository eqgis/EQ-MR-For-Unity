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
        /// 显示安卓Toast
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
            //Unity调用安卓的Toast
            currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                toast.CallStatic<AndroidJavaObject>("makeText", currentActivity, msg, toast.GetStatic<int>("LENGTH_LONG")).Call("show");
            }));
            /*
             * 匿名方法中第二个参数是安卓上下文对象，除了用currentActivity，还可用安卓中的GetApplicationContext()获得上下文。
            AndroidJavaObject context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");
            */
        }

        /// <summary>
        /// 使设备震动
        /// </summary>
        /// <param name="pattern">震动形式</param>
        /// <param name="repeate">震动的次数，-1不重复，非-1为从pattern的指定下标开始重复</param>
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
        /// 使设备震动
        /// </summary>
        /// <param name="millisecond">震动时长,单位毫秒</param>
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
        /// 重启应用
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