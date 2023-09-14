using UnityEngine;

namespace Holo.Speech
{
    /// <summary>
    /// 语音服务出错时触发
    /// </summary>
    /// <param name="error"></param>
    public delegate void OnSpeechError(string error);

    /// <summary>
    /// 语音服务识别委托
    /// </summary>
    /// <param name="content">识别结果</param>
    public delegate void OnAsrResults(string content);

    /// <summary>
    /// ASR就绪
    /// </summary>
    public delegate void OnAsrReady();

    /// <summary>
    /// ASR监测到声音时触发
    /// </summary>
    public delegate void OnAsrBegin();

    /// <summary>
    /// ASR结束时触发
    /// </summary>
    public delegate void OnAsrEnd();

    /// <summary>
    /// ASR监测到音量变化时触发
    /// </summary>
    /// <param name="rms"></param>
    public delegate void OnAsrRmsChanged(float rms);

    /// <summary>
    /// ASR监测超时触发
    /// </summary>
    public delegate void OnAsrTimeout();

    /// <summary>
    /// 语音唤醒时触发
    /// </summary>
    /// <param name="confidence">置信度</param>
    /// <param name="wakeupWord">唤醒词</param>
    public delegate void OnWakeup(double confidence, string wakeupWord);

    /// <summary>
    /// TTS开始合成语音时触发
    /// </summary>
    public delegate void OnTtsSynthesizeStart();

    /// <summary>
    /// TTS语音合成结束时触发
    /// </summary>
    public delegate void OnTtsSynthesizeFinish();

    /// <summary>
    /// TTS开始说话时触发
    /// </summary>
    public delegate void OnTtsSpeechStart();

    /// <summary>
    /// TTS说话结束时触发
    /// </summary>
    public delegate void OnTtsSpeechFinish();

    /// <summary>
    /// 语音助手服务
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
        /// 设置出错时的回调
        /// </summary>
        /// <param name="callback">OnSpeechError</param>
        /// <returns></returns>
        public VoiceAssistantService SetErrorCallback(OnSpeechError callback)
        {
            this.onSpeechError = callback;
            return this;
        }

        /// <summary>
        /// 设置ASR识别到结果时的回调
        /// </summary>
        /// <param name="callback">OnAsrResults</param>
        /// <returns></returns>
        public VoiceAssistantService SetAsrResultsCallback(OnAsrResults callback)
        {
            this.onAsrResults = callback;
            return this;
        }

        /// <summary>
        /// 设置ASR就绪时的回调
        /// </summary>
        /// <param name="callback">OnAsrReady</param>
        /// <returns></returns>
        public VoiceAssistantService SetAsrReadyCallback(OnAsrReady callback)
        {
            this.onAsrReady = callback;
            return this;
        }

        /// <summary>
        /// 设置ASR监测开始时的回调
        /// </summary>
        /// <param name="callback">OnAsrBegin</param>
        /// <returns></returns>
        public VoiceAssistantService SetAsrBeginCallback(OnAsrBegin callback)
        {
            this.onAsrBegin = callback;
            return this;
        }

        /// <summary>
        /// 设置ASR监测结束时的回调
        /// </summary>
        /// <param name="callback">OnAsrEnd</param>
        /// <returns></returns>
        public VoiceAssistantService SetAsrEndCallback(OnAsrEnd callback)
        {
            this.onAsrEnd = callback;
            return this;
        }

        /// <summary>
        /// 设置ASR监测到音量变化时的回调
        /// </summary>
        /// <param name="callback">OnAsrRmsChanged</param>
        /// <returns></returns>
        public VoiceAssistantService SetAsrRmsChangedCallback(OnAsrRmsChanged callback)
        {
            this.onAsrRmsChanged = callback;
            return this;
        }

        /// <summary>
        /// 设置语音识别超时的回调
        /// </summary>
        /// <param name="callback">OnAsrTimeout</param>
        /// <returns></returns>
        public VoiceAssistantService SetAsrTimeoutCallback(OnAsrTimeout callback)
        {
            this.onAsrTimeout = callback;
            return this;
        }

