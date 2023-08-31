using Holo.XR.Android;
using System.Collections;
using UnityEngine;

namespace Holo.Speech
{
    /// <summary>
    /// ˼�س�������������
    /// </summary>
    public class SbcWakeupEngine : ISpeechEngine
    {

        [Header("��������(�����������Ĭ��ֵ)")]
        [Tooltip("���ñ��ػ�����Դ·��")]
        public string wakeupResourcePath;

        /// <summary>
        /// �������
        /// </summary>
        private void OnDestroy()
        {
            DestroyEngine();
        }

        /// <summary>
        /// �����ʼ��
        /// </summary>
        public override void InitEngine(UnitySpeechCallback callback)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                if (engine != null)
                {
                    DestroyEngine();
                }
                engine = new AndroidJavaObject("com.eqgis.speech.sbc.SbcAsrEngine", this.name, cloud);
            }
            //��ʼ������
            StartCoroutine(InnerInitEngine(callback));
        }

        /// <summary>
        /// ִ�������ʼ��
        /// </summary>
        /// <param name="speechCallback"></param>
        private IEnumerator InnerInitEngine(UnitySpeechCallback speechCallback)
        {
            if (engine == null) yield return null;

            //���ǲ�����������·��������Ҫִ���������ء�·�����µĲ�����
            //�˴�Ԥ�����ݲ�ʵ�� todo
            //����sd����·��

            CallEngineMethod("setLocalWakeupResource", wakeupResourcePath);

            //ִ�г�ʼ������
            engine.Call("initFromUnity", speechCallback);
#if DEBUG
            EqLog.i(this.name, "InitEngine successful.");
#endif
        }
    }
}