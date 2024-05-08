
using AOT;
using Eqgis.Utils;
using Holo.XR.Android;
using Holo.XR.Config;

#if ENGINE_XVISIO
using LitJson;
#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Holo.XR.Core
{
    /// <summary>
    /// Xv Cslam地图扫描器
    /// </summary>
    public class XvCslamMapScanner : XvCslamMapControl
    {
        private static string mapFilePath;
        private static string poseFilePath;
        private static string mapPackagePath;

        private void Awake()
        {
            //默认文件夹路径
            if (folderPath == null || folderPath.Equals(""))
            {
                //安卓持久化存储路径为:/Android/data/包路径/
                folderPath = Application.persistentDataPath;
            }
        }

        private void OutputMapInfo()
        {
            if (Application.platform != RuntimePlatform.Android)
                return;
            EqLog.d("XvCslamMapLoader", "MapQuality:" + mapQuality + " ;MatchingPercent:" + mapMatchingPercent);
        }

        public void Start()
        {
            StartMap();

#if DEBUG_MODEL
                InvokeRepeating("OutputMapInfo", 3.0f, 1.0f);
#endif
        }

        private void OnDestroy()
        {
            StopMap();
        }

        /// <summary>
        /// 保存地图
        /// </summary>
        public void SaveMap()
        {
            if (Application.platform != RuntimePlatform.Android)
                return;


            //默认文件夹路
            if (mapName == null || mapName.Equals(""))
            {
                //默认采用时间戳作为文件名称
                mapName = GetFormattedTimestamp();
            }

            try
            {
                //保存地图
                mapFilePath = folderPath + HoloConfig.cacheFolder + mapName + HoloConfig.cslamMapSuffix;

#if ENGINE_XVISIO
                API.xslam_save_map_and_switch_to_cslam(mapFilePath, OnCslamSaved, OnSaveLocalized);
#endif
                //保存位置关系
                poseFilePath = folderPath + HoloConfig.cacheFolder + mapName + HoloConfig.tagPoseSuffix;
                SaveSceneNodeChildren(poseFilePath);
                mapPackagePath = folderPath + "/" + mapName + HoloConfig.mapPackageSuffix;


#if DEBUG_MODEL
                    EqLog.d("XvCslamMapSaver", "save Complete");

                    AndroidUtils.GetInstance().ShowToast("保存成功！\n" + mapPackagePath);
#endif
                complete?.Invoke();
            }catch(Exception e)
            {
                EqLog.e("XvCslamMapSaver", e.ToString());
            }
        }

        //=================内部===========================//
        /// <summary>
        /// 保存场景节点
        /// </summary>
        private void SaveSceneNodeChildren(string jsonFilePath)
        {
#if ENGINE_XVISIO
            string msg = "";
            List<NodePose> mapContentList = new List<NodePose>();
            for (int i = 0; i < content.transform.childCount; i++)
            {
                NodePose mapContentInfo = new NodePose();
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

            //先判断是否存在，再创建
            if (!File.Exists(jsonFilePath))
            {
                FileStream fileStream = new FileStream(jsonFilePath, FileMode.OpenOrCreate);
                fileStream.Close();
            }

            //写入内容
            //File.WriteAllText(jsonFilePath, msg);
            File.WriteAllBytes(jsonFilePath, Encoding.UTF8.GetBytes(msg));
#endif
        }

        /// <summary>
        /// 保存地图的回调实现
        /// </summary>
        /// <param name="status_of_saved_map"></param>
        /// <param name="map_quality"></param>
#if ENGINE_XVISIO
        [MonoPInvokeCallback(typeof(API.detectCslamSaved_callback))]
#endif
        static void OnCslamSaved(int status_of_saved_map, int map_quality)
        {//不是静态方法会出错
            mapQuality = map_quality;

            try
            {
                //保存完成打zip包
                ZipHelper.Instance.Zip(new string[] { mapFilePath, poseFilePath },
                    mapPackagePath, psd, null);
            }
            catch (Exception ex)
            {
                EqLog.w("XvCslamMapScanner", "Fail OnCslamSaved \n"+ ex.ToString());
            }
        }


        /// <summary>
        /// 保存地图匹配度的回调实现
        /// </summary>
        /// <param name="percentc"></param>
#if ENGINE_XVISIO
        [MonoPInvokeCallback(typeof(API.detectLocalized_callback))]
#endif
        static void OnSaveLocalized(float percent)
        {//不是静态方法会出错
            mapMatchingPercent = percent;
        }
    }

}