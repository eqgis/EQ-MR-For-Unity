using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Holo.XR.Editor.Utils;
using Holo.XR.Utils;
using Holo.Data;
using LitJson;

namespace Holo.XR.Editor.UX
{

    public class SceneExportWindow : EditorWindow
    {
        private bool[] sceneSelections;
        private string[] sceneNames;
        private int mainSceneIndex = -1; // Index of the main scene

        private string dataVersion = "1";

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
            GUILayout.Space(10);
            GUILayout.Label("����������г���:", EditorStyles.boldLabel);
            GUILayout.Space(5);

            EditorGUILayout.BeginVertical("box");
            for (int i = 0; i < sceneSelections.Length; i++)
            {
                //sceneSelections[i] = GUILayout.Toggle(sceneSelections[i], sceneNames[i]);
                sceneSelections[i] = true;
                GUILayout.Label((i+1)+"."+sceneNames[i], EditorStyles.miniBoldLabel);
            }
            EditorGUILayout.EndVertical();


            GUILayout.Space(5);
            GUILayout.Label("ע:��File->Build Settins���޸ĳ���");
            GUILayout.Space(10);

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

            GUILayout.Space(10);
            EditorGUIUtility.labelWidth = 60;
            dataVersion = EditorGUILayout.TextField("���ݰ汾:", dataVersion);

            EditorGUIUtility.labelWidth = 0;
            GUILayout.Space(10);


            GUILayout.BeginVertical();

            if (GUILayout.Button("����", GUILayout.Width(100)))
            {
                //�������
                if (!float.TryParse(dataVersion, out float result))
                {
                    Debug.LogError("\"���ݰ汾\"��������������һ���Ϸ�����" + result);
                    PopWindow.Show("\"���ݰ汾\"��������\n������һ���Ϸ�����",120,80);
                    return;
                }

                if (mainSceneIndex == -1)
                {
                    Debug.LogError("��ָ����ڳ���");
                    PopWindow.Show("\"��ڳ���\"��������", 120, 80);
                    return;
                }

                //1������ȸ�DLL������������⣬�������ִ�У�
                ExportUtils.ExecExport();

                //���·��
                string outPutPath = Application.streamingAssetsPath + ExportUtils.hotUpdatePath;
                //2��������̬��Դab��
                CreateAssetBundle(outPutPath);

                //3���������������ļ�·��
                string cfgPath =  outPutPath +"/"+ Config.EditorConfig.GetSceneConfigName();
                //�����������ݲ�д��
                SceneEntity sceneEntity = new SceneEntity();
                //��¼��ڳ���
                sceneEntity.MainScene = sceneNames[mainSceneIndex];
                sceneEntity.FileList = new List<string>();

                //Ҫ������ļ�·��
                string[] files = Directory.GetFiles(outPutPath);

                //���˵���*.meta������
                List<string> sourceFileList = new List<string>();
                foreach (var item in files)
                {
                    if (!item.EndsWith(".meta"))
                    {
                        string filePath = item.Replace("\\", "/");
                        sourceFileList.Add(filePath);

                        string fileName = Path.GetFileName(filePath);
                        if (!fileName.Equals(Config.EditorConfig.GetSceneConfigName()))
                        {
                            sceneEntity.FileList.Add(fileName);
                        }
                    }
                }

                //��¼�ļ��嵥 2023��8��17��21:46:00
                File.WriteAllText(cfgPath, JsonMapper.ToJson(sceneEntity),System.Text.Encoding.UTF8);

                //��������cfg�ļ�
                sourceFileList.Add(cfgPath);

                //���ݰ����·��
                string dataPath = Directory.GetParent(Application.dataPath).ToString() + "/HoloData";
                Directory.CreateDirectory(dataPath);

                //ˢ�����ݿ⣬���Զ�����meta�ļ�
                AssetDatabase.Refresh();

                ZipHelper.Instance.Zip(sourceFileList.ToArray(), dataPath + "/"+Holo.XR.Config.EditorConfig.GetHotDataName()+"_v"+dataVersion+".zip",null,null);

#if UNITY_EDITOR
                Debug.Log("�����ɹ�!");
#endif

#if UNITY_EDITOR_WIN
                string localPath = dataPath.Replace('/', '\\');
                System.Diagnostics.Process.Start("explorer.exe", localPath);
#endif
            }

            GUILayout.EndVertical();
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
        private void CreateAssetBundle(string parent)
        {
            string outputPath = parent + "/tmp";
            //�������·������AB��
            CreateAB(outputPath);
            ExportUtils.Copy(outputPath + "/"+ Holo.XR.Config.EditorConfig.GetHotUpdateAbName(),
                parent + "/" + Holo.XR.Config.EditorConfig.GetHotUpdateAbName());

            //ɾ����ʱ�ļ�
            string[] files = Directory.GetFiles(outputPath);
            foreach (string file in files)
            {
                File.Delete(file);
            }
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
            assetBundleBuilds[0].assetBundleName = Config.EditorConfig.GetHotUpdateAbName();
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