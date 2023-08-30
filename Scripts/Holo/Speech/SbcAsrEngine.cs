using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Holo.Speech
{
    /// <summary>
    /// 思必驰语音自动识别引擎
    /// </summary>
    public class SbcAsrEngine : ISpeechEngine
    {
        [Header("使用云端引擎")]
        public bool cloud = false;

        [Header("本地设置(不设置则采用默认值)")]

        [Tooltip("声学资源路径")]
        public string acousticResourcesPath;

        [Tooltip("语法资源路径")]
        public string grammarResource;

        [Tooltip("vad资源路径")]
        public string vadResource;

        [Tooltip("网络资源路径")]
        public string netBinResourcePath;

        [Header("云端设置(不设置则采用默认值)")]

        [Tooltip("唤醒词列表")]
        public string cloudResourceType;

        [Tooltip("资源类型")]
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
        /// 销毁引擎
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
        /// 引擎初始化
        /// </summary>
        /// <returns></returns>
        public override void InitEngine(UnitySpeechCallback speechCallback)
        {
            //初始化引擎
            StartCoroutine(InnerInitEngine(speechCallback));
        }

        private IEnumerator InnerInitEngine(UnitySpeechCallback speechCallback)
        {
            if (engine == null) yield return null;

            //参数设置
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
                //若是采用在线数据路径，则需要执行数据下载、路径更新的操作。
                //此处预留，暂不实现 todo
                //采用sd绝对路径

                CallEngineMethod("setLocalAcousticResources", acousticResourcesPath);
                CallEngineMethod("setLocalGrammarResource", grammarResource);
                CallEngineMethod("setLocalVadResource", vadResource);
                CallEngineMethod("setLocalNetBinResource", netBinResourcePath);
            }

            //执行初始化步骤
            engine.Call("initFromUnity",speechCallback);
        }

        /// <summary>
        /// 启动ASR引擎
        /// </summary>
        public override void StartEngine()
        {
            if (engine != null)
            {
                engine.Call("start");
            }
        }

        /// <summary>
        /// 停止ASR引擎
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
