using UnityEngine;

namespace Holo.XR.Core
{

    /// <summary>
    /// �ڵ���̬��¼��
    /// </summary>
    public class NodePoseRecorder
    {
        private static NodePoseRecorder instance = null;

        /*��¼��һ��������λ�ú���̬������λ����̬�����ڳ�����תʱʹ��*/
        private Vector3 nextSceneNodeRotation;
        private Vector3 nextSceneNodePosition;

        private Vector3 lastSceneNodeRotation;
        private Vector3 lastSceneNodePosition;

        public int count = 0;

        private NodePoseRecorder()
        {
            nextSceneNodeRotation = Vector3.zero;
            nextSceneNodePosition = Vector3.zero;

            lastSceneNodeRotation = Vector3.zero;
            lastSceneNodePosition = Vector3.zero;
        }

        /// <summary>
        /// �ڵ����λ��
        /// </summary>
        public Vector3 NextSceneNodeRotation { get => nextSceneNodeRotation; set => nextSceneNodeRotation = value; }

        /// <summary>
        /// �ڵ����λ��
        /// </summary>
        public Vector3 NextSceneNodePosition { get => nextSceneNodePosition; set => nextSceneNodePosition = value; }

        //JSON��ȡ�����ʹ��ϣ����л�����������
        /// <summary>
        /// JSON_1 DATA
        /// </summary>
        public Vector3 FirstSceneNodePosition { get => lastSceneNodePosition; set => lastSceneNodePosition = value; }

        /// <summary>
        /// JSON_1 DATA
        /// </summary>
        public Vector3 FirstSceneNodeRotation { get => lastSceneNodeRotation; set => lastSceneNodeRotation = value; }

        /// <summary>
        /// ��ȡʵ��
        /// </summary>
        /// <returns>NodePoseRecorder</returns>
        public static NodePoseRecorder GetInstance()
        {
            if (instance == null)
            {
                instance = new NodePoseRecorder();
            }
            return instance;
        }

    }

}