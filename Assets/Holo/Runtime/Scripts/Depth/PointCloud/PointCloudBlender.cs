#if ENGINE_ARCORE

using Holo.XR.Android;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// 点云混合器
/// <!--ARCamera对象需要挂载DepthSource.cs，Computes a point cloud from the depth map on the CPU.-->
/// </summary>
public class PointCloudBlender : MonoBehaviour
{
    [Header("Mesh")]
    public MeshFilter meshFilter; // 引用MeshFilter组件

    /// <summary>
    /// 传给材质的深度图类型，是否采用ARCore的Raw类型数据
    /// </summary>
    public bool UseRawDepth = true;

    /// <summary>
    ///点云和设备相机之间的平移沿着相机的前进轴。这可以用来在屏幕上显示更大的点云区域
    ///时间，创建一个“第三人称”的相机效果。
    /// </summary>
    [Tooltip("相机向前的偏移距离，单位：米")]
    public float OffsetFromCamera = 1.0f;

    /// <summary>
    /// 点云的置信度
    /// </summary>
    [Header("Confidence Threshold")]
    [Tooltip("置信度阈值,取值区间[0,1]")]
    public float confidenceValue = 0.6f;
    [Tooltip("最大距离阈值")]
    public float distanceThreshold = 5.0f;

    /// <summary>
    /// 点云数量控制
    /// </summary>
    [Header("Point Count"),Tooltip("点云的点数量上限")]
    public int maxCount = 1000000;

    private const double _maxUpdateInvervalInSeconds = 0.5f;
    private const double _minUpdateInvervalInSeconds = 0.07f;
    
    //对应shader中属性，不可修改
    private static readonly string _confidenceThresholdPropertyName = "_ConfidenceThreshold";
    private bool _initialized;
    private ARCameraManager _cameraManager;
    private XRCameraIntrinsics _cameraIntrinsics;
    private Mesh _mesh;

    private Vector3[] _vertices;
    private int _verticesCount = 0;
    private int _verticesIndex = 0;
    private int[] _indices;
    private Color32[] _colors;

    // 每一帧数据存的YUV数据信息
    private byte[] _cameraBufferY;
    private byte[] _cameraBufferU;
    private byte[] _cameraBufferV;
    private int _cameraHeight;
    private int _cameraWidth;
    private int _pixelStrideUV;
    private int _rowStrideY;
    private int _rowStrideUV;
    private double _updateInvervalInSeconds = _minUpdateInvervalInSeconds;
    private double _lastUpdateTimeSeconds;
    private Material _pointCloudMaterial;
    private bool _cachedUseRawDepth = false;

    private void Awake()
    {
        //初始化
        _vertices = new Vector3[maxCount];
        _verticesCount = 0;
        _verticesIndex = 0;
        _indices = new int[maxCount];
        _colors = new Color32[maxCount];
    }

    /// <summary>
    /// 设置置信度
    /// </summary>
    /// <param name="value"></param>
    public void SetConfidence(float value)
    {
        //更新阈值
        confidenceValue = Math.Clamp(value, 0.0f, 1.0f);
    }

    /// <summary>
    /// 重置点云渲染的内容
    /// </summary>
    public void Reset()
    {
        _verticesCount = 0;
        _verticesIndex = 0;
    }

    private void Start()
    {
        _mesh = new Mesh();
        _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        
        try
        {
            //获取渲染材质
            _pointCloudMaterial = GetComponent<Renderer>().material;
            _pointCloudMaterial.SetFloat(_confidenceThresholdPropertyName, confidenceValue);
        }catch (Exception ex)
        {
            EqLog.e("PointCloudBlender", "The Material of PointCloud was wrong.");
#if DEBUG_LOG
            //调试情况下，用Toast进行提示
            AndroidUtils.Toast("The Material of PointCloud was wrong.");
#endif
            return;
        }

        if (meshFilter == null)
        {
            meshFilter = GetComponent<MeshFilter>();
        }

        _cameraManager = FindObjectOfType<ARCameraManager>();
        _cameraManager.frameReceived += OnCameraFrameReceived;

        // 点云的_indices，由于采用点类型，因此indices采用本身自增的数字作为顶点索引
        for (int i = 0; i < maxCount; ++i)
        {
            _indices[i] = i;
        }

        Reset();

    }

