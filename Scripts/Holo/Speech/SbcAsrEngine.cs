using Holo.XR.Android;
using System.Collections;
using UnityEngine;

namespace Holo.Speech
{
    /// <summary>
    /// 思必驰语音自动识别引擎
    /// </summary>
    public class SbcAsrEngine : ISpeechEngine
    {
        [Header("使用云端引擎")]
        public bool cloud = true;

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
        public string[] cloudCustomWakeupWord;

        /// <summary>
        /// 组件销毁
        /// </summary>
        private void OnDestroy()
        {
            DestroyEngine();
        }

        /// <summary>
        /// 引擎初始化
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
            //初始化引擎
            StartCoroutine(InnerInitEngine(speechCallback));
        }

        /// <summary>
        /// 执行引擎初始化
        /// </summary>
        /// <param name="speechCallback"></param>
        private IEnumerator InnerInitEngine(UnitySpeechCallback speechCallback)
        {
            if (engine == null) yield return null;

            //参数设置
            if (cloud)
            {
                CallEngineMethod("setCloudCustomWakeupWord", cloudCustomWakeupWord);
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

#if DEBUG
            EqLog.i(this.name, "InitEngine successful.");
#endif
        }

    }


}
