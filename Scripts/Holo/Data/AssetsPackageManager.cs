using Holo.Data;
using Holo.XR.Android;
using LitJson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Holo.HUR
{
    /// <summary>
    /// 资源包管理器
    /// </summary>
    class AssetsPackageManager
    {
        private static readonly object lockObject = new object();
        private static AssetsPackageManager instance = null;

        private HoloSceneConfig sceneEntity = null;
        private bool loaded = false;

        //编辑器内置资源加载状态
        private bool editorEnnerResourceLoaded = false;

        private AssetsPackageManager()
        {

        }

        /// <summary>
        /// 获取单例
        /// </summary>
        public static AssetsPackageManager Instance
        {
            get
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new AssetsPackageManager();
                    }
                    return instance;
                }
            }
        }

        /// <summary>
        /// 加载场景配置
        /// </summary>
        /// <param name="sceneConfigPath"></param>
        public AssetsPackageManager LoadSceneConfig(string sceneConfigPath)
        {
            if (loaded)return this;

            lock (lockObject)
            {
                if (!File.Exists(sceneConfigPath))
                {
                    //文件不存在
                    return this;
                }

                byte[] bytes = DataIO.ReadFromPath(sceneConfigPath);
                string dataStr = Encoding.UTF8.GetString(bytes);
#if DEBUG
                EqLog.i("APMgr", dataStr);
#endif
                sceneEntity = JsonMapper.ToObject<HoloSceneConfig>(dataStr);
                loaded = true;
                return this;
            }
        }

        /// <summary>
        /// 获取文件清单
        /// </summary>
        /// <returns></returns>
        public List<string> GetFileList()
        {
            if (!loaded)
            {
                return new List<string>();
            }

            return sceneEntity.FileList;
        }

        /// <summary>
        /// 获取AB包资源列表
        /// </summary>
        /// <returns></returns>
        public List<string> GetAssetsBundleList()
        {
            if (!loaded)
            {
                return new List<string>();
            }

            return sceneEntity.AssetsBundleList;
        }

        /// <summary>
        /// 获取热更DLL资源列表
        /// </summary>
        /// <returns></returns>
        public List<string> GetHotUpdateAssemblies()
        {
            if (!loaded)
            {
                return new List<string>();
            }

            return sceneEntity.HotUpdateAssemblies;
        }

        /// <summary>
        /// 获取AOT元数据资源列表
        /// </summary>
        /// <returns></returns>
        public List<string> GetAotMetaAssemblies()
        {
            if (!loaded)
            {
                return new List<string>();
            }

            return sceneEntity.AotMetaAssemblies;
        }

        /// <summary>
        /// 获取入口场景名称
        /// </summary>
        /// <returns></returns>
        public string GetMainSceneName()
        {
            if (!loaded)
            {
                //默认值
                return "Main";
            }
            return sceneEntity.MainScene;
        }

        /// <summary>
        /// 加载Unity编辑器内置资源
        /// </summary>
        public void LoadEditorInnerResource()
        {
            if (editorEnnerResourceLoaded) return;
            try
            {
#if UNITY_EDITOR
                UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Library/unity default resources");
                UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Resources/unity_builtin_extra");
#endif
            }
            catch (Exception e)
            {
                Debug.LogError("loadInnerResource: " + e.Message);
#if DEBUG_LOG
                EqLog.w("DllLoader", "loadInnerResource: " + e.Message);
#endif
            }
            finally
            {
                editorEnnerResourceLoaded = true;
            }
            //AssetDatabase.LoadAllAssetsAtPath("Library/unity editor resources");
        }
    }
}