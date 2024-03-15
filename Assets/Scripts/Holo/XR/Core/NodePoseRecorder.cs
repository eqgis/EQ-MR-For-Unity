using UnityEngine;

namespace Holo.XR.Core
{

    /// <summary>
    /// 节点姿态记录器
    /// </summary>
    public class NodePoseRecorder
    {
        private static NodePoseRecorder instance = null;

        /*记录下一个场景的位置和姿态（绝对位置姿态），在场景跳转时使用*/
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
        /// 节点相对位置
        /// </summary>
        public Vector3 NextSceneNodeRotation { get => nextSceneNodeRotation; set => nextSceneNodeRotation = value; }

        /// <summary>
        /// 节点相对位置
        /// </summary>
        public Vector3 NextSceneNodePosition { get => nextSceneNodePosition; set => nextSceneNodePosition = value; }

        //JSON读取出来就存上，供切换场景后作差
        /// <summary>
        /// JSON_1 DATA
        /// </summary>
        public Vector3 FirstSceneNodePosition { get => lastSceneNodePosition; set => lastSceneNodePosition = value; }

        /// <summary>
        /// JSON_1 DATA
        /// </summary>
        public Vector3 FirstSceneNodeRotation { get => lastSceneNodeRotation; set => lastSceneNodeRotation = value; }

        /// <summary>
        /// 获取实例
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