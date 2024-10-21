
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
    /// 导出工具
    /// </summary>
    class ExportUtils
    {
        private const string ExportConfigTitle = "Holo";

        internal const string hotUpdatePath = "/output";

        /*==============内部方法======================*/

        /// <summary>
        /// 执行导出方法
        /// </summary>
        internal static void ExecExport()
        {
            if (!CheckEnvironment())
            {
                return;
            }
            //添加没有保存场景提示
            UnityEngine.SceneManagement.Scene currentScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();

            if (string.IsNullOrEmpty(currentScene.path))
            {
                EditorUtility.DisplayDialog("提示", "打包前请先保存场景", "OK");
                return;
            }
            // 导出前先保存场景
            if (string.Empty != currentScene.name)
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(currentScene);

            //清除输出文件夹
            string outPutPath = Application.streamingAssetsPath + hotUpdatePath;

            if (File.Exists(outPutPath))
            {
                //删除旧内容
                File.Delete(outPutPath);
            }
            Directory.CreateDirectory(outPutPath);

            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;

#if HYBIRDCLR_ENABLED
            CompileDllCommand.CompileDll(target);

            //CopyABAOTHotUpdateDlls(target);
            CopyAOTAssembliesToStreamingAssets();
            //拷贝热更DLL至SA目录
            CopyHotUpdateAssembliesToStreamingAssets();
#endif
        }

        /// <summary>
        /// 拷贝热更DLL
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
                Copy(dllPath, dllBytesPath);//加密热更DLL
                Debug.Log($"[CopyHotUpdateAssembliesToStreamingAssets] copy hotfix dll {dllPath} -> {dllBytesPath}");
            }
#endif
        }

        /// <summary>
        /// 拷贝AOT元数据
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
                    Debug.LogError($"ab中添加AOT补充元数据dll:{srcDllPath} 时发生错误,文件不存在。裁剪后的AOT dll在BuildPlayer时才能生成，因此需要你先构建一次游戏App后再打包。");
                    continue;
                }
                string dllBytesPath = $"{aotAssembliesDstDir}/{dll}.dll.bytes";
                Copy(srcDllPath, dllBytesPath);//AOT元数据
                Debug.Log($"[CopyAOTAssembliesToStreamingAssets] copy AOT dll {srcDllPath} -> {dllBytesPath}");
            }
#endif
        }

        /// <summary>
        /// 检测平台
        /// </summary>
        /// <returns></returns>
        static bool CheckEnvironment()
        {
#if UNITY_EDITOR
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.iOS && EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                if (EditorUtility.DisplayDialog(ExportConfigTitle,
                "资源导出工具需要切换到移动目标平台才能继续，是否切换？", "否", "是"))
                {
                    return false;
                }
                else
                {
                    int index = EditorUtility.DisplayDialogComplex(ExportConfigTitle,
                    "切换到iOS还是Android？（二者没有区别，可任选其一）", "iOS", "取消", "Android");

                    if (0 == index)
                        if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS))
                        {
                            EditorUtility.DisplayDialog(ExportConfigTitle, "请先下载iOS平台资源包", "OK");
                            return false;
                        }
                        else
                            Debug.Log("已成功切换到iOS平台。");
                    else if (2 == index)
                        if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
                        {
                            EditorUtility.DisplayDialog(ExportConfigTitle, "请先下载Android平台资源包", "OK");
                            return false;
                        }
                        else
                            Debug.Log("已成功切换到Android平台。");
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
        /// 对数据进行加密并拷贝
        /// </summary>
        /// <param name="srcFilePath"></param>
        /// <param name="encryptedFilePath"></param>
        internal static void Copy(string srcFilePath, string encryptedFilePath)
        {
            DataIO.Copy(srcFilePath, encryptedFilePath);
        }

    }

}