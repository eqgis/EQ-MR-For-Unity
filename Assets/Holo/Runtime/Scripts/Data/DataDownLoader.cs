using Holo.HUR;
using Holo.XR.Android;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Holo.Data
{   
    /// <summary>
    /// 数据处理出错的委托事件
    /// </summary>
    /// <param name="message"></param>
    public delegate void OnErrorDelegate(string message);

    /// <summary>
    /// 数据下载器
    /// </summary>
    public class DataDownLoader : MonoBehaviour
    {
        [Header("服务器:数据路径")]
        public string url;

        [Header("自动下载最新数据")]
        public bool autoDownload = true;

        [Header("数据更新完成后执行")]
        public UnityEvent updateDataCompleted;
        //数据保存路径
        private string saveFolderPath;

        /// <summary>
        /// 服务器上检测出来的最新的数据名称(文件夹)
        /// </summary>
        private string lastestDataFolderName;

        /// <summary>
        /// 进度更新
        /// </summary>
        public event ProgressUpdateDelegate OnProgressUpdate;

        public event OnErrorDelegate OnError;

        private void Awake()
        {
            //数据保存的文件夹路径（采用热更数据路径）
            saveFolderPath = Application.persistentDataPath + Holo.XR.Config.HoloConfig.hotUpdateDataFolder;

#if DEBUG_LOG
            if (OnProgressUpdate == null)
            {
                OnProgressUpdate += OnProgressUpdateDebug;
            }
#endif
        }

        void OnProgressUpdateDebug(float progress)
        {
            int pro = (int)(progress * 100);
            EqLog.d("DataDownLoader", "downloading  " + pro + " %");
        }

        private void Start()
        {
            url = url.TrimEnd('/');

            if (autoDownload)
            {
                //Invoke("StartDownload", 0.1f);
                StartCoroutine(CheckDataVersion());
            }
        }

        /// <summary>
        /// 开始数据下载
        /// </summary>
        public void StartDownload()
        {
            StartCoroutine(CheckDataVersion());
        }

        /// <summary>
        /// 检查数据版本
        /// </summary>
        /// <param name="nextAction"></param>
        /// <returns></returns>
        private IEnumerator CheckDataVersion()
        {
#if DEBUG_LOG
            if (Application.platform == RuntimePlatform.Android)
            {
                AndroidUtils.Toast("checking data...");
            }
#endif           
            //更新进度
            if (OnProgressUpdate != null)
            {
                OnProgressUpdate(0f);
            }

            //逻辑：获取服务器的最新版本信息，与本地的版本信息比对
            string baseUrl = url + "/" + XR.Config.HoloConfig.versionFileName;

#if UNITY_EDITOR
            Debug.Log("版本文件路径:" + baseUrl);
            Debug.Log("本地文件路径:" + saveFolderPath);
#endif
            using (UnityWebRequest webRequest = UnityWebRequest.Get(baseUrl))
            {
                yield return webRequest.SendWebRequest();

#if UNITY_2020_3_OR_NEWER
                if (webRequest.result != UnityWebRequest.Result.Success)
#else
                if (webRequest.isHttpError || webRequest.isNetworkError)
#endif
                {
                    EqLog.e("DataDownLoader", "Error downloading file list: " + webRequest.error);
                    Debug.LogWarning("数据版本校验失败—网络连接异常或数据版本文件不存在");
                    OnError?.Invoke(webRequest.error);
                }
                else
                {
                    string fileListText = webRequest.downloadHandler.text;
                    string[] fileLines = fileListText.Split('\n');

                    float maxVersion = 0;
                    foreach (string line in fileLines)
                    {
                        string content = Path.GetFileNameWithoutExtension(line.Trim()); // 去除空格和换行符

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
                                    //文件夹名称，无后缀
                                    lastestDataFolderName = content;
                                }
                            }
                        }
                    }

                    //根据最大版本号，查找本地文件夹,checkLocalData()返回true，表示不需要跟新数据
                    //根据检测返回值，决定是否下载数据
                    bool status = CheckLocalData(maxVersion);
#if DEBUG_LOG
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
            if (!Directory.Exists(saveFolderPath)) { return false; }

            //从数据解压路径读取版本文件
            string localVersionFilePath = saveFolderPath + XR.Config.HoloConfig.versionFileName;
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
                        if (maxVersion > versionValue)
                        {
                            //服务器版本大于本地版本，则需要更新
                            return false;
                        }
                    }
                }
            }
            return true;
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
                //更新进度
                if (OnProgressUpdate != null)
                {
                    //数据无需下载，进度更新为100%
                    OnProgressUpdate(1.0f);
                }
                //执行unity事件
                updateDataCompleted.Invoke();
            }
            else
            {
                string cfgFile = url + "/" + lastestDataFolderName + "/" + XR.Config.HoloConfig.sceneConfig;
#if DEBUG_LOG
                Debug.Log("正在更新...");
#endif
                //读取配置文件，配置文件中带有数据文件清单
                using (UnityWebRequest webRequest = UnityWebRequest.Get(cfgFile))
                {
                    yield return webRequest.SendWebRequest();

#if UNITY_2020_3_OR_NEWER
                    if (webRequest.result != UnityWebRequest.Result.Success)
#else
                    if (webRequest.isHttpError || webRequest.isNetworkError)
#endif
                    {
#if DEBUG_LOG
                        if (Application.platform == RuntimePlatform.Android)
                        {
                            AndroidUtils.Toast("数据清单获取失败—请检查网络");
                        }
#endif
                        Debug.LogWarning("数据清单获取失败—请检查网络");
                        EqLog.e("DataDownLoader", "Error downloading data: " + webRequest.error);
                        OnError?.Invoke(webRequest.error);
                    }
                    else
                    {
                        if (!Directory.Exists(saveFolderPath))
                        {
                            Directory.CreateDirectory(saveFolderPath);
                        }
                        //数据保存路径file/data/scene.txt
                        // 保存下载的数据到本地文件
                        byte[] data = webRequest.downloadHandler.data;
                        //场景配置文件保存路径
                        string sceneCfgPath = saveFolderPath + XR.Config.HoloConfig.sceneConfig;
                        File.WriteAllBytes(sceneCfgPath, data);
#if DEBUG_LOG
                        EqLog.i("DataDownLoader",XR.Config.HoloConfig.sceneConfig + " downloaded and saved.");
#endif
                        //根据文件清单下载其他文件
                        System.Collections.Generic.List<string> fileList 
                            = AssetsPackageManager.Instance.LoadSceneConfig(sceneCfgPath).GetFileList();


                        int count = 0;
                        foreach (string file in fileList)
                        {
                            count++;
                            //网络文件路径
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

                                    if (OnProgressUpdate != null)
                                    {
                                        OnProgressUpdate(((float)count / fileList.Count) * 0.9f);
                                    }
                                }
                                else 
                                {
                                    Debug.LogWarning(file + "not found.");
                                }
                            }
#if UNITY_EDITOR
                            Debug.Log("数据"+ file +"下载完成！");
#endif
                        }

                        //写入版本文件
                        DataIO.WriteVersionFile(saveFolderPath, lastestDataFolderName , true);
                        if (OnProgressUpdate != null)
                        {
                            OnProgressUpdate(1.0f);
                        }
                        //执行unity事件
                        updateDataCompleted.Invoke();
                    }
                }
            }


        }

    }
}