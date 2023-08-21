using Holo.HUR;
using Holo.XR.Android;
using Holo.XR.Utils;
using LitJson;
using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Holo.Data
{
    /// <summary>
    /// ����������
    /// </summary>
    public class DataDownLoader : MonoBehaviour
    {
        [Header("������:����·��")]
        public string url;

        [Header("�Զ�������������")]
        public bool autoDownload = true;

        [Header("���ݸ�����ɺ�ִ��")]
        public UnityEvent updateDataCompleted;
        //���ݱ���·��
        private string saveFolderPath;

        /// <summary>
        /// �������ϼ����������µ���������(�ļ���)
        /// </summary>
        private string lastestDataFolderName;

        private void Awake()
        {
            //���ݱ�����ļ���·���������ȸ�����·����
            saveFolderPath = Application.persistentDataPath + Holo.XR.Config.HoloConfig.hotUpdateDataFolder;
        }

        private void Start()
        {
            url = url.TrimEnd('/');

            if (autoDownload)
            {
                Invoke("StartDownload", 0.1f);
            }
        }

        public void StartDownload()
        {
#if DEBUG
            if (Application.platform == RuntimePlatform.Android)
            {
                AndroidUtils.Toast("checking data...");
            }
#endif
            StartCoroutine(CheckDataVersion());
        }

        /// <summary>
        /// ������ݰ汾
        /// </summary>
        /// <param name="nextAction"></param>
        /// <returns></returns>
        private IEnumerator CheckDataVersion()
        {
            //�߼�����ȡ�����������°汾��Ϣ���뱾�صİ汾��Ϣ�ȶ�
            string baseUrl = url + "/" + XR.Config.HoloConfig.versionFileName;

#if UNITY_EDITOR
            Debug.Log("�汾�ļ�·��:" + baseUrl);
            Debug.Log("�����ļ�·��:" + saveFolderPath);
#endif
            using (UnityWebRequest webRequest = UnityWebRequest.Get(baseUrl))
            {
                yield return webRequest.SendWebRequest();

#if UNITY_2020_3_OR_NEWER
                if (webRequest.result != UnityWebRequest.Result.Success)
#else
                if (!(webRequest.isHttpError || webRequest.isNetworkError))
#endif
                {
#if DEBUG
                    if (Application.platform == RuntimePlatform.Android)
                    {
                        AndroidUtils.Toast("���ݰ汾У��ʧ�ܡ���������");
                    }
#endif
                    EqLog.e("DataDownLoader", "Error downloading file list: " + webRequest.error);
                    Debug.LogWarning("���ݰ汾У��ʧ�ܡ���������");
                }
                else
                {
                    string fileListText = webRequest.downloadHandler.text;
                    string[] fileLines = fileListText.Split('\n');

                    float maxVersion = 0;
                    foreach (string line in fileLines)
                    {
                        string content = Path.GetFileNameWithoutExtension(line.Trim()); // ȥ���ո�ͻ��з�

                        //��#����ͷ����������
                        if (!content.StartsWith("#"))
                        {
                            string[] parts = content.Split(new string[] { "_v" }, StringSplitOptions.None);

                            if (parts.Length == 2)
                            {
                                string prefix = parts[0];
                                string version = parts[1];
                                float versionValue = float.Parse(version);
                                if (maxVersion < versionValue)
                                {
                                    //ȡ���version
                                    maxVersion = versionValue;
                                    //�ļ������ƣ��޺�׺
                                    lastestDataFolderName = content;
                                }
                            }
                        }
                    }

                    //�������汾�ţ����ұ����ļ���,checkLocalData()����true����ʾ����Ҫ��������
                    //���ݼ�ⷵ��ֵ�������Ƿ���������
                    bool status = CheckLocalData(maxVersion);
#if DEBUG
                    if (status)
                    {
                        Debug.Log("�����Ѽ���������");
                    }
                    else
                    {
                        Debug.Log("����δ����������");
                    }
#endif
                    StartCoroutine(DownloadDataZip(!status));

                }
            }
        }

        /// <summary>
        /// �жϱ��������ļ��Ƿ����
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private bool CheckLocalData(float maxVersion)
        {
            //���ļ���·���������ڣ���ֱ�ӷ���false����ʾ������
            if (!Directory.Exists(saveFolderPath)) { return false; }

            //�����ݽ�ѹ·����ȡ�汾�ļ�
            string localVersionFilePath = saveFolderPath + XR.Config.HoloConfig.versionFileName;
            //�ж��ļ��Ƿ����
            if (!File.Exists(localVersionFilePath))
            {
                //�ļ�������
                return false;
            }

            //�汾����
            string versionContent = File.ReadAllText(localVersionFilePath);

            string[] versionList = versionContent.Split('\n');

            foreach (string item in versionList)
            {
                string fileFullName = item.Trim();
                string content = Path.GetFileNameWithoutExtension(fileFullName); // ȥ���ո�ͻ��з�

                //��#����ͷ����������
                if (!content.StartsWith("#"))
                {
                    string[] parts = content.Split(new string[] { "_v" }, StringSplitOptions.None);

                    if (parts.Length == 2)
                    {
                        string version = parts[1];
                        float versionValue = float.Parse(version);
                        if (maxVersion > versionValue)
                        {
                            //�������汾���ڱ��ذ汾������Ҫ����
                            return false;
                        }
                    }
                }
            }
            return true;
        }


        /// <summary>
        /// �������ݰ�
        /// </summary>
        /// <param name="needUpdate"></param>
        /// <param name="nextAction"></param>
        /// <returns></returns>
        private IEnumerator DownloadDataZip(bool needUpdate)
        {
            if (!needUpdate)
            {
                //ִ��unity�¼�
                updateDataCompleted.Invoke();
            }
            else
            {
                string cfgFile = url + "/" + lastestDataFolderName + "/" + XR.Config.HoloConfig.sceneConfig;
#if DEBUG
                Debug.Log("���ڸ���...");
#endif
                //��ȡ�����ļ��������ļ��д��������ļ��嵥
                using (UnityWebRequest webRequest = UnityWebRequest.Get(cfgFile))
                {
                    yield return webRequest.SendWebRequest();

#if UNITY_2020_3_OR_NEWER
                    if (webRequest.result != UnityWebRequest.Result.Success)
#else
                    if (!(webRequest.isHttpError || webRequest.isNetworkError))
#endif
                    {
#if DEBUG
                        if (Application.platform == RuntimePlatform.Android)
                        {
                            AndroidUtils.Toast("�����嵥��ȡʧ�ܡ���������");
                        }
#endif
                        Debug.LogWarning("�����嵥��ȡʧ�ܡ���������");
                        EqLog.e("DataDownLoader", "Error downloading data: " + webRequest.error);
                    }
                    else
                    {
                        if (!Directory.Exists(saveFolderPath))
                        {
                            Directory.CreateDirectory(saveFolderPath);
                        }
                        //���ݱ���·��file/data/scene.txt
                        // �������ص����ݵ������ļ�
                        byte[] data = webRequest.downloadHandler.data;
                        string sceneCfg = Encoding.UTF8.GetString(data);
                        File.WriteAllBytes(saveFolderPath + XR.Config.HoloConfig.sceneConfig, data);
                        Debug.Log("Scene.cfg downloaded and saved.");
                        //�����ļ��嵥���������ļ�
                        SceneEntity sceneEntity = JsonMapper.ToObject<SceneEntity>(sceneCfg);
                        System.Collections.Generic.List<string> fileList = sceneEntity.FileList;
                        foreach (string file in fileList)
                        {
                            //�����ļ�·��
                            string downloadFile = url + "/" + lastestDataFolderName + "/" + file;

                            using (UnityWebRequest request = UnityWebRequest.Get(downloadFile))
                            {
                                yield return request.SendWebRequest();
#if UNITY_2020_3_OR_NEWER
                                if (request.result == UnityWebRequest.Result.Success)
#else
                                if (!(request.isHttpError || request.isNetworkError))
#endif
                                {
                                    File.WriteAllBytes(saveFolderPath + file, request.downloadHandler.data);
                                }
                                else 
                                {
                                    Debug.LogWarning(file + "not found.");
                                }
                            }
#if UNITY_EDITOR
                            Debug.Log("����"+ file +"������ɣ�");
#endif
                        }

                        //д��汾�ļ�
                        DataIO.WriteVersionFile(saveFolderPath, lastestDataFolderName , true);
                        //ִ��unity�¼�
                        updateDataCompleted.Invoke();
                    }
                }
            }


        }

    }
}