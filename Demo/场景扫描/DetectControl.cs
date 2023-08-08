using Holo.XR.Detect;
using Holo.XR.Core;
using UnityEngine;
using UnityEngine.UI;

public class DetectControl : MonoBehaviour
{
    [Header("Status"),Tooltip("״̬��ѡ��")]
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
                ShowToast("����Tagʶ��");
            }
            else
            {
                tagDetect.StopDetect();
                ShowToast("����Tagʶ��");
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
        //Unity���ð�׿��Toast
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaClass Toast = new AndroidJavaClass("android.widget.Toast");
        currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
            Toast.CallStatic<AndroidJavaObject>("makeText", currentActivity, msg, Toast.GetStatic<int>("LENGTH_LONG")).Call("show");
        }));
        /*
         * ���������еڶ��������ǰ�׿�����Ķ��󣬳�����currentActivity�������ð�׿�е�GetApplicationContext()��������ġ�
        AndroidJavaObject context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");
        */
    }
}
