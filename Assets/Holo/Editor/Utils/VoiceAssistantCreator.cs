using Holo.Speech;
using UnityEngine;

namespace Holo.XR.Editor.Utils
{
    /// <summary>
    /// �������ִ�����
    /// </summary>
    class VoiceAssistantCreator : BaseCreator
    {
        /// <summary>
        /// ������������
        /// </summary>
        public static void CreateVoiceAssistant()
        {
            GameObject assistant = new GameObject("AI-Assistant");
            assistant.AddComponent<VoiceAssistant>();

            GameObject settings = new GameObject("Settings");
            settings.transform.parent = assistant.transform;
            settings.AddComponent<SbcPlugin>();

            GameObject engine = new GameObject("Engine");
            engine.transform.parent = assistant.transform;

            engine.AddComponent<SbcWakeupEngine>();
            engine.AddComponent<SbcTtsEngine>();
            engine.AddComponent<SbcAsrEngine>();

            SbcPlugin sbcPlugin = settings.GetComponent<SbcPlugin>();
            VoiceAssistant voiceAssistant = assistant.GetComponent<VoiceAssistant>();
            voiceAssistant.asrEngine = engine.GetComponent<SbcAsrEngine>();
            voiceAssistant.ttsEngine = engine.GetComponent<SbcTtsEngine>();
            voiceAssistant.wakeupEngine = engine.GetComponent<SbcWakeupEngine>();
            voiceAssistant.sbcPlugin = sbcPlugin;

            //��Ȩ�ɹ���ִ�����ֳ�ʼ��
            sbcPlugin.success = new UnityEngine.Events.UnityEvent();
            SbcAuth sbcAuth = SbcAuthUtils.Read();
            sbcPlugin.apiKey = sbcAuth.apiKey;
            sbcPlugin.productID = sbcAuth.productID;
            sbcPlugin.productKey = sbcAuth.productKey;
            sbcPlugin.productSecret = sbcAuth.productSecret;

            //sbcPlugin.success.AddListener(voiceAssistant.InitEngine);
            //// ����Inspector���
            //EditorUtility.SetDirty(sbcPlugin);

        }
    }
}