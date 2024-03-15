using UnityEngine;
using UnityEditor;
using Holo.XR.Editor.Utils;
using System.IO;
using Holo.XR.Core;

namespace Holo.XR.Editor.UX
{
    public class EditWindowXvCslamMapControl : EditorWindow
    {
        private string selectedFilePath = "";

        private void OnGUI()
        {
            GUILayout.Space(15f);

            GUILayout.Label("选择CSlam地图数据（" + Holo.XR.Config.EditorConfig.GetMapPackageSuffix() + "文件)");

            GUILayout.Space(5f);

            if (GUILayout.Button("选择数据", GUILayout.Width(100)))
            {
                // 打开本地文件选择对话框
                selectedFilePath = EditorUtility.OpenFilePanel("选择文件", "", Holo.XR.Config.EditorConfig.GetMapPackageSuffix()); // 可以指定文件类型，如 "*.txt"
            }
            GUILayout.Space(10f);
            //设置标签宽度
            EditorGUIUtility.labelWidth = 40f;

            // 显示文本输入框
            selectedFilePath = EditorGUILayout.TextField("路径:", selectedFilePath);


            GUILayout.Space(10f);

            //注意：路径的“\”要替换为“/”
            if (GUILayout.Button("创建对象"))
            {
                if (selectedFilePath.Trim() == "")
                {
                    //不做操作，直接创建对象
                    XvPrefabsCreator.ImportMapLoader(null, null, CslamMapDataSource.LocalPath,null);
                    this.Close();
                    return;
                }

                //判断是否是网络数据
                if (selectedFilePath.Trim().StartsWith("http"))
                {
                    //获取文件夹路径和文件名称,注意：需要移除后缀
                    string fileName = Path.GetFileNameWithoutExtension(selectedFilePath);
                    //在streamingAssetsPath下，则需要设置为true
                    XvPrefabsCreator.ImportMapLoader("/" + Config.EditorConfig.GetMapPackageSuffix(),
                        fileName,
                        CslamMapDataSource.WebData,
                        selectedFilePath.Trim());
                }
                else
                {
                    //判断路径是否在 Application.streamingAssetsPath下
                    if (selectedFilePath.StartsWith(Application.streamingAssetsPath))
                    {
                        //转换为相对路径
                        string relativePath = selectedFilePath.Replace(Application.streamingAssetsPath, "");
                        //获取文件夹路径和文件名称,注意：需要移除后缀
                        string fileName = Path.GetFileName(relativePath);
                        string folderPath = relativePath.Replace("/" + fileName, "");
                        //在streamingAssetsPath下，则需要设置为true
                        XvPrefabsCreator.ImportMapLoader(folderPath, fileName
                            .Replace("." + Holo.XR.Config.EditorConfig.GetMapPackageSuffix(), ""),
                            CslamMapDataSource.StreamingAssets,null);
                    }
                    else
                    {
                        //目标文件夹
                        string targetFolderPath = Application.streamingAssetsPath + "/" + Holo.XR.Config.EditorConfig.GetMapPackageSuffix();
                        if (!Directory.Exists(targetFolderPath))
                        {
                            //文件夹不存在，则先创建
                            Directory.CreateDirectory(targetFolderPath);
                        }
                        /*======先拷贝，再导入======*/
                        //目标文件名称
                        string fileName = Path.GetFileName(selectedFilePath);
                        string targetFileName = targetFolderPath + "/" + fileName;
                        if (File.Exists(targetFileName))
                        {
                            File.Delete(targetFileName);
                        }
                        File.Copy(selectedFilePath, targetFileName);
                        //刷新数据库，会自动更新meta文件
                        AssetDatabase.Refresh();
                        //转换为相对路径
                        string relativePath = targetFolderPath.Replace(Application.streamingAssetsPath, "");
                        XvPrefabsCreator.ImportMapLoader(relativePath,
                            fileName.Replace("." + Holo.XR.Config.EditorConfig.GetMapPackageSuffix(), ""),
                            CslamMapDataSource.StreamingAssets, null);
                    }
                }

                this.Close();
            }

        }

        private void OnEnable()
        {
            // 设置窗口的最小高度
            minSize = new Vector2(300f, 100f);
        }
    }


}