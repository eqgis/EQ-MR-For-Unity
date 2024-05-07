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
    /// 图片识别到时触发
    /// </summary>
    /// <param name="image"></param>
    public override void OnUpdate(ARImageInfo image)
    {
        //string data = GetImageDataMatcher().Match(image.name);
        //EqLog.i("DetectMethod", "image.name:" + image.name
        //    + "；image.position:" + image.transform.position 
        //    + "childcount: " + image.transform.childCount + "matched:" + data);

        //prefab的预制件是第一个子元素
        UnityEngine.GameObject prefab = image.GetPrefab();
        if (prefab != null && GetImageDataMatcher() != null)
        {
            //若没有点击事件的组件，则添加
            if (!prefab.GetComponent<PrefabClickHandler>())
            {
                //添加点击事件
                PrefabClickHandler prefabClickHandler = prefab.AddComponent<PrefabClickHandler>();
                //先进行数据匹配，再传入数据
                prefabClickHandler.sceneName = GetImageDataMatcher().Match(image.name);
                prefabClickHandler.jumpSceneController = jumpSceneController;
                prefabClickHandler.imageInfo = image;

                //初次添加事件时，同时添加提示

                AndroidUtils.vibrate(30);
                //string path = "Scenes/Hot/Resources/Prefabs/提示弹窗.prefab";
                //GameObject tipsObj = Resources.Load("提示弹窗") as GameObject;
                //GameObject pUI_Node = GameObject.FindWithTag("UI_Node");
                //tipsObj.transform.parent = pUI_Node.transform;
                //TipsUI tipsUI = tipsObj.GetComponent<TipsUI>();
                tipsUI.ShowTips("请点击模型进入场景");
            }
        }

    }

    /// <summary>
    /// 初次识别到图像时触发
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
        AndroidUtils.Toast("图片数据库加载完成");
#endif
    }

}
