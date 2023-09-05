using Holo.XR.Android;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.EventSystems.EventTrigger;

namespace Holo.Speech
{
    /// <summary>
    /// 语音助手状态
    /// </summary>
    enum VoiceAssistantStatus
    {
        /// <summary>
        /// 就绪（等待唤醒）
        /// </summary>
        READY,

        /// <summary>
        /// 说话中（TTS启动后状态）
        /// </summary>
        SPEEKING,

        /// <summary>
        /// 倾听中（ASR启动后状态）
        /// </summary>
        LISTENNING
    }

    /// <summary>
    /// 助手
    /// </summary>
    class AssistantEntity
    {
        public UnityEvent initSuccess { get; set; }
        public string entityName { get; set; }
        public VoiceAssistantStatus status { get; set; }
        public SbcWakeupEngine wakeupEngine { get; set; }
        public SbcTtsEngine ttsEngine { get; set; }
        public SbcAsrEngine asrEngine { get; set; }

        /// <summary>
        /// 单轮交流说话次数。
        /// </summary>
        public int talkCount { get; set; }

        public string[] wakeupResponses = new string[] {
            "!我在","!请指示","!请吩咐","有什么可以帮助您的吗？"
        };

        public string[] positiveResponses = new string[] {
            "好的",
            "没错",
            "当然",
            "可以的",
            "明白了"
        };

        public string[] sayByeResponses = new string[] {   
            "如果你还有其他问题，随时问我",
            "如果需要帮助，请随时联系我",};

        public string[] greetingResponses = new string[] {"你好",
            "你好，有什么我可以帮助你的吗？"
        };

        public string[] helpResponses = new string[] {
            // 提供帮助
            "我可以帮你查找信息",
            "我可以回答你的问题",
            "有什么我可以帮助你的吗？",
        };

        /// <summary>
        /// 获取随机回答
        /// </summary>
        /// <param name="myArray"></param>
        /// <returns></returns>
        public string RandomResponse(string[] myArray)
        {
            // 创建一个随机数生成器
            System.Random random = new System.Random();

            // 生成一个随机的索引
            int randomIndex = random.Next(0, myArray.Length);

            // 获取随机的字符串
            return myArray[randomIndex];
        }
    }

    /// <summary>
    /// 语音助手（组件）
    /// </summary>
    public class VoiceAssistant : MonoBehaviour
    {
        [Header("思必驰SDK配置")]
        public SbcPlugin sbcPlugin;

        [Header("语音引擎")]
        public SbcWakeupEngine wakeupEngine;
        public SbcTtsEngine ttsEngine;
        public SbcAsrEngine asrEngine;

        private AssistantEntity assistant;
        private bool init  = false;

        [Header("识别")]
        public UnityEvent recognizeEvent;

        [Header("初始化成功")]
        public UnityEvent initSuccess;

        /// <summary>
        /// 语音识别的内容
        /// </summary>
        public string content { get; private set; }

        /// <summary>
        /// 语音识别回调结果
        /// </summary>
        /// <param name="str"></param>
        protected virtual void OnAsrResponse(string str)
        {
            this.content = str;
            if (recognizeEvent != null)
            {
                recognizeEvent.Invoke();
            }
        }

        /// <summary>
        /// 语音引擎初始化
        /// </summary>
        public void InitEngine()
        {
            if (init) { return; }

            //实体对象便于后续处理
            assistant = new AssistantEntity();
            assistant.entityName = this.name;
            assistant.ttsEngine = this.ttsEngine;
            assistant.asrEngine = this.asrEngine;
            assistant.wakeupEngine = this.wakeupEngine;
            assistant.talkCount = 0;
            assistant.initSuccess = this.initSuccess;

            //创建对应回调事件后执行初始化
            asrEngine.InitEngine(new AsrCallback(OnAsrResponse, assistant));

            init = true;

            //wakeupEngine.InitEngine();
        }

        /// <summary>
        /// 销毁引擎
        /// </summary>
        public void DestroyEngine()
        {
            //销毁引擎
            wakeupEngine.DestroyEngine();
            ttsEngine.DestroyEngine();
            asrEngine.DestroyEngine();

            //更改状态
            init = false;
        }

        private void OnDestroy()
        {
            DestroyEngine();
        }

        /// <summary>
        /// 唤醒回调
        /// </summary>
        private class WakeupCallback : UnitySpeechCallback
        {
            /// <summary>
            /// 语音助手实体信息
            /// </summary>
            private AssistantEntity entity;
            public WakeupCallback(AssistantEntity entity)
            {
                this.entity = entity;
            }

            public override void OnError(string error)
            {
#if DEBUG
                EqLog.e("VoiceAssistant-WakeupCallback", error);
#endif
            }

            public override void OnInitSuccess()
            {
#if DEBUG
                EqLog.d("VoiceAssistant-WakeupCallback", "OnInitSuccess");
#endif
                //初始化成功后，自动启动唤醒引擎
                entity.status = VoiceAssistantStatus.READY;
                entity.wakeupEngine.StartEngine();
                
                //初始化成功后执行Unity事件
                entity.initSuccess.Invoke();
            }

