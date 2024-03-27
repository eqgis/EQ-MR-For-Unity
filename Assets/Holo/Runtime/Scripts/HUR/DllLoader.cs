
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
    /// ���ݶ�ȡ�Ľ��ȸ���
    /// </summary>
    /// <param name="progress">����</param>
    public delegate void ProgressUpdateDelegate(float progress);

    /// <summary>
    /// DLL������
    /// <inheritdoc>���ڼ����ȸ�����</inheritdoc>
    /// </summary>
    public class DllLoader : MonoBehaviour
    {
        [Header("�������ʱ���Զ���ȡ�ȸ�����")]
        public bool autoReadData = false;

        [Header("��Դ������ɺ��Զ�������ڳ���")]
        public bool autoEnter = true;

        [Header("��Դ������ɺ�ִ���¼�")]
        public UnityEvent loadComplete;

        //�ȸ��� AssetsBundle
        private List<string> assetsBundleNameList;

        //�ȸ��� DLL
        private List<string> hotUpdateAssemblyNameList;
        //����Ԫ����AOT dlls
        private List<string> patchAOT_Assemblies;

        //�־û�Data·��
        private string localFolderPath;

        //�ȸ�ʱ������������
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
        /// ���ȸ���
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
        /// ��ʼ��ȡ����
        /// </summary>
        public void StartReadData()
        {
            //��ȡ�����ļ��е�����������
            string cfgJsonPath = localFolderPath + XR.Config.HoloConfig.sceneConfig;
            if (!File.Exists(cfgJsonPath))
            {
#if DEBUG
                AndroidUtils.GetInstance().ShowToast("ȱ���ȸ����ݰ�");
#endif
            }
            //��ȡ������Դ·����ע�⣺����֮ǰ��ȡ����AssetsPackageManager.Instance�򲻻��ٴζ�ȡ
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
        /// ������Դ
        /// </summary>
        /// <param name="onDownloadComplete">��Դ������ɺ�ִ������</param>
        /// <returns></returns>
        private IEnumerator LoadAssets(Action onDownloadComplete)
        {
            int max = patchAOT_Assemblies.Count + hotUpdateAssemblyNameList.Count + assetsBundleNameList.Count;
            //���½���
            if (OnProgressUpdate != null)
            {
                OnProgressUpdate(0f);
            }

            //ע�⣺����˳��AOTMetaAssemblt->�ȸ�dll->AB��
            int count = 0;
            foreach (var item in patchAOT_Assemblies)
            {
                count++;
                //���½���
                if (OnProgressUpdate != null)
                {
                    //OnProgressUpdate(count, max, item);
                    OnProgressUpdate((float)count / max);
                }

                ReadDataFromPersistent(item + ".dll.bytes", AssetsType.AOT_META_ASSEMBLY);
            }

            //�ȸ�DLL
            foreach (var item in hotUpdateAssemblyNameList)
            {
                count++;
                //���½���
                if (OnProgressUpdate != null)
                {
                    OnProgressUpdate((float)count / max);
                }

                ReadDataFromPersistent(item + ".dll.bytes", AssetsType.HOT_UPDATE_ASSEMBLY);
            }


            foreach (var item in assetsBundleNameList)
            {
                count++;
                //���½���
                if (OnProgressUpdate != null)
                {
                    OnProgressUpdate((float)count / max);
                }

                ReadDataFromPersistent(item, AssetsType.ASSETS_BUNDLE);
            }


            //����100%
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
                        //����AB��
                        AssetBundleManager.Instance.LoadAB(asset, data);
#if DEBUG_LOG
                        Debug.Log($"LoadAssetsBundle:{asset}");
#endif
                        break;
                    case AssetsType.AOT_META_ASSEMBLY:
                        //���ز���Ԫ����
                        /// ע�⣬����Ԫ�����Ǹ�AOT dll����Ԫ���ݣ������Ǹ��ȸ���dll����Ԫ���ݡ�
                        /// �ȸ���dll��ȱԪ���ݣ�����Ҫ���䣬�������LoadMetadataForAOTAssembly�᷵�ش���
                        /// 

                        HomologousImageMode mode = HomologousImageMode.SuperSet;

                        // ����assembly��Ӧ��dll�����Զ�Ϊ��hook��һ��aot���ͺ�����native���������ڣ��ý������汾����
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
        /// ��ȡ��ڳ�������
        /// </summary>
        /// <returns>��ڳ�������</returns>
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