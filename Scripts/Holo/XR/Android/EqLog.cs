using System.IO;
using UnityEngine;

namespace Holo.XR.Android
{
    public class EqLog
    {
        private static AndroidJavaClass logClass = new AndroidJavaClass("android.util.Log");

#if DEBUG_LOG
        private static string logFilePath = Application.persistentDataPath + "/runtime.log";
#endif
        public static void e(string tag,string msg)
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                Debug.LogError(tag + " (e): " + msg);
            }
            else
            {
                logClass.CallStatic<int>("e", tag, msg);
            }

#if DEBUG_LOG
            WriteLog(tag+ ":" + msg);
#endif
        }

        public static void i(string tag, string msg)
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                Debug.Log(tag + " (i): " + msg);
            }
            else
            {
                logClass.CallStatic<int>("i", tag, msg);
            }
#if DEBUG_LOG
            WriteLog(tag + ":" + msg);
#endif
        }

        public static void d(string tag, string msg)
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                Debug.Log(tag + " (d): " + msg);
            }
            else
            {
                logClass.CallStatic<int>("d", tag, msg);
            }
#if DEBUG_LOG
            WriteLog(tag + ":" + msg);
#endif
        }

        public static void w(string tag, string msg)
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                Debug.LogWarning(tag + " (w): " + msg);
            }
            else
            {
                logClass.CallStatic<int>("w", tag, msg);
            }
#if DEBUG_LOG
            WriteLog(tag + ":" + msg);
#endif
        }

#if DEBUG_LOG
        // 写入日志到文件
        private static void WriteLog(string message)
        {
            using (StreamWriter writer = File.AppendText(logFilePath))
            {
                writer.WriteLine($"{System.DateTime.Now}: {message}");
            }
        }
#endif
    }

}