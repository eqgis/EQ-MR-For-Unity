using Holo.XR.Editor.UX;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class PointCloudWindow : EditorWindow
{
    private string ptsFilePath;
    private static Material material;

    [MenuItem("Tool/���ص���/*.pts")]
    public static void ShowWindow()
    {
        material = LoadPointCloudMaterial();
        //EditorWindow.GetWindow(typeof(PointCloudWindow));
        Rect windowRect = new Rect(100, 100, 300, 180);
        PointCloudWindow win = EditorWindow.GetWindowWithRect<PointCloudWindow>(windowRect, false, "����-����");
        //SettingsWindow win = EditorWindow.GetWindow<SettingsWindow>(false, "Holo-Settings");
        //win.titleContent = new GUIContent("ȫ������");
        win.Show();
    }

    private void OnGUI()
    {

        GUILayout.Space(20);
        GUILayout.Label("���ݵ���:", EditorStyles.boldLabel);
        GUILayout.Space(5);

        // ��ʾѡ���ļ��İ�ť
        GUILayout.BeginHorizontal();
        GUILayout.Label("�ļ�·��:", GUILayout.Width(60));
        ptsFilePath = GUILayout.TextField(ptsFilePath);
        if (GUILayout.Button("���", GUILayout.Width(60)))
        {
            ptsFilePath = EditorUtility.OpenFilePanel("ѡ��(*.pts)�ļ�", "", "pts");
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        // ��ʾѡ����ʵİ�ť
        GUILayout.BeginHorizontal();
        GUILayout.Label("����:", GUILayout.Width(60));
        material = (Material)EditorGUILayout.ObjectField(material, typeof(Material), true);
        GUILayout.EndHorizontal();

        GUILayout.Space(20);

        // ��ʾ���� Mesh �İ�ť
        if (GUILayout.Button("���������"))
        {
            LoadPointCloud(ptsFilePath);
        }
        GUILayout.Space(10);
    }

    private static Material LoadPointCloudMaterial()
    {
        string[] guids = AssetDatabase.FindAssets("t:Material PointCloudMaterial");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<Material>(path);
        }
        return null;
    }

    private void LoadPointCloud(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("�ļ�������: " + filePath);
            return;
        }

        EditorUtility.DisplayProgressBar("���ݵ���", "���Ժ�...", 0);
        // ��ȡ�ļ�����
        string[] lines = File.ReadAllLines(filePath);

        int count = lines.Length;

        Vector3[] _vertices = new Vector3[count];
        int[] _indices = new int[count];
        Color32[] _colors = new Color32[count];

        // �������� Mesh
        Mesh pointCloudMesh = new Mesh();


        for (int i = 0; i < count; i++)
        {
            string[] parts = lines[i].Split(' ');
            if (parts.Length >= 7)
            {
                float x = float.Parse(parts[0]);
                float y = float.Parse(parts[1]);
                float z = float.Parse(parts[2]);
                _vertices[i] = new Vector3(x, y, z);

                byte a = byte.Parse(parts[3]);
                byte r = byte.Parse(parts[4]);
                byte g = byte.Parse(parts[5]);
                byte b = byte.Parse(parts[6]);
                _colors[i] = new Color32(r, g, b, a);

                _indices[i] = i;
            }
            float progress = (float)i / count;
            EditorUtility.DisplayProgressBar("���ݵ���", "���Ժ�...", progress);
        }

#if UNITY_2019_3_OR_NEWER
        pointCloudMesh.SetVertices(_vertices, 0, count);
        pointCloudMesh.SetIndices(_indices, 0, count, MeshTopology.Points, 0);
        pointCloudMesh.SetColors(_colors, 0, count);
#else
        // Note that we recommend using Unity 2019.3 or above to compile this scene.
        List<Vector3> vertexList = new List<Vector3>();
        List<Color32> colorList = new List<Color32>();
        List<int> indexList = new List<int>();

        for (int i = 0; i < count; ++i)
        {
            vertexList.Add(_vertices[i]);
            indexList.Add(_indices[i]);
            colorList.Add(_colors[i]);
        }

        pointCloudMesh.SetVertices(vertexList);
        pointCloudMesh.SetIndices(indexList.ToArray(), MeshTopology.Points, 0);
        pointCloudMesh.SetColors(colorList);
#endif // UNITY_2019_3_OR_NEWER

        // ���� Mesh Ϊ asset
        //string assetPath = "Assets/" + Path.GetFileName(filePath) + ".asset";
        //AssetDatabase.CreateAsset(pointCloudMesh, assetPath);
        //AssetDatabase.SaveAssets();
        //AssetDatabase.Refresh();

        string objName = Path.GetFileNameWithoutExtension(filePath);
        // ���� GameObject ����� MeshRenderer �� MeshFilter ���
        GameObject pointCloudObject = new GameObject(objName);
        MeshRenderer renderer = pointCloudObject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material; // ��ѡ��Ĳ��ʸ�ֵ�� MeshRenderer
        MeshFilter filter = pointCloudObject.AddComponent<MeshFilter>();
        filter.mesh = pointCloudMesh;

        // ���� Mesh Ϊ asset
        string assetPath = "Assets/" + objName + ".asset";
        AssetDatabase.CreateAsset(pointCloudMesh, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("PointCloud saved as asset: " + assetPath);
        EditorUtility.ClearProgressBar();
    }
}
