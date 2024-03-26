using Holo.XR.Android;
using Holo.XR.Detect;

public class CustomDetect : DetectCallback
{
    /// <summary>
    /// 重写这个方法，这里包含识别到图像后的具体实现
    /// </summary>
    /// <param name="image"></param>
    public override void OnUpdate(ARImageInfo image)
    {
        string data = GetImageDataMatcher().Match(image.name);
        EqLog.i("DetectMethod", "image.name:" + image.name
            + "；image.position:" + image.transform.position + "matched:" + data);
    }

    public override void OnAdded(ARImageInfo image)
    {
        string data = GetImageDataMatcher().Match(image.name); 
        EqLog.i("DetectMethod", "image.name:" + image.name
            + "；image.position:" + image.transform.position + "matched:"+ data);
        AndroidUtils.Toast("image.name:" + image.name
            + "；image.position:" + image.transform.position + "matched:" + data);

        
    }

    public void LoadCompleted()
    {
        AndroidUtils.Toast("图片数据库加载完成");
    }
}
