
using Holo.XR.Android;
using Holo.XR.Config;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Holo.XR.Core
{
    /// <summary>
    /// ��ͼ��������
    /// </summary>
    /// <param name="quality">��ͼ����</param>
    public delegate void MapQualityListener(int quality);

    /// <summary>
    /// ��ͼƥ��ȼ���
    /// </summary>
    /// <param name="percent">ƥ���</param>
    public delegate void MapMatchingListener(float percent);

    /// <summary>
    /// CSLAM ��ͼ������
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

        //��ͼ����
        protected static int mapQuality = -1;
        //��ͼƥ���
        protected static float mapMatchingPercent = 0.0f;

        private bool start = false;

        protected static string psd = "holotech";

        /// <summary>
        /// ��ȡ��ͼ����
        /// </summary>
        /// <returns>��ͼ����</returns>
        public int GetMapQuality()
        {
            return mapQuality;
        }

        /// <summary>
        /// ��ȡ��ͼƥ���
        /// </summary>
        /// <returns>��ͼƥ���</returns>
        public float GetMapMatchingPercent()
        {
            return mapMatchingPercent;
        }

        private void Awake()
        {
            //Ĭ���ļ���·��
            if (folderPath == null || folderPath.Equals(""))
            {
                //��׿�־û��洢·��Ϊ:/Android/data/��·��/
                folderPath = Application.persistentDataPath;
            }
        }


        /// <summary>
        /// ��ȡ��ǰʱ���
        /// </summary>
        /// <returns></returns>
        protected string GetFormattedTimestamp()
        {
            DateTime currentTime = DateTime.Now;
            string formattedTimestamp = currentTime.ToString("yyyy_MM_dd_HH_mm");
            return formattedTimestamp;
        }

        /// <summary>
        /// ������ͼ
        /// </summary>
        public void StartMap()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                //�ж�һ�£���ֹSaver��Loaderͬʱ���ֵ�����������ظ�����
                if (!start)
                {
                    // ����cslam��ͼ����ģʽ�ӿ�
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
        /// �رս�ͼģʽ
        /// </summary>
        public void StopMap()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                if (start)
                {
                    //���޸ı��״̬
                    start = false;
                    // �ر�cslam��ͼ����ģʽ�ӿ�
                    API.xslam_stop_map();
                }
                if (AndroidUtils.debug)
                {
                    AndroidUtils.GetInstance().ShowToast("xslam_stop_map");
                }
            }
        }

        /// <summary>
        /// �ж��Ƿ�����ͼ
        /// </summary>
        /// <returns>״̬</returns>
        public bool IsStart()
        {
            return start;
        }
    }

    /// <summary>
    /// �ڵ���̬��Ϣ
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
