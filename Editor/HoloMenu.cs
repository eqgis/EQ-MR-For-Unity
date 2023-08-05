
using Holo.XR.Editor.Utils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;

namespace Holo.XR.Editor
{
    public class HoloMenu :EditorUtility
    {

        #region MenuItem
        [MenuItem("Holo/Build Bundle - B50R", false)]
        static void BuildBundle()
        {
            string parentFolderPath = Application.streamingAssetsPath;
            //string name = GetFormattedTimestamp();
            // 获取当前场景路径
            string scenePath = EditorSceneManager.GetActiveScene().path;
            // 根据场景名生成AssetBundle的名字
            string name = "scene_" + System.IO.Path.GetFileNameWithoutExtension(scenePath).ToLower();

            string folderPath = parentFolderPath + "/" + name;

            if (!File.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

#if UNITY_ANDROID//安卓端
            Debug.Log("安卓平台打包成功");
            BuildPipeline.BuildAssetBundles(folderPath,
            BuildAssetBundleOptions.ChunkBasedCompression,
            BuildTarget.Android);
#elif UNITY_IPHONE//IOS
         Debug.Log("IOS平台打包成功");
        BulidPipeline.BulidAssetBundles(folderPath,
        BulidAssetBundleOptions.ChunkBasedCompression ,
        BulidTarget.iOS);
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR//PC或则编辑器
        Debug.Log("PC平台打包成功");
        BuildPipeline.BuildAssetBundles(folderPath,
        BuildAssetBundleOptions.ChunkBasedCompression ,
        BuildTarget.StandaloneWindows);
#endif
            //导出场景信息,注意：所挂载的C#脚本无法导出
            Scene activeScene = SceneManager.GetActiveScene();
            SceneInfoSaver.ExportSceneJson(activeScene, folderPath);

            ZipWrapper.Zip(new string[] { folderPath }, folderPath + ".zip", null, null);
            EditorUtility.DisplayDialog("提示", "打包成功!\n场景名称:"+activeScene.name+"\n文件路径:\n" + folderPath + ".zip", "ok");

#if UNITY_EDITOR_WIN
            string localPath = parentFolderPath.Replace('/', '\\');
            System.Diagnostics.Process.Start("explorer.exe", localPath);
#endif
        }
        #endregion


        [MenuItem("Holo/Scene Config/Import MapLoader", false, 104)]
        static void ImportMapLoader()
        {
            XvPrefabsUtils.ImportMapLoader(null,null,false);
        }

        [MenuItem("Holo/Scene Config/Import MapScanner", false, 105)]
        static void ImportMapScanner()
        {
            XvPrefabsUtils.ImportMapScanner(null,null);
        }

        [MenuItem("Holo/Scene Config/Import Default Config", false,101)]
        static void ImportDefaultConfig()
        {
            XvPrefabsUtils.ImportXvManager();
            XvPrefabsUtils.ImportGesture();
            XvPrefabsUtils.ImportXvThrowScene();
        }

    }


}
