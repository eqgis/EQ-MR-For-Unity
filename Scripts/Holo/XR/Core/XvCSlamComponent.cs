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
        private static int switch_map_quality = 0;//���ص�ͼƷ��
        private static float percentcD = -1;//��ͼƥ���
        private static int status_of_saved_mapq = 0;//��ͼ״̬
        private static int save_map_qualityq = 0;//��ͼƥ���
        public GameObject content;

        [Header("Core"), Tooltip("��ͼ�ļ�")]
        public string cslamFileName = "map";

        [Tooltip("ͬ�����½ڵ�λ��")]
        public bool updateNodePose = true;
        [Tooltip("����������")]
        public bool saveToSdCard = true;

        [Header("Status UI")]
        public Text infoTxt;

        private bool ifStart = false;

        // Start is called before the first frame update
        void Start()
        {
        }

        /// <summary>
        /// ���ص�ͼ�Ļص�����ʵ��
        /// </summary>
        /// <param name="map_quality"></param>
        [MonoPInvokeCallback(typeof(API.detectSwitched_callback))]
        static void OnCslamSwitched(int map_quality)
        {
            switch_map_quality = map_quality;
        }

        /// <summary>
        /// �����ͼƥ��ȵĻص�ʵ��
        /// </summary>
        /// <param name="percentc"></param>
        [MonoPInvokeCallback(typeof(API.detectLocalized_callback))]
        static void OnSaveLocalized(float percentc)
        {
            percentcD = percentc;
        }

        /// <summary>
        /// ���ص�ͼ��ƥ��ȵĻص�ʵ��
        /// </summary>
        /// <param name="percentc"></param>
        [MonoPInvokeCallback(typeof(API.detectLocalized_callback))]
        static void OnLoadLocalized(float percentc)
        {
            percentcD = percentc;

        }

        /// <summary>
        /// �����ͼ�Ļص�ʵ��
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
        /// ����slam
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

                AndroidUtils.GetInstance().ShowToast("CSLAM �ѿ�����");
            }
        }

        /// <summary>
        /// ֹͣslam
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

                AndroidUtils.GetInstance().ShowToast("SLAM �ѹرգ�");
            }
        }

        /// <summary>
        /// �����ͼ
        /// </summary>
        public void SaveMap()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                API.xslam_save_map_and_switch_to_cslam(Application.persistentDataPath + "/" + cslamFileName + ".bin", OnCslamSaved, OnSaveLocalized);

                string msg = "��ͼ�ѱ��棡";
                if (updateNodePose)
                {
                    //���泡���ڵ��ֱ���ӽڵ��λ��
                    SaveSceneNodeChildren(Application.persistentDataPath + "/" + cslamFileName + ".json");
                    msg = "��ͼ�������ڵ��ѱ��棡";
                }
                AndroidUtils.GetInstance().ShowToast(msg);
            }
        }

        /// <summary>
        /// ���ص�ͼ
        /// </summary>
        public void LoadMap()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                API.xslam_load_map_and_switch_to_cslam(Application.persistentDataPath + "/" + cslamFileName + ".bin", OnCslamSwitched, OnLoadLocalized);

                string msg = "��ͼ�Ѽ��أ�";
                if (updateNodePose)
                {
                    //���³����ڵ���Ӷ����λ��
                    UpdateSceneNodeChildren(Application.persistentDataPath + "/" + cslamFileName + ".json");
                    msg = "��ͼ�������ڵ��Ѽ��أ�";
                }
                AndroidUtils.GetInstance().ShowToast(msg);
            }
        }

        /// <summary>
        /// �����ͼ��ָ���ļ���
        /// </summary>
        /// <param name="folderPath">�ļ���·��</param>
        public void SaveMapToCustomFolder(string folderPath)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                API.xslam_save_map_and_switch_to_cslam(folderPath + "/" + cslamFileName + ".bin", OnCslamSaved, OnSaveLocalized);

                string msg = "��ͼ�ѱ��棡";
                if (updateNodePose)
                {
                    //���泡���ڵ��ֱ���ӽڵ��λ��
                    SaveSceneNodeChildren(folderPath + "/" + cslamFileName + ".json");
                    msg = "��ͼ�������ڵ��ѱ��棡";
                }
                AndroidUtils.GetInstance().ShowToast(msg);
            }
        }

        /// <summary>
        /// ��ָ���ļ��м��ص�ͼ
        /// </summary>
        /// <param name="folderPath">�ļ���·��</param>
        public void LoadMapFromCustomFolder(string folderPath)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                API.xslam_load_map_and_switch_to_cslam(folderPath + "/" + cslamFileName + ".bin", OnCslamSwitched, OnLoadLocalized);

                string msg = "��ͼ�Ѽ��أ�";
                if (updateNodePose)
                {
                    //���³����ڵ���Ӷ����λ��
                    UpdateSceneNodeChildren(folderPath + "/" + cslamFileName + ".json");
                    msg = "��ͼ�������ڵ��Ѽ��أ�";
                }
                AndroidUtils.GetInstance().ShowToast(msg);
            }
        }

        /// <summary>
        /// ���泡���ڵ�
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
                //���ж��Ƿ���ڣ��ٴ���
                if (!File.Exists(jsonFilePath))
                {
                    FileStream fileStream = new FileStream(jsonFilePath, FileMode.OpenOrCreate);
                    fileStream.Close();
                }

                //д������
                File.WriteAllText(jsonFilePath, msg);
            }
            else
            {
                PlayerPrefs.SetString(POSE_KEY, msg);
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// ��ȡ�����ڵ�
        /// </summary>
        private void UpdateSceneNodeChildren(string jsonFilePath)
        {
            string data;
            if (saveToSdCard)
            {
                //���ж��Ƿ���ڣ��ٴ���
                if (!File.Exists(jsonFilePath))
                {
                    data = "";
                }
                else
                {
                    //��ȡ����
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
                    infoTxt.text = string.Format("����ƥ���:{0}",
                        /*ʵʱ��ͼƥ���*/percentcD);
                }
            }
        }


        //���������ͼ�ļ�������
        internal void updateOutputMapName(string res)
        {
            this.cslamFileName = res;
        }

    }

}