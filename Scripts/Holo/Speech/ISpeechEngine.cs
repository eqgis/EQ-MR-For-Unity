using Holo.XR.Android;
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

        /// <summary>
        /// 调用安卓引擎中的方法
        /// </summary>
        /// <param name="funcName">方法名</param>
        protected void CallEngineMethod(string funcName)
        {
            if (engine != null)
            {
                engine.Call(funcName);
            }
        }

        /// <summary>
        /// 调用安卓引擎中的方法
        /// </summary>
        /// <param name="funcName">方法名</param>
        /// <param name="param">参数</param>
        protected void CallEngineMethod(string funcName,string param)
        {
            //在外部判断Engine是否为null
            if (string.IsNullOrEmpty(param.Trim())) return;

            engine.Call(funcName,param);
        }

        /// <summary>
        /// 调用安卓引擎中的方法
        /// </summary>
        /// <param name="funcName">方法名</param>
        /// <param name="param">参数</param>
        protected void CallEngineMethod(string funcName, string[] param)
        {
            //在外部判断Engine是否为null
            if (param != null && param.Length != 0)
            {
                engine.Call(funcName, param);
            }
        }


        /// <summary>
        /// 引擎初始化
        /// </summary>
        /// <param name="callback">事件回调</param>
        public abstract void InitEngine(UnitySpeechCallback callback);

        /// <summary>
        /// 启动引擎
        /// </summary>
        public virtual void StartEngine()
        {
            CallEngineMethod("start");
#if DEBUG
            EqLog.i(this.name, "StartEngine successful.");
#endif
        }

        /// <summary>
        /// 停止引擎
        /// </summary>
        public void StopEngine()
        {
            CallEngineMethod("stop");
#if DEBUG
            EqLog.i(this.name, "StopEngine successful.");
#endif
        }

        /// <summary>
        /// 销毁引擎
        /// </summary>
        public void DestroyEngine()
        {
            if (engine != null)
            {
                engine.Call("destroy");
                engine.Dispose();
                engine = null;
#if DEBUG
                EqLog.i(this.name, "DestroyEngine successful.");
#endif
            }
        }
    }

}