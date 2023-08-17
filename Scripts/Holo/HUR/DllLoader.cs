
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.IO;
using System.Reflection;
using LitJson;
using Holo.Data;
using Holo.XR.Android;
using UnityEngine.SceneManagement;

#if HYBIRDCLR_ENABLED
using HybridCLR;
#endif

namespace Holo.HUR
{
    /// <summary>
    /// 进度更新
    /// </summary>
    /// <param name="progress"></param>
    /// <param name="currentDownloadAssets"></param>
    public delegate void ProgressUpdateDelegate(int currentIndex,int total, string currentDownloadAssets);

    /// <summary>
    /// DLL加载器
    /// <inheritdoc>用于加载热更内容</inheritdoc>
    /// </summary>
    public class DllLoader : MonoBehaviour
    {
        [Header("组件启动时，自动读取热更数据")]
        public bool autoReadData = false;

        [Header("资源加载完成后，自动加载入口场景")]
        public bool autoEnter = true;

        [Header("资源加载完成后执行事件")]
        public UnityEvent loadComplete;

        [Header("热更新 AssetsBundle")]
        public List<string> assetsBundleNameList = new List<string>();

        [Header("热更新 DLL")]
        public List<string> hotUpdateAssemblyNameList = new List<string>();

        [Header("补充元数据AOT dlls")]
        public List<string> patchAOT_Assemblies = new List<string>();

        //持久化Data路径
        private string localFolderPath;

        //热更时的主场景名称
        private string hotUpdateMainSceneName;

        private void Awake()
        {
            localFolderPath = Application.persistentDataPath + XR.Config.HoloConfig.hotUpdateDataFolder;
        }

        public event ProgressUpdateDelegate OnProgressUpdate;

        void Start()
        {
            if (autoReadData)
            {
                StartReadData();
            }
        }

        public void StartReadData()
        {
            //读取配置文件中的主场景名称
            string cfgJsonPath = localFolderPath + XR.Config.HoloConfig.sceneConfig;
            if (!File.Exists(cfgJsonPath))
            {
#if DEBUG
                AndroidUtils.GetInstance().ShowToast("缺少热更数据包");
#endif
                //默认入口为“Main”
                this.hotUpdateMainSceneName = "Main";
            }
            else
            {
                string jsonStr = File.ReadAllText(cfgJsonPath);
                SceneEntity sceneEntity = JsonMapper.ToObject<SceneEntity>(jsonStr);
                this.hotUpdateMainSceneName = sceneEntity.MainScene;
#if DEBUG_LOG
                EqLog.d("DllLoader-Start-MainSceneName:", hotUpdateMainSceneName);
#endif
            }


            StartCoroutine(LoadAssets(this.OnLoadComplete));
        }

        #region read assets

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="onDownloadComplete">资源加载完成后执行任务</param>
        /// <returns></returns>
        private IEnumerator LoadAssets(Action onDownloadComplete)
        {
            int max = patchAOT_Assemblies.Count + hotUpdateAssemblyNameList.Count + assetsBundleNameList.Count;
            //注意：加载顺序，AOTMetaAssemblt->热更dll->AB包
            int count = 0;
            foreach (var item in patchAOT_Assemblies)
            {
                count++;
                //更新进度
                if (OnProgressUpdate != null)
                {
                    OnProgressUpdate(count, max, item);
                }

                ReadDataFromPersistent(item + ".dll.bytes", AssetsType.AOT_META_ASSEMBLY);
            }

            //热更DLL
            foreach (var item in hotUpdateAssemblyNameList)
            {
                count++;
                //更新进度
                if (OnProgressUpdate != null)
                {
                    OnProgressUpdate(count, max, item);
                }

                ReadDataFromPersistent(item + ".dll.bytes", AssetsType.HOT_UPDATE_ASSEMBLY);
            }

            foreach (var item in assetsBundleNameList)
            {
                count++;
                //更新进度
                if (OnProgressUpdate != null)
                {
                    OnProgressUpdate(count, max, item);
                }

                ReadDataFromPersistent(item, AssetsType.ASSETS_BUNDLE);
            }


            //进度100%
            if (OnProgressUpdate != null)
            {
                OnProgressUpdate(max, max, null);
            }

            onDownloadComplete();

            yield return null;
        }

        private void ReadDataFromPersistent(string asset,AssetsType type)
        {

#if HYBIRDCLR_ENABLED
            string dllPath = localFolderPath + asset;
            if (!File.Exists(dllPath))
            {
#if DEBUG_LOG
                EqLog.w("DllLoader", "Caould not find " + asset);
#endif
                return;
            }
            byte[] data = DataIO.Read(dllPath,type.ToString());

            switch (type)
            {
                case AssetsType.ASSETS_BUNDLE:
                    //加载AB包
                    AssetBundleManager.Instance.LoadAB(asset,data);
#if DEBUG_LOG
                    Debug.Log($"LoadAssetsBundle:{asset}");
#endif
                    break;
                case AssetsType.AOT_META_ASSEMBLY:
                    //加载补充元数据
                    /// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
                    /// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
                    /// 
                    HomologousImageMode mode = HomologousImageMode.SuperSet;

                    // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
                    LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(data, mode);
#if DEBUG_LOG
                    Debug.Log($"LoadMetadataForAOTAssembly:{asset}. mode:{mode} ret:{err}");
#endif
                    break;
                case AssetsType.HOT_UPDATE_ASSEMBLY:
                    Assembly.Load(data);
#if DEBUG_LOG
                    Debug.Log($"LoadHotUpdateAssembly:{asset}");
#endif
                    break;
            }
            data = null;
#endif
        }


        #endregion



        void OnLoadComplete()
        {

            if (loadComplete != null)
            {
                loadComplete.Invoke();
            }

            if (autoEnter)
            {
                Invoke("ToMainScene", 0.5f);
            }
        }

        private void ToMainScene()
        {
#if DEBUG_LOG
            Debug.Log($"SceneManager.LoadScene({hotUpdateMainSceneName})");
#endif
            SceneManager.LoadScene(hotUpdateMainSceneName, LoadSceneMode.Single);
        }

    }


}