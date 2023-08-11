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
            GUILayout.Label("ѡ�񳡾�����:", EditorStyles.boldLabel);

            for (int i = 0; i < sceneSelections.Length; i++)
            {
                sceneSelections[i] = GUILayout.Toggle(sceneSelections[i], sceneNames[i]);
            }

            GUILayout.Label("ָ��������Ϊ���:", EditorStyles.boldLabel);

            // Only show the main scene selection menu if at least one scene is selected
            if (AnySceneSelected())
            {
                mainSceneIndex = EditorGUILayout.Popup(mainSceneIndex, GetSelectedSceneNames());
            }
            else
            {
                GUILayout.Label("δѡ�񵼳�����");
            }

            if (GUILayout.Button("����", GUILayout.Width(100)))
            {
                //�������
                if (mainSceneIndex == -1)
                {
                    Debug.LogError("��ָ����ڳ���");
                    return;
                }

                //����ȸ�DLL������������⣬�������ִ�У�
                ExportUtils.ExecExport();

                //������̬��Դab��
                CreateAssetBundle();

                //ˢ�����ݿ⣬���Զ�����meta�ļ�
                AssetDatabase.Refresh();

#if UNITY_EDITOR
                Debug.Log("�����ɹ�!");
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
        /// ����AB��
        /// </summary>
        private void CreateAssetBundle()
        {
            string outputPath = Application.streamingAssetsPath + ExportUtils.hotUpdatePath;
            //�������·������AB��
            CreateAB(outputPath);

        }

        /// <summary>
        /// �������·������AB��
        /// </summary>
        /// <param name="outputPath">���·��</param>
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
                    //Assets/Holo/Demo/04_�����ȸ���/New Scene.unity
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
            BuildAssetBundleOptions.None��Ĭ�Ϲ���AssetBundle�ķ�ʽ��ʹ��LZMA�㷨ѹ�������㷨ѹ����С�����Ǽ���ʱ�䳤������ʹ��֮ǰ����Ҫ�����ѹ����ѹ�Ժ�������ֻ�ʹ��LZ4�㷨����ѹ�����������ְ��Ͳ�Ҫ���������ѹ�ˡ���Ҳ���ǵ�һ�ν�ѹ������֮��ͱ���ˡ�
            BuildAssetBundleOptions.UncompressedAssetBundle����ѹ�����ݣ����󣬵��Ǽ��غܿ졣
            BuildAssetBundleOptions.ChunkBaseCompression��ʹ��LZ4�㷨ѹ����ѹ����û��LZMA�ߣ����Ǽ�����Դ���������ѹ�����ַ����й��оأ�����Ϊ�Ƚϳ��á�
             */
            BuildPipeline.BuildAssetBundles(outputPath, assetBundleBuilds, BuildAssetBundleOptions.ChunkBasedCompression, target);

            Debug.Log("Main Scene: " + sceneNames[mainSceneIndex]);
        }
    }

}