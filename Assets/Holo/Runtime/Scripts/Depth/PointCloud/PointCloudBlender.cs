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
/// ���ƻ����
/// <!--ARCamera������Ҫ����DepthSource.cs��Computes a point cloud from the depth map on the CPU.-->
/// </summary>
public class PointCloudBlender : MonoBehaviour
{
    [Header("Mesh")]
    public MeshFilter meshFilter; // ����MeshFilter���

    /// <summary>
    /// �������ʵ����ͼ���ͣ��Ƿ����ARCore��Raw��������
    /// </summary>
    public bool UseRawDepth = true;

    /// <summary>
    ///���ƺ��豸���֮���ƽ�����������ǰ���ᡣ�������������Ļ����ʾ����ĵ�������
    ///ʱ�䣬����һ���������˳ơ������Ч����
    /// </summary>
    [Tooltip("�����ǰ��ƫ�ƾ��룬��λ����")]
    public float OffsetFromCamera = 1.0f;

    /// <summary>
    /// ���Ƶ����Ŷ�
    /// </summary>
    [Header("Confidence Threshold")]
    [Tooltip("���Ŷ���ֵ,ȡֵ����[0,1]")]
    public float confidenceValue = 0.6f;
    [Tooltip("��������ֵ")]
    public float distanceThreshold = 5.0f;

    /// <summary>
    /// ������������
    /// </summary>
    [Header("Point Count"),Tooltip("���Ƶĵ���������")]
    public int maxCount = 1000000;

    private const double _maxUpdateInvervalInSeconds = 0.5f;
    private const double _minUpdateInvervalInSeconds = 0.07f;
    
    //��Ӧshader�����ԣ������޸�
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

