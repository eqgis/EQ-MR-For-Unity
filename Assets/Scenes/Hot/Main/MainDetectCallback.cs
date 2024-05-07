using Holo.XR.Android;
using Holo.XR.Core;
using Holo.XR.Detect;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainDetectCallback : DetectCallback
{
    public ExSceneTransition jumpSceneController;
    public TipsUI tipsUI;

    /// <summary>
    /// ͼƬʶ��ʱ����
    /// </summary>
    /// <param name="image"></param>
    public override void OnUpdate(ARImageInfo image)
    {
        //string data = GetImageDataMatcher().Match(image.name);
        //EqLog.i("DetectMethod", "image.name:" + image.name
        //    + "��image.position:" + image.transform.position 
        //    + "childcount: " + image.transform.childCount + "matched:" + data);

        //prefab��Ԥ�Ƽ��ǵ�һ����Ԫ��
        UnityEngine.GameObject prefab = image.GetPrefab();
        if (prefab != null && GetImageDataMatcher() != null)
        {
            //��û�е���¼�������������
            if (!prefab.GetComponent<PrefabClickHandler>())
            {
                //��ӵ���¼�
                PrefabClickHandler prefabClickHandler = prefab.AddComponent<PrefabClickHandler>();
                //�Ƚ�������ƥ�䣬�ٴ�������
                prefabClickHandler.sceneName = GetImageDataMatcher().Match(image.name);
                prefabClickHandler.jumpSceneController = jumpSceneController;
                prefabClickHandler.imageInfo = image;

                //��������¼�ʱ��ͬʱ�����ʾ

                AndroidUtils.vibrate(30);
                //string path = "Scenes/Hot/Resources/Prefabs/��ʾ����.prefab";
                //GameObject tipsObj = Resources.Load("��ʾ����") as GameObject;
                //GameObject pUI_Node = GameObject.FindWithTag("UI_Node");
                //tipsObj.transform.parent = pUI_Node.transform;
                //TipsUI tipsUI = tipsObj.GetComponent<TipsUI>();
                tipsUI.ShowTips("����ģ�ͽ��볡��");
            }
        }

    }

    /// <summary>
    /// ����ʶ��ͼ��ʱ����
    /// </summary>
    /// <param name="image"></param>
    public override void OnAdded(ARImageInfo image)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    public void LoadCompleted()
    {
#if DEBUG_LOG
        AndroidUtils.Toast("ͼƬ���ݿ�������");
#endif
    }

}
