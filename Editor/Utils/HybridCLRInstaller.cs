using Holo.XR.Editor.UX;
using Holo.XR.Utils;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
using UnityEditorInternal;
using UnityEngine;
using System.Threading;
using System;
using System.Reflection;

namespace Holo.XR.Editor.Utils
{
    class HybridCLRInstaller
    {
        internal static void Import()
        {
            if (HasImportHybridCLR())
            {

                if (HasInstalledHybridCLR())
                {
                    PopWindow.Show("HybridCLR �Ѵ���\n����ִ�й���install��", 200, 80);
                }
                else
                {
                    PopWindow.Show("HybridCLR �Ѵ���\n��δִ�С�install��", 200, 80);
                }
                return;
            }

            //"./Assets"
            string dataPath = Application.dataPath;
            string hybridPath = dataPath + "/Holo/Editor/3rds/hybridclr";
            string clrTargetPath = dataPath + "/../Packages";
            if (unZip(hybridPath, clrTargetPath))
            {
                // ִ���ļ����Ʋ�����
                Thread.Sleep(1000); // �ӳ�1��
                AssetDatabase.Refresh();
                UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
                //PopWindow.Show("HybridCLR �������!\n��ִ�� HybridCLR->Installer...", 200, 80);

                string il2cppPath = dataPath + "/Holo/Editor/3rds/libil2cpp";
                string targetPath = dataPath + "/../HybridCLRData/il2cpp_local";
                if (unZip(il2cppPath,targetPath))
                {
                    Debug.Log("HybridCLR �������!");
                    PopWindow.Show("HybridCLR �������!��", 200, 80);
                }
                else
                {
                    Debug.LogWarning("���ڲ˵���ִ�� HybridCLR->Installer...");
                    PopWindow.Show("HybridCLR �������!\n��ִ�� HybridCLR->Installer...", 200, 80);
                }

                // ִ���ļ����Ʋ�����
                Thread.Sleep(1000); // �ӳ�1��
                AssetDatabase.Refresh();
                //UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();

#if UNITY_EDITOR_WIN
                string localPath = clrTargetPath.Replace('/', '\\');
                System.Diagnostics.Process.Start("explorer.exe", localPath);
#endif
            }
            else
            {
                Debug.LogError("ȱ��hybridclr��װ��������hybriclr�ļ��Ƿ�ʧ���ļ�·����" + hybridPath);
                Debug.LogWarning("��ο�Hybridclr�ĵ��ֶ���װ��https://hybridclr.doc.code-philosophy.com/docs/beginner/quickstart\"");
                PopWindow.Show("�������", 200, 80);
            }
        }

        private static bool unZip(string hybridPath, string targetPath)
        {
            if (!File.Exists(hybridPath))
            {
                return false;
            }
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }
            ZipHelper.Instance.UnzipFile(hybridPath, targetPath);
            return true;
        }

        /// <summary>
        /// �ж�HybridCLR�Ƿ������install����
        /// </summary>
        /// <returns></returns>
        static bool HasInstalledHybridCLR()
        {
            return Directory.Exists($"{LocalIl2CppDir}/libil2cpp/hybridclr");
        }

        static bool HasImportHybridCLR()
        {
            return Directory.Exists($"{ProjectDir}/Packages/com.code-philosophy.hybridclr@3.4.1");
        }

        public static string ProjectDir { get; } = Directory.GetParent(Application.dataPath).ToString();

        public static string HybridCLRDataDir => ProjectDir + "/HybridCLRData";

        public static string LocalUnityDataDir => $"{HybridCLRDataDir}/LocalIl2CppData-{Application.platform}";

        public static string LocalIl2CppDir => LocalUnityDataDir + "/il2cpp";

        public static string GeneratedCppDir => LocalIl2CppDir + "/libil2cpp/hybridclr/generated";
    }
}