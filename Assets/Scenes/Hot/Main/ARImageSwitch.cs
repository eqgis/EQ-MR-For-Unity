using Holo.XR.Detect;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARImageSwitch : MonoBehaviour
{
    public ARCoreImageDetector detector;

     void OnDisable()
    {
        detector.m_TrackedImageManager.enabled = false;
        detector.enabled = false;
    }
    private void OnEnable()
    {
        detector.m_TrackedImageManager.enabled = true;
        detector.enabled = true;
    }
}
