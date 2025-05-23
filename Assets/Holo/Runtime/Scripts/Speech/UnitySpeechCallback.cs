using System;
using UnityEngine;

namespace Holo.Speech
{
    /// <summary>
    /// Unity中语音回调
    /// </summary>
    public class UnitySpeechCallback : AndroidJavaProxy
    {
        public UnitySpeechCallback() : base("com.eqgis.speech.UnitySpeechCallback") { }

        /// <summary>
        /// 检测到说话时回调
        /// </summary>
        public virtual void OnBeginningOfSpeech() { }

        /// <summary>
        /// 说话结束时回调
        /// </summary>
        public virtual void OnEndOfSpeech() { }

        /// <summary>
        /// 语音识别就绪
        /// </summary>
        public virtual void OnReadyForSpeech() { }

        /// <summary>
        /// 音频音量发生改变时回调，在主UI线程
        /// </summary>
        /// <param name="var1"></param>
        public virtual void OnRmsChanged(float var1) { }

        /// <summary>
        /// 出错回调
        /// </summary>
        /// <param name="error"></param>
        public virtual void OnError(String error) { }

        /// <summary>
        /// 初始化成功
        /// </summary>
        public virtual void OnInitSuccess() { }

        /// <summary>
        /// ASR-识别出结果
        /// </summary>
        /// <param name="var1">识别结果</param>
        public virtual void OnResults(String var1) { }

        /// <summary>
        /// ASR-超时响应
        /// </summary>
        public virtual void OnAsrTimeout() { }

        /// <summary>
        ///  WakeUp-语音唤醒成功
        /// </summary>
        /// <param name="confidence">置信度</param>
        /// <param name="wakeupWord">唤醒词</param>
        public virtual void OnWakeup(double confidence, String wakeupWord) { }

        /// <summary>
        /// TTS-开始合成
        /// </summary>
        /// <param name="utteranceId"></param>
        public virtual void OnSynthesizeStart(String utteranceId) { }

        /// <summary>
        /// TTS-合成结束
        /// </summary>
        /// <param name="utteranceId"></param>
        public virtual void OnSynthesizeFinish(String utteranceId) { }

        /// <summary>
        /// TTS-开始播放
        /// </summary>
        /// <param name="utteranceId"></param>
        public virtual void OnSpeechStart(String utteranceId) { }

        /// <summary>
        /// TTS-播放完成
        /// </summary>
        /// <param name="utteranceId"></param>
        public virtual void OnSpeechFinish(String utteranceId) { }
    }
}