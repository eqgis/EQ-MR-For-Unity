using Holo.XR.Android;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Holo.XR.Core
{
    /// <summary>
    /// ������ת������
    /// </summary>
    public class JumpSceneController : MonoBehaviour
    {
        public GameObject[] dontDestoryGroup;

        private GameObject uiRaycastCamera;//ע�������Ƕ�̬��ֵ�����

        [Header("Target Scene")]
        public string sceneName;

        [Header("NextSceneNode Ref")]
        [Tooltip("�ö������ΪSceneNode��ֱ���Ӷ���")]
        public Transform nextSceneNodeTransform;


        [Tooltip("�Զ���ת")]
        public bool auto = false;

        // Start is called before the first frame update
        void Start()
        {
            for (int i = 0; i < dontDestoryGroup.Length; i++)
            {
                DontDestroyOnLoad(dontDestoryGroup[i]);
            }

            uiRaycastCamera = GameObject.Find("UIRaycastCamera");
            if (uiRaycastCamera != null)
            {
                DontDestroyOnLoad(uiRaycastCamera);
            }

            if (auto)
            {
                Invoke("ToNewScene", 0F);
            }
        }

        /// <summary>
        /// ��ת���³���
        /// </summary>
        /// <param name="sceneName">Ŀ�곡������</param>
        public void ToNewScene(string sceneName)
        {
            this.sceneName = sceneName;
            ToNewScene();
        }

        /// <summary>
        /// ��ת���³���
        /// </summary>
        public void ToNewScene()
        {
            try
            {
                //���ȵ��ã���ֹTagʶ����߳���ʶ���ȡλ����Ϣ��currentSceneNodeTransform��pose���޸�
                if (nextSceneNodeTransform != null)
                {
                    NodePoseRecorder nodePoseRecorder = NodePoseRecorder.GetInstance();

                    //��¼��һ�������ڵ㣬����ڳ�ʼ�����ڵ�����λ�á����ڿ����������л������������������Ҫʵʱ���ۼӡ�
                    nodePoseRecorder.NextSceneNodePosition = nodePoseRecorder.NextSceneNodePosition + nextSceneNodeTransform.localPosition;
                    nodePoseRecorder.NextSceneNodeRotation = nodePoseRecorder.NextSceneNodeRotation + nextSceneNodeTransform.localEulerAngles;

                    //if (AndroidUtils.debug)
                    //{
                    //    EqLog.d("JumpSceneController", "nextSceneNodeTransform.localPosition:" + nextSceneNodeTransform.localPosition.ToString() + "\n" +
                    //        "nextSceneNodeTransform.position:" + nextSceneNodeTransform.position);
                    //}
                }

                SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            }catch(Exception e)
            {
                EqLog.e("JumpSceneController",e.ToString());
            }
        }

    }
}