    private void Update()
    {
        try
        {
            // 在深度API初始化完成之前，等待
            if (!_initialized && DepthSource.Initialized)
            {
                _initialized = true;
            }

            if (_initialized)
            {
                if (_cachedUseRawDepth != UseRawDepth)
                {
                    DepthSource.SwitchToRawDepth(UseRawDepth);
                    _cachedUseRawDepth = UseRawDepth;
                }
                UpdateRawPointCloud();
            }

            transform.position = DepthSource.ARCamera.transform.forward * OffsetFromCamera;
            float normalizedDeltaTime = Mathf.Clamp01(
                (float)(Time.deltaTime - _minUpdateInvervalInSeconds));
            _updateInvervalInSeconds = Mathf.Lerp((float)_minUpdateInvervalInSeconds,
                (float)_maxUpdateInvervalInSeconds,
                normalizedDeltaTime);
        }
        catch (Exception e)
        {
            EqLog.e("PointCloudBlender", e.Message);
        }
    }

    /// <summary>
    /// 当前相机帧更新时触发
    /// </summary>
    /// <param name="eventArgs"></param>
    void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    {
        if (_cameraManager.TryAcquireLatestCpuImage(out XRCpuImage cameraImage))
        {
            using (cameraImage)
            {
                if (cameraImage.format == XRCpuImage.Format.AndroidYuv420_888)
                {
                    OnImageAvailable(cameraImage);
                }
            }
        }
    }

    /// <summary>
    /// 将新的CPU映像转换为稍后要访问的字节缓冲区和缓存。
    /// </summary>
    /// <param name="image">The new CPU image to process.</param>
    private void OnImageAvailable(XRCpuImage image)
    {
        if (_cameraBufferY == null || _cameraBufferU == null || _cameraBufferV == null)
        {
            _cameraWidth = image.width;
            _cameraHeight = image.height;
            _rowStrideY = image.GetPlane(0).rowStride;
            _rowStrideUV = image.GetPlane(1).rowStride;
            _pixelStrideUV = image.GetPlane(1).pixelStride;
            _cameraBufferY = new byte[image.GetPlane(0).data.Length];
            _cameraBufferU = new byte[image.GetPlane(1).data.Length];
            _cameraBufferV = new byte[image.GetPlane(2).data.Length];
        }

        image.GetPlane(0).data.CopyTo(_cameraBufferY);
        image.GetPlane(1).data.CopyTo(_cameraBufferU);
        image.GetPlane(2).data.CopyTo(_cameraBufferV);
    }

