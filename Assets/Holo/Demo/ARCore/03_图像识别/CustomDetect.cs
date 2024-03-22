using Holo.XR.Android;
using Holo.XR.Detect;

public class CustomDetect : DetectCallback
{
    /// <summary>
    /// ��д����������������ʶ��ͼ���ľ���ʵ��
    /// </summary>
    /// <param name="image"></param>
    public override void OnUpdate(ARImageInfo image)
    {
        EqLog.i("DetectMethod", "image.name:" + image.name
            + "��image.position:" + image.transform.position);
    }

    public override void OnAdded(ARImageInfo image)
    {
        EqLog.i("DetectMethod", "image.name:" + image.name
            + "��image.position:" + image.transform.position);
        AndroidUtils.Toast("image.name:" + image.name
            + "��image.position:" + image.transform.position);
    }

    public void LoadCompleted()
    {
        AndroidUtils.Toast("ͼƬ���ݿ�������");
    }
}
