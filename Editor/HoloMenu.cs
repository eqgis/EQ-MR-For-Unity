
using Holo.XR.Editor.Utils;
using Holo.XR.Editor.UX;
#if HYBIRDCLR_ENABLED
using HybridCLR.Editor.Installer;
#endif
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Windows;

namespace Holo.XR.Editor
{
    public class HoloMenu : EditorUtility
    {
        #region Test
        //        [MenuItem("Holo-XR/Build Bundle - B50R", false)]
        //        static void BuildBundle()
        //        {
        //            string parentFolderPath = Application.streamingAssetsPath;
        //            //string name = GetFormattedTimestamp();
        //            // ��ȡ��ǰ����·��
        //            string scenePath = EditorSceneManager.GetActiveScene().path;
        //            // ���ݳ���������AssetBundle������
        //            string name = "scene_" + System.IO.Path.GetFileNameWithoutExtension(scenePath).ToLower();

        //            string folderPath = parentFolderPath + "/" + name;

        //            if (!File.Exists(folderPath))
        //            {
        //                Directory.CreateDirectory(folderPath);
        //            }

        //#if UNITY_ANDROID//��׿��
        //            Debug.Log("��׿ƽ̨����ɹ�");
        //            BuildPipeline.BuildAssetBundles(folderPath,
        //            BuildAssetBundleOptions.ChunkBasedCompression,
        //            BuildTarget.Android);
        //#elif UNITY_IPHONE//IOS
        //         Debug.Log("IOSƽ̨����ɹ�");
        //        BulidPipeline.BulidAssetBundles(folderPath,
        //        BulidAssetBundleOptions.ChunkBasedCompression ,
        //        BulidTarget.iOS);
        //#elif UNITY_STANDALONE_WIN || UNITY_EDITOR//PC����༭��
        //        Debug.Log("PCƽ̨����ɹ�");
        //        BuildPipeline.BuildAssetBundles(folderPath,
        //        BuildAssetBundleOptions.ChunkBasedCompression ,
        //        BuildTarget.StandaloneWindows);
        //#endif
        //            ////����������Ϣ,ע�⣺�����ص�C#�ű��޷�����
        //            //Scene activeScene = SceneManager.GetActiveScene();
        //            //SceneInfoSaver.ExportSceneJson(activeScene, folderPath);

        //            //ZipWrapper.Zip(new string[] { folderPath }, folderPath + ".zip", null, null);
        //            //EditorUtility.DisplayDialog("��ʾ", "����ɹ�!\n��������:"+activeScene.name+"\n�ļ�·��:\n" + folderPath + ".zip", "ok");

        //#if UNITY_EDITOR_WIN
        //            string localPath = parentFolderPath.Replace('/', '\\');
        //            System.Diagnostics.Process.Start("explorer.exe", localPath);
        //#endif
        //        }
        #endregion


        #region XVisio Config
#if ENGINE_XVISIO
        [MenuItem("Holo-XR/XVisio Config/Import MapLoader", false, 104)]
        static void ImportMapLoader()
        {
            // �����Զ��嵯�������óߴ�
            Rect windowRect = new Rect(100, 100, 360, 136);
            EditWindowXvCslamMapControl window = EditorWindow.GetWindowWithRect<EditWindowXvCslamMapControl>(windowRect, true, "���ݵ���");
            window.Show();
        }

        [MenuItem("Holo-XR/XVisio Config/Import MapScanner", false, 105)]
        static void ImportMapScanner()
        {
            XvPrefabsCreator.ImportMapScanner(null, null);
        }

        [MenuItem("Holo-XR/XVisio Config/Import Default Config", false, 101)]
        static void ImportDefaultConfig()
        {
            XvPrefabsCreator.ImportXvManager();
            XvPrefabsCreator.ImportGesture();
            XvPrefabsCreator.ImportXvThrowScene();
        }
#endif

        #endregion

        #region ARCore Config
#if ENGINE_ARCORE

#endif

#endregion


        #region HotUpdate 


        [MenuItem("Holo-XR/HotUpdate/Import Dll Loader", false, 201)]
        static void ImportDllLoader()
        {
            HURComponentCreator.ImportDllLoader();
        }



        [MenuItem("Holo-XR/HotUpdate/Import HybridCLR", false, 301)]
        static void InstallHybirdCLR()
        {
            HybridCLRInstaller.Import();
        }

#if HYBIRDCLR_ENABLED

        [MenuItem("Holo-XR/HotUpdate/HybridCLR Installer...", priority = 302)]
        private static void Open()
        {
            InstallerWindow window = EditorWindow.GetWindow<InstallerWindow>("HybridCLR Installer", true);
            window.minSize = new Vector2(800f, 500f);
        }

        [MenuItem("Holo-XR/HotUpdate/HybridCLR Settings...", priority = 303)]
        private static void OpenSettings() => SettingsService.OpenProjectSettings("Project/HybridCLR Settings");

#endif

        [MenuItem("Holo-XR/CleanCache", false, 401)]
        private static void CleanCache()
        {
            string outPutPath = Application.streamingAssetsPath + ExportUtils.hotUpdatePath;

            if (System.IO.Directory.Exists(outPutPath))
            {
                // ɾ�������ļ�
                foreach (string file in System.IO.Directory.GetFiles(outPutPath))
                {
                    File.Delete(file);
                }

                // �ݹ�ɾ���������ļ��к����ǵ�����
                foreach (string directory in System.IO.Directory.GetDirectories(outPutPath))
                {
                    System.IO.Directory.Delete(directory, true);
                }
            }
            AssetDatabase.Refresh();
            Debug.Log("�����:" + outPutPath);
        }

        [MenuItem("Holo-XR/BuildBundle-Android", false, 402)]
        private static void BuildBundle_Android()
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
