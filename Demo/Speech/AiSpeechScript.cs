using Holo.Speech;
using UnityEngine;
using UnityEngine.UI;

public class AiSpeechScript : MonoBehaviour
{

    public SbcTtsEngine ttsEngine;
    public SbcAsrEngine asrEngine;
    public SbcWakeupEngine wakeupEngine;

    public InputField textInput;

    public Text textC;

    public VoiceAssistant voiceAssistant;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("日志输出：");

    }

    /// <summary>
    /// 更新文本
    /// </summary>
    public void UpdateText()
    {
        textC.text = voiceAssistant.content;
    }

    public void AuthSuccess()
    {
        Holo.XR.Android.AndroidUtils.Toast("鉴权成功！！！");
    }

    public void AuthError(string error)
    {
        Debug.Log(error);
        Holo.XR.Android.AndroidUtils.Toast(error);
    }

    public void TtsMethod()
    {
        ttsEngine.textContent = textInput.text;
        ttsEngine.StartEngine();
    }

    public void InitTtsEngine()
    {
        //ttsEngine.InitEngine(new SpeechCallback("TTS"));
        //asrEngine.InitEngine(new SpeechCallback("ASR"));
        //wakeupEngine.InitEngine(new SpeechCallback("Wakeup"));

        voiceAssistant.InitEngine();
    }

    public void AsrMethod()
    {
        asrEngine.StartEngine();
    }

    public void WakeupMethod()
    {
        wakeupEngine.StartEngine();
    }

    private class SpeechCallback : UnitySpeechCallback
    {
        public string type;

        public SpeechCallback(string type)
        {
            this.type = type;
        }

        public override void OnBeginningOfSpeech()
        {
            Holo.XR.Android.AndroidUtils.Toast(type + " OnBeginningOfSpeech:");
        }

        public override void OnEndOfSpeech()
        {
            Holo.XR.Android.AndroidUtils.Toast(type + " OnEndOfSpeech:");
        }

        public override void OnError(string error)
        {
            Holo.XR.Android.AndroidUtils.Toast(type + " OnError: " + error);
        }

        public override void OnInitSuccess()
        {
            Holo.XR.Android.AndroidUtils.Toast(type + " OnInitSuccess");
        }

        public override void OnReadyForSpeech()
        {
            Holo.XR.Android.AndroidUtils.Toast(type + " OnReadyForSpeech:");
        }

        public override void OnResults(string var1)
        {
            Holo.XR.Android.AndroidUtils.Toast(type + " OnResults:" + var1);
        }

        public override void OnRmsChanged(float var1)
        {
        }

        public override void OnSpeechFinish(string utteranceId)
        {
            Holo.XR.Android.AndroidUtils.Toast(type + " OnSpeechFinish:TTS");
        }

        public override void OnSpeechStart(string utteranceId)
        {
            Holo.XR.Android.AndroidUtils.Toast(type + " OnSpeechStart:TTS");
        }

        public override void OnSynthesizeFinish(string utteranceId)
        {
        }

        public override void OnSynthesizeStart(string utteranceId)
        {
        }

        public override void OnWakeup(double confidence, string wakeupWord)
        {
            Holo.XR.Android.AndroidUtils.Toast(type + " wakeupWord" + wakeupWord + "  confidence:" + confidence);
        }
    }
}
