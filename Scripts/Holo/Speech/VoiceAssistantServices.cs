using UnityEngine;

namespace Holo.Speech
{
    /// <summary>
    /// �����������ʱ����
    /// </summary>
    /// <param name="error"></param>
    public delegate void OnSpeechError(string error);

    /// <summary>
    /// ��������ʶ��ί��
    /// </summary>
    /// <param name="content">ʶ����</param>
    public delegate void OnAsrResults(string content);

    /// <summary>
    /// ASR����
    /// </summary>
    public delegate void OnAsrReady();

    /// <summary>
    /// ASR��⵽����ʱ����
    /// </summary>
    public delegate void OnAsrBegin();

    /// <summary>
    /// ASR����ʱ����
    /// </summary>
    public delegate void OnAsrEnd();

    /// <summary>
    /// ASR��⵽�����仯ʱ����
    /// </summary>
    /// <param name="rms"></param>
    public delegate void OnAsrRmsChanged(float rms);

    /// <summary>
    /// ASR��ⳬʱ����
    /// </summary>
    public delegate void OnAsrTimeout();

    /// <summary>
    /// ��������ʱ����
    /// </summary>
    /// <param name="confidence">���Ŷ�</param>
    /// <param name="wakeupWord">���Ѵ�</param>
    public delegate void OnWakeup(double confidence, string wakeupWord);

    /// <summary>
    /// TTS��ʼ�ϳ�����ʱ����
    /// </summary>
    public delegate void OnTtsSynthesizeStart();

    /// <summary>
    /// TTS�����ϳɽ���ʱ����
    /// </summary>
    public delegate void OnTtsSynthesizeFinish();

    /// <summary>
    /// TTS��ʼ˵��ʱ����
    /// </summary>
    public delegate void OnTtsSpeechStart();

    /// <summary>
    /// TTS˵������ʱ����
    /// </summary>
    public delegate void OnTtsSpeechFinish();

    /// <summary>
    /// �������ַ���
    /// </summary>
    public class VoiceAssistantService
    {
        private static readonly object lockObject = new object();
        private static VoiceAssistantService instance = null;
        private AndroidJavaObject service;

        //SetErrorCallback
        private OnSpeechError onSpeechError { get; set; }

        //SetAsrResultsCallback
        private OnAsrResults onAsrResults { get; set; }

        //SetAsrReadyCallback
        private OnAsrReady onAsrReady { get; set; }

        //SetAsrBeginCallback
        private OnAsrBegin onAsrBegin { get; set; }

        //SetAsrEndCallback
        private OnAsrEnd onAsrEnd { get; set; }

        //SetAsrRmsChangedCallback
        private OnAsrRmsChanged onAsrRmsChanged { get; set; }

        //SetAsrTimeoutCallback
        private OnAsrTimeout onAsrTimeout { get; set; }

        //SetWakeupCallback
        private OnWakeup onWakeup { get; set; }

        //SetTtsSynthesizeStartCallback
        private OnTtsSynthesizeStart onTtsSynthesizeStart { get; set; }

        //SetTtsSynthesizeFinishCallback
        private OnTtsSynthesizeFinish onTtsSynthesizeFinish { get; set; }

        //SetTtsSpeechStartCallback
        private OnTtsSpeechStart onTtsSpeechStart { get; set; }

        //SetTtsSpeechFinishCallback
        private OnTtsSpeechFinish onTtsSpeechFinish { get; set; }

        /// <summary>
        /// ���ó���ʱ�Ļص�
        /// </summary>
        /// <param name="callback">OnSpeechError</param>
        /// <returns></returns>
        public VoiceAssistantService SetErrorCallback(OnSpeechError callback)
        {
            this.onSpeechError = callback;
            return this;
        }

        /// <summary>
        /// ����ASRʶ�𵽽��ʱ�Ļص�
        /// </summary>
        /// <param name="callback">OnAsrResults</param>
        /// <returns></returns>
        public VoiceAssistantService SetAsrResultsCallback(OnAsrResults callback)
        {
            this.onAsrResults = callback;
            return this;
        }

