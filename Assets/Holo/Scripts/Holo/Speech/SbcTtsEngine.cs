using Holo.XR.Android;
using System.Collections;
using UnityEngine;

namespace Holo.Speech
{
    /// <summary>
    /// 思必驰语音合成引擎
    /// </summary>
    public class SbcTtsEngine : ISpeechEngine
    {
        [Header("本地设置(不设置则采用默认值)")]

        [Tooltip("后端音色资源路径")]
        public string[] backResBinArray;

        [Tooltip("合成前端资源路径,包含文本归一化，分词的，韵律等")]
        public string frontBinResource;

        [Tooltip("合成字典路径")]
        public string dictResource;

        [Tooltip("语音合成结果保存路径")]
        public string saveAudioFilePath;

        //需要转语音的文本内容
        public string textContent { get; set; }

        //当前仅集成了本地引擎
        private bool cloud = false;

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
            //初始化引擎
            StartCoroutine(InnerInitEngine(callback));
        }

        /// <summary>
        /// 启动引擎
        /// </summary>
        public override void StartEngine()
        {
            //传入文本
            if (engine == null) { throw new System.Exception("The engine is not initialized."); }
            //设置语音合成的文本内容
            CallEngineMethod("setContent", textContent);
            //启动引擎
            base.StartEngine();
        }

        /// <summary>
        /// 暂停播放语音
        /// </summary>
        public void Pause()
        {
            CallEngineMethod("pause");
#if DEBUG
            EqLog.i(this.name, "Pause successful.");
#endif
        }

        /// <summary>
        /// 继续播放语音
        /// </summary>
        public void Resume()
        {
            CallEngineMethod("resume");
#if DEBUG
            EqLog.i(this.name, "Resume successful.");
#endif
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
                yield return null;
            }
            else
            {
                //若是采用在线数据路径，则需要执行数据下载、路径更新的操作。
                //此处预留，暂不实现 todo
                //此处采用sd绝对路径

                CallEngineMethod("setBackResBinArray", backResBinArray);
                CallEngineMethod("setDictResource", dictResource);
                CallEngineMethod("setFrontBinResource", frontBinResource);
                CallEngineMethod("setSaveAudioFilePath", saveAudioFilePath);
            }

            //执行初始化步骤
            engine.Call("initFromUnity", speechCallback);

#if DEBUG
            EqLog.i(this.name, "InitEngine successful.");
#endif
        }
    }
}