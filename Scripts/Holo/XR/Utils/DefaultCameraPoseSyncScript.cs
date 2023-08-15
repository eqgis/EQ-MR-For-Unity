
using UnityEngine;

namespace Holo.XR.Utils
{
    /// <summary>
    /// 默认相机同步脚本
    /// </summary>
    public class DefaultCameraPoseSyncScript : MonoBehaviour
    {
        [Header("MR Camera")]
        public Transform mMRCameraTransform;


        private void FixedUpdate()
        {
            //让场景中的默认相机和MR头显的姿态保持一致
            this.transform.position = mMRCameraTransform.position;
            this.transform.rotation = mMRCameraTransform.rotation;
        }
    }
}
