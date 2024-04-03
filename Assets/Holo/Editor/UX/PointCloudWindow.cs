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

    [MenuItem("Tool/加载点云/*.pts")]
    public static void ShowWindow()
    {
        material = LoadPointCloudMaterial();
        //EditorWindow.GetWindow(typeof(PointCloudWindow));
        Rect windowRect = new Rect(100, 100, 300, 180);
        PointCloudWindow win = EditorWindow.GetWindowWithRect<PointCloudWindow>(windowRect, false, "点云-数据");
        //SettingsWindow win = EditorWindow.GetWindow<SettingsWindow>(false, "Holo-Settings");
        //win.titleContent = new GUIContent("全局设置");
        win.Show();
    }

    private void OnGUI()
    {

        GUILayout.Space(20);
        GUILayout.Label("数据导入:", EditorStyles.boldLabel);
        GUILayout.Space(5);

        // 显示选择文件的按钮
        GUILayout.BeginHorizontal();
        GUILayout.Label("文件路径:", GUILayout.Width(60));
        ptsFilePath = GUILayout.TextField(ptsFilePath);
        if (GUILayout.Button("浏览", GUILayout.Width(60)))
        {
            ptsFilePath = EditorUtility.OpenFilePanel("选择(*.pts)文件", "", "pts");
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        // 显示选择材质的按钮
        GUILayout.BeginHorizontal();
        GUILayout.Label("材质:", GUILayout.Width(60));
        material = (Material)EditorGUILayout.ObjectField(material, typeof(Material), true);
        GUILayout.EndHorizontal();

        GUILayout.Space(20);

        // 显示生成 Mesh 的按钮
        if (GUILayout.Button("添加至场景"))
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
            Debug.LogError("文件不存在: " + filePath);
            return;
        }

        EditorUtility.DisplayProgressBar("数据导入", "请稍候...", 0);
        // 读取文件内容
        string[] lines = File.ReadAllLines(filePath);

        int count = lines.Length;

        Vector3[] _vertices = new Vector3[count];
        int[] _indices = new int[count];
        Color32[] _colors = new Color32[count];

        // 创建点云 Mesh
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
            EditorUtility.DisplayProgressBar("数据导入", "请稍候...", progress);
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

        // 保存 Mesh 为 asset
        //string assetPath = "Assets/" + Path.GetFileName(filePath) + ".asset";
        //AssetDatabase.CreateAsset(pointCloudMesh, assetPath);
        //AssetDatabase.SaveAssets();
        //AssetDatabase.Refresh();

        string objName = Path.GetFileNameWithoutExtension(filePath);
        // 创建 GameObject 并添加 MeshRenderer 和 MeshFilter 组件
        GameObject pointCloudObject = new GameObject(objName);
        MeshRenderer renderer = pointCloudObject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material; // 将选择的材质赋值给 MeshRenderer
        MeshFilter filter = pointCloudObject.AddComponent<MeshFilter>();
        filter.mesh = pointCloudMesh;

        // 保存 Mesh 为 asset
        string assetPath = "Assets/" + objName + ".asset";
        AssetDatabase.CreateAsset(pointCloudMesh, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("PointCloud saved as asset: " + assetPath);
        EditorUtility.ClearProgressBar();
    }
}
