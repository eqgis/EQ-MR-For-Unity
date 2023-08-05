
using AOT;
using Holo.XR.Android;
using Holo.XR.Config;
using Holo.XR.Utils;
using LitJson;
using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;

namespace Holo.XR.Core
{
    /// <summary>
    /// CSLAM 地图加载器
    /// </summary>
    public class XvCslamMapLoader : XvCslamMapControl
    {

        [Header("Load Settings")]
        public bool autoLoad = true;

        [Tooltip("The file is in the directory of streamingAssets.")]
        public bool streamingAssets = false;

        //默认自动加载的延时时间，仅影响第一次场景加载
        private int delay = 5000;

        private void Awake()
        {
            //默认文件夹路径
            if (folderPath == null || folderPath.Equals(""))
            {
                //安卓持久化存储路径为:/Android/data/包路径/
                folderPath = Application.persistentDataPath;
            }
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
            GameObject headCamera = GameObject.Find("Head");
            if (AndroidUtils.debug && Application.platform == RuntimePlatform.Android)
            {
                AndroidUtils.GetInstance().ShowToast("position:" + headCamera.transform.position.ToString());
            }
            headCamera.transform.rotation = Quaternion.Euler(0, 0, 0);
            headCamera.transform.position = Vector3.zero;


            if (AndroidUtils.debug)
            {
                InvokeRepeating("OutputMapInfo", 3.0f, 1.0f);
            }
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

            if (Application.platform != RuntimePlatform.Android)
                return;
            StartCoroutine(LoadMapAsync());
        }

        private IEnumerator LoadMapAsync()
        {
            //先读取缓存" ./cache/..."
            string cacheFolderPath = folderPath + HoloConfig.cacheFolder;
            string mapPath = cacheFolderPath + mapName + HoloConfig.cslamMapSuffix;
            string tagPose = cacheFolderPath + mapName + HoloConfig.tagPoseSuffix;

            if (!File.Exists(mapPath) || !File.Exists(tagPose))
            {
                //地图文件不存在或者Tag姿态文件不存在

                if (streamingAssets)
                {
                    //若是采用streamingAssets的模式，则数据存在streamingAssets文件下，若第二个参数为false，则不强制更新。持久化路径下会先查找文件
                    //"持久化路径/mapName.homap"
                    //从streamAssets中cp到持久化路径
                    AndroidUtils.Toast("1");
                    //string targetMapPachagePath = IoUtils.CopyMapToPersistentDataPath(folderPath + "/" + mapName + HoloConfig.mapPackageSuffix, false);

                    string relativePath = folderPath + "/" + mapName + HoloConfig.mapPackageSuffix;
                    //格式校验
                    relativePath.Replace("\\", "/");
                    if (!relativePath.StartsWith("/"))
                    {
                        relativePath = "/" + relativePath;
                    }

                    //示例数据路径:"/homap/test.homap"
                    //如果持久化目录下没有文件，先从streamingAssets里复制一份到持久化目录
                    string targetPersistentPath = Application.persistentDataPath + relativePath;
                    if (!File.Exists(targetPersistentPath))
                    {
                        EqLog.d("IKKYU", Application.streamingAssetsPath + relativePath);

                        WWW www = new WWW(Application.streamingAssetsPath + relativePath);
                        yield return www;
                        if (www.isDone)
                        {
                            File.WriteAllBytes(targetPersistentPath, www.bytes);
                        }
                    }
                    //改用持久化数据的文件夹路径
                    folderPath = targetPersistentPath.Replace("/" + mapName + HoloConfig.mapPackageSuffix, "");

                    AndroidUtils.Toast("3");
                    //解压
                    UnZipMapFilePackage(folderPath, mapName);
                }
                else
                {
                    try
                    {
                        //进行解压操作
                        string mapPackagePath = folderPath + "/" + mapName + HoloConfig.mapPackageSuffix;
                        if (!File.Exists(mapPackagePath))
                        {
                            if (AndroidUtils.debug)
                            {
                                AndroidUtils.GetInstance().ShowToast("Map文件不存在");
                            }

                            EqLog.e("XvCslamMapLoader", "Do not find file. " + mapPackagePath);
                        }
                        else
                        {
                            //解压
                            UnZipMapFilePackage(folderPath, mapName);
                        }
                    }
                    catch (Exception e)
                    {
                        EqLog.e("XvCslamMapLoader", e.ToString());
                    }
                }
            }


            try
            {
                //使用XVisio API 读取cslam
                API.xslam_load_map_and_switch_to_cslam(mapPath, OnCslamSwitched, OnLoadLocalized);
                //读取位置关系
                LoadSceneNodeChildren(tagPose);

                if (AndroidUtils.debug)
                {
                    AndroidUtils.GetInstance().ShowToast("匹配成功！\n" + mapPath);
                    EqLog.d("XvCslamMapLoader", "loadingComplete");
                }
                complete?.Invoke();
            }
            catch (Exception e)
            {
                EqLog.e("XvCslamMapLoader", e.ToString());
            }



        }
        //=================内部===========================//

        /// <summary>
        /// 解压地图文件包
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="fileName"></param>
        private void UnZipMapFilePackage(string folderPath, string fileName)
        {
            string filePath = folderPath + "/" + fileName + HoloConfig.mapPackageSuffix;
            string cachePath = folderPath + HoloConfig.cacheFolder;
            //解压输出至缓存路径
            ZipHelper.Instance.UnzipFile(filePath, cachePath,psd,null);
        }

        /// <summary>
        /// 读取场景节点
        /// </summary>
        private void LoadSceneNodeChildren(string jsonFilePath)
        {
            string data;

            //先判断是否存在，再创建
            if (!File.Exists(jsonFilePath))
            {
                if (AndroidUtils.debug)
                {
                    AndroidUtils.GetInstance().ShowToast("缺少Tag位置信息");
                }
                data = "";
            }
            else
            {
                //读取内容
                //data = File.ReadAllText(jsonFilePath);
                byte[] bytes = File.ReadAllBytes(jsonFilePath);
                data = Encoding.UTF8.GetString(bytes);
            }

            EqLog.d("XvCslamMapLoader", data);
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
                //    AndroidUtils.Toast("NPR: Count:" + npr.count + "Position:"+ npr.NextSceneNodePosition.ToString());
                //    EqLog.d("IKKYU", "NPR: Count:" + npr.count + "Position:" + npr.NextSceneNodePosition.ToString());
                //}
            }
            else
            {
                EqLog.e("XvCslamMapLoader", "data was null");
            }
        }

        /// <summary>
        /// 加载地图的回调函数实现
        /// </summary>
        /// <param name="map_quality">地图质量</param>
        [MonoPInvokeCallback(typeof(API.detectSwitched_callback))]
        static void OnCslamSwitched(int map_quality)
        {//不是静态方法会出错
            mapQuality = map_quality;
        }

        /// <summary>
        /// 加载地图的匹配度的回调实现
        /// </summary>
        /// <param name="percent">匹配度</param>
        [MonoPInvokeCallback(typeof(API.detectLocalized_callback))]
        static void OnLoadLocalized(float percent)
        {//不是静态方法会出错
            mapMatchingPercent = percent;
        }
    }
}