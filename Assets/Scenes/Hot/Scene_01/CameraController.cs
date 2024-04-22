using Holo.XR.Android;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;

public class CameraController : MonoBehaviour
{
    public Transform m_RefTransform;
    public Camera m_Camera;

    public ShiftScript targetObj;

    public ShiftScript m_ARCoreSession;
    private bool m_MoveCamera = false;
    
    //基准位置
    private Vector3 basePosition;

    void Start()
    {
        targetObj.OnFinish += OnTargetFinish;
        basePosition = m_ARCoreSession.gameObject.transform.position;
    }

    private void OnTargetFinish()
    {
        //当目标对象移动完成后执行
        targetObj.gameObject.SetActive(false);
    }

    void Update()
    {
        try
        {

            Touch touch;
            if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
            {
                return;
            }

            // 发射射线
            Ray ray = m_Camera.ScreenPointToRay(touch.position);
            RaycastHit hit;

            // 检测射线是否与物体发生了碰撞
            if (Physics.Raycast(ray, out hit))
            {
                if (!m_MoveCamera)
                {

                    //目标位置，
                    Vector3 targetPosition = hit.point - ray.direction * hit.distance * 0.2f;
                    targetPosition.y = basePosition.y;

                    targetObj.gameObject.SetActive(true);
                    //震动
                    AndroidUtils.vibrate(30);
                    //移动目标对象
                    targetObj.StartMove(m_Camera.transform.position, new Vector3(hit.point.x,basePosition.y, hit.point.z));

                    StartCoroutine(MoveCamera(m_ARCoreSession.gameObject.transform.position, targetPosition));
                }
            }

        }
        catch (System.Exception e)
        {
            EqLog.e("IKKYU",e.Message);
        }
    }

    private IEnumerator MoveCamera(Vector3 position, Vector3 targetPosition)
    {
        Debug.Log("Target Position: " + targetPosition);
        m_MoveCamera = true;
        yield return new WaitForSeconds(0.5f);
        //移动相机
        m_ARCoreSession.StartMove(position, targetPosition);
        m_MoveCamera = false;
    }

}
