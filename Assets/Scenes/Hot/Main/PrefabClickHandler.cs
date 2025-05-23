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
    internal ExSceneTransition mSceneTransition;
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
            mSceneTransition.LoadScene(sceneName,imageInfo);
        }
        catch (Exception e)
        {
            EqLog.e("PrefabClickHandler", e.Message);
        }
    }
}
