using Holo.XR.Editor.UX;
using UnityEditor;
using UnityEngine;

namespace Holo.XR.Editor.Utils
{
    /// <summary>
    /// 热更组件Creator
    /// </summary>
    class HURComponentCreator : BaseCreator
    {

        private const string DllLoaderTag = "DllLoader";
        /// <summary>
        /// 导入DLL加载器
        /// </summary>
        internal static void ImportDllLoader()
        {
#if HYBIRDCLR_ENABLED
            string tag = CheckTag(DllLoaderTag);
            GameObject dllLoader = GameObject.FindWithTag(tag);
            if (dllLoader != null)
            {
                PopWindow.Show("对象已存在\n请查看\"" + dllLoader.name + "\"子节点",200,80);
                return;
            }
            GameObject[] obj = GetHoloRootNode();
            GameObject mapObj = obj[0];

            dllLoader = CreateObject("Prefabs/HUR/DllLoader");
            dllLoader.transform.parent = mapObj.transform;
            dllLoader.tag = tag;
            PopWindow.Show("已添加至\"" + mapObj.name + "\"", 200, 80);
#else
            PopWindow.Show("未启用热更新",200,80);
#endif
        }

        internal static void ExportDllAndAssetsBundle()
        {
            SceneExportWindow win = EditorWindow.GetWindow<SceneExportWindow>(false, "Export-Settings");
            win.Show();
        }
    }
}