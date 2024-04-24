using Holo.XR.Android;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class CameraController : MonoBehaviour
{
    public Transform m_RefTransform;
    public Camera m_Camera;

    public ShiftScript targetObj;

    public ShiftScript m_ARCoreSession;
    private bool m_MoveCamera = false;

    private Canvas mUI_Canvas;
    
    //��׼λ��
    private Vector3 basePosition;

    void Start()
    {
        targetObj.OnFinish += OnTargetFinish;
        basePosition = m_ARCoreSession.gameObject.transform.position;
    }

    private void OnTargetFinish()
    {
        //��Ŀ������ƶ���ɺ�ִ��
        targetObj.gameObject.SetActive(false);
    }

    void Update()
    {
        try
        {

            Touch touch;
            if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began || CheckGuiRaycastObjects())
            {
                return;
            }

            // ��������
            Ray ray = m_Camera.ScreenPointToRay(touch.position);
            RaycastHit hit;

            // ��������Ƿ������巢������ײ
            if (Physics.Raycast(ray, out hit))
            {
                if (!m_MoveCamera)
                {

                    //Ŀ��λ�ã�
                    Vector3 targetPosition = hit.point - ray.direction * hit.distance * 0.2f;
                    targetPosition.y = basePosition.y;

                    targetObj.gameObject.SetActive(true);
                    //��
                    AndroidUtils.vibrate(30);
                    //�ƶ�Ŀ�����
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
        //�ƶ����
        m_ARCoreSession.StartMove(position, targetPosition);
        m_MoveCamera = false;
    }


    bool CheckGuiRaycastObjects()
    {
        if (!mUI_Canvas)
        {
            return false;
        }

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.pressPosition = Input.mousePosition;//touch.position
        eventData.position = Input.mousePosition;//touch.position

        var results = new List<RaycastResult>();
        mUI_Canvas.GetComponent<GraphicRaycaster>().Raycast(eventData, results);
        //EventSystem.current.RaycastAll(eventData, results);   //ʹ�ô˷�ʽҲ��
        return results.Count > 0;
    }
}
