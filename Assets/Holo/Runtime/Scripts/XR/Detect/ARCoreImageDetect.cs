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
    /// AR图片识别的状态
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
        /// 图片数据
        /// </summary>
        [Serializable]
        public class ImageData
        {
            [SerializeField, Tooltip("图像源，必须标记为可读")]
            Texture2D m_Texture;

            public Texture2D texture
            {
                get => m_Texture;
                set => m_Texture = value;
            }

            [SerializeField, Tooltip("名称")]
            string m_Name;

            public string name
            {
                get => m_Name;
                set => m_Name = value;
            }

            [SerializeField, Tooltip("宽度（单位:米），真实世界图片的宽度")]
            float m_Width;


            [SerializeField, Tooltip("Exif信息（示例：“字段：属性值”）")]
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

        [Tooltip("在Start时自动载入图片"), Header("Dynamic Image Database")]
        public bool autoLoadImage = true;

        [SerializeField, Tooltip("添加图片数据")]
        ImageData[] m_Images;

        [Header("When Image Load Completed"),Tooltip("图像数据库更新完成时执行以下事件")]
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
                //自动载入图片    
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
        /// 给指定图片名称的Image对象添加Exif信息
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
        /// 移除指定图片名称的Image对象的指定字段的Exif信息
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
            //处于跟踪状态
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                //监听回调
                if (detectCallback != null)
                {
                    string imageName = trackedImage.referenceImage.name;
#if DEBUG_LOG
                    EqLog.d("ARCoreImageDetect", "image:" + imageName);
#endif
                    ARImageInfo imageInfo;
                    //更新extents、size、transform数据
                    if (m_ImageDic.ContainsKey(imageName))
                    {
                        imageInfo = m_ImageDic[imageName];
                        imageInfo.extents = trackedImage.extents;
                        imageInfo.size = trackedImage.size;
                        imageInfo.transform = trackedImage.transform;
                    }
                    else
                    {
                        //不包括，需新建
                        imageInfo = new ARImageInfo();
                        //名称
                        imageInfo.name = imageName;
                        imageInfo.extents = trackedImage.extents;
                        imageInfo.size = trackedImage.size;
                        imageInfo.transform = trackedImage.transform;

                        //添加
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
        /// 更新图片数据库
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

                        // ARCore 图片数据库只可以传入可读的Texture2D
                        foreach (var image in m_Images)
                        {
                            if (!image.texture.isReadable)
                            {
                                //注意：若是传入的texture不是可读状态，则这里clone一个
                                //在热更使用流程中，若图片资源未在AOT程序集
                                //（只存在热更的AB包（"PresetAssets"）时，会出现不可读的状态）
                                //此时就需要clone了，这里采用可读状态做判断
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
                            //图片数据加载完成
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
            // 创建一个与纹理大小相同的临时 RenderTexture
            RenderTexture tmp = RenderTexture.GetTemporary(
                                texture.width,
                                texture.height,
                                0,
                                RenderTextureFormat.Default,
                                RenderTextureReadWrite.Linear);


            // 将纹理上的像素 Blit 到 RenderTexture
            Graphics.Blit(texture, tmp);


            // 备份当前设置的 RenderTexture
            RenderTexture previous = RenderTexture.active;


            // 将当前的 RenderTexture 设置为我们创建的临时
            RenderTexture.active = tmp;


            // 创建一个新的可读 Texture2D 将像素复制到它
            Texture2D myTexture2D = new Texture2D(texture.width, texture.height);


            // 将像素从 RenderTexture 复制到新的 Texture
            myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            myTexture2D.Apply();


            // 重置活动的 RenderTexture
            RenderTexture.active = previous;


            // 释放临时的RenderTexture
            RenderTexture.ReleaseTemporary(tmp);


            // “myTexture2D”现在具有与“texture”相同的像素，它是重新
            return myTexture2D;
        }
        //end
    }


}

#endif