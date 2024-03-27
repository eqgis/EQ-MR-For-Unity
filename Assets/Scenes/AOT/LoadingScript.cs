using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScript : MonoBehaviour
{
    public float rotationSpeed = 50.0f; // 旋转速度
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {

        // 获取当前帧的时间增量
        float deltaTime = Time.deltaTime;

        // 根据时间增量和旋转速度来旋转Cube
        transform.Rotate(Vector3.up * rotationSpeed * deltaTime);
    }
}
