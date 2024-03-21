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

    public class ARCoreImageDetect : MonoBehaviour
    {
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


            [SerializeField, Tooltip("Exif��Ϣ��ʾ�������ֶΣ�����ֵ����")]
            string[] m_Exif;

            public float width
            {
                get => m_Width;
                set => m_Width = value;
            }


            public string[] exif
            {
                get => m_Exif;
                set => m_Exif = value;
            }

            public AddReferenceImageJobState jobState { get; set; }
        }


        [Header("ARTrackedImageManager")]
        public ARTrackedImageManager m_TrackedImageManager;
       
        [Header("ImageTracking Event")]
        public DetectCallback detectCallback;

        private Dictionary<string,ARImageInfo> m_ImageDic = new Dictionary<string, ARImageInfo>();

        [Tooltip("��Startʱ�Զ�����ͼƬ"), Header("Dynamic Image Database")]
        public bool autoLoadImage = true;

        [SerializeField, Tooltip("���ͼƬ����")]
        ImageData[] m_Images;

        [Header("When Image Load Completed"),Tooltip("ͼ�����ݿ�������ʱִ�������¼�")]
        public UnityEvent loadComplete;


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
            if (autoLoadImage) { 
                //�Զ�����ͼƬ    
                UpdateImageData();
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


        void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
        {
            foreach (var trackedImage in eventArgs.added)
            {
                // Give the initial image a reasonable default scale
                //trackedImage.transform.localScale = new Vector3(0.01f, 1f, 0.01f);
                UpdateInfo(trackedImage);
            }

            foreach (var trackedImage in eventArgs.updated)
                UpdateInfo(trackedImage);

        }

        /// <summary>
        /// ��ָ��ͼƬ���Ƶ�Image�������Exif��Ϣ
        /// </summary>
        /// <param name="imageName"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void addExif(string imageName,string key, string value)
        {
            if (!m_ImageDic.ContainsKey(imageName))
            {
                ARImageInfo imageInfo = new ARImageInfo();
                imageInfo.name = imageName;
                m_ImageDic.Add(imageName, imageInfo);
            }

            m_ImageDic[imageName].addExif(key, value);

        }

        /// <summary>
        /// �Ƴ�ָ��ͼƬ���Ƶ�Image�����ָ���ֶε�Exif��Ϣ
        /// </summary>
        /// <param name="imageName"></param>
        /// <param name="key"></param>
        public void removeExif(string imageName, string key)
        {
            if (m_ImageDic.ContainsKey(imageName))
            {
                m_ImageDic[imageName].removeExif(key);
            }
        }

        void UpdateInfo(ARTrackedImage trackedImage)
        {
            //���ڸ���״̬
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                //�����ص�
                if (detectCallback != null)
                {
                    string imageName = trackedImage.referenceImage.name;
#if DEBUG_LOG
                    EqLog.d("ARCoreImageDetect", "image:" + imageName);
#endif
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

                    detectCallback.OnDetect(imageInfo);
                }

                trackedImage.transform.gameObject.SetActive(true);

            }
            else
            {
                trackedImage.transform.gameObject.SetActive(false);
            }

        }


        private ARImageDataState m_State = ARImageDataState.NoImagesAdded;


        /// <summary>
        /// ����ͼƬ���ݿ�
        /// </summary>

        public void UpdateImageData()
        {
            m_State = ARImageDataState.AddImagesRequested;
#if DEBUG_LOG
            EqLog.d("ARCoreImageDetect", "updateImageData:status->" + m_State);
#endif
        }

        void SetError(string errorMessage)
        {
            m_State = ARImageDataState.Error;
            EqLog.e("ARCoreImageDetect", $"Error: {errorMessage}");
        }

        void Update()
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
                                    image.jobState = mutableLibrary.ScheduleAddImageWithValidationJob(image.texture, image.name, image.width);
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