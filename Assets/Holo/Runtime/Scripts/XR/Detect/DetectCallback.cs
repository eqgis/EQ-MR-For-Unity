using UnityEngine;

namespace Holo.XR.Detect
{
    public class DetectCallback : MonoBehaviour
    {
        [Header("Image Data Matcher"), Tooltip("用于图像与数据进行匹配")]
        public ImageDataMatcher matcher;

        public virtual void OnAdded(ARImageInfo image) { }
        public virtual void OnUpdate(ARImageInfo image) { }
        public virtual void OnRemoved(ARImageInfo image) { }

        public ImageDataMatcher GetImageDataMatcher() { return matcher; }
    }
}