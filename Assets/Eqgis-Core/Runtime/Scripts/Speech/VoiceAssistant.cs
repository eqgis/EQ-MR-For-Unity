using Holo.XR.Android;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.EventSystems.EventTrigger;

namespace Holo.Speech
{
    /// <summary>
    /// ��������״̬
    /// </summary>
    enum VoiceAssistantStatus
    {
        /// <summary>
        /// �������ȴ����ѣ�
        /// </summary>
        READY,

        /// <summary>
        /// ˵���У�TTS������״̬��
        /// </summary>
        SPEEKING,

        /// <summary>
        /// �����У�ASR������״̬��
        /// </summary>
        LISTENNING
    }

    /// <summary>
    /// ����
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
        /// ���ֽ���˵��������
        /// </summary>
        public int talkCount { get; set; }

        public string[] wakeupResponses = new string[] {
            "!����","!��ָʾ","!��Ը�","��ʲô���԰���������"
        };

        public string[] positiveResponses = new string[] {
            "�õ�",
            "û��",
            "��Ȼ",
            "���Ե�",
            "������"
        };

        public string[] sayByeResponses = new string[] {   
            "����㻹���������⣬��ʱ����",
            "�����Ҫ����������ʱ��ϵ��",};

        public string[] greetingResponses = new string[] {"���",
            "��ã���ʲô�ҿ��԰��������"
        };

        public string[] helpResponses = new string[] {
            // �ṩ����
            "�ҿ��԰��������Ϣ",
            "�ҿ��Իش��������",
            "��ʲô�ҿ��԰��������",
        };

        /// <summary>
        /// ��ȡ����ش�
        /// </summary>
        /// <param name="myArray"></param>
        /// <returns></returns>
        public string RandomResponse(string[] myArray)
        {
            // ����һ�������������
            System.Random random = new System.Random();

            // ����һ�����������
            int randomIndex = random.Next(0, myArray.Length);

            // ��ȡ������ַ���
            return myArray[randomIndex];
        }
    }

    /// <summary>
    /// �������֣������
    /// </summary>
    public class VoiceAssistant : MonoBehaviour
    {
        [Header("˼�س�SDK����")]
        public SbcPlugin sbcPlugin;

        [Header("��������")]
        public SbcWakeupEngine wakeupEngine;
        public SbcTtsEngine ttsEngine;
        public SbcAsrEngine asrEngine;

        private AssistantEntity assistant;
        private bool init  = false;

        [Header("ʶ��")]
        public UnityEvent recognizeEvent;

        [Header("��ʼ���ɹ�")]
        public UnityEvent initSuccess;

        /// <summary>
        /// ����ʶ�������
        /// </summary>
        public string content { get; private set; }

        /// <summary>
        /// ����ʶ��ص����
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
        /// ���������ʼ��
        /// </summary>
        public void InitEngine()
        {
            if (init) { return; }

            //ʵ�������ں�������
            assistant = new AssistantEntity();
            assistant.entityName = this.name;
            assistant.ttsEngine = this.ttsEngine;
            assistant.asrEngine = this.asrEngine;
            assistant.wakeupEngine = this.wakeupEngine;
            assistant.talkCount = 0;
            assistant.initSuccess = this.initSuccess;

            //������Ӧ�ص��¼���ִ�г�ʼ��
            asrEngine.InitEngine(new AsrCallback(OnAsrResponse, assistant));

            init = true;

            //wakeupEngine.InitEngine();
        }

        /// <summary>
        /// ��������
        /// </summary>
        public void DestroyEngine()
        {
            //��������
            wakeupEngine.DestroyEngine();
            ttsEngine.DestroyEngine();
            asrEngine.DestroyEngine();

            //����״̬
            init = false;
        }

        private void OnDestroy()
        {
            DestroyEngine();
        }

        /// <summary>
        /// ���ѻص�
        /// </summary>
        private class WakeupCallback : UnitySpeechCallback
        {
            /// <summary>
            /// ��������ʵ����Ϣ
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
                //��ʼ���ɹ����Զ�������������
                entity.status = VoiceAssistantStatus.READY;
                entity.wakeupEngine.StartEngine();
                
                //��ʼ���ɹ���ִ��Unity�¼�
                entity.initSuccess.Invoke();
            }

            public override void OnWakeup(double confidence, string wakeupWord)
            {
                if (entity.status == VoiceAssistantStatus.READY)
                {
                    //���ֽ���˵����������
                    entity.talkCount = 0;
                    //����TTS����,Ӧ��
                    entity.ttsEngine.textContent = entity.RandomResponse(entity.wakeupResponses);
                    entity.status = VoiceAssistantStatus.SPEEKING;
                    entity.ttsEngine.StartEngine();
                }
            }
        }

        /// <summary>
        /// �����ϳɻص�
        /// </summary>
        private class TtsCallback : UnitySpeechCallback
        {

            /// <summary>
            /// ��������ʵ����Ϣ
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
            /// ��ʼ��������
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
            /// �������Ž���
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
                        //˵������������״̬Ϊready
                        //��һ��˵����Ӧ���������ASR
                        entity.status = VoiceAssistantStatus.LISTENNING;
                        entity.asrEngine.StartEngine();
                        break;
                    default:
                        //˵������������״̬Ϊready
                        entity.status = VoiceAssistantStatus.READY;
                        break;
                }

                //���ֽ���˵������+1
                entity.talkCount++;
            }
        }

        /// <summary>
        /// ����ʶ��ص�
        /// </summary>
        private class AsrCallback : UnitySpeechCallback
        {
            private string content;

            /// <summary>
            /// ��������ʵ����Ϣ
            /// </summary>
            private AssistantEntity entity;

            private Recognize recognize;

            public AsrCallback(Recognize recognize, AssistantEntity entity)
            {
                this.entity = entity;
                this.recognize = recognize;
            }

            /// <summary>
            /// ʶ��ί��
            /// </summary>
            /// <param name="content">ʶ����</param>
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
            /// ��ʱ��Ӧ
            /// </summary>
            public override void OnAsrTimeout()
            {
                if (entity.status == VoiceAssistantStatus.LISTENNING)
                {
                    //��ʱ�ˣ��ر�ASR
                    entity.asrEngine.StopEngine();
                    //����TTS����
                    entity.ttsEngine.textContent = entity.RandomResponse(entity.sayByeResponses);
                    entity.ttsEngine.StartEngine();
                }

#if DEBUG
                EqLog.d("VoiceAssistant-AsrCallback", "OnAsrTimeout");
#endif
            }

            /// <summary>
            /// ASR�����󴥷�
            /// </summary>
            public override void OnReadyForSpeech()
            {
#if DEBUG
                EqLog.d("VoiceAssistant-AsrCallback", "OnReadyForSpeech");
#endif
                //��ʼ����
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
                //��⵽�������󣬻ص�
#if DEBUG
                EqLog.d("VoiceAssistant-AsrCallback", "OnBeginningOfSpeech");
#endif
            }

            /// <summary>
            /// ʶ�����ص�
            /// </summary>
            /// <param name="content"></param>
            public override void OnResults(string str)
            {
                this.content = str;
                recognize(this.content);
            }

            public override void OnEndOfSpeech()
            {
                //ʶ�����
#if DEBUG
                EqLog.d("VoiceAssistant-AsrCallback", "OnEndOfSpeech");
#endif
                entity.status = VoiceAssistantStatus.READY;
            }
        }
    }
}