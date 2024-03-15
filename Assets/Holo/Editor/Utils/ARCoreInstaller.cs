using Holo.XR.Editor.UX;
using Holo.XR.Utils;
using System.IO;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace Holo.XR.Editor.Utils
{ 
    class ARCoreInstaller
    {
        internal static void Import()
        {
            if (HasImportARCore())
            {
                PopWindow.Show("ARCore Extensions �Ѵ���\n\n�����ظ���װ ��", 200, 80);
                return;
            }

            //"./Assets"
            string dataPath = Application.dataPath;
            string arcoreExtensions = dataPath + "/Holo/Editor/3rds/arcore_extensions";
            string targetPath = dataPath + "/../Packages";
            if (unZip(arcoreExtensions, targetPath))
            {
                // ִ���ļ����Ʋ�����
                Thread.Sleep(1000); // �ӳ�1��
                AssetDatabase.Refresh();
                UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
                //PopWindow.Show("HybridCLR �������!\n��ִ�� HybridCLR->Installer...", 200, 80);


                Debug.Log("ARCoreExtensions �������!");
                PopWindow.Show("ARCoreExtensions �������!��", 200, 80);

                // ִ���ļ����Ʋ�����
                Thread.Sleep(1000); // �ӳ�1��
                AssetDatabase.Refresh();
                //UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();

#if UNITY_EDITOR_WIN
                string localPath = targetPath.Replace('/', '\\');
                System.Diagnostics.Process.Start("explorer.exe", localPath);
#endif
            }
            else
            {
                Debug.LogError("ȱ��ARCoreExtensions��װ��������arcore_extensions�ļ��Ƿ�ʧ���ļ�·����" + arcoreExtensions);
                PopWindow.Show("�������", 200, 80);
            }
        }

        private static string ProjectDir { get; } = Directory.GetParent(Application.dataPath).ToString();

        static bool HasImportARCore()
        {
            return Directory.Exists($"{ProjectDir}/Packages/arcore_extensions_v1.41");
        }

        private static bool unZip(string srcPath, string targetPath)
        {
            if (!File.Exists(srcPath))
            {
                return false;
            }
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }
            ZipHelper.Instance.UnzipFile(srcPath, targetPath);
            return true;
        }

    }
}