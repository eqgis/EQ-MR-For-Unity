using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITalk : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitSuccess()
    {
        Holo.XR.Android.AndroidUtils.Toast("Init Success.");
    }
}
