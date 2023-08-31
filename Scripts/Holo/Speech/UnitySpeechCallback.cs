using System;
using UnityEngine;

namespace Holo.Speech
{
    /// <summary>
    /// Unity�������ص�
    /// </summary>
    public abstract class UnitySpeechCallback : AndroidJavaProxy
    {
        public UnitySpeechCallback() : base("com.eqgis.speech.UnitySpeechCallback") { }

        /// <summary>
        /// ��⵽˵��ʱ�ص�
        /// </summary>
        public abstract void OnBeginningOfSpeech();

        /// <summary>
        /// ˵������ʱ�ص�
        /// </summary>
        public abstract void OnEndOfSpeech();

        /// <summary>
        /// ����ʶ�����
        /// </summary>
        public abstract void OnReadyForSpeech();

        /// <summary>
        /// ��Ƶ���������ı�ʱ�ص�������UI�߳�
        /// </summary>
        /// <param name="var1"></param>
        public abstract void OnRmsChanged(float var1);

        /// <summary>
        /// ����ص�
        /// </summary>
        /// <param name="error"></param>
        public abstract void OnError(String error);

        /// <summary>
        /// ��ʼ���ɹ�
        /// </summary>
        public abstract void OnInitSuccess();

        /// <summary>
        /// ASR-ʶ������
        /// </summary>
        /// <param name="var1">ʶ����</param>
        public abstract void OnResults(String var1);

        /// <summary>
        ///  WakeUp-�������ѳɹ�
        /// </summary>
        /// <param name="confidence">���Ŷ�</param>
        /// <param name="wakeupWord">���Ѵ�</param>
        public abstract void OnWakeup(double confidence, String wakeupWord);

        /// <summary>
        /// TTS-��ʼ�ϳ�
        /// </summary>
        /// <param name="utteranceId"></param>
        public abstract void OnSynthesizeStart(String utteranceId);

        /// <summary>
        /// TTS-�ϳɽ���
        /// </summary>
        /// <param name="utteranceId"></param>
        public abstract void OnSynthesizeFinish(String utteranceId);

        /// <summary>
        /// TTS-��ʼ����
        /// </summary>
        /// <param name="utteranceId"></param>
        public abstract void OnSpeechStart(String utteranceId);

        /// <summary>
        /// TTS-�������
        /// </summary>
        /// <param name="utteranceId"></param>
        public abstract void OnSpeechFinish(String utteranceId);
    }
}