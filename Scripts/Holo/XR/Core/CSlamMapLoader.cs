using UnityEngine;

namespace Holo.XR.Core
{
    //CSLAM Map º”‘ÿ∆˜
    [System.Obsolete("This entire class is obsolete. Use the XvCslamMapLoader instead.")]
    public class CSlamMapLoader : MonoBehaviour
    {
        [Header("Load Settings")]
        [Tooltip("—” ±∂¡»°CSlam Map£¨∫¡√Î")]
        public int delay = 3000;

        public XvCSlamComponent slamComponent;

        public bool enable = false;
        // Start is called before the first frame update
        void Start()
        {
            if (enable)
            {
                Invoke("ReadSlamMap", delay / 1000.0f);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }


        public void ReadSlamMap()
        {
            slamComponent.LoadMap();
        }
    }
}
