using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 热更测试脚本
/// </summary>
public class CubeScript : MonoBehaviour
{
    public float rotationSpeed = 50.0f; // 旋转速度
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        // 获取当前帧的时间增量
        float deltaTime = Time.deltaTime;

        // 根据时间增量和旋转速度来旋转Cube
        transform.Rotate(Vector3.up * rotationSpeed * deltaTime);
    }
}
