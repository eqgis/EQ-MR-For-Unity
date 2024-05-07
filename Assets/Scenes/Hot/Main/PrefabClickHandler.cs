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
    internal ExSceneTransition jumpSceneController;
    internal ARImageInfo imageInfo;

    public void OnPointerClick(PointerEventData eventData)
    {
        //��
        AndroidUtils.vibrate(50);


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
            //���³����л���������transform���������ͼƬ��transform��
            //��ô�ڳ����л�������һ������ԭ������imageΪԭ�㡣
            //����Ҫ��NodePoseRecorder���ʹ�ã�ע�⣺�������У������³������´�����ARSession,��������Ҫ��ȥ֮ǰ�����λ��
            //��֪���ڵ�ǰ�����У��������½�ARSession
            //��Ҫ�ر�ע����ǣ�����ʶ���ͼƬ��widthһ��Ҫ��ARCoreImageDetector���õĳߴ�һ��
            jumpSceneController.transform.position = imageInfo.transform.position;

            Vector3 imageAngles = imageInfo.transform.eulerAngles;
            
            //���������Y�᷽�����ת�Ƕ�
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
