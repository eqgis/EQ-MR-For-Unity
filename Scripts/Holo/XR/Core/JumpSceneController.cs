using Holo.XR.Android;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Holo.XR.Core
{
    /// <summary>
    /// 场景跳转控制器
    /// </summary>
    public class JumpSceneController : MonoBehaviour
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
        public void ToNewScene(string sceneName)
        {
            this.sceneName = sceneName;
            ToNewScene();
        }

        /// <summary>
        /// 跳转至新场景
        /// </summary>
        public void ToNewScene()
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

                SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            }catch(Exception e)
            {
                EqLog.e("JumpSceneController",e.ToString());
            }
        }

    }
}
