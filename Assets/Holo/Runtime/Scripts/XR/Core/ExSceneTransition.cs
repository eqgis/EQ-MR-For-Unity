using Holo.XR.Android;
using Holo.XR.Detect;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Holo.XR.Core
{
    /// <summary>
    /// 场景切换器
    /// </summary>
    public class ExSceneTransition : MonoBehaviour
    {
        public GameObject[] dontDestoryGroup;

        private GameObject uiRaycastCamera;//注意这里是动态赋值的组件

        [Header("Target Scene")]
        public string sceneName;

        [Header("NextSceneNode Ref")]
        [Tooltip("该对象必须为SceneNode的直接子对象")]
        public Transform nextSceneNodeTransform;


        [Tooltip("自动跳转")]
        public bool auto = false;

        [Header("Fade")]
        public bool fade = true;
        [Tooltip("场景切换淡入淡出速度")]
        public float fadeSpeed = 1.0f;
        private float alpha = 1f;
        private int fadeDir = -1;

        [Header("Paras"),Tooltip("Start时，是否重置位姿(建议在识别图片进行定位的流程中启用)")]
        public bool resetPoseInStart = false;

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            BeginFade(-1);
        }

        private void BeginFade(int direction)
        {
            fadeDir = direction;
        }

        // Start is called before the first frame update
        void Start()
        {
            for (int i = 0; i < dontDestoryGroup.Length; i++)
            {
                DontDestroyOnLoad(dontDestoryGroup[i]);
            }

            uiRaycastCamera = GameObject.Find("UIRaycastCamera");
            if (uiRaycastCamera != null)
            {
                DontDestroyOnLoad(uiRaycastCamera);
            }

            if (resetPoseInStart)
            {
                //重置Node的位姿
                NodePoseRecorder nodePoseRecorder = NodePoseRecorder.GetInstance();

                //记录下一个场景节点，相对于初始场景节点的相对位置。这里在Start时重置，可以避免多个场景切换后，位姿累加
                nodePoseRecorder.NextSceneNodePosition = Vector3.zero;
                nodePoseRecorder.NextSceneNodeRotation = Vector3.zero;

            }

            if (auto)
            {
                Invoke("ToNewScene", 0F);
            }
        }

        /// <summary>
        /// 跳转至新场景
        /// </summary>
        /// <param name="sceneName">目标场景名称</param>
        public void LoadScene(string sceneName)
        {
            this.sceneName = sceneName;
            LoadScene();
        }

        /// <summary>
        /// 跳转至新场景
        /// </summary>
        /// <param name="sceneName">目标场景名称</param>
        /// <param name="imageInfo">AR图像信息</param>
        public void LoadScene(string sceneName, ARImageInfo imageInfo)
        {
            //更新场景切换控制器的transform，这里采用图片的transform。
            //那么在场景切换后，则下一场景的原点则以image为原点。
            //（需要与NodePoseRecorder结合使用）注意：在流程中，若是新场景重新创建了ARSession,则这里需要减去之前的相机位姿
            //已知：在当前流程中，并不会新建ARSession
            //需要特别注意的是：用于识别的图片的width一定要与ARCoreImageDetector设置的尺寸一样
            nextSceneNodeTransform.position = imageInfo.transform.position;

            nextSceneNodeTransform.eulerAngles = imageInfo.transform.eulerAngles;

            this.sceneName = sceneName;
            LoadScene();
        }

        /// <summary>
        /// 跳转至新场景
        /// </summary>
        public void LoadScene()
        {
            try
            {
                //最先调用，防止Tag识别或者场景识别读取位置信息后，currentSceneNodeTransform的pose被修改
                if (nextSceneNodeTransform != null)
                {
                    NodePoseRecorder nodePoseRecorder = NodePoseRecorder.GetInstance();

                    //记录下一个场景节点，相对于初始场景节点的相对位置。由于可能有连续切换多个场景的情况，因此要实时地累加。
                    nodePoseRecorder.NextSceneNodePosition = nodePoseRecorder.NextSceneNodePosition + nextSceneNodeTransform.localPosition;
                    nodePoseRecorder.NextSceneNodeRotation = nodePoseRecorder.NextSceneNodeRotation + nextSceneNodeTransform.localEulerAngles;

                    //if (AndroidUtils.debug)
                    //{
                    //    EqLog.d("JumpSceneController", "nextSceneNodeTransform.localPosition:" + nextSceneNodeTransform.localPosition.ToString() + "\n" +
                    //        "nextSceneNodeTransform.position:" + nextSceneNodeTransform.position);
                    //}
                }

                if (fade)
                {
                    InnerLoadScene(sceneName);
                }
                else
                {
                    SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
                }
            }catch(Exception e)
            {
                EqLog.e("JumpSceneController",e.ToString());
            }
        }

        /// <summary>
        /// 使用淡入淡出效果的场景切换
        /// </summary>
        /// <param name="sceneName"></param>
        private void InnerLoadScene(string sceneName)
        {
            BeginFade(1);
            StartCoroutine(LoadSceneAsync(sceneName));
        }

        IEnumerator LoadSceneAsync(string sceneName)
        {
            yield return new WaitForSeconds(((1.0f - alpha) / fadeSpeed) * Time.deltaTime * 100);
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

            while (!asyncOperation.isDone)
            {
                yield return null;
            }
        }

        private void OnGUI()
        {
            alpha += fadeDir * fadeSpeed * Time.deltaTime;
            alpha = Mathf.Clamp01(alpha);

            GUI.color = new Color(0, 0, 0, alpha);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height),
                Texture2D.whiteTexture, ScaleMode.StretchToFill);
        }
    }
}
