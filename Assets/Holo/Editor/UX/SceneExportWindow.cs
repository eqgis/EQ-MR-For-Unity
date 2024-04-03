using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Holo.XR.Editor.Utils;
using Holo.XR.Utils;
using Holo.Data;
using LitJson;
using System.Text;
using Holo.HUR;
using System.Linq;
using System;

namespace Holo.XR.Editor.UX
{

    public class SceneExportWindow : EditorWindow
    {
        private bool[] sceneSelections;
        private string[] sceneNames;
        private int mainSceneIndex = -1; // Index of the main scene

        private string dataVersion;
        private string dataPath;

        private void OnEnable()
        {
            int sceneCount = EditorBuildSettings.scenes.Length;
            sceneSelections = new bool[sceneCount];
            sceneNames = new string[sceneCount];

            for (int i = 0; i < sceneCount; i++)
            {
                sceneNames[i] = System.IO.Path.GetFileNameWithoutExtension(EditorBuildSettings.scenes[i].path);
            }

            //���ݰ����·��
            dataPath = Directory.GetParent(Application.dataPath).ToString() + "/HoloData";

            dataVersion = DataIO.ReadNewVersion(dataPath);
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
                GUILayout.Label((i + 1) + "." + sceneNames[i], EditorStyles.miniBoldLabel);
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


            if (GUILayout.Button("����", GUILayout.Width(100)))
            {
                //�������
                if (!float.TryParse(dataVersion, out float result))
                {
                    Debug.LogError("\"���ݰ汾\"��������������һ���Ϸ�����" + result);
                    PopWindow.Show("\"���ݰ汾\"��������\n������һ���Ϸ�����", 120, 80);
                    return;
                }

                if (mainSceneIndex == -1)
                {
                    Debug.LogError("��ָ����ڳ���");
                    PopWindow.Show("\"��ڳ���\"��������", 120, 80);
                    return;
                }


                try
                {
                    EditorUtility.DisplayProgressBar("Building", "...", 0);
                    //���·��
                    string outPutPath = Application.streamingAssetsPath + ExportUtils.hotUpdatePath;
                    //��ʱ�ļ����·��
                    string tmpOutputPath = outPutPath + "/tmp";

                    EditorUtility.DisplayProgressBar("Export Scene ", "Export Scene ", 5);

                    if (Directory.Exists(outPutPath))
                    {
                        // ɾ�������ļ�
                        foreach (string file in Directory.GetFiles(outPutPath))
                        {
                            File.Delete(file);
                        }

                        // �ݹ�ɾ���������ļ��к����ǵ�����
                        foreach (string directory in Directory.GetDirectories(outPutPath))
                        {
                            Directory.Delete(directory, true);
                        }
                    }

                    //1������ȸ�DLL������������⣬�������ִ�У�
                    ExportUtils.ExecExport();

                    //2��������̬��Դab��
                    CreateAssetBundle(outPutPath, tmpOutputPath);

                    //3���������������ļ�·��
                    string cfgTmpPath = tmpOutputPath + "/" + Config.EditorConfig.GetSceneConfigName();
                    string cfgPath = outPutPath + "/" + Config.EditorConfig.GetSceneConfigName();
                    //�����������ݲ�д��
                    HoloSceneConfig sceneEntity = new HoloSceneConfig();
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

#if HYBIRDCLR_ENABLED
                    //��¼AB����Դ�б�
                    sceneEntity.AssetsBundleList = new List<string>() {
                    Config.EditorConfig.GetPreAssestName(),
                    Config.EditorConfig.GetHotUpdateAbName()
                };

                    //��¼�ȸ�DLL�б�
                    List<string> hotUpdateAssemblyNameList = new List<string>();
                    foreach (var item in HybridCLR.Editor.SettingsUtil.HotUpdateAssemblyFilesExcludePreserved)
                    {
                        if (item.EndsWith(".dll"))
                        {
                            hotUpdateAssemblyNameList.Add(item.Substring(0, item.Length - 4));
                        }
                        else
                        {
                            hotUpdateAssemblyNameList.Add(item);
                        }
                    }
                    sceneEntity.HotUpdateAssemblies = hotUpdateAssemblyNameList;

                    //��¼AOT Ԫ�����б�
                    List<string> aotMetaAssemblyNames = new List<string>();
                    foreach (var item in HybridCLR.Editor.SettingsUtil.AOTAssemblyNames)
                    {
                        aotMetaAssemblyNames.Add(item);
                    }
                    sceneEntity.AotMetaAssemblies = aotMetaAssemblyNames;
#endif
                    //��¼�ļ��嵥 2023��8��17��21:46:00
                    File.WriteAllText(cfgTmpPath, JsonMapper.ToJson(sceneEntity), new UTF8Encoding(false));
                    //���ɼ�������
                    DataIO.Copy(cfgTmpPath, cfgPath);

                    //��������cfg�ļ�
                    sourceFileList.Add(cfgPath);

                    Directory.CreateDirectory(dataPath);

                    string zipFileName = Holo.XR.Config.EditorConfig.GetHotDataName() + "_v" + dataVersion;
                    ZipHelper.Instance.Zip(sourceFileList.ToArray(), dataPath + "/" + zipFileName + ".zip", null, null);

                    //д��汾��Ϣ
                    DataIO.WriteVersionFile(dataPath, zipFileName, false);

                    //ɾ����ʱ�ļ��С���ʱ�ļ����д洢δ���ܵ����ݣ�����ʱ�á�
                    //FileUtil.DeleteFileOrDirectory(tmpOutputPath);
                    //ˢ�����ݿ⣬���Զ�����meta�ļ�
                    AssetDatabase.Refresh();

                    Debug.Log("�����ɹ�!");
                }
                catch(Exception ex)
                {
                    Debug.LogException(ex);
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                }


                string localPath = dataPath.Replace('/', '\\');
                EditorUtility.DisplayDialog("Success", "Path:\n" + localPath, "ok");
#if UNITY_EDITOR_WIN
                System.Diagnostics.Process.Start("explorer.exe", localPath);
#endif
                this.Close();
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
        /// <param name="parent">��������ļ�·��</param>
        /// <param name="outputPath">��ʱ����ļ�·��</param>
        private void CreateAssetBundle(string parent,string outputPath)
        {
            //�������·������AB��
            CreateAB(outputPath);
            //����������һ��
            ExportUtils.Copy(outputPath + "/" + Holo.XR.Config.EditorConfig.GetHotUpdateAbName(),
                parent + "/" + Holo.XR.Config.EditorConfig.GetHotUpdateAbName());

            ExportUtils.Copy(outputPath + "/" + Holo.XR.Config.EditorConfig.GetPreAssestName(),
                parent + "/" + Holo.XR.Config.EditorConfig.GetPreAssestName());
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

            //������
            HashSet<string> allDependencies = new HashSet<string>();

            // Collect scenes to build into the AssetBundle
            List<string> scenesToBuild = new List<string>();

            for (int i = 0; i < sceneSelections.Length; i++)
            {
                if (sceneSelections[i])
                {
                    //Assets/Holo/Demo/04_�����ȸ���/New Scene.unity
                    string path = EditorBuildSettings.scenes[i].path;
                    scenesToBuild.Add(path);

                    //��ȡ����������������
                    string[] dependencies = AssetDatabase.GetDependencies(path, true);
                    //�ų�cs�ű��ͳ���·����ͨ��HashSetȥ��
                    foreach (string dep in dependencies)
                    {
                        if (dep.EndsWith(".cs") || dep.EndsWith(".unity"))
                        {
                            continue;
                        }
                        else
                        {
                            allDependencies.Add(dep);
                        }
                    }
                }
            }

            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            // Build the AssetBundle
            AssetBundleBuild[] assetBundleBuilds = new AssetBundleBuild[2];
            assetBundleBuilds[0].assetBundleName = Config.EditorConfig.GetPreAssestName();
            assetBundleBuilds[0].assetNames = allDependencies.ToArray();
            assetBundleBuilds[1].assetBundleName = Config.EditorConfig.GetHotUpdateAbName();
            assetBundleBuilds[1].assetNames = scenesToBuild.ToArray();

            /*
            BuildAssetBundleOptions.None��Ĭ�Ϲ���AssetBundle�ķ�ʽ��ʹ��LZMA�㷨ѹ�������㷨ѹ����С�����Ǽ���ʱ�䳤������ʹ��֮ǰ����Ҫ�����ѹ����ѹ�Ժ�������ֻ�ʹ��LZ4�㷨����ѹ�����������ְ��Ͳ�Ҫ���������ѹ�ˡ���Ҳ���ǵ�һ�ν�ѹ������֮��ͱ���ˡ�
            BuildAssetBundleOptions.UncompressedAssetBundle����ѹ�����ݣ����󣬵��Ǽ��غܿ졣
            BuildAssetBundleOptions.ChunkBaseCompression��ʹ��LZ4�㷨ѹ����ѹ����û��LZMA�ߣ����Ǽ�����Դ���������ѹ�����ַ����й��оأ�����Ϊ�Ƚϳ��á�
             */
            BuildPipeline.BuildAssetBundles(outputPath, assetBundleBuilds, BuildAssetBundleOptions.None, target);

            Debug.Log("Main Scene: " + sceneNames[mainSceneIndex]);
        }
    }

}