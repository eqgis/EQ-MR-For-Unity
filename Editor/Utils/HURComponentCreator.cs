using Holo.HUR;
using Holo.XR.Editor.UX;
#if HYBIRDCLR_ENABLED
using HybridCLR.Editor;
#endif
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
                PopWindow.Show("�����Ѵ���\n��鿴\"" + dllLoader.name + "\"�ڵ�", 200, 80);
                return;
            }
            GameObject[] obj = GetHoloRootNode();
            GameObject mapObj = obj[0];

            //dllLoader = CreateObject("Prefabs/HUR/DllLoader");
            dllLoader = new GameObject(DllLoaderTag);
            //��ӡ�DllLoader�����
            dllLoader.AddComponent<DllLoader>();
            //����Ĭ�ϵ��ȸ�����
            DllLoader loader = dllLoader.GetComponent<DllLoader>();
            loader.assetsBundleNameList.Add(Holo.XR.Config.EditorConfig.GetHotUpdateAbName());

            //ͬ���ȸ�DLL�б�
            foreach (var item in SettingsUtil.HotUpdateAssemblyFilesExcludePreserved)
            {
                if (item.EndsWith(".dll"))
                {
                    loader.hotUpdateAssemblyNameList.Add(item.Substring(0, item.Length - 4));
                }
                else
                {
                    loader.hotUpdateAssemblyNameList.Add(item);
                }
            }

            //ͬ��AOT Ԫ�����б�
            foreach (var item in SettingsUtil.AOTAssemblyNames)
            {
                loader.patchAOT_Assemblies.Add(item);
            }

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