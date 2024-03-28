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
    //��������
    public string sceneName;


    [HideInInspector]
    public ExSceneTransition jumpSceneController;

    public void OnPointerClick(PointerEventData eventData)
    {


#if DEBUG_LOG
        AndroidUtils.Toast("���볡����" + sceneName);
#endif
        //������ת
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
    /// ��ת���³���
    /// </summary>
    public void ToNewScene()
    {
        try
        {
            //SceneManager.LoadScene(sceneName);
            jumpSceneController.LoadScene(sceneName);
        }
        catch (Exception e)
        {
            EqLog.e("PrefabClickHandler", e.Message);
        }
    }
}
