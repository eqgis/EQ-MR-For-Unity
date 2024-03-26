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
        string data = GetImageDataMatcher().Match(image.name);
        EqLog.i("DetectMethod", "image.name:" + image.name
            + "��image.position:" + image.transform.position + "matched:" + data);
    }

    public override void OnAdded(ARImageInfo image)
    {
        string data = GetImageDataMatcher().Match(image.name); 
        EqLog.i("DetectMethod", "image.name:" + image.name
            + "��image.position:" + image.transform.position + "matched:"+ data);
        AndroidUtils.Toast("image.name:" + image.name
            + "��image.position:" + image.transform.position + "matched:" + data);

        
    }

    public void LoadCompleted()
    {
        AndroidUtils.Toast("ͼƬ���ݿ�������");
    }
}
