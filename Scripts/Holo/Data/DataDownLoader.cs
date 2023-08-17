using Holo.XR.Android;
using Holo.XR.Utils;
using System;
using System.Collections;
using System.IO;
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

        [Header("�����������ݰ汾�ļ�")]
        public string webVersionFileName = "version.txt";

        [Header("�Զ�������������")]
        public bool autoDownload = true;

        [Header("���ݸ�����ɺ�ִ��")]
        public UnityEvent updateDataCompleted;
        //���ݽ�ѹ·��
        private string unZipFolderPath;

        /// <summary>
        /// �������ϼ����������µ���������(����ʽ��׺)
        /// </summary>
        private string lastestDataFullName;

        //���ݰ���ʱ�洢·��
        private string dataSavePath;

        private void Awake()
        {
            //���ݱ�����ļ���·���������ȸ�����·����
            unZipFolderPath = Application.persistentDataPath + Holo.XR.Config.HoloConfig.hotUpdateDataFolder;
        }

        private void Start()
        {
            url.TrimEnd('/');

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
            string baseUrl = url + "/" + webVersionFileName;
            using (UnityWebRequest webRequest = UnityWebRequest.Get(baseUrl))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
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
                        string fileFullName = line.Trim();
                        string content = Path.GetFileNameWithoutExtension(fileFullName); // ȥ���ո�ͻ��з�

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
                                    lastestDataFullName = fileFullName; ;
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
            if (!Directory.Exists(unZipFolderPath)) { return false; }

            //�����ݽ�ѹ·����ȡ�汾�ļ�
            string localVersionFilePath = unZipFolderPath + webVersionFileName;
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
                        if (maxVersion >= versionValue)
                        {
                            //���ڰ汾��¼��>= web���ݰ汾���򲻸���
                            return true;
                        }
                    }
                }
            }
            return false;
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
                //���ݲ���Ҫ����
                //nextAction.Invoke();
                execTaskAfterUnzip();
            }
            else
            {
                string zipUrl = url + "/" + lastestDataFullName;
#if DEBUG
                Debug.Log("���ڸ���...");
#endif

                using (UnityWebRequest webRequest = UnityWebRequest.Get(zipUrl))
                {
                    yield return webRequest.SendWebRequest();

                    if (webRequest.result != UnityWebRequest.Result.Success)
                    {
#if DEBUG
                        if (Application.platform == RuntimePlatform.Android)
                        {
                            AndroidUtils.Toast("��������ʧ�ܡ���������");
                        }
#endif
                        EqLog.e("DataDownLoader", "Error downloading data: " + webRequest.error);
                        Debug.LogWarning("��������ʧ�ܡ���������");
                    }
                    else
                    {
                        if (!Directory.Exists(unZipFolderPath))
                        {
                            Directory.CreateDirectory(unZipFolderPath);
                        }
                        //���ݱ���·��
                        dataSavePath = unZipFolderPath + lastestDataFullName;
                        // �������ص����ݵ������ļ�
                        File.WriteAllBytes(dataSavePath, webRequest.downloadHandler.data);
                        Debug.Log("Zip downloaded and saved to: " + dataSavePath);

                        //�����ݽ�ѹ·����ȡ�汾�ļ�
                        string localVersionFilePath = unZipFolderPath + webVersionFileName;
                        //�ж��ļ��Ƿ����
                        if (File.Exists(localVersionFilePath))
                        {
                            File.Delete(localVersionFilePath);
                        }
                        //�ڱ���·��д�뵱ǰ���ݰ汾��Ϣ
                        File.WriteAllText(localVersionFilePath, "###Data Version###\n" + lastestDataFullName);

                        //�������ݽ�ѹ��ɺ��Action
                        UnzipCallback unzipCallback = new UnzipCallback();
                        unzipCallback.after = execTaskAfterUnzip;
                        //��ѹ����
                        ZipHelper.Instance.UnzipFile(dataSavePath, unZipFolderPath, "ikkyu", unzipCallback);

                        Debug.Log("����������ɣ�");


                    }
                }
            }
        }


        /// <summary>
        /// ��ѹ��ɺ�ִ������
        /// </summary>
        private void execTaskAfterUnzip()
        {
            //��ѹ����Զ�������ݰ�
            if (File.Exists(dataSavePath))
            {
                //File.Delete(dataSavePath);
            }

            //ִ��unity�¼�
            updateDataCompleted.Invoke();
        }
    }


}