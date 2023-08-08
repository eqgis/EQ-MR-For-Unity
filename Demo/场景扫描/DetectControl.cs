using Holo.XR.Detect;
using Holo.XR.Core;
using UnityEngine;
using UnityEngine.UI;

public class DetectControl : MonoBehaviour
{
    [Header("Status"),Tooltip("状态单选框")]
    public Toggle tagDetectToggle;
    public Toggle sceneDetectToggle;


    public TagDetect tagDetect;
    public XvCslamMapScanner xvCslamMapScanner;


    // Start is called before the first frame update
    void Start()
    {

        tagDetectToggle.onValueChanged.AddListener(delegate {
            if (tagDetectToggle.isOn)
            {
                tagDetect.StartDetect();
                ShowToast("启用Tag识别！");
            }
            else
            {
                tagDetect.StopDetect();
                ShowToast("禁用Tag识别！");
            }
        });

        sceneDetectToggle.onValueChanged.AddListener(delegate
        {
            if (sceneDetectToggle.isOn)
            {
                xvCslamMapScanner.StartMap();
            }
            else
            {
                xvCslamMapScanner.StopMap();
            }
        });
    }



    public void ShowToast(string msg)
    {
        //Unity调用安卓的Toast
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaClass Toast = new AndroidJavaClass("android.widget.Toast");
        currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
            Toast.CallStatic<AndroidJavaObject>("makeText", currentActivity, msg, Toast.GetStatic<int>("LENGTH_LONG")).Call("show");
        }));
        /*
         * 匿名方法中第二个参数是安卓上下文对象，除了用currentActivity，还可用安卓中的GetApplicationContext()获得上下文。
        AndroidJavaObject context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");
        */
    }
}
