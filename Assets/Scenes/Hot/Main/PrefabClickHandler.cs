using Holo.XR.Android;
using Holo.XR.Core;
using Holo.XR.Detect;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PrefabClickHandler : MonoBehaviour,IPointerClickHandler
{
    //场景名称
    public string sceneName;


    [HideInInspector]
    internal ExSceneTransition jumpSceneController;
    internal ARImageInfo imageInfo;

    public void OnPointerClick(PointerEventData eventData)
    {
        //震动
        AndroidUtils.vibrate(50);


#if DEBUG_LOG
        AndroidUtils.Toast("载入场景：" + sceneName);
#endif
        //场景跳转
        Invoke("ToNewScene", 0.5F);
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// 跳转至新场景
    /// </summary>
    public void ToNewScene()
    {
        try
        {
            //更新场景切换控制器的transform，这里采用图片的transform。
            //那么在场景切换后，则下一场景的原点则以image为原点。
            //（需要与NodePoseRecorder结合使用）注意：在流程中，若是新场景重新创建了ARSession,则这里需要减去之前的相机位姿
            //已知：在当前流程中，并不会新建ARSession
            //需要特别注意的是：用于识别的图片的width一定要与ARCoreImageDetector设置的尺寸一样
            jumpSceneController.transform.position = imageInfo.transform.position;

            Vector3 imageAngles = imageInfo.transform.eulerAngles;
            
            //这里仅保留Y轴方向的旋转角度
            jumpSceneController.transform.eulerAngles = new Vector3 (0, imageAngles.y, 0);  

            //jumpSceneController.transform.position = imageInfo.transform.position;
            //jumpSceneController.transform.eulerAngles = imageInfo.transform.eulerAngles;

            jumpSceneController.LoadScene(sceneName);
        }
        catch (Exception e)
        {
            EqLog.e("PrefabClickHandler", e.Message);
        }
    }
}
