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
    /// ��Դ��������
    /// </summary>
    class AssetsPackageManager
    {
        private static readonly object lockObject = new object();
        private static AssetsPackageManager instance = null;

        private HoloSceneConfig sceneEntity = null;
        private bool loaded = false;

        //�༭��������Դ����״̬
        private bool editorEnnerResourceLoaded = false;

        private AssetsPackageManager()
        {

        }

        /// <summary>
        /// ��ȡ����
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
        /// ���س�������
        /// </summary>
        /// <param name="sceneConfigPath"></param>
        public AssetsPackageManager LoadSceneConfig(string sceneConfigPath)
        {
            if (loaded)return this;

            lock (lockObject)
            {
                if (!File.Exists(sceneConfigPath))
                {
                    //�ļ�������
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
        /// ��ȡ�ļ��嵥
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
        /// ��ȡAB����Դ�б�
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
        /// ��ȡ�ȸ�DLL��Դ�б�
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
        /// ��ȡAOTԪ������Դ�б�
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
        /// ��ȡ��ڳ�������
        /// </summary>
        /// <returns></returns>
        public string GetMainSceneName()
        {
            if (!loaded)
            {
                //Ĭ��ֵ
                return "Main";
            }
            return sceneEntity.MainScene;
        }

        /// <summary>
        /// ����Unity�༭��������Դ
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