using Holo.XR.Detect;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARImageSwitch : MonoBehaviour
{
    public ARCoreImageDetector detector;
    // Start is called before the first frame update

    void Start()
    {
        detector.m_TrackedImageManager.enabled = true;
        detector.enabled = true;
    }

     void OnDestory()
    {
        detector.m_TrackedImageManager.enabled = false;
        detector.enabled = false;
    }
}
