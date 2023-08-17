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
    /// 数据下载器
    /// </summary>
    public class DataDownLoader : MonoBehaviour
    {
        [Header("服务器:数据路径")]
        public string url;

        [Header("服务器：数据版本文件")]
        public string webVersionFileName = "version.txt";

        [Header("自动下载最新数据")]
        public bool autoDownload = true;

        [Header("数据更新完成后执行")]
        public UnityEvent updateDataCompleted;
        //数据解压路径
        private string unZipFolderPath;

        /// <summary>
        /// 服务器上检测出来的最新的数据名称(带格式后缀)
        /// </summary>
        private string lastestDataFullName;

        //数据包临时存储路径
        private string dataSavePath;

        private void Awake()
        {
            //数据保存的文件夹路径（采用热更数据路径）
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
        /// 检查数据版本
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
                        AndroidUtils.Toast("数据版本校验失败—请检查网络");
                    }
#endif
                    EqLog.e("DataDownLoader", "Error downloading file list: " + webRequest.error);
                    Debug.LogWarning("数据版本校验失败—请检查网络");
                }
                else
                {
                    string fileListText = webRequest.downloadHandler.text;
                    string[] fileLines = fileListText.Split('\n');

                    float maxVersion = 0;
                    foreach (string line in fileLines)
                    {
                        string fileFullName = line.Trim();
                        string content = Path.GetFileNameWithoutExtension(fileFullName); // 去除空格和换行符

                        //“#”开头则跳过该行
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
                                    //取最大version
                                    maxVersion = versionValue;
                                    lastestDataFullName = fileFullName; ;
                                }
                            }
                        }
                    }

                    //根据最大版本号，查找本地文件夹,checkLocalData()返回true，表示不需要跟新数据
                    //根据检测返回值，决定是否下载数据
                    bool status = CheckLocalData(maxVersion);
#if DEBUG
                    if (status)
                    {
                        Debug.Log("本地已检索到数据");
                    }
                    else
                    {
                        Debug.Log("本地未检索到数据");
                    }
#endif
                    StartCoroutine(DownloadDataZip(!status));

                }
            }
        }

        /// <summary>
        /// 判断本地数据文件是否存在
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private bool CheckLocalData(float maxVersion)
        {
            //若文件夹路径都不存在，则直接返回false，表示不存在
            if (!Directory.Exists(unZipFolderPath)) { return false; }

            //从数据解压路径读取版本文件
            string localVersionFilePath = unZipFolderPath + webVersionFileName;
            //判断文件是否存在
            if (!File.Exists(localVersionFilePath))
            {
                //文件不存在
                return false;
            }

            //版本内容
            string versionContent = File.ReadAllText(localVersionFilePath);

            string[] versionList = versionContent.Split('\n');

            foreach (string item in versionList)
            {
                string fileFullName = item.Trim();
                string content = Path.GetFileNameWithoutExtension(fileFullName); // 去除空格和换行符

                //“#”开头则跳过该行
                if (!content.StartsWith("#"))
                {
                    string[] parts = content.Split(new string[] { "_v" }, StringSplitOptions.None);

                    if (parts.Length == 2)
                    {
                        string version = parts[1];
                        float versionValue = float.Parse(version);
                        if (maxVersion >= versionValue)
                        {
                            //存在版本记录，>= web数据版本，则不更新
                            return true;
                        }
                    }
                }
            }
            return false;
        }


        /// <summary>
        /// 下载数据包
        /// </summary>
        /// <param name="needUpdate"></param>
        /// <param name="nextAction"></param>
        /// <returns></returns>
        private IEnumerator DownloadDataZip(bool needUpdate)
        {
            if (!needUpdate)
            {
                //数据不需要更新
                //nextAction.Invoke();
                execTaskAfterUnzip();
            }
            else
            {
                string zipUrl = url + "/" + lastestDataFullName;
#if DEBUG
                Debug.Log("正在更新...");
#endif

                using (UnityWebRequest webRequest = UnityWebRequest.Get(zipUrl))
                {
                    yield return webRequest.SendWebRequest();

                    if (webRequest.result != UnityWebRequest.Result.Success)
                    {
#if DEBUG
                        if (Application.platform == RuntimePlatform.Android)
                        {
                            AndroidUtils.Toast("数据下载失败—请检查网络");
                        }
#endif
                        EqLog.e("DataDownLoader", "Error downloading data: " + webRequest.error);
                        Debug.LogWarning("数据下载失败—请检查网络");
                    }
                    else
                    {
                        if (!Directory.Exists(unZipFolderPath))
                        {
                            Directory.CreateDirectory(unZipFolderPath);
                        }
                        //数据保存路径
                        dataSavePath = unZipFolderPath + lastestDataFullName;
                        // 保存下载的数据到本地文件
                        File.WriteAllBytes(dataSavePath, webRequest.downloadHandler.data);
                        Debug.Log("Zip downloaded and saved to: " + dataSavePath);

                        //从数据解压路径读取版本文件
                        string localVersionFilePath = unZipFolderPath + webVersionFileName;
                        //判断文件是否存在
                        if (File.Exists(localVersionFilePath))
                        {
                            File.Delete(localVersionFilePath);
                        }
                        //在本地路径写入当前数据版本信息
                        File.WriteAllText(localVersionFilePath, "###Data Version###\n" + lastestDataFullName);

                        //设置数据解压完成后的Action
                        UnzipCallback unzipCallback = new UnzipCallback();
                        unzipCallback.after = execTaskAfterUnzip;
                        //解压数据
                        ZipHelper.Instance.UnzipFile(dataSavePath, unZipFolderPath, "ikkyu", unzipCallback);

                        Debug.Log("数据下载完成！");


                    }
                }
            }
        }


        /// <summary>
        /// 解压完成后执行任务
        /// </summary>
        private void execTaskAfterUnzip()
        {
            //解压完成自动清除数据包
            if (File.Exists(dataSavePath))
            {
                //File.Delete(dataSavePath);
            }

            //执行unity事件
            updateDataCompleted.Invoke();
        }
    }


}