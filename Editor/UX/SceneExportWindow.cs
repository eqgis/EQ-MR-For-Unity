using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Holo.XR.Editor.Utils;

namespace Holo.XR.Editor.UX
{

    public class SceneExportWindow : EditorWindow
    {
        private bool[] sceneSelections;
        private string[] sceneNames;
        private int mainSceneIndex = -1; // Index of the main scene

        private void OnEnable()
        {
            int sceneCount = EditorBuildSettings.scenes.Length;
            sceneSelections = new bool[sceneCount];
            sceneNames = new string[sceneCount];

            for (int i = 0; i < sceneCount; i++)
            {
                sceneNames[i] = System.IO.Path.GetFileNameWithoutExtension(EditorBuildSettings.scenes[i].path);
            }
        }

        private void OnGUI()
        {
            GUILayout.Label("选择场景导出:", EditorStyles.boldLabel);

            for (int i = 0; i < sceneSelections.Length; i++)
            {
                sceneSelections[i] = GUILayout.Toggle(sceneSelections[i], sceneNames[i]);
            }

            GUILayout.Label("指定场景作为入口:", EditorStyles.boldLabel);

            // Only show the main scene selection menu if at least one scene is selected
            if (AnySceneSelected())
            {
                mainSceneIndex = EditorGUILayout.Popup(mainSceneIndex, GetSelectedSceneNames());
            }
            else
            {
                GUILayout.Label("未选择导出场景");
            }

            if (GUILayout.Button("导出", GUILayout.Width(100)))
            {
                //参数检查
                if (mainSceneIndex == -1)
                {
                    Debug.LogError("请指定入口场景");
                    return;
                }

                //打包热更DLL（包含环境检测，因此优先执行）
                ExportUtils.ExecExport();

                //创建静态资源ab包
                CreateAssetBundle();

                //刷新数据库，会自动更新meta文件
                AssetDatabase.Refresh();

#if UNITY_EDITOR
                Debug.Log("导出成功!");
#endif
            }
        }

        private bool AnySceneSelected()
        {
            foreach (bool selection in sceneSelections)
            {
                if (selection)
                {
                    return true;
                }
            }
            return false;
        }

        private string[] GetSelectedSceneNames()
        {
            List<string> selectedNames = new List<string>();
            for (int i = 0; i < sceneSelections.Length; i++)
            {
                if (sceneSelections[i])
                {
                    selectedNames.Add(sceneNames[i]);
                }
            }
            return selectedNames.ToArray();
        }

        /// <summary>
        /// 创建AB包
        /// </summary>
        private void CreateAssetBundle()
        {
            string outputPath = Application.streamingAssetsPath + ExportUtils.hotUpdatePath;
            //根据输出路径创建AB包
            CreateAB(outputPath);

        }

        /// <summary>
        /// 根据输出路径创建AB包
        /// </summary>
        /// <param name="outputPath">输出路径</param>
        private void CreateAB(string outputPath)
        {
            if (!File.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            // Collect scenes to build into the AssetBundle
            List<string> scenesToBuild = new List<string>();

            for (int i = 0; i < sceneSelections.Length; i++)
            {
                if (sceneSelections[i])
                {
                    //Assets/Holo/Demo/04_场景热更新/New Scene.unity
                    string path = EditorBuildSettings.scenes[i].path;
                    scenesToBuild.Add(path);
                }
            }

            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            // Build the AssetBundle
            AssetBundleBuild[] assetBundleBuilds = new AssetBundleBuild[1];
            assetBundleBuilds[0].assetBundleName = Holo.XR.Config.EditorConfig.GetHotUpdateAbName();
            assetBundleBuilds[0].assetNames = scenesToBuild.ToArray();

            /*
            BuildAssetBundleOptions.None：默认构建AssetBundle的方式。使用LZMA算法压缩，此算法压缩包小，但是加载时间长，而且使用之前必须要整体解压。解压以后，这个包又会使用LZ4算法重新压缩，这样这种包就不要对其整体解压了。（也就是第一次解压很慢，之后就变快了。
            BuildAssetBundleOptions.UncompressedAssetBundle：不压缩数据，包大，但是加载很快。
            BuildAssetBundleOptions.ChunkBaseCompression：使用LZ4算法压缩，压缩率没有LZMA高，但是加载资源不必整体解压。这种方法中规中矩，我认为比较常用。
             */
            BuildPipeline.BuildAssetBundles(outputPath, assetBundleBuilds, BuildAssetBundleOptions.ChunkBasedCompression, target);

            Debug.Log("Main Scene: " + sceneNames[mainSceneIndex]);
        }
    }

}