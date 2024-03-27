
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.IO;
using System.Reflection;
using Holo.Data;
using Holo.XR.Android;
using UnityEngine.SceneManagement;

#if HYBIRDCLR_ENABLED
using HybridCLR;
#endif

namespace Holo.HUR
{
    /// <summary>
    /// 数据读取的进度更新
    /// </summary>
    /// <param name="progress">进度</param>
    public delegate void ProgressUpdateDelegate(float progress);

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

        //热更新 AssetsBundle
        private List<string> assetsBundleNameList;

        //热更新 DLL
        private List<string> hotUpdateAssemblyNameList;
        //补充元数据AOT dlls
        private List<string> patchAOT_Assemblies;

        //持久化Data路径
        private string localFolderPath;

        //热更时的主场景名称
        private string hotUpdateMainSceneName;

        private void Awake()
        {
            localFolderPath = Application.persistentDataPath + XR.Config.HoloConfig.hotUpdateDataFolder;

#if DEBUG_LOG
            if (OnProgressUpdate == null)
            {
                OnProgressUpdate += OnProgressUpdateDebug;
            }
#endif
        }

        /// <summary>
        /// 进度更新
        /// </summary>
        public event ProgressUpdateDelegate OnProgressUpdate;


        void OnProgressUpdateDebug(float progress)
        {
            int pro = (int)(progress * 100);
            EqLog.d("DllLoader", "loading  " + pro + " %");
        }

        void Start()
        {
            if (autoReadData)
            {
                StartReadData();
            }
        }

        /// <summary>
        /// 开始读取数据
        /// </summary>
        public void StartReadData()
        {
            //读取配置文件中的主场景名称
            string cfgJsonPath = localFolderPath + XR.Config.HoloConfig.sceneConfig;
            if (!File.Exists(cfgJsonPath))
            {
#if DEBUG
                AndroidUtils.GetInstance().ShowToast("缺少热更数据包");
#endif
            }
            //读取配置资源路径，注意：若这之前读取过，AssetsPackageManager.Instance则不会再次读取
            AssetsPackageManager apMgr = AssetsPackageManager.Instance;
            apMgr.LoadSceneConfig(cfgJsonPath);
            this.hotUpdateMainSceneName = apMgr.GetMainSceneName();
            this.assetsBundleNameList = apMgr.GetAssetsBundleList();
            this.hotUpdateAssemblyNameList = apMgr.GetHotUpdateAssemblies();
            this.patchAOT_Assemblies = apMgr.GetAotMetaAssemblies();
#if DEBUG_LOG
            EqLog.d("DllLoader-Start-MainSceneName:", hotUpdateMainSceneName);
#endif


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
            //更新进度
            if (OnProgressUpdate != null)
            {
                OnProgressUpdate(0f);
            }

            //注意：加载顺序，AOTMetaAssemblt->热更dll->AB包
            int count = 0;
            foreach (var item in patchAOT_Assemblies)
            {
                count++;
                //更新进度
                if (OnProgressUpdate != null)
                {
                    //OnProgressUpdate(count, max, item);
                    OnProgressUpdate((float)count / max);
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
                    OnProgressUpdate((float)count / max);
                }

                ReadDataFromPersistent(item + ".dll.bytes", AssetsType.HOT_UPDATE_ASSEMBLY);
            }


            foreach (var item in assetsBundleNameList)
            {
                count++;
                //更新进度
                if (OnProgressUpdate != null)
                {
                    OnProgressUpdate((float)count / max);
                }

                ReadDataFromPersistent(item, AssetsType.ASSETS_BUNDLE);
            }


            //进度100%
            if (OnProgressUpdate != null)
            {
                OnProgressUpdate(1.0f);
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
                EqLog.w("DllLoader", "Could not find " + asset);
#endif
                return;
            }
            try
            {
                byte[] data = DataIO.Read(dllPath, type.ToString());

                switch (type)
                {
                    case AssetsType.ASSETS_BUNDLE:
                        //加载AB包
                        AssetBundleManager.Instance.LoadAB(asset, data);
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
            }catch (Exception e)
            {
                EqLog.e("DllLoader",e.ToString());
            }
            
#endif
        }


        #endregion



        private void OnLoadComplete()
        {
#if DEBUG_LOG
            Debug.Log("OnLoadComplete");
#endif
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
            //SceneManager.LoadScene(hotUpdateMainSceneName, LoadSceneMode.Single);

            SceneManager.LoadSceneAsync(hotUpdateMainSceneName, LoadSceneMode.Single);
        }


        /// <summary>
        /// 获取入口场景名称
        /// </summary>
        /// <returns>入口场景名称</returns>
        public string getEntrance()
        {
            if(hotUpdateMainSceneName != string.Empty)
            {
                return hotUpdateMainSceneName;
            }
            throw new NullReferenceException();
        }
    }


}