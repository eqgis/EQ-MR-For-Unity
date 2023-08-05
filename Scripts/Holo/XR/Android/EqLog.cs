using UnityEngine;

namespace Holo.XR.Android
{
    public class EqLog
    {
        private static AndroidJavaClass logClass = new AndroidJavaClass("android.util.Log");
        public static void e(string tag,string msg)
        {
            //Debug.LogError(tag + " (e): " + msg);
            logClass.CallStatic<int>("e", tag, msg);
        }

        public static void i(string tag, string msg)
        {
            //Debug.Log(tag + " (i): " + msg);
            logClass.CallStatic<int>("i", tag, msg);
        }

        public static void d(string tag, string msg)
        {
            //Debug.Log(tag + " (d): " + msg);
            logClass.CallStatic<int>("d", tag, msg);
        }

        public static void w(string tag, string msg)
        {
            //Debug.LogWarning(tag + " (w): " + msg);
            logClass.CallStatic<int>("w", tag, msg);
        }
    }

}