            public override void OnWakeup(double confidence, string wakeupWord)
            {
                if (entity.status == VoiceAssistantStatus.READY)
                {
                    //单轮交流说话次数重置
                    entity.talkCount = 0;
                    //启动TTS引擎,应答。
                    entity.ttsEngine.textContent = entity.RandomResponse(entity.wakeupResponses);
                    entity.status = VoiceAssistantStatus.SPEEKING;
                    entity.ttsEngine.StartEngine();
                }
            }
        }

        /// <summary>
        /// 语音合成回调
        /// </summary>
        private class TtsCallback : UnitySpeechCallback
        {

            /// <summary>
            /// 语音助手实体信息
            /// </summary>
            private AssistantEntity entity;
            public TtsCallback(AssistantEntity entity)
            {
                this.entity = entity;
            }

            public override void OnError(string error)
            {
#if DEBUG
                EqLog.e("VoiceAssistant-TtsCallback", error);
#endif
            }

            public override void OnInitSuccess()
            {
                entity.wakeupEngine.InitEngine(new WakeupCallback(entity));
#if DEBUG
                EqLog.d("VoiceAssistant-TtsCallback", "OnInitSuccess");
#endif
            }

            /// <summary>
            /// 开始播放语音
            /// </summary>
            /// <param name="utteranceId"></param>
            public override void OnSpeechStart(string utteranceId)
            {
                entity.status = VoiceAssistantStatus.SPEEKING;
#if DEBUG
                EqLog.d("VoiceAssistant-TtsCallback", "OnSpeechStart");
#endif
            }

            /// <summary>
            /// 语音播放结束
            /// </summary>
            /// <param name="utteranceId"></param>
            public override void OnSpeechFinish(string utteranceId)
            {
#if DEBUG
                EqLog.d("VoiceAssistant-TtsCallback", "OnSpeechFinish");
#endif
                switch(entity.talkCount)
                {
                    case 0:
                        //说话结束，设置状态为ready
                        //第一次说话是应答语，则启动ASR
                        entity.status = VoiceAssistantStatus.LISTENNING;
                        entity.asrEngine.StartEngine();
                        break;
                    default:
                        //说话结束，设置状态为ready
                        entity.status = VoiceAssistantStatus.READY;
                        break;
                }

                //单轮交流说话次数+1
                entity.talkCount++;
            }
        }

        /// <summary>
        /// 语音识别回调
        /// </summary>
        private class AsrCallback : UnitySpeechCallback
        {
            private string content;

            /// <summary>
            /// 语音助手实体信息
            /// </summary>
            private AssistantEntity entity;

            private Recognize recognize;

            public AsrCallback(Recognize recognize, AssistantEntity entity)
            {
                this.entity = entity;
                this.recognize = recognize;
            }

            /// <summary>
            /// 识别委托
            /// </summary>
            /// <param name="content">识别结果</param>
            public delegate void Recognize(string content);

            public override void OnError(string error)
            {
#if DEBUG
                EqLog.e("VoiceAssistant-AsrCallback", error);
#endif
            }

            public override void OnInitSuccess()
            {
                entity.ttsEngine.InitEngine(new TtsCallback(entity));
#if DEBUG
                EqLog.d("VoiceAssistant-AsrCallback", "OnInitSuccess");
#endif
            }

            /// <summary>
            /// 超时响应
            /// </summary>
            public override void OnAsrTimeout()
            {
                if (entity.status == VoiceAssistantStatus.LISTENNING)
                {
                    //超时了，关闭ASR
                    entity.asrEngine.StopEngine();
                    //启动TTS引擎
                    entity.ttsEngine.textContent = entity.RandomResponse(entity.sayByeResponses);
                    entity.ttsEngine.StartEngine();
                }

#if DEBUG
                EqLog.d("VoiceAssistant-AsrCallback", "OnAsrTimeout");
#endif
            }

            /// <summary>
            /// ASR就绪后触发
            /// </summary>
            public override void OnReadyForSpeech()
            {
#if DEBUG
                EqLog.d("VoiceAssistant-AsrCallback", "OnReadyForSpeech");
#endif
                //开始倾听
                entity.status = VoiceAssistantStatus.LISTENNING;
            }

            public override void OnRmsChanged(float var1)
            {
#if DEBUG
                EqLog.d("VoiceAssistant-AsrCallback", "OnRmsChanged:" + var1);
#endif
            }

            public override void OnBeginningOfSpeech()
            {
                //监测到有声音后，回调
#if DEBUG
                EqLog.d("VoiceAssistant-AsrCallback", "OnBeginningOfSpeech");
#endif
            }

            /// <summary>
            /// 识别结果回调
            /// </summary>
            /// <param name="content"></param>
            public override void OnResults(string str)
            {
                this.content = str;
                recognize(this.content);
            }

            public override void OnEndOfSpeech()
            {
                //识别结束
#if DEBUG
                EqLog.d("VoiceAssistant-AsrCallback", "OnEndOfSpeech");
#endif
                entity.status = VoiceAssistantStatus.READY;
            }
        }
    }
}