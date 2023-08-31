using Holo.XR.Android;
using System.Collections;
using UnityEngine;

namespace Holo.Speech
{
    /// <summary>
    /// 思必驰语音唤醒引擎
    /// </summary>
    public class SbcWakeupEngine : ISpeechEngine
    {

        [Header("本地设置(不设置则采用默认值)")]
        [Tooltip("设置本地唤醒资源路径")]
        public string wakeupResourcePath;

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
                engine = new AndroidJavaObject("com.eqgis.speech.sbc.SbcAsrEngine", this.name, cloud);
            }
            //初始化引擎
            StartCoroutine(InnerInitEngine(callback));
        }

        /// <summary>
        /// 执行引擎初始化
        /// </summary>
        /// <param name="speechCallback"></param>
        private IEnumerator InnerInitEngine(UnitySpeechCallback speechCallback)
        {
            if (engine == null) yield return null;

            //若是采用在线数据路径，则需要执行数据下载、路径更新的操作。
            //此处预留，暂不实现 todo
            //采用sd绝对路径

            CallEngineMethod("setLocalWakeupResource", wakeupResourcePath);

            //执行初始化步骤
            engine.Call("initFromUnity", speechCallback);
#if DEBUG
            EqLog.i(this.name, "InitEngine successful.");
#endif
        }
    }
}