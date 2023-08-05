
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
    /// CSLAM ��ͼ������
    /// </summary>
    public class XvCslamMapLoader : XvCslamMapControl
    {

        [Header("Load Settings")]
        public bool autoLoad = true;

        [Tooltip("The file is in the directory of streamingAssets.")]
        public bool streamingAssets = false;

        //Ĭ���Զ����ص���ʱʱ�䣬��Ӱ���һ�γ�������
        private int delay = 5000;

        private void Awake()
        {
            //Ĭ���ļ���·��
            if (folderPath == null || folderPath.Equals(""))
            {
                //��׿�־û��洢·��Ϊ:/Android/data/��·��/
                folderPath = Application.persistentDataPath;
            }
        }

        public void Start()
        {
            StartMap();
            if (autoLoad)
            {
                //��һ��ʶ�𳡾���Ҫ��ʱһ��
                if (NodePoseRecorder.GetInstance().count == 0)
                {
                    //�Զ���ȡ��ͼ
                    Invoke("LoadMap", delay / 1000.0f);
                }
                else
                {
                    //�Զ���ȡ��ͼ
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
        /// ���ص�ͼ
        /// </summary>
        public void LoadMap()
        {

            if (Application.platform != RuntimePlatform.Android)
                return;
            StartCoroutine(LoadMapAsync());
        }

        private IEnumerator LoadMapAsync()
        {
            //�ȶ�ȡ����" ./cache/..."
            string cacheFolderPath = folderPath + HoloConfig.cacheFolder;
            string mapPath = cacheFolderPath + mapName + HoloConfig.cslamMapSuffix;
            string tagPose = cacheFolderPath + mapName + HoloConfig.tagPoseSuffix;

            if (!File.Exists(mapPath) || !File.Exists(tagPose))
            {
                //��ͼ�ļ������ڻ���Tag��̬�ļ�������

                if (streamingAssets)
                {
                    //���ǲ���streamingAssets��ģʽ�������ݴ���streamingAssets�ļ��£����ڶ�������Ϊfalse����ǿ�Ƹ��¡��־û�·���»��Ȳ����ļ�
                    //"�־û�·��/mapName.homap"
                    //��streamAssets��cp���־û�·��
                    AndroidUtils.Toast("1");
                    //string targetMapPachagePath = IoUtils.CopyMapToPersistentDataPath(folderPath + "/" + mapName + HoloConfig.mapPackageSuffix, false);

                    string relativePath = folderPath + "/" + mapName + HoloConfig.mapPackageSuffix;
                    //��ʽУ��
                    relativePath.Replace("\\", "/");
                    if (!relativePath.StartsWith("/"))
                    {
                        relativePath = "/" + relativePath;
                    }

                    //ʾ������·��:"/homap/test.homap"
                    //����־û�Ŀ¼��û���ļ����ȴ�streamingAssets�︴��һ�ݵ��־û�Ŀ¼
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
                    //���ó־û����ݵ��ļ���·��
                    folderPath = targetPersistentPath.Replace("/" + mapName + HoloConfig.mapPackageSuffix, "");

                    AndroidUtils.Toast("3");
                    //��ѹ
                    UnZipMapFilePackage(folderPath, mapName);
                }
                else
                {
                    try
                    {
                        //���н�ѹ����
                        string mapPackagePath = folderPath + "/" + mapName + HoloConfig.mapPackageSuffix;
                        if (!File.Exists(mapPackagePath))
                        {
                            if (AndroidUtils.debug)
                            {
                                AndroidUtils.GetInstance().ShowToast("Map�ļ�������");
                            }

                            EqLog.e("XvCslamMapLoader", "Do not find file. " + mapPackagePath);
                        }
                        else
                        {
                            //��ѹ
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
                //ʹ��XVisio API ��ȡcslam
                API.xslam_load_map_and_switch_to_cslam(mapPath, OnCslamSwitched, OnLoadLocalized);
                //��ȡλ�ù�ϵ
                LoadSceneNodeChildren(tagPose);

                if (AndroidUtils.debug)
                {
                    AndroidUtils.GetInstance().ShowToast("ƥ��ɹ���\n" + mapPath);
                    EqLog.d("XvCslamMapLoader", "loadingComplete");
                }
                complete?.Invoke();
            }
            catch (Exception e)
            {
                EqLog.e("XvCslamMapLoader", e.ToString());
            }



        }
        //=================�ڲ�===========================//

        /// <summary>
        /// ��ѹ��ͼ�ļ���
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="fileName"></param>
        private void UnZipMapFilePackage(string folderPath, string fileName)
        {
            string filePath = folderPath + "/" + fileName + HoloConfig.mapPackageSuffix;
            string cachePath = folderPath + HoloConfig.cacheFolder;
            //��ѹ���������·��
            ZipHelper.Instance.UnzipFile(filePath, cachePath,psd,null);
        }

        /// <summary>
        /// ��ȡ�����ڵ�
        /// </summary>
        private void LoadSceneNodeChildren(string jsonFilePath)
        {
            string data;

            //���ж��Ƿ���ڣ��ٴ���
            if (!File.Exists(jsonFilePath))
            {
                if (AndroidUtils.debug)
                {
                    AndroidUtils.GetInstance().ShowToast("ȱ��Tagλ����Ϣ");
                }
                data = "";
            }
            else
            {
                //��ȡ����
                //data = File.ReadAllText(jsonFilePath);
                byte[] bytes = File.ReadAllBytes(jsonFilePath);
                data = Encoding.UTF8.GetString(bytes);
            }

            EqLog.d("XvCslamMapLoader", data);
            if (data != "")
            {
                JsonData jd = JsonMapper.ToObject(data);
                //List<MapContentVO> mapContentList = new List<MapContentVO>();
                //��ȡ�ڴ��еı������ڵ��������һ������ԭ�����̬��
                NodePoseRecorder npr = NodePoseRecorder.GetInstance();


                //��ȡһ�Σ���һ����
                npr.count++;
                if (npr.count != 1)
                {
                    //ԭ�����ڵ�λ����̬�롰�״�ʶ�𳡾��ĳ����ڵ�λ�ˡ�����һ��
                    Vector3 firstSceneNodePosition = npr.FirstSceneNodePosition;
                    Vector3 firstSceneNodeRotation = npr.FirstSceneNodeRotation;

                    content.transform.position = firstSceneNodePosition;
                    content.transform.eulerAngles = firstSceneNodeRotation;

                    for (int i = 0; i < jd.Count; i++)
                    {
                        GameObject sceneNode = content.transform.Find(jd[i]["o_name"].ToString()).gameObject;
                        //Vector3 v_t = new Vector3(float.Parse(jd[i]["t_x"].ToString()), float.Parse(jd[i]["t_y"].ToString()), float.Parse(jd[i]["t_z"].ToString()));
                        //Vector3 v_r = new Vector3(float.Parse(jd[i]["r_x"].ToString()), float.Parse(jd[i]["r_y"].ToString()), float.Parse(jd[i]["r_z"].ToString()));

                        //���³����ڵ�(�Լ���Щͬ���ڵ㣨��content��ֱ���ӽڵ㡱��)�����λ�á�
                        sceneNode.transform.localPosition = npr.NextSceneNodePosition;
                        sceneNode.transform.localEulerAngles = npr.NextSceneNodeRotation;
                    }
                }
                else
                {
                    //��һ�� ��JSON
                    for (int i = 0; i < jd.Count; i++)
                    {
                        GameObject pic = content.transform.Find(jd[i]["o_name"].ToString()).gameObject;
                        Vector3 v_t = new Vector3(float.Parse(jd[i]["t_x"].ToString()), float.Parse(jd[i]["t_y"].ToString()), float.Parse(jd[i]["t_z"].ToString()));
                        Vector3 v_r = new Vector3(float.Parse(jd[i]["r_x"].ToString()), float.Parse(jd[i]["r_y"].ToString()), float.Parse(jd[i]["r_z"].ToString()));

                        pic.transform.localPosition = new Vector3(v_t.x, v_t.y, v_t.z);
                        pic.transform.localEulerAngles = new Vector3(v_r.x, v_r.y, v_r.z);

                        /**
                         * ע��
                         * ��һ�ζ�ȡmap.bin��ȷ��������ϵ��ͨ����ȡmap.jsonȷ��SceneNode��λ�á�
                         * 
                         */
                        //��һ�Σ�����json 1
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
        /// ���ص�ͼ�Ļص�����ʵ��
        /// </summary>
        /// <param name="map_quality">��ͼ����</param>
        [MonoPInvokeCallback(typeof(API.detectSwitched_callback))]
        static void OnCslamSwitched(int map_quality)
        {//���Ǿ�̬���������
            mapQuality = map_quality;
        }

        /// <summary>
        /// ���ص�ͼ��ƥ��ȵĻص�ʵ��
        /// </summary>
        /// <param name="percent">ƥ���</param>
        [MonoPInvokeCallback(typeof(API.detectLocalized_callback))]
        static void OnLoadLocalized(float percent)
        {//���Ǿ�̬���������
            mapMatchingPercent = percent;
        }
    }
}