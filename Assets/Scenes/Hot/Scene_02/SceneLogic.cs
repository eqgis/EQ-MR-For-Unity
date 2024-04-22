#if ENGINE_ARCORE
using Holo.XR.Android;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class SceneLogic : MonoBehaviour
{
    public GameObject sceneNode;

    public ARPlaneManager planeManager;

    public ARRaycastManager raycastManager;

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private bool isInit = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(isInit) { return; }
        Touch touch;
        if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
        {
            return;
        }

#if DEBUG_LOG
        Debug.Log("IKKYU-TOUCH");
#endif
        hits.Clear();
        //射线检测，类型：无限平面
        if(raycastManager.Raycast(touch.position, hits,TrackableType.PlaneWithinPolygon))
        {
            if (hits.Count > 0)
            {
                //获取最近的，射线检测结果按距离排序
                ARRaycastHit aRRaycastHit = hits[0];
                Pose pose = aRRaycastHit.pose;
#if DEBUG_LOG
                AndroidUtils.Toast("Pose:" + pose.ToString());
#endif
                sceneNode.transform.position = pose.position;
                sceneNode.transform.rotation = pose.rotation;

                if (!sceneNode.activeSelf)
                {
                    sceneNode.SetActive(true);
                }

                //planeManager.requestedDetectionMode = PlaneDetectionMode.None;
                //isInit = true;
            }

        }
    }
}

#endif