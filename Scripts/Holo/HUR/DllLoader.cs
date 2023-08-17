
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
    /// ���ȸ���
    /// </summary>
    /// <param name="progress"></param>
    /// <param name="currentDownloadAssets"></param>
    public delegate void ProgressUpdateDelegate(int currentIndex,int total, string currentDownloadAssets);

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

        [Header("�ȸ��� AssetsBundle")]
        public List<string> assetsBundleNameList = new List<string>();

        [Header("�ȸ��� DLL")]
        public List<string> hotUpdateAssemblyNameList = new List<string>();

        [Header("����Ԫ����AOT dlls")]
        public List<string> patchAOT_Assemblies = new List<string>();

        //�־û�Data·��
        private string localFolderPath;

        //�ȸ�ʱ������������
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
            //��ȡ�����ļ��е�����������
            string cfgJsonPath = localFolderPath + XR.Config.HoloConfig.sceneConfig;
            if (!File.Exists(cfgJsonPath))
            {
#if DEBUG
                AndroidUtils.GetInstance().ShowToast("ȱ���ȸ����ݰ�");
#endif
                //Ĭ�����Ϊ��Main��
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
        /// ������Դ
        /// </summary>
        /// <param name="onDownloadComplete">��Դ������ɺ�ִ������</param>
        /// <returns></returns>
        private IEnumerator LoadAssets(Action onDownloadComplete)
        {
            int max = patchAOT_Assemblies.Count + hotUpdateAssemblyNameList.Count + assetsBundleNameList.Count;
            //ע�⣺����˳��AOTMetaAssemblt->�ȸ�dll->AB��
            int count = 0;
            foreach (var item in patchAOT_Assemblies)
            {
                count++;
                //���½���
                if (OnProgressUpdate != null)
                {
                    OnProgressUpdate(count, max, item);
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
                    OnProgressUpdate(count, max, item);
                }

                ReadDataFromPersistent(item + ".dll.bytes", AssetsType.HOT_UPDATE_ASSEMBLY);
            }

            foreach (var item in assetsBundleNameList)
            {
                count++;
                //���½���
                if (OnProgressUpdate != null)
                {
                    OnProgressUpdate(count, max, item);
                }

                ReadDataFromPersistent(item, AssetsType.ASSETS_BUNDLE);
            }


            //����100%
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
                    //����AB��
                    AssetBundleManager.Instance.LoadAB(asset,data);
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