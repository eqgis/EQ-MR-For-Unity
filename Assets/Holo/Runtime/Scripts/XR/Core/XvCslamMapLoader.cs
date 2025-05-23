
using AOT;
using Eqgis.Utils;
using Holo.XR.Android;
using Holo.XR.Config;

#if ENGINE_XVISIO
using LitJson;
#endif

using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Holo.XR.Core
{
    /// <summary>
    /// CSLAM地图数据源
    /// </summary>
    public enum CslamMapDataSource
    {
        LocalPath,
        StreamingAssets,
        WebData
    }
    /// <summary>
    /// CSLAM 地图加载器
    /// </summary>
    public class XvCslamMapLoader : XvCslamMapControl
    {

        [Header("Load Settings")]
        public bool autoLoad = true;

        public CslamMapDataSource sourceType = CslamMapDataSource.LocalPath;

        [Header("Web Data")]
        public string webUrl = null;


        //默认自动加载的延时时间，仅影响第一次场景加载
        private int delay = 5000;

        //持久化路径
        private string persistentDataPath;
        //资源路径
        private string streamingAssetsPath;

        private void Awake()
        {
            //默认文件夹路径
            if (folderPath == null || folderPath.Equals(""))
            {
                //安卓持久化存储路径为:/Android/data/包路径/
                folderPath = Application.persistentDataPath;
            }

            persistentDataPath = Application.persistentDataPath;
            streamingAssetsPath = Application.streamingAssetsPath;
        }

        public void Start()
        {
            StartMap();
            if (autoLoad)
            {
                //第一次识别场景需要延时一下
                if (NodePoseRecorder.GetInstance().count == 0)
                {
                    //自动读取地图
                    Invoke("LoadMap", delay / 1000.0f);
                }
                else
                {
                    //自动读取地图
                    Invoke("LoadMap", 0.1f);
                }
            }


#if DEBUG_MODEL
            InvokeRepeating("OutputMapInfo", 3.0f, 1.0f);
#endif
        }


        private void OutputMapInfo()
        {
            if (Application.platform != RuntimePlatform.Android)
                return;
            EqLog.d("XvCslamMapLoader", "MapQuality:" + mapQuality + " ;MatchingPercent:" + mapMatchingPercent);
        }


        private void OnDestroy()
        {
            StopMap();
        }

        /// <summary>
        /// 加载地图
        /// </summary>
        public void LoadMap()
        {

            //if (Application.platform != RuntimePlatform.Android)
            //    return;

            switch (sourceType)
            {
                case CslamMapDataSource.LocalPath:
                    LoadMapByLoacalFolder();
                    break;
                case CslamMapDataSource.StreamingAssets:
                    LoadMapByStreamingAssets();
                    break;
                case CslamMapDataSource.WebData:
                    LoadMapByWebUrl();
                    break;

            }
        }

        //=================内部===========================//
       
        /// <summary>
        /// 从设备本地文件夹加载CSLAM数据包
        /// </summary>
        private void LoadMapByLoacalFolder()
        {
            //先读取缓存" ./cache/..."
            string cacheFolderPath = folderPath + HoloConfig.cacheFolder;
            string mapPath = cacheFolderPath + mapName + HoloConfig.cslamMapSuffix;
            string tagPose = cacheFolderPath + mapName + HoloConfig.tagPoseSuffix;
            if (!File.Exists(mapPath) || !File.Exists(tagPose))
            {
                //地图文件不存在或者Tag姿态文件不存在
                try
                {
                    //进行解压操作
                    string mapPackagePath = folderPath + "/" + mapName + HoloConfig.mapPackageSuffix;
                    if (!File.Exists(mapPackagePath))
                    {

#if DEBUG_MODEL
                            AndroidUtils.GetInstance().ShowToast("Map文件不存在");
#endif

                        EqLog.e("XvCslamMapLoader", "Do not find file. " + mapPackagePath);
                    }
                    else
                    {
                        //解压
                        UnZipMapFilePackage(folderPath, mapName);
                        ReadCslamMap(mapPath, tagPose);
                    }
                }
                catch (Exception e)
                {
                    EqLog.e("XvCslamMapLoader", e.ToString());
                }
            }
            else
            {
                ReadCslamMap(mapPath, tagPose);
            }
        }

        /// <summary>
        /// 从StreamingAssets加载对应CSLAM地图包
        /// </summary>
        private void LoadMapByStreamingAssets()
        {
            //文件夹相对路径格式校验
            if (!folderPath.StartsWith("/"))
            {
                folderPath = "/" + folderPath;
            }

            //要拷贝至的目标文件夹路径
            string destinationFolderPath = persistentDataPath + folderPath;

            //先读取缓存" ./cache/..."
            string cacheFolderPath = destinationFolderPath + HoloConfig.cacheFolder;
            string mapPath = cacheFolderPath + mapName + HoloConfig.cslamMapSuffix;
            string tagPose = cacheFolderPath + mapName + HoloConfig.tagPoseSuffix;
            if (!File.Exists(mapPath) || !File.Exists(tagPose))
            {
                //streamingAssetsPath路径下的地图文件包路径
                string sourceFilePath = streamingAssetsPath + folderPath + "/" + mapName + HoloConfig.mapPackageSuffix;

                //目标文件路径
                string destinationFilePath = destinationFolderPath + "/" + mapName + HoloConfig.mapPackageSuffix;

                //判断目标文件是否存在，文件不存在，则执行拷贝操作
                if (!File.Exists(destinationFilePath))
                {
                    if (!Directory.Exists(destinationFolderPath))
                    {
                        //创建目标文件夹
                        Directory.CreateDirectory(destinationFolderPath);
                    }

                    StartCoroutine(CopyMapPackageFromRequest(
                        sourceFilePath,
                        destinationFilePath,
                        destinationFolderPath,
                        mapPath,
                        tagPose
                        ));
                }
                else
                {
                    //在目标文件夹解压指定名称的地图文件
                    UnZipMapFilePackage(destinationFolderPath, mapName);
                    ReadCslamMap(mapPath, tagPose);
                }
            }
            else
            {
                ReadCslamMap(mapPath, tagPose);
            }
        }

        /// <summary>
        /// 通过URL加载地图数据
        /// <code>注：第一次请求会生成缓存，见持久化路径的homap文件夹</code>
        /// </summary>
        private void LoadMapByWebUrl()
        {
            //文件夹相对路径格式校验
            if (!folderPath.StartsWith("/"))
            {
                folderPath = "/" + folderPath;
            }

            //要拷贝至的目标文件夹路径
            string destinationFolderPath = persistentDataPath + folderPath;

            //先读取缓存" ./cache/..."
            string cacheFolderPath = destinationFolderPath + HoloConfig.cacheFolder;
            string mapPath = cacheFolderPath + mapName + HoloConfig.cslamMapSuffix;
            string tagPose = cacheFolderPath + mapName + HoloConfig.tagPoseSuffix;
            if (!File.Exists(mapPath) || !File.Exists(tagPose))
            {
                //web路径下的地图文件包路径webUrl

                //目标文件路径
                string destinationFilePath = destinationFolderPath + "/" + mapName + HoloConfig.mapPackageSuffix;

                //判断目标文件是否存在，文件不存在，则执行拷贝操作
                if (!File.Exists(destinationFilePath))
                {
                    if (!Directory.Exists(destinationFolderPath))
                    {
                        //创建目标文件夹
                        Directory.CreateDirectory(destinationFolderPath);
                    }

                    //将webUrl的数据拷贝至持久化路径
                    StartCoroutine(CopyMapPackageFromRequest(
                        webUrl,
                        destinationFilePath,
                        destinationFolderPath,
                        mapPath,
                        tagPose
                        ));
                }
                else
                {
                    //在目标文件夹解压指定名称的地图文件
                    UnZipMapFilePackage(destinationFolderPath, mapName);
                    ReadCslamMap(mapPath, tagPose);
                }
            }
            else
            {
                ReadCslamMap(mapPath, tagPose);
            }
        }

        /// <summary>
        /// 从StreamAssets或webUrl中将地图包拷贝
        /// </summary>
        private IEnumerator CopyMapPackageFromRequest(string sourceFilePath, string destinationFilePath,
            string destinationFolderPath, string mapPath, string tagPose)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(sourceFilePath))
            {
                yield return www.SendWebRequest();

#if UNITY_2020_3_OR_NEWER
                if (www.result == UnityWebRequest.Result.Success)
#else
                if(!(www.isHttpError || www.isNetworkError))
#endif
                {
                    byte[] data = www.downloadHandler.data;

                    if (data != null && data.Length > 0)
                    {
                        File.WriteAllBytes(destinationFilePath, data);
                        //在目标文件夹解压指定名称的地图文件
                        UnZipMapFilePackage(destinationFolderPath, mapName);
                        ReadCslamMap(mapPath, tagPose);
                    }
                    else
                    {
                        EqLog.e("XvCslamMapLoader", "Failed to read file data from StreamingAssets.");
                    }
                }
                else
                {
                    EqLog.e("XvCslamMapLoader", "Failed to read file: " + www.error);
                }
            }
        }

        /// <summary>
        /// 读取Cslam地图
        /// </summary>
        /// <param name="mapPath"></param>
        /// <param name="tagPose"></param>
        private void ReadCslamMap(string mapPath, string tagPose)
        {
            try
            {
#if ENGINE_XVISIO
                //使用XVisio API 读取cslam
                API.xslam_load_map_and_switch_to_cslam(mapPath, OnCslamSwitched, OnLoadLocalized);
#endif
                //读取位置关系
                bool status = LoadSceneNodeChildren(tagPose);


#if DEBUG_MODEL
                if (status)
                {
                    AndroidUtils.GetInstance().ShowToast("匹配成功！\n" + mapPath);
                    EqLog.d("XvCslamMapLoader", "loadingComplete");
                }
#endif
                complete?.Invoke();
            }
            catch (Exception e)
            {
                EqLog.e("XvCslamMapLoader", e.ToString());
            }
        }

        /// <summary>
        /// 解压地图文件包
        /// </summary>
        /// <param name="folderPathDes"></param>
        /// <param name="fileName"></param>
        private void UnZipMapFilePackage(string folderPathDes, string fileName)
        {
            string filePath = folderPathDes + "/" + fileName + HoloConfig.mapPackageSuffix;
            string cachePath = folderPathDes + HoloConfig.cacheFolder;
            //解压输出至缓存路径
            ZipHelper.Instance.UnzipFile(filePath, cachePath,psd,null);
        }

        /// <summary>
        /// 读取场景节点
        /// </summary>
        private bool LoadSceneNodeChildren(string jsonFilePath)
        {
            string data;

            //先判断是否存在，再创建
            if (!File.Exists(jsonFilePath))
            {

#if DEBUG_MODEL
                    AndroidUtils.GetInstance().ShowToast("缺少Tag位置信息");
#endif
                data = "";
                return false;
            }
            else
            {
                //读取内容
                //data = File.ReadAllText(jsonFilePath);
                byte[] bytes = File.ReadAllBytes(jsonFilePath);
                data = Encoding.UTF8.GetString(bytes);
            }

#if DEBUG_MODEL
            EqLog.d("XvCslamMapLoader", data);
#endif

#if ENGINE_XVISIO
            if (data != "")
            {
                JsonData jd = JsonMapper.ToObject(data);
                //List<MapContentVO> mapContentList = new List<MapContentVO>();
                //读取内存中的本场景节点相对于上一个场景原点的姿态差
                NodePoseRecorder npr = NodePoseRecorder.GetInstance();


                //读取一次，记一次数
                npr.count++;
                if (npr.count != 1)
                {
                    //原理：父节点位置姿态与“首次识别场景的场景节点位姿”保持一致
                    Vector3 firstSceneNodePosition = npr.FirstSceneNodePosition;
                    Vector3 firstSceneNodeRotation = npr.FirstSceneNodeRotation;

                    content.transform.position = firstSceneNodePosition;
                    content.transform.eulerAngles = firstSceneNodeRotation;

                    for (int i = 0; i < jd.Count; i++)
                    {
                        GameObject sceneNode = content.transform.Find(jd[i]["o_name"].ToString()).gameObject;
                        //Vector3 v_t = new Vector3(float.Parse(jd[i]["t_x"].ToString()), float.Parse(jd[i]["t_y"].ToString()), float.Parse(jd[i]["t_z"].ToString()));
                        //Vector3 v_r = new Vector3(float.Parse(jd[i]["r_x"].ToString()), float.Parse(jd[i]["r_y"].ToString()), float.Parse(jd[i]["r_z"].ToString()));

                        //更新场景节点(以及这些同级节点（“content的直接子节点”）)的相对位置。
                        sceneNode.transform.localPosition = npr.NextSceneNodePosition;
                        sceneNode.transform.localEulerAngles = npr.NextSceneNodeRotation;
                    }
                }
                else
                {
                    //第一次 读JSON
                    for (int i = 0; i < jd.Count; i++)
                    {
                        GameObject pic = content.transform.Find(jd[i]["o_name"].ToString()).gameObject;
                        Vector3 v_t = new Vector3(float.Parse(jd[i]["t_x"].ToString()), float.Parse(jd[i]["t_y"].ToString()), float.Parse(jd[i]["t_z"].ToString()));
                        Vector3 v_r = new Vector3(float.Parse(jd[i]["r_x"].ToString()), float.Parse(jd[i]["r_y"].ToString()), float.Parse(jd[i]["r_z"].ToString()));

                        pic.transform.localPosition = new Vector3(v_t.x, v_t.y, v_t.z);
                        pic.transform.localEulerAngles = new Vector3(v_r.x, v_r.y, v_r.z);

                        /**
                         * 注：
                         * 第一次读取map.bin后，确定了坐标系。通过读取map.json确定SceneNode的位置。
                         * 
                         */
                        //第一次，存下json 1
                        npr.FirstSceneNodePosition = new Vector3(v_t.x, v_t.y, v_t.z);
                        npr.FirstSceneNodeRotation = new Vector3(v_r.x, v_r.y, v_r.z);

                        //pic.transform.DOLocalMove(v_t, 0.3f);
                        //pic.transform.DORotate(v_r, 0.3f);
                    }

                }
                //if (AndroidUtils.debug)
                //{
                //    EqLog.d("IKKYU", "NPR: Count:" + npr.count + "Position:" + npr.NextSceneNodePosition.ToString());
                //}
            }
            else
            {
                EqLog.e("XvCslamMapLoader", "data was null");
            }
#endif
            return true;
        }

        /// <summary>
        /// 加载地图的回调函数实现
        /// </summary>
        /// <param name="map_quality">地图质量</param>
#if ENGINE_XVISIO
        [MonoPInvokeCallback(typeof(API.detectSwitched_callback))]
#endif
        static void OnCslamSwitched(int map_quality)
        {//不是静态方法会出错
            mapQuality = map_quality;
        }

        /// <summary>
        /// 加载地图的匹配度的回调实现
        /// </summary>
        /// <param name="percent">匹配度</param>

#if ENGINE_XVISIO
        [MonoPInvokeCallback(typeof(API.detectLocalized_callback))]
#endif
        static void OnLoadLocalized(float percent)
        {//不是静态方法会出错
            mapMatchingPercent = percent;
        }
    }

}