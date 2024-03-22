using UnityEngine;

namespace Holo.XR.Detect
{
    public class DetectCallback : MonoBehaviour
    {
        public virtual void OnAdded(ARImageInfo image) { }
        public virtual void OnUpdate(ARImageInfo image) { }
        public virtual void OnRemoved(ARImageInfo image) { }

    }
}