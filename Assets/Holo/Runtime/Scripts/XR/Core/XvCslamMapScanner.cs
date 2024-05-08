
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
    /// Xv Cslam��ͼɨ����
    /// </summary>
    public class XvCslamMapScanner : XvCslamMapControl
    {
        private static string mapFilePath;
        private static string poseFilePath;
        private static string mapPackagePath;

        private void Awake()
        {
            //Ĭ���ļ���·��
            if (folderPath == null || folderPath.Equals(""))
            {
                //��׿�־û��洢·��Ϊ:/Android/data/��·��/
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
        /// �����ͼ
        /// </summary>
        public void SaveMap()
        {
            if (Application.platform != RuntimePlatform.Android)
                return;


            //Ĭ���ļ���·
            if (mapName == null || mapName.Equals(""))
            {
                //Ĭ�ϲ���ʱ�����Ϊ�ļ�����
                mapName = GetFormattedTimestamp();
            }

            try
            {
                //�����ͼ
                mapFilePath = folderPath + HoloConfig.cacheFolder + mapName + HoloConfig.cslamMapSuffix;

#if ENGINE_XVISIO
                API.xslam_save_map_and_switch_to_cslam(mapFilePath, OnCslamSaved, OnSaveLocalized);
#endif
                //����λ�ù�ϵ
                poseFilePath = folderPath + HoloConfig.cacheFolder + mapName + HoloConfig.tagPoseSuffix;
                SaveSceneNodeChildren(poseFilePath);
                mapPackagePath = folderPath + "/" + mapName + HoloConfig.mapPackageSuffix;


#if DEBUG_MODEL
                    EqLog.d("XvCslamMapSaver", "save Complete");

                    AndroidUtils.GetInstance().ShowToast("����ɹ���\n" + mapPackagePath);
#endif
                complete?.Invoke();
            }catch(Exception e)
            {
                EqLog.e("XvCslamMapSaver", e.ToString());
            }
        }

        //=================�ڲ�===========================//
        /// <summary>
        /// ���泡���ڵ�
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

            //���ж��Ƿ���ڣ��ٴ���
            if (!File.Exists(jsonFilePath))
            {
                FileStream fileStream = new FileStream(jsonFilePath, FileMode.OpenOrCreate);
                fileStream.Close();
            }

            //д������
            //File.WriteAllText(jsonFilePath, msg);
            File.WriteAllBytes(jsonFilePath, Encoding.UTF8.GetBytes(msg));
#endif
        }

        /// <summary>
        /// �����ͼ�Ļص�ʵ��
        /// </summary>
        /// <param name="status_of_saved_map"></param>
        /// <param name="map_quality"></param>
#if ENGINE_XVISIO
        [MonoPInvokeCallback(typeof(API.detectCslamSaved_callback))]
#endif
        static void OnCslamSaved(int status_of_saved_map, int map_quality)
        {//���Ǿ�̬���������
            mapQuality = map_quality;

            try
            {
                //������ɴ�zip��
                ZipHelper.Instance.Zip(new string[] { mapFilePath, poseFilePath },
                    mapPackagePath, psd, null);
            }
            catch (Exception ex)
            {
                EqLog.w("XvCslamMapScanner", "Fail OnCslamSaved \n"+ ex.ToString());
            }
        }


        /// <summary>
        /// �����ͼƥ��ȵĻص�ʵ��
        /// </summary>
        /// <param name="percentc"></param>
#if ENGINE_XVISIO
        [MonoPInvokeCallback(typeof(API.detectLocalized_callback))]
#endif
        static void OnSaveLocalized(float percent)
        {//���Ǿ�̬���������
            mapMatchingPercent = percent;
        }
    }

}