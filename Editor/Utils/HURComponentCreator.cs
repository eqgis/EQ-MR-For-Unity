using Holo.XR.Editor.UX;
using UnityEditor;
using UnityEngine;

namespace Holo.XR.Editor.Utils
{
    /// <summary>
    /// �ȸ����Creator
    /// </summary>
    class HURComponentCreator : BaseCreator
    {

        private const string DllLoaderTag = "DllLoader";
        /// <summary>
        /// ����DLL������
        /// </summary>
        internal static void ImportDllLoader()
        {
#if HYBIRDCLR_ENABLED
            string tag = CheckTag(DllLoaderTag);
            GameObject dllLoader = GameObject.FindWithTag(tag);
            if (dllLoader != null)
            {
                PopWindow.Show("�����Ѵ���\n��鿴\"" + dllLoader.name + "\"�ӽڵ�",200,80);
                return;
            }
            GameObject[] obj = GetHoloRootNode();
            GameObject mapObj = obj[0];

            dllLoader = CreateObject("Prefabs/HUR/DllLoader");
            dllLoader.transform.parent = mapObj.transform;
            dllLoader.tag = tag;
            PopWindow.Show("�������\"" + mapObj.name + "\"", 200, 80);
#else
            PopWindow.Show("δ�����ȸ���",200,80);
#endif
        }

        internal static void ExportDllAndAssetsBundle()
        {
            SceneExportWindow win = EditorWindow.GetWindow<SceneExportWindow>(false, "Export-Settings");
            win.Show();
        }
    }
}