using UnityEngine;
using UnityEditor;
using Holo.XR.Editor.Utils;
using System.IO;

public class CustomEditorWindow : EditorWindow
{
    private string selectedFilePath = "";

    [MenuItem("MyTools/Open Custom Window")]
    public static void ShowWindow()
    {
        // �����Զ��嵯�������óߴ�
        Rect windowRect = new Rect(100, 100, 500, 200);
        CustomEditorWindow window = EditorWindow.GetWindowWithRect<CustomEditorWindow>(windowRect, true, "Custom Window");
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("ѡ��CSLAM��ͼ����:", EditorStyles.boldLabel);

        GUILayout.Label("��֧�֡�/Assets/StreamingAssets/��Ŀ¼�µ�����:", EditorStyles.boldLabel);

        GUILayout.Space(15f);
        if (GUILayout.Button("ѡ������", GUILayout.Width(100)))
        {
            // �򿪱����ļ�ѡ��Ի���
            selectedFilePath = EditorUtility.OpenFilePanel("ѡ���ļ�", "", Holo.XR.Config.EditorConfig.GetMapPackageSuffix()); // ����ָ���ļ����ͣ��� "*.txt"
        }
        GUILayout.Space(10f);

        // ��ʾ�ı������
        selectedFilePath = EditorGUILayout.TextField("·��:", selectedFilePath);

        GUILayout.Label("Assets·��:" + Application.streamingAssetsPath, EditorStyles.boldLabel);

        //���ñ�ǩ���
        EditorGUIUtility.labelWidth = 40f;


        //ע�⣺·���ġ�\��Ҫ�滻Ϊ��/��
        if (GUILayout.Button("��������"))
        {
            if (selectedFilePath.Trim() == "")
            {
                //����������ֱ�Ӵ�������
                XvPrefabsUtils.ImportMapLoader(null, null, false);
                this.Close();
                return;
            }
            //�ж�·���Ƿ��� Application.streamingAssetsPath��
            if (selectedFilePath.StartsWith(Application.streamingAssetsPath))
            {
                //ת��Ϊ���·��
                string relativePath = selectedFilePath.Replace(Application.streamingAssetsPath, "");
                //��ȡ�ļ���·�����ļ�����,ע�⣺��Ҫ�Ƴ���׺
                string fileName = Path.GetFileName(relativePath);
                string folderPath = relativePath.Replace("/" + fileName, "");
                //��streamingAssetsPath�£�����Ҫ����Ϊtrue
                XvPrefabsUtils.ImportMapLoader(folderPath, fileName
                    .Replace("." + Holo.XR.Config.EditorConfig.GetMapPackageSuffix(), ""),
                    true);
            }
            else
            {
                //Ŀ���ļ���
                string targetFolderPath = Application.streamingAssetsPath + "/" + Holo.XR.Config.EditorConfig.GetMapPackageSuffix();
                if (!Directory.Exists(targetFolderPath))
                {
                    //�ļ��в����ڣ����ȴ���
                    Directory.CreateDirectory(targetFolderPath);
                }
                /*======�ȿ������ٵ���======*/
                //Ŀ���ļ�����
                string fileName = Path.GetFileName(selectedFilePath);
                string targetFileName = targetFolderPath + "/" + fileName;
                if (File.Exists(targetFileName))
                {
                    File.Delete(targetFileName);
                }
                File.Copy(selectedFilePath, targetFileName);
                //ת��Ϊ���·��
                string relativePath = targetFolderPath.Replace(Application.streamingAssetsPath, "");
                XvPrefabsUtils.ImportMapLoader(relativePath, 
                    fileName.Replace("." + Holo.XR.Config.EditorConfig.GetMapPackageSuffix(), ""),
                    true);
            }

            this.Close();
        }

    }

    private void OnEnable()
    {
        // ���ô��ڵ���С�߶�
        minSize = new Vector2(300f, 100f);
    }
}

