using Holo.XR.Android;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Holo.XR.Core
{
    /// <summary>
    /// �����л���
    /// </summary>
    public class ExSceneTransition : MonoBehaviour
    {
        public GameObject[] dontDestoryGroup;

        private GameObject uiRaycastCamera;//ע�������Ƕ�̬��ֵ�����

        [Header("Target Scene")]
        public string sceneName;

        [Header("NextSceneNode Ref")]
        [Tooltip("�ö������ΪSceneNode��ֱ���Ӷ���")]
        public Transform nextSceneNodeTransform;


        [Tooltip("�Զ���ת")]
        public bool auto = false;

        [Header("Fade")]
        public bool fade = true;
        [Tooltip("�����л����뵭���ٶ�")]
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
        /// ��ת���³���
        /// </summary>
        /// <param name="sceneName">Ŀ�곡������</param>
        public void LoadScene(string sceneName)
        {
            this.sceneName = sceneName;
            LoadScene();
        }

        /// <summary>
        /// ��ת���³���
        /// </summary>
        public void LoadScene()
        {
            try
            {
                //���ȵ��ã���ֹTagʶ����߳���ʶ���ȡλ����Ϣ��currentSceneNodeTransform��pose���޸�
                if (nextSceneNodeTransform != null)
                {
                    NodePoseRecorder nodePoseRecorder = NodePoseRecorder.GetInstance();

                    //��¼��һ�������ڵ㣬����ڳ�ʼ�����ڵ�����λ�á����ڿ����������л������������������Ҫʵʱ���ۼӡ�
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
        /// ʹ�õ��뵭��Ч���ĳ����л�
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
