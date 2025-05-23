using Holo.XR.Editor.UX;
#if HYBIRDCLR_ENABLED
using Holo.Data;
using Holo.HUR;
using HybridCLR.Editor;
using System;
#endif
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
        private const string DataDownLoaderTag = "DataDownLoader";
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
                PopWindow.Show("对象已存在\n请查看\"" + dllLoader.name + "\"节点", 200, 80);
                return;
            }
            GameObject[] obj = GetHoloRootNode();
            GameObject mapObj = obj[0];

            //dllLoader = CreateObject("Prefabs/HUR/DllLoader");
            dllLoader = new GameObject(DllLoaderTag);
            //添加“DllLoader”组件
            dllLoader.AddComponent<DllLoader>();
            //给定默认的热更配置
            DllLoader loader = dllLoader.GetComponent<DllLoader>();

            //同步资源列表

            //同步热更DLL列表
            //foreach (var item in SettingsUtil.HotUpdateAssemblyFilesExcludePreserved)
            //{
            //    if (item.EndsWith(".dll"))
            //    {
            //        loader.hotUpdateAssemblyNameList.Add(item.Substring(0, item.Length - 4));
            //    }
            //    else
            //    {
            //        loader.hotUpdateAssemblyNameList.Add(item);
            //    }
            //}

            //同步AOT 元数据列表
            //foreach (var item in SettingsUtil.AOTAssemblyNames)
            //{
            //    loader.patchAOT_Assemblies.Add(item);
            //}

            dllLoader.transform.parent = mapObj.transform;
            dllLoader.tag = tag;
            PopWindow.Show("已添加至\"" + mapObj.name + "\"", 200, 80);
#else
            PopWindow.Show("未启用热更新",200,80);
#endif
        }

        internal static void ExportDllAndAssetsBundle()
        {
            SceneExportWindow win = EditorWindow.GetWindow<SceneExportWindow>(true, "Export-Settings");
            win.Show();
        }

        internal static void ImportDataDownloader()
        {
#if HYBIRDCLR_ENABLED
            string tag = CheckTag(DataDownLoaderTag);
            GameObject datadownloader = GameObject.FindWithTag(tag);
            if (datadownloader != null)
            {
                PopWindow.Show("对象已存在\n请查看\"" + datadownloader.name + "\"节点", 200, 80);
                return;
            }
            GameObject[] obj = GetHoloRootNode();
            GameObject mapObj = obj[0];

            datadownloader = new GameObject(DataDownLoaderTag);
            //添加“DataDownLoader”组件
            datadownloader.AddComponent<DataDownLoader>();

            datadownloader.transform.parent = mapObj.transform;
            datadownloader.tag = tag;
            PopWindow.Show("已添加至\"" + mapObj.name + "\"", 200, 80);
#else
            PopWindow.Show("未启用热更新",200,80);
#endif
        }
    }
}