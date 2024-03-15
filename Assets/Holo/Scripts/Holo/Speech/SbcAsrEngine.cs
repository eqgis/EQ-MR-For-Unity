using Holo.XR.Android;
using System.Collections;
using UnityEngine;

namespace Holo.Speech
{
    /// <summary>
    /// ˼�س������Զ�ʶ������
    /// </summary>
    public class SbcAsrEngine : ISpeechEngine
    {
        [Header("ʹ���ƶ�����")]
        public bool cloud = true;

        [Header("��������(�����������Ĭ��ֵ)")]

        [Tooltip("��ѧ��Դ·��")]
        public string acousticResourcesPath;

        [Tooltip("�﷨��Դ·��")]
        public string grammarResource;

        [Tooltip("vad��Դ·��")]
        public string vadResource;

        [Tooltip("������Դ·��")]
        public string netBinResourcePath;

        [Header("�ƶ�����(�����������Ĭ��ֵ)")]

        [Tooltip("���Ѵ��б�")]
        public string cloudResourceType;

        [Tooltip("��Դ����")]
        public string[] cloudCustomWakeupWord;

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
        public override void InitEngine(UnitySpeechCallback speechCallback)
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
            StartCoroutine(InnerInitEngine(speechCallback));
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
                CallEngineMethod("setCloudCustomWakeupWord", cloudCustomWakeupWord);
                CallEngineMethod("setCloudResourceType", cloudResourceType);
            }
            else
            {
                //���ǲ�����������·��������Ҫִ���������ء�·�����µĲ�����
                //�˴�Ԥ�����ݲ�ʵ�� todo
                //����sd����·��

                CallEngineMethod("setLocalAcousticResources", acousticResourcesPath);
                CallEngineMethod("setLocalGrammarResource", grammarResource);
                CallEngineMethod("setLocalVadResource", vadResource);
                CallEngineMethod("setLocalNetBinResource", netBinResourcePath);
            }

            //ִ�г�ʼ������
            engine.Call("initFromUnity",speechCallback);

#if DEBUG
            EqLog.i(this.name, "InitEngine successful.");
#endif
        }

    }


}
