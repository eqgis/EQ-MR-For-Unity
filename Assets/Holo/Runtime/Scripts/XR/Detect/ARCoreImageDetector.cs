#if ENGINE_ARCORE
using Holo.XR.Android;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Holo.XR.Detect
{
    /// <summary>
    /// ARͼƬʶ���״̬
    /// </summary>
    internal enum ARImageDataState
    {
        NoImagesAdded,
        AddImagesRequested,
        AddingImages,
        Done,
        Error
    }

    /// <summary>
    /// ͼƬ����
    /// </summary>
    [Serializable]
    public class ImageData
    {
        [SerializeField, Tooltip("ͼ��Դ��������Ϊ�ɶ�")]
        Texture2D m_Texture;

        public Texture2D texture
        {
            get => m_Texture;
            set => m_Texture = value;
        }

        [SerializeField, Tooltip("����")]
        string m_Name;

        public string name
        {
            get => m_Name;
            set => m_Name = value;
        }

        [SerializeField, Tooltip("��ȣ���λ:�ף�����ʵ����ͼƬ�Ŀ��")]
        float m_Width;

        [Header("TrackedImagePrefab"), Tooltip("ʶ��ͼ�����صĶ���")]
        public GameObject prefab;

        public float width
        {
            get => m_Width;
            set => m_Width = value;
        }

        public AddReferenceImageJobState jobState { get; set; }
    }


    public class ARCoreImageDetector : MonoBehaviour
    {


        [Header("ARTrackedImageManager")]
        public ARTrackedImageManager m_TrackedImageManager;

       
        [Header("ImageTracking Event")]
        public DetectCallback detectCallback;

        private Dictionary<string,ARImageInfo> m_ImageDic = new Dictionary<string, ARImageInfo>();

        [Tooltip("��Startʱ�Զ�����ͼƬ,��ǰ��ʱ3���ִ��")]
        public bool autoLoadImage = true;

        [Tooltip("ʹ��ͼƬ�ĳߴ磬��Ϊfalse����ͼ��׷��ʱ��ȡ��ͼ��ߴ����Ϊ1.0"), Header("Dynamic Image Database")]
        public bool useImageSize = true;

        [Tooltip("����ͼƬʶ��ʱ����ʾPrefab")]
        public bool onlyActiveWhenTracking = false;

        [SerializeField, Tooltip("���ͼƬ����")]
        ImageData[] m_Images;

        [Header("When Image Load Completed"),Tooltip("ͼ�����ݿ�������ʱִ�������¼�")]
        public UnityEvent loadComplete;

        //Ԥ������dic
        Dictionary<string, GameObject> m_PrefabsDictionary = new Dictionary<string, GameObject>();

        //�������Ԥ�Ƽ�
        Dictionary<string, GameObject> m_Instantiated = new Dictionary<string, GameObject>();

        private ARImageDataState m_State = ARImageDataState.NoImagesAdded;


        /// <summary>
        /// ��ȡͼ������
        /// </summary>
        /// <returns></returns>
        public ImageData[] GetImageData() { return m_Images; }

        void Awake()
        {
            if (m_TrackedImageManager == null)
            {
                EqLog.e("ARCoreImageDetect", "ARTrackedImageManager was null.");
            }
        }

        void OnEnable()
        {
#if DEBUG_LOG
            EqLog.d("ARCoreImageDetect", "OnEnable");
#endif
            m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
        }

        void OnDisable()
        {
#if DEBUG_LOG
            EqLog.d("ARCoreImageDetect", "OnDisable");
#endif
            m_TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
        }

        private void Start()
        {
            if (autoLoadImage)
            {
                //�Զ�����ͼƬ����ʱִ��
                Invoke("LoadImageData", 3.0f);
            }
#if DEBUG_LOG
            EqLog.d("ARCoreImageDetect", "Start");
#endif
        }

        private void OnDestroy()
        {
#if DEBUG_LOG
            EqLog.d("ARCoreImageDetect", "OnDestroy");
#endif
        }


        /// <summary>
        /// �����¼�
        /// </summary>
        /// <param name="eventArgs"></param>
        void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
        {
            //added ��һ��ʶ�𵽣������added
            foreach (var trackedImage in eventArgs.added)
            {
                // Give the initial image a reasonable default scale
                //trackedImage.transform.localScale = new Vector3(0.01f, 1f, 0.01f);
                ARImageInfo aRImageInfo = UpdateInfo(trackedImage);

                //����Ԥ�Ƽ�
                CreatePrefab(aRImageInfo);
                Callback(trackedImage, aRImageInfo,0);
            }

            //updated ����ʶ�𵽣���һֱ����updateed
            foreach (var trackedImage in eventArgs.updated)
            {
                //������Ϣ������λ�˵�������Ϣ
                Callback(trackedImage, UpdateInfo(trackedImage),1);
            }

            ////removed
            //foreach (var trackedImage in eventArgs.removed)
            //{
            //    //�Ƴ�Ԥ�Ƽ�,��ʱ���Ƴ�
            //    //RemovePrefab(trackedImage.referenceImage.name);
            //    UpdateInfo(trackedImage,2);
            //}

        }

        /// <summary>
        /// �Ƴ�Ԥ�Ƽ�
        /// <param name="name">ͼƬ����</param>
        /// </summary>
        public void RemovePrefab(string name)
        {
            if (m_Instantiated.TryGetValue(name, out var gameObject))
            {
                //���ٶ���
                Destroy(gameObject);
                m_Instantiated.Remove(name);
            }
        }

        /// <summary>
        /// ����Ԥ�Ƽ�
        /// </summary>
        /// <param name="trackedImage"></param>
        void CreatePrefab(ARImageInfo trackedImage)
        {
            if (trackedImage == null)
            {
                return;
            }
            //��prefabDic���ж��壬����Ҫ���뵽������
            if (m_PrefabsDictionary.TryGetValue(trackedImage.name, out var prefab))
            {
                //����û�����볡��������Ҫ����ʵ�����������󶨸��ڵ�trackedImage.transform����������
                try
                {
                    m_Instantiated[trackedImage.name] = Instantiate(prefab, trackedImage.transform);
#if DEBUG_LOG
                    EqLog.d("ARCoreImageDetect ", "Instantiate:"
                        + trackedImage.name + " prefabsDic:" + m_PrefabsDictionary.Count
                        + " m_Instantiated.count:" + m_Instantiated.Count
                        + " prefabName:"+prefab.name);
#endif
                }
                catch (Exception ex)
                {
                    EqLog.w("ARCoreImageDetect", $"CreatePrefab-Error: {ex.Message}");
                }

            }
        }

        private void Callback(ARTrackedImage trackedImage,ARImageInfo imageInfo,int type)
        {
            if (detectCallback != null)
            {
                //�����ص�
                switch (type)
                {
                    case 0:
                        if (trackedImage.trackingState == TrackingState.Tracking)
                            detectCallback.OnAdded(imageInfo);
                        break;
                    case 1:
                        if (trackedImage.trackingState == TrackingState.Tracking)
                            detectCallback.OnUpdate(imageInfo);
                        break;
                }
            }
        }

        /// <summary>
        /// ����ARImage����Ϣ��������callback
        /// </summary>
        /// <param name="trackedImage"></param>
        ARImageInfo UpdateInfo(ARTrackedImage trackedImage)
        {
            if (onlyActiveWhenTracking)
            {
                //���ڸ���״̬
                if (trackedImage.trackingState == TrackingState.Tracking)
                {
                    trackedImage.transform.gameObject.SetActive(true);

                }
                else
                {
                    trackedImage.transform.gameObject.SetActive(false);
                }
            }
            else
            {
                trackedImage.transform.gameObject.SetActive(true);
            }

            if (useImageSize)
            {
                // Give the initial image a reasonable default scale
                var minLocalScalar = Mathf.Min(trackedImage.size.x, trackedImage.size.y) / 2;
                trackedImage.transform.localScale = new Vector3(minLocalScalar, minLocalScalar, minLocalScalar);
            }

            string imageName = trackedImage.referenceImage.name;

            ARImageInfo imageInfo;
            //����extents��size��transform����
            if (m_ImageDic.ContainsKey(imageName))
            {
                imageInfo = m_ImageDic[imageName];
                imageInfo.extents = trackedImage.extents;
                imageInfo.size = trackedImage.size;
                imageInfo.transform = trackedImage.transform;
            }
            else
            {
                //�����������½�
                imageInfo = new ARImageInfo();
                //����
                imageInfo.name = imageName;
                imageInfo.extents = trackedImage.extents;
                imageInfo.size = trackedImage.size;
                imageInfo.transform = trackedImage.transform;
                //���
                m_ImageDic.Add(imageName, imageInfo);
            }

            return imageInfo;
        }



        /// <summary>
        /// ����ͼƬ���ݿ�
        /// <!--����done�������ִ��-->
        /// </summary>

        public void LoadImageData()
        {
            switch (m_State)
            {
                case ARImageDataState.NoImagesAdded:
                    m_State = ARImageDataState.AddImagesRequested;
                    break;
            }
#if DEBUG_LOG
            EqLog.d("ARCoreImageDetect", "LoadImageData:status->" + m_State);
#endif
        }

        void SetError(string errorMessage)
        {
            m_State = ARImageDataState.Error;
            EqLog.e("ARCoreImageDetect", $"Error: {errorMessage}");
        }

        void Update()
        {
            try
            {

                switch (m_State)
                {
                    case ARImageDataState.AddImagesRequested:
                        {
                            if (m_Images == null)
                            {
                                SetError("No images to add.");
                                break;
                            }

                            if (m_TrackedImageManager == null)
                            {
                                SetError($"No {nameof(ARTrackedImageManager)} available.");
                                break;
                            }

                            // ARCore ͼƬ���ݿ�ֻ���Դ���ɶ���Texture2D
                            foreach (var image in m_Images)
                            {
                                if (!image.texture.isReadable)
                                {
                                    //ע�⣺���Ǵ����texture���ǿɶ�״̬��������cloneһ��
                                    //���ȸ�ʹ�������У���ͼƬ��Դδ��AOT����
                                    //��ֻ�����ȸ���AB����"PresetAssets"��ʱ������ֲ��ɶ���״̬��
                                    //��ʱ����Ҫclone�ˣ�������ÿɶ�״̬���ж�
                                    image.texture = cloneTexture(image.texture);
#if DEBUG_LOG
                                    EqLog.d("ARCoreImageDetect", "cloneTexture:->" + image.texture);
#endif
                                }
                            }

                            if (m_TrackedImageManager.referenceLibrary is MutableRuntimeReferenceImageLibrary mutableLibrary)
                            {
                                try
                                {
                                    foreach (var image in m_Images)
                                    {
                                        // Note: You do not need to do anything with the returned JobHandle, but it can be
                                        // useful if you want to know when the image has been added to the library since it may
                                        // take several frames.
                                        //��ͼƬ���ݿ����ͼ��Texture
                                        image.jobState = mutableLibrary.ScheduleAddImageWithValidationJob(image.texture, image.name, image.width);

                                        //��prefabDic�����image��Ӧ��prefab
                                        m_PrefabsDictionary.Add(image.name, image.prefab);
#if DEBUG_LOG
                                        EqLog.d("ARCoreImageDetect", "PrefabsDictionary.add:->" + image.texture);
#endif
                                    }

                                    m_State = ARImageDataState.AddingImages;
                                }
                                catch (InvalidOperationException e)
                                {
                                    SetError($"ScheduleAddImageJob threw exception: {e.Message}");
                                }
                            }
                            else
                            {
                                SetError($"The reference image library is not mutable.");
                            }

                            break;
                        }
                    case ARImageDataState.AddingImages:
                        {
                            // Check for completion
                            var done = true;
                            foreach (var image in m_Images)
                            {
                                if (!image.jobState.jobHandle.IsCompleted)
                                {
                                    done = false;
                                    break;
                                }
                            }

                            if (done)
                            {
                                m_State = ARImageDataState.Done;
                                //ͼƬ���ݼ������
                                if (loadComplete != null)
                                {
                                    loadComplete.Invoke();
                                }
                            }

                            break;
                        }
                }
            }catch(Exception e)
            {
                EqLog.w("ARCoreImageDetector", e.Message);
            }
        }



        private Texture2D cloneTexture(Texture2D texture)
        {
            // ����һ���������С��ͬ����ʱ RenderTexture
            RenderTexture tmp = RenderTexture.GetTemporary(
                                texture.width,
                                texture.height,
                                0,
                                RenderTextureFormat.Default,
                                RenderTextureReadWrite.Linear);


            // �������ϵ����� Blit �� RenderTexture
            Graphics.Blit(texture, tmp);


            // ���ݵ�ǰ���õ� RenderTexture
            RenderTexture previous = RenderTexture.active;


            // ����ǰ�� RenderTexture ����Ϊ���Ǵ�������ʱ
            RenderTexture.active = tmp;


            // ����һ���µĿɶ� Texture2D �����ظ��Ƶ���
            Texture2D myTexture2D = new Texture2D(texture.width, texture.height);


            // �����ش� RenderTexture ���Ƶ��µ� Texture
            myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            myTexture2D.Apply();


            // ���û�� RenderTexture
            RenderTexture.active = previous;


            // �ͷ���ʱ��RenderTexture
            RenderTexture.ReleaseTemporary(tmp);


            // ��myTexture2D�����ھ����롰texture����ͬ�����أ���������
            return myTexture2D;
        }
        //end
    }


}

#endif