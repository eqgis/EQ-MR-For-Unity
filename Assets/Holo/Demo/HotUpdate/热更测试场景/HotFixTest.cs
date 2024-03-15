using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotFixTest : MonoBehaviour
{
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        float deltaTime = Time.deltaTime;
        transform.Rotate(Vector3.up * 50 * deltaTime);

    }
}
