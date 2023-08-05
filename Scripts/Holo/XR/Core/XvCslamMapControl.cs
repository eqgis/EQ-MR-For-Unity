
using Holo.XR.Android;
using Holo.XR.Config;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Holo.XR.Core
{
    /// <summary>
    /// 地图质量监听
    /// </summary>
    /// <param name="quality">地图质量</param>
    public delegate void MapQualityListener(int quality);

    /// <summary>
    /// 地图匹配度监听
    /// </summary>
    /// <param name="percent">匹配度</param>
    public delegate void MapMatchingListener(float percent);

    /// <summary>
    /// CSLAM 地图控制器
    /// </summary>
    public class XvCslamMapControl : MonoBehaviour
    {

        [Header("Folder Path")]
        public string folderPath;

        [Header("File Name")]
        public string mapName;

        [Tooltip("When completed")]
        public UnityEvent complete;

        [Tooltip("Root Node")]
        public GameObject content;

        //地图质量
        protected static int mapQuality = -1;
        //地图匹配度
        protected static float mapMatchingPercent = 0.0f;

        private bool start = false;

        protected static string psd = "holotech";

        /// <summary>
        /// 获取地图质量
        /// </summary>
        /// <returns>地图质量</returns>
        public int GetMapQuality()
        {
            return mapQuality;
        }

        /// <summary>
        /// 获取地图匹配度
        /// </summary>
        /// <returns>地图匹配度</returns>
        public float GetMapMatchingPercent()
        {
            return mapMatchingPercent;
        }

        private void Awake()
        {
            //默认文件夹路径
            if (folderPath == null || folderPath.Equals(""))
            {
                //安卓持久化存储路径为:/Android/data/包路径/
                folderPath = Application.persistentDataPath;
            }
        }


        /// <summary>
        /// 获取当前时间戳
        /// </summary>
        /// <returns></returns>
        protected string GetFormattedTimestamp()
        {
            DateTime currentTime = DateTime.Now;
            string formattedTimestamp = currentTime.ToString("yyyy_MM_dd_HH_mm");
            return formattedTimestamp;
        }

        /// <summary>
        /// 开启建图
        /// </summary>
        public void StartMap()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                //判断一下，防止Saver和Loader同时出现的情况，导致重复调用
                if (!start)
                {
                    // 开启cslam地图共享模式接口
                    API.xslam_start_map();
                    start = true;
                }
                if (AndroidUtils.debug)
                {
                    AndroidUtils.GetInstance().ShowToast("xslam_start_map");
                }
            }
        }

        /// <summary>
        /// 关闭建图模式
        /// </summary>
        public void StopMap()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                if (start)
                {
                    //先修改标记状态
                    start = false;
                    // 关闭cslam地图共享模式接口
                    API.xslam_stop_map();
                }
                if (AndroidUtils.debug)
                {
                    AndroidUtils.GetInstance().ShowToast("xslam_stop_map");
                }
            }
        }

        /// <summary>
        /// 判断是否开启建图
        /// </summary>
        /// <returns>状态</returns>
        public bool IsStart()
        {
            return start;
        }
    }

    /// <summary>
    /// 节点姿态信息
    /// </summary>
    public class NodePose
    {
        public string t_x = "";
        public string t_y = "";
        public string t_z = "";
        public string o_name = "";
        public string r_x = "";
        public string r_y = "";
        public string r_z = "";
    }
}