        /// <summary>
        /// ����ASR����ʱ�Ļص�
        /// </summary>
        /// <param name="callback">OnAsrReady</param>
        /// <returns></returns>
        public VoiceAssistantService SetAsrReadyCallback(OnAsrReady callback)
        {
            this.onAsrReady = callback;
            return this;
        }

        /// <summary>
        /// ����ASR��⿪ʼʱ�Ļص�
        /// </summary>
        /// <param name="callback">OnAsrBegin</param>
        /// <returns></returns>
        public VoiceAssistantService SetAsrBeginCallback(OnAsrBegin callback)
        {
            this.onAsrBegin = callback;
            return this;
        }

        /// <summary>
        /// ����ASR������ʱ�Ļص�
        /// </summary>
        /// <param name="callback">OnAsrEnd</param>
        /// <returns></returns>
        public VoiceAssistantService SetAsrEndCallback(OnAsrEnd callback)
        {
            this.onAsrEnd = callback;
            return this;
        }

        /// <summary>
        /// ����ASR��⵽�����仯ʱ�Ļص�
        /// </summary>
        /// <param name="callback">OnAsrRmsChanged</param>
        /// <returns></returns>
        public VoiceAssistantService SetAsrRmsChangedCallback(OnAsrRmsChanged callback)
        {
            this.onAsrRmsChanged = callback;
            return this;
        }

        /// <summary>
        /// ��������ʶ��ʱ�Ļص�
        /// </summary>
        /// <param name="callback">OnAsrTimeout</param>
        /// <returns></returns>
        public VoiceAssistantService SetAsrTimeoutCallback(OnAsrTimeout callback)
        {
            this.onAsrTimeout = callback;
            return this;
        }

        /// <summary>
        /// �����������ѵĻص�
        /// </summary>
        /// <param name="callback">OnWakeup</param>
        /// <returns></returns>
        public VoiceAssistantService SetWakeupCallback(OnWakeup callback)
        {
            this.onWakeup = callback;
            return this;
        }

        /// <summary>
        /// ����TTS-�����ϳɿ�ʼʱ�Ļص�
        /// </summary>
        /// <param name="callback">OnTtsSynthesizeStart</param>
        /// <returns></returns>
        public VoiceAssistantService SetTtsSynthesizeStartCallback(OnTtsSynthesizeStart callback)
        {
            this.onTtsSynthesizeStart = callback;
            return this;
        }

        /// <summary>
        /// ����TTS-�����ϳ����ʱ�Ļص�
        /// </summary>
        /// <param name="callback">OnTtsSynthesizeFinish</param>
        /// <returns></returns>
        public VoiceAssistantService SetTtsSynthesizeFinishCallback(OnTtsSynthesizeFinish callback)
        {
            this.onTtsSynthesizeFinish = callback;
            return this;
        }

        /// <summary>
        /// ����TTS-˵����ʼʱ�Ļص�
        /// </summary>
        /// <param name="callback">OnTtsSpeechStart</param>
        /// <returns></returns>
        public VoiceAssistantService SetTtsSpeechStartCallback(OnTtsSpeechStart callback)
        {
            this.onTtsSpeechStart = callback;
            return this;
        }

        /// <summary>
        /// ����TTS-˵������ʱ�Ļص�
        /// </summary>
        /// <param name="callback">OnTtsSpeechFinish</param>
        /// <returns></returns>
        public VoiceAssistantService SetTtsSpeechFinishCallback(OnTtsSpeechFinish callback)
        {
            this.onTtsSpeechFinish = callback;
            return this;
        }

