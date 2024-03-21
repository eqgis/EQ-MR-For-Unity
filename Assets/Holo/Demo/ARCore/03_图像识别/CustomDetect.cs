using Holo.XR.Android;
using Holo.XR.Detect;

public class CustomDetect : DetectCallback
{
    /// <summary>
    /// ��д����������������ʶ��ͼ���ľ���ʵ��
    /// </summary>
    /// <param name="image"></param>
    public override void OnDetect(ARImageInfo image)
    {
        EqLog.i("DetectMethod", "image.name:" + image.name
            + "��image.position:" + image.transform.position
            + "exif-key��"+image.getExif("key"));
    }

    public void LoadCompleted()
    {
        AndroidUtils.Toast("ͼƬ���ݿ�������");
    }
}
