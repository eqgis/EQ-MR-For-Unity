
using UnityEngine;
using UnityEngine.UI;
using XvSdk;

namespace Holo.XR.Detect
{

    public class TagDetect : MonoBehaviour
    {
        public string tagGroupName = "36h11";
        public double size = 0.16;

        public static Vector3 rootPosition;
        public static Quaternion rootRrotation;
        public static bool isFound = false;
        public GameObject rootNode;

        private TagDetection[] tagDetection;
        private bool ifCheck = false;

        public Text idText;

        //当前最大辨识度
        private float currentConfidence = 0;


        [Header("Other Settings")]
        public bool horizontal = true;

        // Start is called before the first frame update
        void Start()
        {
            //if (Application.isEditor)
            //{
            //    ifCheck = false;
            //}
            //else
            //{
            //    ifCheck = true;
            //}
        }

        // Update is called once per frame
        void Update()
        {
            if (ifCheck)
            {
                Detect();
            }
        }

        public void StartDetect()
        {
            ifCheck = true;
        }

        public void StopDetect()
        {
            ifCheck = false;

            //AprilTag.StopDetector();
        }

        /// <summary>
        /// 显示根节点内容
        /// </summary>
        public void ShowRootNode()
        {
            if (!isFound)
            {
                return;
            }
            rootNode.SetActive(true);
        }

        /// <summary>
        /// 隐藏根节点内容
        /// </summary>
        public void HiddenRootNode()
        {
            rootNode.SetActive(false);
        }

        /// <summary>
        /// 检测
        /// </summary>
        void Detect()
        {
            tagDetection = AprilTag.StartDetector(tagGroupName, size);//鱼眼 模式
                                                                      //tagDetection = AprilTag.StartRgbDetector("36h11", 0.024);//rgb 模式-

            if (tagDetection == null || tagDetection.Length == 0)
            {
                isFound = false;
                return;
            }
            else
            {
                for (int i = 0; i < tagDetection.Length; i++)
                {
                    TagDetection tag = tagDetection[i];
                    Debug.Log("AprilTagDemo##StartDetect translation:" + string.Format($"{i}=id:{tag.id},translation:{tag.translation.ToString()}"));
                    Debug.Log("AprilTagDemo##StartDetect rotation:" + string.Format($"{i}=id:{tag.id},{tag.rotation.ToString()}"));
                    Debug.Log("AprilTagDemo##StartDetect quaternion:" + string.Format($"{i}=id:{tag.id},{tag.quaternion.ToString()}"));
                }

                //持续迭代，采用最高辨识度的姿态信息
                float confidence = tagDetection[0].confidence;
                if (confidence > currentConfidence)
                {
                    currentConfidence = confidence;
                    rootPosition = tagDetection[0].translation;
                    rootRrotation = new Quaternion(tagDetection[0].quaternion[0], tagDetection[0].quaternion[1], tagDetection[0].quaternion[2], tagDetection[0].quaternion[3]);
                    ShowRootNode();
                    rootNode.transform.position = rootPosition;

                    if (horizontal)
                    {
                        Quaternion q = rootRrotation * Quaternion.Euler(-90, 0, 0);
                        q.x = 0; q.z = 0;
                        rootNode.transform.rotation = q.normalized;
                    }
                    else
                    {
                        rootNode.transform.rotation = rootRrotation;
                    }

                    if (idText != null)
                    {
                        int id = tagDetection[0].id;
                        idText.text = "标志 ID:" + id.ToString() + " 辨识度：" + currentConfidence.ToString("0.000");
                    }
                    isFound = true;
                }

            }

        }

        public void ResetCurrentConfidence()
        {
            //重置为0后，则会重新开始识别过程迭代
            currentConfidence = 0;
        }


    }
}
