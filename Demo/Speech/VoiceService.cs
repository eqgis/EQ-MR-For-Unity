using Holo.Speech;
using Holo.XR.Android;
using UnityEngine;

public class VoiceService : MonoBehaviour
{
    private void Awake()
    {
        //������������
        VoiceAssistantService.Instance.Bind();
        Holo.XR.Android.AndroidUtils.Toast("Bind");
        EqLog.i("Ikkyu","Bind:  ");
    }

    void Start()
    {
        //���ü����ص�
        VoiceAssistantService.Instance
            .SetAsrResultsCallback(/*����ʶ��������*/UpdateText)
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
        //��׿Toast����ʾ˵������
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
        EqLog.i("Ikkyu","word��" + wakeupWord + "_" + confiendce);
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
