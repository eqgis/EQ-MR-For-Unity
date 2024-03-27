using Holo.XR.Android;
using Holo.XR.Core;
using Holo.XR.Detect;

public class MainDetectCallback : DetectCallback
{
    public JumpSceneController jumpSceneController;

    /// <summary>
    /// ͼƬʶ��ʱ����
    /// </summary>
    /// <param name="image"></param>
    public override void OnUpdate(ARImageInfo image)
    {
        string data = GetImageDataMatcher().Match(image.name);
        EqLog.i("DetectMethod", "image.name:" + image.name
            + "��image.position:" + image.transform.position 
            + "childcount: " + image.transform.childCount + "matched:" + data);

        //prefab��Ԥ�Ƽ��ǵ�һ����Ԫ��
        UnityEngine.GameObject prefab = image.GetPrefab();
        if (prefab != null)
        {
            //��û�е���¼�������������
            if (!prefab.GetComponent<PrefabClickHandler>())
            {
                //��ӵ���¼�
                PrefabClickHandler prefabClickHandler = prefab.AddComponent<PrefabClickHandler>();
                //�Ƚ�������ƥ�䣬�ٴ�������
                prefabClickHandler.sceneName = GetImageDataMatcher().Match(image.name);
                prefabClickHandler.jumpSceneController = jumpSceneController;
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