        /// <summary>
        /// ��ȡ����
        /// </summary>
        public static VoiceAssistantService Instance
        {
            get
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new VoiceAssistantService();
                    }
                    return instance;
                }
            }
        }

        /// <summary>
        /// ���캯��
        /// </summary>
        private VoiceAssistantService()
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                return;
            }

            AndroidJavaClass voiceAssistantServiceCls = new AndroidJavaClass("com.eqgis.speech.VoiceAssistantService");
            service = voiceAssistantServiceCls.CallStatic<AndroidJavaObject>("getInstance");
        }

        /// <summary>
        /// ����������
        /// </summary>
        public void Bind()
        {
            if (service == null) return;

            service.Call("bind");
        }

        /// <summary>
        /// ��ʼʹ����������
        /// </summary>
        /// <param name="speechRecognize">����ʶ��ص�</param>
        public void StartService()
        {
            if (service == null) return;

            service.Call("startService", new Callback(this));
        }

        /// <summary>
        /// ֹͣʹ����������
        /// </summary>
        public void StopService() {  
            if (service == null) return;

            service.Call("stopService");
        }

        /// <summary>
        /// ������������
        /// </summary>
        public void Unbind()
        {
            if (service == null) return;

            service.Call("unbind");
        }


        /// <summary>
        /// ����ʶ��ص�
        /// </summary>
        private class Callback : UnitySpeechCallback
        {
            private VoiceAssistantService service;

            public Callback(VoiceAssistantService service)
            {
                this.service = service;
            }

            /// <summary>
            /// ʶ�����ص�
            /// </summary>
            /// <param name="content"></param>
            public override void OnResults(string str)
            {
                if(service.onAsrResults != null)
                {
                    service.onAsrResults(str);
                }
            }


            /// <summary>
            /// ASR����
            /// </summary>
            public override void OnReadyForSpeech()
            {
                if(service.onAsrReady != null)
                {
                    service.onAsrReady();
                }
            }

            /// <summary>
            /// ASR��⵽��ʼ����������
            /// </summary>
            public override void OnBeginningOfSpeech()
            {
                if (service.onAsrBegin != null)
                {
                    service.onAsrBegin();
                }
            }

            /// <summary>
            /// ASR����
            /// </summary>
            public override void OnEndOfSpeech()
            {
                if (service.onAsrEnd != null)
                {
                    service.onAsrEnd();
                }
            }

            /// <summary>
            /// ASR��⵽�������仯
            /// </summary>
            /// <param name="var1"></param>
            public override void OnRmsChanged(float var1)
            {
                if (service.onAsrRmsChanged != null)
                {
                    service.onAsrRmsChanged(var1);
                }
            }

            /// <summary>
            /// ASR\TTS\Wakeup����ʱ����
            /// </summary>
            /// <param name="error"></param>
            public override void OnError(string error)
            {
                if (service.onSpeechError != null)
                {
                    service.onSpeechError(error);
                }
            }

            /// <summary>
            /// ASR��ʱ
            /// </summary>
            public override void OnAsrTimeout()
            {
                if (service.onAsrTimeout != null)
                {
                    service.onAsrTimeout();
                }
            }

            /// <summary>
            /// ����
            /// </summary>
            /// <param name="confidence">���Ŷ�</param>
            /// <param name="wakeupWord">���Ѵ�</param>
            public override void OnWakeup(double confidence, string wakeupWord)
            {
                if (service.onWakeup != null)
                {
                    service.onWakeup(confidence,wakeupWord);
                }
            }

            /// <summary>
            /// ������ʼ�ϳ�
            /// </summary>
            /// <param name="utteranceId"></param>
            public override void OnSynthesizeStart(string utteranceId)
            {
                if (service.onTtsSynthesizeStart != null)
                {
                    service.onTtsSynthesizeStart();
                }
            }

            /// <summary>
            /// �����ϳɽ���
            /// </summary>
            /// <param name="utteranceId"></param>
            public override void OnSynthesizeFinish(string utteranceId)
            {
                if (service.onTtsSynthesizeFinish != null)
                {
                    service.onTtsSynthesizeFinish();
                }
            }

            /// <summary>
            /// TTS��ʼ˵��
            /// </summary>
            /// <param name="utteranceId"></param>
            public override void OnSpeechStart(string utteranceId)
            {
                if (service.onTtsSpeechStart != null)
                {
                    service.onTtsSpeechStart();
                }
            }

            /// <summary>
            /// TTS˵������
            /// </summary>
            /// <param name="utteranceId"></param>
            public override void OnSpeechFinish(string utteranceId)
            {
                if (service.onTtsSpeechFinish != null)
                {
                    service.onTtsSpeechFinish();
                }
            }
        }
    }

}