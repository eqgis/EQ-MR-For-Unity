using Holo.XR.Android;
using System.Collections;
using UnityEngine;

namespace Holo.Speech
{
    /// <summary>
    /// ˼�س������ϳ�����
    /// </summary>
    public class SbcTtsEngine : ISpeechEngine
    {
        [Header("��������(�����������Ĭ��ֵ)")]

        [Tooltip("�����ɫ��Դ·��")]
        public string[] backResBinArray;

        [Tooltip("�ϳ�ǰ����Դ·��,�����ı���һ�����ִʵģ����ɵ�")]
        public string frontBinResource;

        [Tooltip("�ϳ��ֵ�·��")]
        public string dictResource;

        [Tooltip("�����ϳɽ������·��")]
        public string saveAudioFilePath;

        //��Ҫת�������ı�����
        public string textContent { get; set; }

        //��ǰ�������˱�������
        private bool cloud = false;

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
                engine = new AndroidJavaObject("com.eqgis.speech.sbc.SbcTtsEngine", this.name, cloud);
            }
            //��ʼ������
            StartCoroutine(InnerInitEngine(callback));
        }

        /// <summary>
        /// ��������
        /// </summary>
        public override void StartEngine()
        {
            //�����ı�
            if (engine == null) { throw new System.Exception("The engine is not initialized."); }
            //���������ϳɵ��ı�����
            CallEngineMethod("setContent", textContent);
            //��������
            base.StartEngine();
        }

        /// <summary>
        /// ��ͣ��������
        /// </summary>
        public void Pause()
        {
            CallEngineMethod("pause");
#if DEBUG
            EqLog.i(this.name, "Pause successful.");
#endif
        }

        /// <summary>
        /// ������������
        /// </summary>
        public void Resume()
        {
            CallEngineMethod("resume");
#if DEBUG
            EqLog.i(this.name, "Resume successful.");
#endif
        }

        /// <summary>
        /// ִ�������ʼ��
        /// </summary>
        /// <param name="speechCallback"></param>
        private IEnumerator InnerInitEngine(UnitySpeechCallback speechCallback)
        {
            if (engine == null) yield return null;

            //��������
            if (cloud)
            {
                yield return null;
            }
            else
            {
                //���ǲ�����������·��������Ҫִ���������ء�·�����µĲ�����
                //�˴�Ԥ�����ݲ�ʵ�� todo
                //�˴�����sd����·��

                CallEngineMethod("setBackResBinArray", backResBinArray);
                CallEngineMethod("setDictResource", dictResource);
                CallEngineMethod("setFrontBinResource", frontBinResource);
                CallEngineMethod("setSaveAudioFilePath", saveAudioFilePath);
            }

            //ִ�г�ʼ������
            engine.Call("initFromUnity", speechCallback);

#if DEBUG
            EqLog.i(this.name, "InitEngine successful.");
#endif
        }
    }
}