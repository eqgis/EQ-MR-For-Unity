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

            //数据包输出路径
            dataPath = Directory.GetParent(Application.dataPath).ToString() + "/HoloData";

            dataVersion = DataIO.ReadNewVersion(dataPath);
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            GUILayout.Label("打包下列所有场景:", EditorStyles.boldLabel);
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
            GUILayout.Label("注:在File->Build Settins中修改场景");
            GUILayout.Space(10);

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

            GUILayout.Space(10);
            EditorGUIUtility.labelWidth = 60;
            dataVersion = EditorGUILayout.TextField("数据版本:", dataVersion);

            EditorGUIUtility.labelWidth = 0;
            GUILayout.Space(10);


            if (GUILayout.Button("导出", GUILayout.Width(100)))
            {
                //参数检查
                if (!float.TryParse(dataVersion, out float result))
                {
                    Debug.LogError("\"数据版本\"设置有误，请输入一个合法数字" + result);
                    PopWindow.Show("\"数据版本\"设置有误\n请输入一个合法数字", 120, 80);
                    return;
                }

                if (mainSceneIndex == -1)
                {
                    Debug.LogError("请指定入口场景");
                    PopWindow.Show("\"入口场景\"设置有误", 120, 80);
                    return;
                }


                try
                {
                    EditorUtility.DisplayProgressBar("Building", "...", 0);
                    //输出路径
                    string outPutPath = Application.streamingAssetsPath + ExportUtils.hotUpdatePath;
                    //临时文件输出路径
                    string tmpOutputPath = outPutPath + "/tmp";

                    EditorUtility.DisplayProgressBar("Export Scene ", "Export Scene ", 5);

                    if (Directory.Exists(outPutPath))
                    {
                        // 删除所有文件
                        foreach (string file in Directory.GetFiles(outPutPath))
                        {
                            File.Delete(file);
                        }

                        // 递归删除所有子文件夹和它们的内容
                        foreach (string directory in Directory.GetDirectories(outPutPath))
                        {
                            Directory.Delete(directory, true);
                        }
                    }

                    //1、打包热更DLL（包含环境检测，因此优先执行）
                    ExportUtils.ExecExport();

                    //2、创建静态资源ab包
                    CreateAssetBundle(outPutPath, tmpOutputPath);

                    //3、创建场景配置文件路径
                    string cfgTmpPath = tmpOutputPath + "/" + Config.EditorConfig.GetSceneConfigName();
                    string cfgPath = outPutPath + "/" + Config.EditorConfig.GetSceneConfigName();
                    //构建场景数据并写入
                    HoloSceneConfig sceneEntity = new HoloSceneConfig();
                    //记录入口场景
                    sceneEntity.MainScene = sceneNames[mainSceneIndex];
                    sceneEntity.FileList = new List<string>();

                    //要打包的文件路径
                    string[] files = Directory.GetFiles(outPutPath);

                    //过滤掉（*.meta）内容
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
                    //记录AB包资源列表
                    sceneEntity.AssetsBundleList = new List<string>() {
                    Config.EditorConfig.GetPreAssestName(),
                    Config.EditorConfig.GetHotUpdateAbName()
                };

                    //记录热更DLL列表
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

                    //记录AOT 元数据列表
                    List<string> aotMetaAssemblyNames = new List<string>();
                    foreach (var item in HybridCLR.Editor.SettingsUtil.AOTAssemblyNames)
                    {
                        aotMetaAssemblyNames.Add(item);
                    }
                    sceneEntity.AotMetaAssemblies = aotMetaAssemblyNames;
#endif
                    //记录文件清单 2023年8月17日21:46:00
                    File.WriteAllText(cfgTmpPath, JsonMapper.ToJson(sceneEntity), new UTF8Encoding(false));
                    //生成加密数据
                    DataIO.Copy(cfgTmpPath, cfgPath);

                    //输出包添加cfg文件
                    sourceFileList.Add(cfgPath);

                    Directory.CreateDirectory(dataPath);

                    string zipFileName = Holo.XR.Config.EditorConfig.GetHotDataName() + "_v" + dataVersion;
                    ZipHelper.Instance.Zip(sourceFileList.ToArray(), dataPath + "/" + zipFileName + ".zip", null, null);

                    //写入版本信息
                    DataIO.WriteVersionFile(dataPath, zipFileName, false);

                    //删除临时文件夹【临时文件夹中存储未加密的数据，调试时用】
                    //FileUtil.DeleteFileOrDirectory(tmpOutputPath);
                    //刷新数据库，会自动更新meta文件
                    AssetDatabase.Refresh();

                    Debug.Log("导出成功!");
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
        /// 创建AB包
        /// </summary>
        /// <param name="parent">最终输出文件路径</param>
        /// <param name="outputPath">临时输出文件路径</param>
        private void CreateAssetBundle(string parent,string outputPath)
        {
            //根据输出路径创建AB包
            CreateAB(outputPath);
            //拷贝两个，一个
            ExportUtils.Copy(outputPath + "/" + Holo.XR.Config.EditorConfig.GetHotUpdateAbName(),
                parent + "/" + Holo.XR.Config.EditorConfig.GetHotUpdateAbName());

            ExportUtils.Copy(outputPath + "/" + Holo.XR.Config.EditorConfig.GetPreAssestName(),
                parent + "/" + Holo.XR.Config.EditorConfig.GetPreAssestName());
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

            //依赖项
            HashSet<string> allDependencies = new HashSet<string>();

            // Collect scenes to build into the AssetBundle
            List<string> scenesToBuild = new List<string>();

            for (int i = 0; i < sceneSelections.Length; i++)
            {
                if (sceneSelections[i])
                {
                    //Assets/Holo/Demo/04_场景热更新/New Scene.unity
                    string path = EditorBuildSettings.scenes[i].path;
                    scenesToBuild.Add(path);

                    //获取场景中所有依赖项
                    string[] dependencies = AssetDatabase.GetDependencies(path, true);
                    //排除cs脚本和场景路径，通过HashSet去重
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
            BuildAssetBundleOptions.None：默认构建AssetBundle的方式。使用LZMA算法压缩，此算法压缩包小，但是加载时间长，而且使用之前必须要整体解压。解压以后，这个包又会使用LZ4算法重新压缩，这样这种包就不要对其整体解压了。（也就是第一次解压很慢，之后就变快了。
            BuildAssetBundleOptions.UncompressedAssetBundle：不压缩数据，包大，但是加载很快。
            BuildAssetBundleOptions.ChunkBaseCompression：使用LZ4算法压缩，压缩率没有LZMA高，但是加载资源不必整体解压。这种方法中规中矩，我认为比较常用。
             */
            BuildPipeline.BuildAssetBundles(outputPath, assetBundleBuilds, BuildAssetBundleOptions.None, target);

            Debug.Log("Main Scene: " + sceneNames[mainSceneIndex]);
        }
    }

}