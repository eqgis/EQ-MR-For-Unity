using Holo.XR.Android;
using System.Collections;
using UnityEngine;

namespace Holo.Speech
{
    /// <summary>
    /// ����ͨ������
    /// </summary>
    public abstract class ISpeechEngine : MonoBehaviour
    {

        //��׿java����
        protected AndroidJavaObject engine;

        /// <summary>
        /// ���ð�׿�����еķ���
        /// </summary>
        /// <param name="funcName">������</param>
        protected void CallEngineMethod(string funcName)
        {
            if (engine != null)
            {
                engine.Call(funcName);
            }
        }

        /// <summary>
        /// ���ð�׿�����еķ���
        /// </summary>
        /// <param name="funcName">������</param>
        /// <param name="param">����</param>
        protected void CallEngineMethod(string funcName,string param)
        {
            //���ⲿ�ж�Engine�Ƿ�Ϊnull
            if (string.IsNullOrEmpty(param.Trim())) return;

            engine.Call(funcName,param);
        }

        /// <summary>
        /// ���ð�׿�����еķ���
        /// </summary>
        /// <param name="funcName">������</param>
        /// <param name="param">����</param>
        protected void CallEngineMethod(string funcName, string[] param)
        {
            //���ⲿ�ж�Engine�Ƿ�Ϊnull
            if (param != null && param.Length != 0)
            {
                engine.Call(funcName, param);
            }
        }


        /// <summary>
        /// �����ʼ��
        /// </summary>
        /// <param name="callback">�¼��ص�</param>
        public abstract void InitEngine(UnitySpeechCallback callback);

        /// <summary>
        /// ��������
        /// </summary>
        public virtual void StartEngine()
        {
            CallEngineMethod("start");
#if DEBUG
            EqLog.i(this.name, "StartEngine successful.");
#endif
        }

        /// <summary>
        /// ֹͣ����
        /// </summary>
        public void StopEngine()
        {
            CallEngineMethod("stop");
#if DEBUG
            EqLog.i(this.name, "StopEngine successful.");
#endif
        }

        /// <summary>
        /// ��������
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