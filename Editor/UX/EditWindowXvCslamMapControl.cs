using UnityEngine;
using UnityEditor;
using Holo.XR.Editor.Utils;
using System.IO;

namespace Holo.XR.Editor.UX
{
    public class EditWindowXvCslamMapControl : EditorWindow
    {
        private string selectedFilePath = "";

        private void OnGUI()
        {
            GUILayout.Space(15f);

            GUILayout.Label("ѡ��CSlam��ͼ���ݣ�" + Holo.XR.Config.EditorConfig.GetMapPackageSuffix() + "�ļ�)");

            GUILayout.Space(5f);

            if (GUILayout.Button("ѡ������", GUILayout.Width(100)))
            {
                // �򿪱����ļ�ѡ��Ի���
                selectedFilePath = EditorUtility.OpenFilePanel("ѡ���ļ�", "", Holo.XR.Config.EditorConfig.GetMapPackageSuffix()); // ����ָ���ļ����ͣ��� "*.txt"
            }
            GUILayout.Space(10f);
            //���ñ�ǩ���
            EditorGUIUtility.labelWidth = 40f;

            // ��ʾ�ı������
            selectedFilePath = EditorGUILayout.TextField("·��:", selectedFilePath);


            GUILayout.Space(10f);

            //ע�⣺·���ġ�\��Ҫ�滻Ϊ��/��
            if (GUILayout.Button("��������"))
            {
                if (selectedFilePath.Trim() == "")
                {
                    //����������ֱ�Ӵ�������
                    XvPrefabsCreator.ImportMapLoader(null, null, false);
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
                    XvPrefabsCreator.ImportMapLoader(folderPath, fileName
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
                    //ˢ�����ݿ⣬���Զ�����meta�ļ�
                    AssetDatabase.Refresh();
                    //ת��Ϊ���·��
                    string relativePath = targetFolderPath.Replace(Application.streamingAssetsPath, "");
                    XvPrefabsCreator.ImportMapLoader(relativePath,
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


}