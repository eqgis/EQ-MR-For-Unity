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
            //Unity���ð�׿��Toast
            currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                toast.CallStatic<AndroidJavaObject>("makeText", currentActivity, msg, toast.GetStatic<int>("LENGTH_LONG")).Call("show");
            }));
            /*
             * ���������еڶ��������ǰ�׿�����Ķ��󣬳�����currentActivity�������ð�׿�е�GetApplicationContext()��������ġ�
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