
using Holo.XR.Editor.Utils;
using Holo.XR.Editor.UX;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Windows;

namespace Holo.XR.Editor
{
    public class HoloMenu :EditorUtility
    {
        #region MenuItem
        [MenuItem("Holo-XR/Build Bundle - B50R", false)]
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
            ////导出场景信息,注意：所挂载的C#脚本无法导出
            //Scene activeScene = SceneManager.GetActiveScene();
            //SceneInfoSaver.ExportSceneJson(activeScene, folderPath);

            //ZipWrapper.Zip(new string[] { folderPath }, folderPath + ".zip", null, null);
            //EditorUtility.DisplayDialog("提示", "打包成功!\n场景名称:"+activeScene.name+"\n文件路径:\n" + folderPath + ".zip", "ok");

#if UNITY_EDITOR_WIN
            string localPath = parentFolderPath.Replace('/', '\\');
            System.Diagnostics.Process.Start("explorer.exe", localPath);
#endif
        }
        #endregion


        #region Scene Config
        [MenuItem("Holo-XR/Scene Config/Import MapLoader", false, 104)]
        static void ImportMapLoader()
        {
            // 创建自定义弹窗并设置尺寸
            Rect windowRect = new Rect(100, 100, 360, 136);
            EditWindowXvCslamMapControl window = EditorWindow.GetWindowWithRect<EditWindowXvCslamMapControl>(windowRect, true, "数据导入");
            window.Show();
        }

        [MenuItem("Holo-XR/Scene Config/Import MapScanner", false, 105)]
        static void ImportMapScanner()
        {
            XvPrefabsCreator.ImportMapScanner(null,null);
        }

        [MenuItem("Holo-XR/Scene Config/Import Default Config", false,101)]
        static void ImportDefaultConfig()
        {
            XvPrefabsCreator.ImportXvManager();
            XvPrefabsCreator.ImportGesture();
            XvPrefabsCreator.ImportXvThrowScene();
        }

        #endregion


        #region HotUpdate 
        [MenuItem("Holo-XR/HotUpdate/Import Dll Loader", false, 101)]
        static void ImportDllLoader()
        {
            HURComponentCreator.ImportDllLoader();
        }

        [MenuItem("Holo-XR/HotUpdate/BuildBundle-Android")]
        public static void BuildBundle_Android()
        {
            HURComponentCreator.ExportDllAndAssetsBundle();
        }

        #endregion

        #region Other
        //在Unity菜单中创建一个菜单路径用于设置宏定义
        [MenuItem("Holo-XR/Settings")]
        public static void Setting()
        {
            Rect windowRect = new Rect(100, 100, 240, 180);
            SettingsWindow win = EditorWindow.GetWindowWithRect<SettingsWindow>(windowRect, false, "Holo-Settings");
            //win.titleContent = new GUIContent("全局设置");
            win.Show();
        }

        #endregion
    }
}
