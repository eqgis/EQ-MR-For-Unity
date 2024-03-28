using Holo.XR.Android;
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
