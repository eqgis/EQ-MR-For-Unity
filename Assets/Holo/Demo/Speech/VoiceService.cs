using Holo.Speech;
using Holo.XR.Android;
using UnityEngine;

public class VoiceService : MonoBehaviour
{
    private void Awake()
    {
        //启动语音服务
        VoiceAssistantService.Instance.Bind();
        Holo.XR.Android.AndroidUtils.Toast("Bind");
        EqLog.i("Ikkyu","Bind:  ");
    }

    void Start()
    {
        //设置监听回调
        VoiceAssistantService.Instance
            .SetAsrResultsCallback(/*设置识别结果监听*/UpdateText)
            .SetAsrReadyCallback(OnAsrReady)
            .SetAsrEndCallback(OnAsrEnd)
            .SetWakeupCallback(WhenWakeup)
            .SetTtsSpeechStartCallback(WhenTtsStart)
            .SetTtsSpeechFinishCallback(WhenTtsEnd);


        VoiceAssistantService.Instance.StartService();
        Holo.XR.Android.AndroidUtils.Toast("Start");
        EqLog.i("Ikkyu","Start:  ");
    }

    void UpdateText(string text)
    {
        //安卓Toast，显示说话内容
        Holo.XR.Android.AndroidUtils.Toast(text);
        EqLog.i("Ikkyu","UpdateText:  " + text);
    }

    void OnAsrReady()
    {
        EqLog.i("Ikkyu","OnAsrReady");
    }
    void OnAsrEnd()
    {
        EqLog.i("Ikkyu","OnAsrEnd");
    }

    void WhenWakeup(double confiendce,string wakeupWord)
    {
        EqLog.i("Ikkyu","word：" + wakeupWord + "_" + confiendce);
    }

    void WhenTtsStart()
    {
        EqLog.i("Ikkyu","WhenTtsStart");
    }

    void WhenTtsEnd()
    {
        EqLog.i("Ikkyu","WhenTtsEnd");
    }

    private void OnDestroy()
    {
        VoiceAssistantService.Instance.Unbind();
    }
}