    // ÿһ֡���ݴ��YUV������Ϣ
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
        //��ʼ��
        _vertices = new Vector3[maxCount];
        _verticesCount = 0;
        _verticesIndex = 0;
        _indices = new int[maxCount];
        _colors = new Color32[maxCount];
    }

    /// <summary>
    /// �������Ŷ�
    /// </summary>
    /// <param name="value"></param>
    public void SetConfidence(float value)
    {
        //������ֵ
        confidenceValue = Math.Clamp(value, 0.0f, 1.0f);
    }

    /// <summary>
    /// ���õ�����Ⱦ������
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
            //��ȡ��Ⱦ����
            _pointCloudMaterial = GetComponent<Renderer>().material;
            _pointCloudMaterial.SetFloat(_confidenceThresholdPropertyName, confidenceValue);
        }catch (Exception ex)
        {
            EqLog.e("PointCloudBlender", "The Material of PointCloud was wrong.");
#if DEBUG_LOG
            //��������£���Toast������ʾ
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

        // ���Ƶ�_indices�����ڲ��õ����ͣ����indices���ñ���������������Ϊ��������
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
            // �����API��ʼ�����֮ǰ���ȴ�
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
    /// ��ǰ���֡����ʱ����
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
    /// ���µ�CPUӳ��ת��Ϊ�Ժ�Ҫ���ʵ��ֽڻ������ͻ��档
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
    /// �����ͼ�м���3D���㣬��ʹ��Point���͸���Mesh��
    /// </summary>
    private void UpdateRawPointCloud()
    {
        // ARCoreδ����
        if (!_initialized || _cameraBufferY == null)
        {
            return;
        }

        // ʱ����̫�̣���������
        if (Time.realtimeSinceStartup - _lastUpdateTimeSeconds < _updateInvervalInSeconds)
        {
            return;
        }

        _pointCloudMaterial.SetFloat(_confidenceThresholdPropertyName, confidenceValue);

        //��ɫ�����ͼ��ͨ���в�ͬ�ĳ���ȡ����ͼ���Ӧ��
        //�����ͼ������Ĳü�����ȿ�߱ȵ�����
        float depthAspectRatio = (float)DepthSource.DepthHeight / DepthSource.DepthWidth;
        int colorHeightDepthAspectRatio = (int)(_cameraWidth * depthAspectRatio);
        int colorHeightOffset = (_cameraHeight - colorHeightDepthAspectRatio) / 2;

        short[] depthArray = DepthSource.DepthArray;
        if (depthArray.Length != DepthSource.DepthWidth * DepthSource.DepthHeight)
        {
            //������ݻ�������
            return;
        }

        byte[] confidenceArray = DepthSource.ConfidenceArray;
        bool noConfidenceAvailable = depthArray.Length != confidenceArray.Length;

        // �����ͼ�д�������
        StringBuilder stringBuilder = new StringBuilder();
        for (int y = 0; y < DepthSource.DepthHeight; y++)
        {
            for (int x = 0; x < DepthSource.DepthWidth; x++)
            {
                int depthIndex = (y * DepthSource.DepthWidth) + x;
                float depthInM = depthArray[depthIndex] * DepthSource.MillimeterToMeter;
                float confidence = noConfidenceAvailable ? 1f : confidenceArray[depthIndex] / 255f;

                // �������ֵΪ0�������Ŷ�Ϊ0�����
                if (depthInM == 0f || confidence == 0f)
                {
                    continue;
                }

                // �������ֵΪ0�������Ŷ�С��< ��ֵ�����
                if (depthInM > distanceThreshold || confidence < confidenceValue)
                {
                    continue;
                }

                // ����ռ����꣬��Ļ���� + ���ֵ => �ռ�����
                Vector3 vertex = DepthSource.TransformVertexToWorldSpace(
                    DepthSource.ComputeVertex(x, y, depthInM));

                //���㵱ǰ��Ļ�����Ӧ������yuvֵ
                int colorX = x * _cameraWidth / DepthSource.DepthWidth;
                int colorY = colorHeightOffset +
                    (y * colorHeightDepthAspectRatio / DepthSource.DepthHeight);
                int linearIndexY = (colorY * _rowStrideY) + colorX;
                int linearIndexUV = ((colorY / 2) * _rowStrideUV) + ((colorX / 2) * _pixelStrideUV);

                // ÿ��ͨ����ֵ����һ���޷��ŵ�byte
                byte channelValueY = _cameraBufferY[linearIndexY];
                byte channelValueU = _cameraBufferU[linearIndexUV];
                byte channelValueV = _cameraBufferV[linearIndexUV];

                //����RGBֵ yuv -> rgb
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

                // �ﵽ��󶥵���ʱ�����µĶ��������滻�ɶ�������
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
            // �����ݴӻ�����д���ļ�
            //writer.Write(stringBuilder.ToString());
            writer.Flush(); // ȷ�����ݱ�д�뵽�ļ���
        }
        else
        {
            if (writer != null)
            {
                //writer.Write(stringBuilder.ToString());
                writer.Flush(); // ȷ�����ݱ�д�뵽�ļ���
                writer.Close();
                writer = null;
            }
        }

        if (_verticesCount == 0)
        {
            return;
        }

        // ����Mesh����
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


    //���Ʋɼ���ʵʱ����·��
    private string currentDataPath;
    private StreamWriter writer;

    public void StartCollect()
    {

#if DEBUG_LOG
        AndroidUtils.Toast("StartCollect...");
#endif
        // ��ȡ��ǰʱ��
        DateTime currentTime = DateTime.Now;

        // ��ʽ��ʱ���ַ�������Ϊ�ļ�����һ����
        string fileName = "Data_" + currentTime.ToString("yyyy_MM_dd_HH_mm_ss") + ".pts";

        string persistentDataPath = Application.persistentDataPath;
        currentDataPath = Path.Combine(persistentDataPath, "PointCloud", fileName);
        // ���������������ݵ��ļ���
        Directory.CreateDirectory(Path.GetDirectoryName(currentDataPath));

        writer = new StreamWriter(currentDataPath,/*׷��*/true);

    }

    public void StopCollect()
    {
        currentDataPath = null;

#if DEBUG_LOG
        AndroidUtils.Toast("StopCollect...");
#endif
    }

    /// <summary>
    /// �����������
    /// </summary>
    public void SavePointCloud()
    {
#if DEBUG_LOG
        AndroidUtils.Toast("saving...");
#endif
        // ��ȡ��ǰʱ��
        DateTime currentTime = DateTime.Now;

        // ��ʽ��ʱ���ַ�������Ϊ�ļ�����һ����
        string fileName = "Data_" + currentTime.ToString("yyyy_MM_dd_HH_mm_ss") + ".pts";

        string persistentDataPath = Application.persistentDataPath;
        string filePath = Path.Combine(persistentDataPath, "PointCloud", fileName);
        // ���������������ݵ��ļ�
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


        //// ��ȡ��������
        Vector3[] vertices = mesh.vertices;
        ////int[] triangles = mesh.triangles;//�����������Ƕ���������
        Color[] colors = mesh.colors;

        int minCount = Math.Min(vertices.Length, colors.Length);

        StringBuilder stringBuilder = new StringBuilder();
        // д�붥������
        for (int i = 0; i < minCount; i++)
        {
            //PTS��ʽǰ������ ��x��y��z�� ���� ���У����ĸ��ǡ�ǿ�ȡ�ֵ����������ǡ���ɫֵ����R��G��B�� 
            Vector3 vertex = vertices[i];
            Color c = colors[i];
            //��������ALPHA��ʾǿ��ֵ
            //writer.WriteLine(vertex.x + " " + vertex.y + " " + vertex.z
            //    + " " + (int)(c.a * 255) + " " + (int)(c.r * 255) + " " + (int)(c.g * 255) + " " + (int)(c.b * 255));
            stringBuilder.AppendLine(vertex.x + " " + vertex.y + " " + vertex.z
                + " " + (int)(c.a * 255) + " " + (int)(c.r * 255) + " " + (int)(c.g * 255) + " " + (int)(c.b * 255));
        }


        // ���������������ݵ��ļ�
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        using (StreamWriter wr = new StreamWriter(filePath, false))
        {
            wr.Write(stringBuilder.ToString());
            wr.Flush();
        }

        //using (StreamWriter writer = new StreamWriter(filePath))
        //{
        //    //// д�붥������
        //    //foreach (Vector3 vertex in vertices)
        //    //{
        //    //    writer.WriteLine("v " + vertex.x + " " + vertex.y + " " + vertex.z);
        //    //}

        //    //// ��ɫ����
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