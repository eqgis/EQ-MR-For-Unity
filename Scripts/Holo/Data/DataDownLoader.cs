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
        //数据保存路径
        private string saveFolderPath;

        /// <summary>
        /// 服务器上检测出来的最新的数据名称(文件夹)
        /// </summary>
        private string lastestDataFolderName;

        private void Awake()
        {
            //数据保存的文件夹路径（采用热更数据路径）
            saveFolderPath = Application.persistentDataPath + Holo.XR.Config.HoloConfig.hotUpdateDataFolder;
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
            //逻辑：获取服务器的最新版本信息，与本地的版本信息比对
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
            if (!Directory.Exists(saveFolderPath)) { return false; }

            //从数据解压路径读取版本文件
            string localVersionFilePath = saveFolderPath + webVersionFileName;
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
                //执行unity事件
                updateDataCompleted.Invoke();
            }
            else
            {
                string cfgFile = url + "/" + lastestDataFolderName + "/" + XR.Config.HoloConfig.sceneConfig;
#if DEBUG
                Debug.Log("正在更新...");
#endif
                //读取配置文件，配置文件中带有数据文件清单
                using (UnityWebRequest webRequest = UnityWebRequest.Get(cfgFile))
                {
                    yield return webRequest.SendWebRequest();

                    if (webRequest.result != UnityWebRequest.Result.Success)
                    {
#if DEBUG
                        if (Application.platform == RuntimePlatform.Android)
                        {
                            AndroidUtils.Toast("数据清单获取失败—请检查网络");
                        }
#endif
                        EqLog.e("DataDownLoader", "Error downloading data: " + webRequest.error);
                        Debug.LogWarning("数据清单获取失败—请检查网络");
                    }
                    else
                    {
                        if (!Directory.Exists(saveFolderPath))
                        {
                            Directory.CreateDirectory(saveFolderPath);
                        }
                        //数据保存路径file/data/scene.cfg
                        // 保存下载的数据到本地文件
                        byte[] data = webRequest.downloadHandler.data;
                        string sceneCfg = Encoding.UTF8.GetString(data);
                        File.WriteAllBytes(saveFolderPath + XR.Config.HoloConfig.sceneConfig, data);
                        Debug.Log("Scene.cfg downloaded and saved.");
                        //根据文件清单下载其他文件
                        SceneEntity sceneEntity = JsonMapper.ToObject<SceneEntity>(sceneCfg);
                        System.Collections.Generic.List<string> fileList = sceneEntity.FileList;
                        foreach (string file in fileList)
                        {
                            //网络文件路径
                            string downloadFile = url + "/" + lastestDataFolderName + "/" + file;

                            using (UnityWebRequest request = UnityWebRequest.Get(downloadFile))
                            {
                                yield return request.SendWebRequest();
                                if (webRequest.result == UnityWebRequest.Result.Success)
                                {
                                    File.WriteAllBytes(saveFolderPath + file, webRequest.downloadHandler.data);
                                }
                                else {
                                    Debug.LogWarning(file + "not found.");
                                }
                            }

                            //从数据解压路径读取版本文件
                            string localVersionFilePath = saveFolderPath + webVersionFileName;
                            //判断文件是否存在
                            if (File.Exists(localVersionFilePath))
                            {
                                File.Delete(localVersionFilePath);
                            }
                            //在本地路径写入当前数据版本信息
                            File.WriteAllText(localVersionFilePath, "###Data Version###\n" + lastestDataFolderName);

                            Debug.Log("数据下载完成！");

                            //执行unity事件
                            updateDataCompleted.Invoke();
                        }
                    }
                }
            }


        }

    }
}