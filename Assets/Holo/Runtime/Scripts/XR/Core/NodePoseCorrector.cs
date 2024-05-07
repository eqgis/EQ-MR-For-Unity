using UnityEngine;

namespace Holo.XR.Core
{
    /// <summary>
    /// 节点位姿校正器
    /// <see cref="NodePoseRecorder"/>
    /// <see cref="ExSceneTransition"/>
    /// </summary>
    public class NodePoseCorrector : MonoBehaviour
    {
        public bool isHorizontal = false;
        private void Awake()
        {
            NodePoseRecorder npr = NodePoseRecorder.GetInstance();
            //更新场景节点(以及这些同级节点（“content的直接子节点”）)的相对位置。
            this.transform.localPosition = npr.NextSceneNodePosition;

            if (isHorizontal)
            {
                //只采用Y轴的旋转角度。确保场景的水平面不被修改
                this.transform.localEulerAngles = new Vector3(0, npr.NextSceneNodeRotation.y, 0);
            }
            else
            {
                this.transform.localEulerAngles = npr.NextSceneNodeRotation;
            }
        }


    }
}