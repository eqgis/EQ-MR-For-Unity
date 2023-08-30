using System.Collections;
using UnityEngine;

namespace Holo.Speech
{
    /// <summary>
    /// 语音通用引擎
    /// </summary>
    public abstract class ISpeechEngine : MonoBehaviour
    {

        //安卓java对象
        protected AndroidJavaObject engine;

        protected void CallEngineMethod(string funcName,string param)
        {
            //在外部判断Engine是否为null
            if (string.IsNullOrEmpty(param)) return;

            engine.Call(funcName,param);
        }


        /// <summary>
        /// 引擎初始化
        /// </summary>
        /// <param name="callback">事件回调</param>
        public abstract void InitEngine(UnitySpeechCallback callback);

        /// <summary>
        /// 启动引擎
        /// </summary>
        public abstract void StartEngine();

        /// <summary>
        /// 停止引擎
        /// </summary>
        public abstract void StopEngine();

        /// <summary>
        /// 销毁引擎
        /// </summary>
        public abstract void DestroyEngine();
    }

}