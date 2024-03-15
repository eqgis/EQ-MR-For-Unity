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
                PopWindow.Show("ARCore Extensions 已存在\n\n请勿重复安装 ！", 200, 80);
                return;
            }

            //"./Assets"
            string dataPath = Application.dataPath;
            string arcoreExtensions = dataPath + "/Holo/Editor/3rds/arcore_extensions";
            string targetPath = dataPath + "/../Packages";
            if (unZip(arcoreExtensions, targetPath))
            {
                // 执行文件复制操作后
                Thread.Sleep(1000); // 延迟1秒
                AssetDatabase.Refresh();
                UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
                //PopWindow.Show("HybridCLR 导入完成!\n请执行 HybridCLR->Installer...", 200, 80);


                Debug.Log("ARCoreExtensions 导入完成!");
                PopWindow.Show("ARCoreExtensions 导入完成!”", 200, 80);

                // 执行文件复制操作后
                Thread.Sleep(1000); // 延迟1秒
                AssetDatabase.Refresh();
                //UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();

#if UNITY_EDITOR_WIN
                string localPath = targetPath.Replace('/', '\\');
                System.Diagnostics.Process.Start("explorer.exe", localPath);
#endif
            }
            else
            {
                Debug.LogError("缺少ARCoreExtensions安装包，请检查arcore_extensions文件是否丢失。文件路径：" + arcoreExtensions);
                PopWindow.Show("导入出错！", 200, 80);
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