using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Holo.Speech
{
    /// <summary>
    /// ˼�س������Զ�ʶ������
    /// </summary>
    public class SbcAsrEngine : ISpeechEngine
    {
        [Header("ʹ���ƶ�����")]
        public bool cloud = false;

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
        public List<string> cloudCustomWakeupWord;

        // Start is called before the first frame update
        void Start()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                engine = new AndroidJavaObject("com.eqgis.speech.sbc.SbcAsrEngine", this.name, cloud);
                
            }
        }

        private void OnDestroy()
        {
            if (engine != null)
            {
                DestroyEngine();
                engine.Dispose();
            }
        }

        /// <summary>
        /// ��������
        /// </summary>
        public override void DestroyEngine()
        {
            if (engine != null)
            {
                engine.Call("destroy");
                engine = null;
            }
        }

        /// <summary>
        /// �����ʼ��
        /// </summary>
        /// <returns></returns>
        public override void InitEngine(UnitySpeechCallback speechCallback)
        {
            //��ʼ������
            StartCoroutine(InnerInitEngine(speechCallback));
        }

        private IEnumerator InnerInitEngine(UnitySpeechCallback speechCallback)
        {
            if (engine == null) yield return null;

            //��������
            if (cloud)
            {
                if (cloudCustomWakeupWord != null && cloudCustomWakeupWord.Count != 0)
                {
                    engine.Call("setCloudCustomWakeupWord", cloudCustomWakeupWord.ToArray());
                }
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
        }

        /// <summary>
        /// ����ASR����
        /// </summary>
        public override void StartEngine()
        {
            if (engine != null)
            {
                engine.Call("start");
            }
        }

        /// <summary>
        /// ֹͣASR����
        /// </summary>
        public override void StopEngine()
        {
            if (engine != null)
            {
                engine.Call("stop");
            }
        }

    }


}
