using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �ȸ����Խű�
/// </summary>
public class CubeScript : MonoBehaviour
{
    public float rotationSpeed = 50.0f; // ��ת�ٶ�
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        // ��ȡ��ǰ֡��ʱ������
        float deltaTime = Time.deltaTime;

        // ����ʱ����������ת�ٶ�����תCube
        transform.Rotate(Vector3.up * rotationSpeed * deltaTime);
    }
}
