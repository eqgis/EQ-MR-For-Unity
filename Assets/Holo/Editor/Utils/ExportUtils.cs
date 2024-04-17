
using Holo.HUR;
#if HYBIRDCLR_ENABLED
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
#endif
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Holo.XR.Editor.Utils
{
    /// <summary>
    /// ��������
    /// </summary>
    class ExportUtils
    {
        private const string ExportConfigTitle = "Holo";

        internal const string hotUpdatePath = "/output";

        /*==============�ڲ�����======================*/

        /// <summary>
        /// ִ�е�������
        /// </summary>
        internal static void ExecExport()
        {
            if (!CheckEnvironment())
            {
                return;
            }
            //���û�б��泡����ʾ
            UnityEngine.SceneManagement.Scene currentScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();

            if (string.IsNullOrEmpty(currentScene.path))
            {
                EditorUtility.DisplayDialog("��ʾ", "���ǰ���ȱ��泡��", "OK");
                return;
            }
            // ����ǰ�ȱ��泡��
            if (string.Empty != currentScene.name)
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(currentScene);

            //�������ļ���
            string outPutPath = Application.streamingAssetsPath + hotUpdatePath;

            if (File.Exists(outPutPath))
            {
                //ɾ��������
                File.Delete(outPutPath);
            }
            Directory.CreateDirectory(outPutPath);

            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;

#if HYBIRDCLR_ENABLED
            CompileDllCommand.CompileDll(target);

            //CopyABAOTHotUpdateDlls(target);
            CopyAOTAssembliesToStreamingAssets();
            //�����ȸ�DLL��SAĿ¼
            CopyHotUpdateAssembliesToStreamingAssets();
#endif
        }

        /// <summary>
        /// �����ȸ�DLL
        /// </summary>
        public static void CopyHotUpdateAssembliesToStreamingAssets()
        {
#if HYBIRDCLR_ENABLED
            var target = EditorUserBuildSettings.activeBuildTarget;

            string hotfixDllSrcDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
            string hotfixAssembliesDstDir = Application.streamingAssetsPath + hotUpdatePath;
            foreach (var dll in SettingsUtil.HotUpdateAssemblyFilesExcludePreserved)
            {
                string dllPath = $"{hotfixDllSrcDir}/{dll}";
                string dllBytesPath = $"{hotfixAssembliesDstDir}/{dll}.bytes";
                Copy(dllPath, dllBytesPath);//�����ȸ�DLL
                Debug.Log($"[CopyHotUpdateAssembliesToStreamingAssets] copy hotfix dll {dllPath} -> {dllBytesPath}");
            }
#endif
        }

        /// <summary>
        /// ����AOTԪ����
        /// </summary>
        public static void CopyAOTAssembliesToStreamingAssets()
        {
#if HYBIRDCLR_ENABLED
            var target = EditorUserBuildSettings.activeBuildTarget;
            string aotAssembliesSrcDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(target);
            string aotAssembliesDstDir = Application.streamingAssetsPath + hotUpdatePath;

            foreach (var dll in SettingsUtil.AOTAssemblyNames)
            {
                string srcDllPath = $"{aotAssembliesSrcDir}/{dll}.dll";
                if (!File.Exists(srcDllPath))
                {
                    Debug.LogError($"ab�����AOT����Ԫ����dll:{srcDllPath} ʱ��������,�ļ������ڡ��ü����AOT dll��BuildPlayerʱ�������ɣ������Ҫ���ȹ���һ����ϷApp���ٴ����");
                    continue;
                }
                string dllBytesPath = $"{aotAssembliesDstDir}/{dll}.dll.bytes";
                Copy(srcDllPath, dllBytesPath);//AOTԪ����
                Debug.Log($"[CopyAOTAssembliesToStreamingAssets] copy AOT dll {srcDllPath} -> {dllBytesPath}");
            }
#endif
        }

        /// <summary>
        /// ���ƽ̨
        /// </summary>
        /// <returns></returns>
        static bool CheckEnvironment()
        {
#if UNITY_EDITOR
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.iOS && EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                if (EditorUtility.DisplayDialog(ExportConfigTitle,
                "��Դ����������Ҫ�л����ƶ�Ŀ��ƽ̨���ܼ������Ƿ��л���", "��", "��"))
                {
                    return false;
                }
                else
                {
                    int index = EditorUtility.DisplayDialogComplex(ExportConfigTitle,
                    "�л���iOS����Android��������û�����𣬿���ѡ��һ��", "iOS", "ȡ��", "Android");

                    if (0 == index)
                        if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS))
                        {
                            EditorUtility.DisplayDialog(ExportConfigTitle, "��������iOSƽ̨��Դ��", "OK");
                            return false;
                        }
                        else
                            Debug.Log("�ѳɹ��л���iOSƽ̨��");
                    else if (2 == index)
                        if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
                        {
                            EditorUtility.DisplayDialog(ExportConfigTitle, "��������Androidƽ̨��Դ��", "OK");
                            return false;
                        }
                        else
                            Debug.Log("�ѳɹ��л���Androidƽ̨��");
                    else
                        return false;
                }
            }

            return true;
#else
				return false;
#endif
        }


        /// <summary>
        /// �����ݽ��м��ܲ�����
        /// </summary>
        /// <param name="srcFilePath"></param>
        /// <param name="encryptedFilePath"></param>
        internal static void Copy(string srcFilePath, string encryptedFilePath)
        {
            DataIO.Copy(srcFilePath, encryptedFilePath);
        }

    }

}