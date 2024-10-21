
using UnityEngine;

namespace Holo.XR.Utils
{
    /// <summary>
    /// Ĭ�����ͬ���ű�
    /// </summary>
    public class DefaultCameraPoseSyncScript : MonoBehaviour
    {
        [Header("MR Camera")]
        public Transform mMRCameraTransform;


        private void FixedUpdate()
        {
            //�ó����е�Ĭ�������MRͷ�Ե���̬����һ��
            this.transform.position = mMRCameraTransform.position;
            this.transform.rotation = mMRCameraTransform.rotation;
        }
    }
}
