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

        [Header("TrackedImagePrefab"), Tooltip("识别到图像后加载的对象")]
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

        [Tooltip("在Start时自动载入图片,当前延时3秒后执行")]
        public bool autoLoadImage = true;

        [Tooltip("使用图片的尺寸，若为false，则图像追踪时获取的图像尺寸会设为1.0"), Header("Dynamic Image Database")]
        public bool useImageSize = true;

        [Tooltip("仅在图片识别到时才显示Prefab")]
        public bool onlyActiveWhenTracking = false;

        [SerializeField, Tooltip("添加图片数据")]
        ImageData[] m_Images;

        [Header("When Image Load Completed"),Tooltip("图像数据库更新完成时执行以下事件")]
        public UnityEvent loadComplete;

        //预设对象的dic
        Dictionary<string, GameObject> m_PrefabsDictionary = new Dictionary<string, GameObject>();

        //已载入的预制件
        Dictionary<string, GameObject> m_Instantiated = new Dictionary<string, GameObject>();

        private ARImageDataState m_State = ARImageDataState.NoImagesAdded;


        /// <summary>
        /// 获取图像数据
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
                //自动载入图片，延时执行
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
        /// 跟踪事件
        /// </summary>
        /// <param name="eventArgs"></param>
        void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
        {
            //added 第一次识别到，会进入added
            foreach (var trackedImage in eventArgs.added)
            {
                // Give the initial image a reasonable default scale
                //trackedImage.transform.localScale = new Vector3(0.01f, 1f, 0.01f);
                ARImageInfo aRImageInfo = UpdateInfo(trackedImage);

                //创建预制件
                CreatePrefab(aRImageInfo);
                Callback(trackedImage, aRImageInfo,0);
            }

            //updated 后续识别到，会一直处于updateed
            foreach (var trackedImage in eventArgs.updated)
            {
                //更新信息，包含位姿等属性信息
                Callback(trackedImage, UpdateInfo(trackedImage),1);
            }

            ////removed
            //foreach (var trackedImage in eventArgs.removed)
            //{
            //    //移除预制件,暂时不移除
            //    //RemovePrefab(trackedImage.referenceImage.name);
            //    UpdateInfo(trackedImage,2);
            //}

        }

        /// <summary>
        /// 移除预制件
        /// <param name="name">图片名称</param>
        /// </summary>
        public void RemovePrefab(string name)
        {
            if (m_Instantiated.TryGetValue(name, out var gameObject))
            {
                //销毁对象
                Destroy(gameObject);
                m_Instantiated.Remove(name);
            }
        }

        /// <summary>
        /// 创建预制件
        /// </summary>
        /// <param name="trackedImage"></param>
        void CreatePrefab(ARImageInfo trackedImage)
        {
            if (trackedImage == null)
            {
                return;
            }
            //若prefabDic中有定义，则需要载入到场景中
            if (m_PrefabsDictionary.TryGetValue(trackedImage.name, out var prefab))
            {
                //若还没有载入场景，则需要进行实例化操作（绑定父节点trackedImage.transform），并存入
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
                //监听回调
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
        /// 更新ARImage的信息，并触发callback
        /// </summary>
        /// <param name="trackedImage"></param>
        ARImageInfo UpdateInfo(ARTrackedImage trackedImage)
        {
            if (onlyActiveWhenTracking)
            {
                //处于跟踪状态
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

            return imageInfo;
        }



        /// <summary>
        /// 加载图片数据库
        /// <!--仅在done的情况下执行-->
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
                                        //向图片数据库添加图像Texture
                                        image.jobState = mutableLibrary.ScheduleAddImageWithValidationJob(image.texture, image.name, image.width);

                                        //向prefabDic中添加image对应的prefab
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
                                //图片数据加载完成
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