    /// <summary>
    /// 从深度图中计算3D顶点，并使用Point类型更新Mesh。
    /// </summary>
    private void UpdateRawPointCloud()
    {
        // ARCore未就绪
        if (!_initialized || _cameraBufferY == null)
        {
            return;
        }

        // 时间间隔太短，则先跳过
        if (Time.realtimeSinceStartup - _lastUpdateTimeSeconds < _updateInvervalInSeconds)
        {
            return;
        }

        _pointCloudMaterial.SetFloat(_confidenceThresholdPropertyName, confidenceValue);

        //彩色和深度图像通常有不同的长宽比。深度图像对应于
        //到相机图像的中心裁剪到深度宽高比的区域。
        float depthAspectRatio = (float)DepthSource.DepthHeight / DepthSource.DepthWidth;
        int colorHeightDepthAspectRatio = (int)(_cameraWidth * depthAspectRatio);
        int colorHeightOffset = (_cameraHeight - colorHeightDepthAspectRatio) / 2;

        short[] depthArray = DepthSource.DepthArray;
        if (depthArray.Length != DepthSource.DepthWidth * DepthSource.DepthHeight)
        {
            //深度数据还不可用
            return;
        }

        byte[] confidenceArray = DepthSource.ConfidenceArray;
        bool noConfidenceAvailable = depthArray.Length != confidenceArray.Length;

        // 从深度图中创建点云
        StringBuilder stringBuilder = new StringBuilder();
        for (int y = 0; y < DepthSource.DepthHeight; y++)
        {
            for (int x = 0; x < DepthSource.DepthWidth; x++)
            {
                int depthIndex = (y * DepthSource.DepthWidth) + x;
                float depthInM = depthArray[depthIndex] * DepthSource.MillimeterToMeter;
                float confidence = noConfidenceAvailable ? 1f : confidenceArray[depthIndex] / 255f;

                // 忽略深度值为0，和置信度为0的情况
                if (depthInM == 0f || confidence == 0f)
                {
                    continue;
                }

                // 忽略深度值为0，和置信度小于< 阈值的情况
                if (depthInM > distanceThreshold || confidence < confidenceValue)
                {
                    continue;
                }

                // 计算空间坐标，屏幕坐标 + 深度值 => 空间坐标
                Vector3 vertex = DepthSource.TransformVertexToWorldSpace(
                    DepthSource.ComputeVertex(x, y, depthInM));

                //计算当前屏幕坐标对应的像素yuv值
                int colorX = x * _cameraWidth / DepthSource.DepthWidth;
                int colorY = colorHeightOffset +
                    (y * colorHeightDepthAspectRatio / DepthSource.DepthHeight);
                int linearIndexY = (colorY * _rowStrideY) + colorX;
                int linearIndexUV = ((colorY / 2) * _rowStrideUV) + ((colorX / 2) * _pixelStrideUV);

                // 每个通道的值都是一个无符号的byte
                byte channelValueY = _cameraBufferY[linearIndexY];
                byte channelValueU = _cameraBufferU[linearIndexUV];
                byte channelValueV = _cameraBufferV[linearIndexUV];

                //计算RGB值 yuv -> rgb
                byte[] rgb = ConvertYuvToRgb(channelValueY, channelValueU, channelValueV);
                byte confidenceByte = (byte)(confidence * 255f);
                Color32 color = new Color32(rgb[0], rgb[1], rgb[2], confidenceByte);

                //save
                if(writer != null)
                {
                    // stringBuilder.AppendLine(vertex.x + " " + vertex.y + " " + vertex.z
                    //+ " " + color.a + " " + color.r + " " + color.g + " " + color.b);
                    writer.WriteLine(vertex.x + " " + vertex.y + " " + vertex.z
                        + " " + color.a + " " + color.r + " " + color.g + " " + color.b);
                }
                //SaveCurrentPoint(vertex, color);

                if (_verticesCount < maxCount - 1)
                {
                    ++_verticesCount;
                }

                // 达到最大顶点数时，最新的顶点数据替换旧顶点数据
                if (_verticesIndex >= maxCount)
                {
                    _verticesIndex = 0;
                }

                _vertices[_verticesIndex] = vertex;
                _colors[_verticesIndex] = color;
                ++_verticesIndex;
            }
        }

        if (currentDataPath != null)
        {
            // 将数据从缓冲区写入文件
            //writer.Write(stringBuilder.ToString());
            writer.Flush(); // 确保数据被写入到文件中
        }
        else
        {
            if (writer != null)
            {
                //writer.Write(stringBuilder.ToString());
                writer.Flush(); // 确保数据被写入到文件中
                writer.Close();
                writer = null;
            }
        }

        if (_verticesCount == 0)
        {
            return;
        }

        // 更新Mesh参数
#if UNITY_2019_3_OR_NEWER
        _mesh.SetVertices(_vertices, 0, _verticesCount);
        _mesh.SetIndices(_indices, 0, _verticesCount, MeshTopology.Points, 0);
        _mesh.SetColors(_colors, 0, _verticesCount);
#else
        // Note that we recommend using Unity 2019.3 or above to compile this scene.
        List<Vector3> vertexList = new List<Vector3>();
        List<Color32> colorList = new List<Color32>();
        List<int> indexList = new List<int>();

        for (int i = 0; i < _verticesCount; ++i)
        {
            vertexList.Add(_vertices[i]);
            indexList.Add(_indices[i]);
            colorList.Add(_colors[i]);
        }

        _mesh.SetVertices(vertexList);
        _mesh.SetIndices(indexList.ToArray(), MeshTopology.Points, 0);
        _mesh.SetColors(colorList);
#endif // UNITY_2019_3_OR_NEWER

        meshFilter.mesh = _mesh;
        _lastUpdateTimeSeconds = Time.realtimeSinceStartup;
    }

    /// <summary>
    /// Converts a YUV color value into RGB. Input YUV values are expected in the range [0, 255].
    /// </summary>
    /// <param name="y">The pixel value of the Y plane in the range [0, 255].</param>
    /// <param name="u">The pixel value of the U plane in the range [0, 255].</param>
    /// <param name="v">The pixel value of the V plane in the range [0, 255].</param>
    /// <returns>RGB values are in the range [0.0, 1.0].</returns>
    private byte[] ConvertYuvToRgb(byte y, byte u, byte v)
    {
        // See https://en.wikipedia.org/wiki/YUV.
        float yFloat = y / 255.0f; // Range [0.0, 1.0].
        float uFloat = (u * 0.872f / 255.0f) - 0.436f; // Range [-0.436, 0.436].
        float vFloat = (v * 1.230f / 255.0f) - 0.615f; // Range [-0.615, 0.615].
        float rFloat = Mathf.Clamp01(yFloat + (1.13983f * vFloat));
        float gFloat = Mathf.Clamp01(yFloat - (0.39465f * uFloat) - (0.58060f * vFloat));
        float bFloat = Mathf.Clamp01(yFloat + (2.03211f * uFloat));
        byte r = (byte)(rFloat * 255f);
        byte g = (byte)(gFloat * 255f);
        byte b = (byte)(bFloat * 255f);
        return new[] { r, g, b };
    }


