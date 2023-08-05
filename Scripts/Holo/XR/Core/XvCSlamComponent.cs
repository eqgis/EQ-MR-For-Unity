using AOT;
using Holo.XR.Android;
using LitJson;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Holo.XR.Core
{

    [System.Obsolete("This entire class is obsolete. Use XvCslamMapScanner instead.")]
    public class XvCSlamComponent : MonoBehaviour
    {
        private const string POSE_KEY = "MapInfo";
        private static int switch_map_quality = 0;//加载地图品质
        private static float percentcD = -1;//地图匹配度
        private static int status_of_saved_mapq = 0;//地图状态
        private static int save_map_qualityq = 0;//地图匹配度
        public GameObject content;

        [Header("Core"), Tooltip("建图文件")]
        public string cslamFileName = "map";

        [Tooltip("同步更新节点位置")]
        public bool updateNodePose = true;
        [Tooltip("保存至本地")]
        public bool saveToSdCard = true;

        [Header("Status UI")]
        public Text infoTxt;

        private bool ifStart = false;

        // Start is called before the first frame update
        void Start()
        {
        }

        /// <summary>
        /// 加载地图的回调函数实现
        /// </summary>
        /// <param name="map_quality"></param>
        [MonoPInvokeCallback(typeof(API.detectSwitched_callback))]
        static void OnCslamSwitched(int map_quality)
        {
            switch_map_quality = map_quality;
        }

        /// <summary>
        /// 保存地图匹配度的回调实现
        /// </summary>
        /// <param name="percentc"></param>
        [MonoPInvokeCallback(typeof(API.detectLocalized_callback))]
        static void OnSaveLocalized(float percentc)
        {
            percentcD = percentc;
        }

        /// <summary>
        /// 加载地图的匹配度的回调实现
        /// </summary>
        /// <param name="percentc"></param>
        [MonoPInvokeCallback(typeof(API.detectLocalized_callback))]
        static void OnLoadLocalized(float percentc)
        {
            percentcD = percentc;

        }

        /// <summary>
        /// 保存地图的回调实现
        /// </summary>
        /// <param name="status_of_saved_map"></param>
        /// <param name="map_quality"></param>
        [MonoPInvokeCallback(typeof(API.detectCslamSaved_callback))]
        static void OnCslamSaved(int status_of_saved_map, int map_quality)
        {
            status_of_saved_mapq = status_of_saved_map;
            save_map_qualityq = map_quality;

        }


        /// <summary>
        /// 开启slam
        /// </summary>
        public void StartSlam()
        {
            if (ifStart == true)
            {
                return;
            }
            ifStart = true;

            //#if UNITY_ANDROID && !UNITY_EDITOR
            //#endif
            if (Application.platform == RuntimePlatform.Android)
            {
                API.xslam_start_map();

                AndroidUtils.GetInstance().ShowToast("CSLAM 已开启！");
            }
        }

        /// <summary>
        /// 停止slam
        /// </summary>
        public void StopSlam()
        {
            if (ifStart)
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    API.xslam_stop_map();
                }
                ifStart = false;

                AndroidUtils.GetInstance().ShowToast("SLAM 已关闭！");
            }
        }

        /// <summary>
        /// 保存地图
        /// </summary>
        public void SaveMap()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                API.xslam_save_map_and_switch_to_cslam(Application.persistentDataPath + "/" + cslamFileName + ".bin", OnCslamSaved, OnSaveLocalized);

                string msg = "地图已保存！";
                if (updateNodePose)
                {
                    //保存场景节点的直接子节点的位置
                    SaveSceneNodeChildren(Application.persistentDataPath + "/" + cslamFileName + ".json");
                    msg = "地图及场景节点已保存！";
                }
                AndroidUtils.GetInstance().ShowToast(msg);
            }
        }

        /// <summary>
        /// 加载地图
        /// </summary>
        public void LoadMap()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                API.xslam_load_map_and_switch_to_cslam(Application.persistentDataPath + "/" + cslamFileName + ".bin", OnCslamSwitched, OnLoadLocalized);

                string msg = "地图已加载！";
                if (updateNodePose)
                {
                    //更新场景节点的子对象的位置
                    UpdateSceneNodeChildren(Application.persistentDataPath + "/" + cslamFileName + ".json");
                    msg = "地图及场景节点已加载！";
                }
                AndroidUtils.GetInstance().ShowToast(msg);
            }
        }

        /// <summary>
        /// 保存地图至指定文件夹
        /// </summary>
        /// <param name="folderPath">文件夹路径</param>
        public void SaveMapToCustomFolder(string folderPath)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                API.xslam_save_map_and_switch_to_cslam(folderPath + "/" + cslamFileName + ".bin", OnCslamSaved, OnSaveLocalized);

                string msg = "地图已保存！";
                if (updateNodePose)
                {
                    //保存场景节点的直接子节点的位置
                    SaveSceneNodeChildren(folderPath + "/" + cslamFileName + ".json");
                    msg = "地图及场景节点已保存！";
                }
                AndroidUtils.GetInstance().ShowToast(msg);
            }
        }

        /// <summary>
        /// 从指定文件夹加载地图
        /// </summary>
        /// <param name="folderPath">文件夹路径</param>
        public void LoadMapFromCustomFolder(string folderPath)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                API.xslam_load_map_and_switch_to_cslam(folderPath + "/" + cslamFileName + ".bin", OnCslamSwitched, OnLoadLocalized);

                string msg = "地图已加载！";
                if (updateNodePose)
                {
                    //更新场景节点的子对象的位置
                    UpdateSceneNodeChildren(folderPath + "/" + cslamFileName + ".json");
                    msg = "地图及场景节点已加载！";
                }
                AndroidUtils.GetInstance().ShowToast(msg);
            }
        }

        /// <summary>
        /// 保存场景节点
        /// </summary>
        private void SaveSceneNodeChildren(string jsonFilePath)
        {
            string msg = "";
            List<MapContentVO> mapContentList = new List<MapContentVO>();
            for (int i = 0; i < content.transform.childCount; i++)
            {
                MapContentVO mapContentInfo = new MapContentVO();
                GameObject pic = content.transform.GetChild(i).gameObject;
                mapContentInfo.t_x = pic.transform.localPosition.x.ToString();
                mapContentInfo.t_y = pic.transform.localPosition.y.ToString();
                mapContentInfo.t_z = pic.transform.localPosition.z.ToString();
                mapContentInfo.r_x = pic.transform.localEulerAngles.x.ToString();
                mapContentInfo.r_y = pic.transform.localEulerAngles.y.ToString();
                mapContentInfo.r_z = pic.transform.localEulerAngles.z.ToString();
                mapContentInfo.o_name = pic.name;
                mapContentList.Add(mapContentInfo);
            }
            msg = JsonMapper.ToJson(mapContentList);

            if (saveToSdCard)
            {
                //先判断是否存在，再创建
                if (!File.Exists(jsonFilePath))
                {
                    FileStream fileStream = new FileStream(jsonFilePath, FileMode.OpenOrCreate);
                    fileStream.Close();
                }

                //写入内容
                File.WriteAllText(jsonFilePath, msg);
            }
            else
            {
                PlayerPrefs.SetString(POSE_KEY, msg);
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// 读取场景节点
        /// </summary>
        private void UpdateSceneNodeChildren(string jsonFilePath)
        {
            string data;
            if (saveToSdCard)
            {
                //先判断是否存在，再创建
                if (!File.Exists(jsonFilePath))
                {
                    data = "";
                }
                else
                {
                    //读取内容
                    data = File.ReadAllText(jsonFilePath);
                }
            }
            else
            {
                data = PlayerPrefs.GetString(POSE_KEY);
            }

            if (data != "")
            {
                JsonData jd = JsonMapper.ToObject(data);
                List<MapContentVO> mapContentList = new List<MapContentVO>();
                for (int i = 0; i < jd.Count; i++)
                {
                    GameObject pic = content.transform.Find(jd[i]["o_name"].ToString()).gameObject;
                    Vector3 v_t = new Vector3(float.Parse(jd[i]["t_x"].ToString()), float.Parse(jd[i]["t_y"].ToString()), float.Parse(jd[i]["t_z"].ToString()));
                    Vector3 v_r = new Vector3(float.Parse(jd[i]["r_x"].ToString()), float.Parse(jd[i]["r_y"].ToString()), float.Parse(jd[i]["r_z"].ToString()));

                    pic.transform.localPosition = new Vector3(v_t.x, v_t.y, v_t.z);
                    pic.transform.localEulerAngles = new Vector3(v_r.x, v_r.y, v_r.z);
                    //pic.transform.DOLocalMove(v_t, 0.3f);
                    //pic.transform.DORotate(v_r, 0.3f);
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (infoTxt != null)
            {
                if (!ifStart)
                {
                    infoTxt.text = "";
                }
                else
                {
                    infoTxt.text = string.Format("场景匹配度:{0}",
                        /*实时地图匹配度*/percentcD);
                }
            }
        }


        //更新输出地图文件的名称
        internal void updateOutputMapName(string res)
        {
            this.cslamFileName = res;
        }

    }

}