        /// <summary>
        /// 设置语音唤醒的回调
        /// </summary>
        /// <param name="callback">OnWakeup</param>
        /// <returns></returns>
        public VoiceAssistantService SetWakeupCallback(OnWakeup callback)
        {
            this.onWakeup = callback;
            return this;
        }

        /// <summary>
        /// 设置TTS-语音合成开始时的回调
        /// </summary>
        /// <param name="callback">OnTtsSynthesizeStart</param>
        /// <returns></returns>
        public VoiceAssistantService SetTtsSynthesizeStartCallback(OnTtsSynthesizeStart callback)
        {
            this.onTtsSynthesizeStart = callback;
            return this;
        }

        /// <summary>
        /// 设置TTS-语音合成完成时的回调
        /// </summary>
        /// <param name="callback">OnTtsSynthesizeFinish</param>
        /// <returns></returns>
        public VoiceAssistantService SetTtsSynthesizeFinishCallback(OnTtsSynthesizeFinish callback)
        {
            this.onTtsSynthesizeFinish = callback;
            return this;
        }

        /// <summary>
        /// 设置TTS-说话开始时的回调
        /// </summary>
        /// <param name="callback">OnTtsSpeechStart</param>
        /// <returns></returns>
        public VoiceAssistantService SetTtsSpeechStartCallback(OnTtsSpeechStart callback)
        {
            this.onTtsSpeechStart = callback;
            return this;
        }

        /// <summary>
        /// 设置TTS-说话结束时的回调
        /// </summary>
        /// <param name="callback">OnTtsSpeechFinish</param>
        /// <returns></returns>
        public VoiceAssistantService SetTtsSpeechFinishCallback(OnTtsSpeechFinish callback)
        {
            this.onTtsSpeechFinish = callback;
            return this;
        }

        /// <summary>
        /// 获取单例
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
        /// 构造函数
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
        /// 绑定语音服务
        /// </summary>
        public void Bind()
        {
            if (service == null) return;

            service.Call("bind");
        }

        /// <summary>
        /// 开始使用语音服务
        /// </summary>
        /// <param name="speechRecognize">语音识别回调</param>
        public void StartService()
        {
            if (service == null) return;

            service.Call("startService", new Callback(this));
        }

        /// <summary>
        /// 停止使用语音服务
        /// </summary>
        public void StopService() {  
            if (service == null) return;

            service.Call("stopService");
        }

        /// <summary>
        /// 解除语音服务绑定
        /// </summary>
        public void Unbind()
        {
            if (service == null) return;

            service.Call("unbind");
        }


        /// <summary>
        /// 语音识别回调
        /// </summary>
        private class Callback : UnitySpeechCallback
        {
            private VoiceAssistantService service;

            public Callback(VoiceAssistantService service)
            {
                this.service = service;
            }

            /// <summary>
            /// 识别结果回调
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
            /// ASR就绪
            /// </summary>
            public override void OnReadyForSpeech()
            {
                if(service.onAsrReady != null)
                {
                    service.onAsrReady();
                }
            }

            /// <summary>
            /// ASR检测到开始有声音输入
            /// </summary>
            public override void OnBeginningOfSpeech()
            {
                if (service.onAsrBegin != null)
                {
                    service.onAsrBegin();
                }
            }

            /// <summary>
            /// ASR结束
            /// </summary>
            public override void OnEndOfSpeech()
            {
                if (service.onAsrEnd != null)
                {
                    service.onAsrEnd();
                }
            }

            /// <summary>
            /// ASR检测到有音量变化
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
            /// ASR\TTS\Wakeup出错时触发
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
            /// ASR超时
            /// </summary>
            public override void OnAsrTimeout()
            {
                if (service.onAsrTimeout != null)
                {
                    service.onAsrTimeout();
                }
            }

            /// <summary>
            /// 唤醒
            /// </summary>
            /// <param name="confidence">置信度</param>
            /// <param name="wakeupWord">唤醒词</param>
            public override void OnWakeup(double confidence, string wakeupWord)
            {
                if (service.onWakeup != null)
                {
                    service.onWakeup(confidence,wakeupWord);
                }
            }

            /// <summary>
            /// 语音开始合成
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
            /// 语音合成结束
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
            /// TTS开始说话
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
            /// TTS说话结束
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