    //点云采集的实时保存路径
    private string currentDataPath;
    private StreamWriter writer;

    public void StartCollect()
    {

#if DEBUG_LOG
        AndroidUtils.Toast("StartCollect...");
#endif
        // 获取当前时间
        DateTime currentTime = DateTime.Now;

        // 格式化时间字符串，作为文件名的一部分
        string fileName = "Data_" + currentTime.ToString("yyyy_MM_dd_HH_mm_ss") + ".pts";

        string persistentDataPath = Application.persistentDataPath;
        currentDataPath = Path.Combine(persistentDataPath, "PointCloud", fileName);
        // 创建保存网格数据的文件夹
        Directory.CreateDirectory(Path.GetDirectoryName(currentDataPath));

        writer = new StreamWriter(currentDataPath,/*追加*/true);

    }

    public void StopCollect()
    {
        currentDataPath = null;

#if DEBUG_LOG
        AndroidUtils.Toast("StopCollect...");
#endif
    }

    /// <summary>
    /// 保存点云数据
    /// </summary>
    public void SavePointCloud()
    {
#if DEBUG_LOG
        AndroidUtils.Toast("saving...");
#endif
        // 获取当前时间
        DateTime currentTime = DateTime.Now;

        // 格式化时间字符串，作为文件名的一部分
        string fileName = "Data_" + currentTime.ToString("yyyy_MM_dd_HH_mm_ss") + ".pts";

        string persistentDataPath = Application.persistentDataPath;
        string filePath = Path.Combine(persistentDataPath, "PointCloud", fileName);
        // 创建保存网格数据的文件
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

        SavePointCloud(filePath);

    }


    private void SavePointCloud(string filePath)
    {
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
#if DEBUG_LOG
            EqLog.e("PointCloudBlender", "MeshFilter or Mesh not assigned.");
            AndroidUtils.Toast("MeshFilter or Mesh not assigned!");
#endif
        }

        Mesh mesh = meshFilter.sharedMesh;


        //// 获取网格数据
        Vector3[] vertices = mesh.vertices;
        ////int[] triangles = mesh.triangles;//点云无需三角顶点索引，
        Color[] colors = mesh.colors;

        int minCount = Math.Min(vertices.Length, colors.Length);

        StringBuilder stringBuilder = new StringBuilder();
        // 写入顶点数据
        for (int i = 0; i < minCount; i++)
        {
            //PTS格式前三个是 （x，y，z） 坐标 其中，第四个是“强度”值，最后三个是“颜色值”（R，G，B） 
            Vector3 vertex = vertices[i];
            Color c = colors[i];
            //这里暂用ALPHA表示强度值
            //writer.WriteLine(vertex.x + " " + vertex.y + " " + vertex.z
            //    + " " + (int)(c.a * 255) + " " + (int)(c.r * 255) + " " + (int)(c.g * 255) + " " + (int)(c.b * 255));
            stringBuilder.AppendLine(vertex.x + " " + vertex.y + " " + vertex.z
                + " " + (int)(c.a * 255) + " " + (int)(c.r * 255) + " " + (int)(c.g * 255) + " " + (int)(c.b * 255));
        }


        // 创建保存网格数据的文件
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        using (StreamWriter wr = new StreamWriter(filePath, false))
        {
            wr.Write(stringBuilder.ToString());
            wr.Flush();
        }

        //using (StreamWriter writer = new StreamWriter(filePath))
        //{
        //    //// 写入顶点数据
        //    //foreach (Vector3 vertex in vertices)
        //    //{
        //    //    writer.WriteLine("v " + vertex.x + " " + vertex.y + " " + vertex.z);
        //    //}

        //    //// 颜色数据
        //    //for (int i = 0; i < colors.Length; i++)
        //    //{
        //    //    Color c = colors[i];
        //    //    writer.WriteLine("c " + c.r + " " + c.g + " " + c.b + " " + c.a);
        //    //}
        //}
#if DEBUG_LOG
        EqLog.d("PointCloudBlender", "Mesh saved to " + filePath);
        AndroidUtils.Toast("success!");
#endif
    }
}

#endif