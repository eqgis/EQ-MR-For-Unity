
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
            // ��ȡ��ǰ����·��
            string scenePath = EditorSceneManager.GetActiveScene().path;
            // ���ݳ���������AssetBundle������
            string name = "scene_" + System.IO.Path.GetFileNameWithoutExtension(scenePath).ToLower();

            string folderPath = parentFolderPath + "/" + name;

            if (!File.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

#if UNITY_ANDROID//��׿��
            Debug.Log("��׿ƽ̨����ɹ�");
            BuildPipeline.BuildAssetBundles(folderPath,
            BuildAssetBundleOptions.ChunkBasedCompression,
            BuildTarget.Android);
#elif UNITY_IPHONE//IOS
         Debug.Log("IOSƽ̨����ɹ�");
        BulidPipeline.BulidAssetBundles(folderPath,
        BulidAssetBundleOptions.ChunkBasedCompression ,
        BulidTarget.iOS);
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR//PC����༭��
        Debug.Log("PCƽ̨����ɹ�");
        BuildPipeline.BuildAssetBundles(folderPath,
        BuildAssetBundleOptions.ChunkBasedCompression ,
        BuildTarget.StandaloneWindows);
#endif
            ////����������Ϣ,ע�⣺�����ص�C#�ű��޷�����
            //Scene activeScene = SceneManager.GetActiveScene();
            //SceneInfoSaver.ExportSceneJson(activeScene, folderPath);

            //ZipWrapper.Zip(new string[] { folderPath }, folderPath + ".zip", null, null);
            //EditorUtility.DisplayDialog("��ʾ", "����ɹ�!\n��������:"+activeScene.name+"\n�ļ�·��:\n" + folderPath + ".zip", "ok");

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
            // �����Զ��嵯�������óߴ�
            Rect windowRect = new Rect(100, 100, 360, 136);
            EditWindowXvCslamMapControl window = EditorWindow.GetWindowWithRect<EditWindowXvCslamMapControl>(windowRect, true, "���ݵ���");
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
        //��Unity�˵��д���һ���˵�·���������ú궨��
        [MenuItem("Holo-XR/Settings")]
        public static void Setting()
        {
            Rect windowRect = new Rect(100, 100, 240, 180);
            SettingsWindow win = EditorWindow.GetWindowWithRect<SettingsWindow>(windowRect, false, "Holo-Settings");
            //win.titleContent = new GUIContent("ȫ������");
            win.Show();
        }

        #endregion
    }
}
