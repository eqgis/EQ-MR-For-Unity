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

        protected void CallEngineMethod(string funcName,string param)
        {
            //���ⲿ�ж�Engine�Ƿ�Ϊnull
            if (string.IsNullOrEmpty(param)) return;

            engine.Call(funcName,param);
        }


        /// <summary>
        /// �����ʼ��
        /// </summary>
        /// <param name="callback">�¼��ص�</param>
        public abstract void InitEngine(UnitySpeechCallback callback);

        /// <summary>
        /// ��������
        /// </summary>
        public abstract void StartEngine();

        /// <summary>
        /// ֹͣ����
        /// </summary>
        public abstract void StopEngine();

        /// <summary>
        /// ��������
        /// </summary>
        public abstract void DestroyEngine();
    }

}