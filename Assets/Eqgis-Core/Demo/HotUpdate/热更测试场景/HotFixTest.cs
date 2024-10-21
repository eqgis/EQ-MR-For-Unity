using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Holo.XR.Android;

public class HotFixTest : MonoBehaviour
{
    void Start()
    {
        AndroidUtils.Toast("start!");
    }

    // Update is called once per frame
    void Update()
    {

        float deltaTime = Time.deltaTime;
        transform.Rotate(Vector3.up * 50 * deltaTime);
        EqLog.d("IKKYU","Update!");

    }
}
