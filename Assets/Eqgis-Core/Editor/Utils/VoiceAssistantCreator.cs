using Holo.Speech;
using UnityEngine;

namespace Holo.XR.Editor.Utils
{
    /// <summary>
    /// 语音助手创建器
    /// </summary>
    class VoiceAssistantCreator : BaseCreator
    {
        /// <summary>
        /// 创建语音助手
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

            //鉴权成功后，执行助手初始化
            sbcPlugin.success = new UnityEngine.Events.UnityEvent();
            SbcAuth sbcAuth = SbcAuthUtils.Read();
            sbcPlugin.apiKey = sbcAuth.apiKey;
            sbcPlugin.productID = sbcAuth.productID;
            sbcPlugin.productKey = sbcAuth.productKey;
            sbcPlugin.productSecret = sbcAuth.productSecret;

            //sbcPlugin.success.AddListener(voiceAssistant.InitEngine);
            //// 更新Inspector面板
            //EditorUtility.SetDirty(sbcPlugin);

        }
    }
}