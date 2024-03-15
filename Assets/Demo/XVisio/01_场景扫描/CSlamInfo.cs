
using Holo.XR.Core;
using UnityEngine;
using UnityEngine.UI;

public class CSlamInfo : MonoBehaviour
{
    public Text text;
    public XvCslamMapControl xvCslamMapControl;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        text.text = "∆•≈‰∂»:"+xvCslamMapControl.GetMapMatchingPercent();
    }
}
