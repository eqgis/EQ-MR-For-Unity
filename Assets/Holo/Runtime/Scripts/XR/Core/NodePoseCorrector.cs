using UnityEngine;

namespace Holo.XR.Core
{
    /// <summary>
    /// �ڵ�λ��У����
    /// <see cref="NodePoseRecorder"/>
    /// <see cref="ExSceneTransition"/>
    /// </summary>
    public class NodePoseCorrector : MonoBehaviour
    {
        public bool isHorizontal = false;
        private void Awake()
        {
            NodePoseRecorder npr = NodePoseRecorder.GetInstance();
            //���³����ڵ�(�Լ���Щͬ���ڵ㣨��content��ֱ���ӽڵ㡱��)�����λ�á�
            this.transform.localPosition = npr.NextSceneNodePosition;

            if (isHorizontal)
            {
                //ֻ����Y�����ת�Ƕȡ�ȷ��������ˮƽ�治���޸�
                this.transform.localEulerAngles = new Vector3(0, npr.NextSceneNodeRotation.y, 0);
            }
            else
            {
                this.transform.localEulerAngles = npr.NextSceneNodeRotation;
            }
        }


    }
}