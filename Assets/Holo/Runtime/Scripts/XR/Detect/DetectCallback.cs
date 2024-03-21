using UnityEngine;

namespace Holo.XR.Detect
{
    public class DetectCallback : MonoBehaviour
    {
        public virtual void OnDetect(ARImageInfo image) { }

    }
}