using UnityEngine;

namespace Holo.XR.Android
{

    public class AndroidUtils
    {
        private AndroidJavaClass unityPlayer;
        private AndroidJavaObject currentActivity;
        private AndroidJavaClass toast;
        private static AndroidUtils instance = null;
        public static bool debug = true;

        private AndroidUtils()
        {
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

        public static void Toast(string msg)
        {
            GetInstance().ShowToast(msg);
        }

        public void ShowToast(string msg)
        {
            //Unity调用安卓的Toast
            currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                toast.CallStatic<AndroidJavaObject>("makeText", currentActivity, msg, toast.GetStatic<int>("LENGTH_LONG")).Call("show");
            }));
            /*
             * 匿名方法中第二个参数是安卓上下文对象，除了用currentActivity，还可用安卓中的GetApplicationContext()获得上下文。
            AndroidJavaObject context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");
            */
        }

        internal void Destroy()
        {
            toast.Dispose();
            currentActivity.Dispose();
            unityPlayer.Dispose();
        }
    }
}