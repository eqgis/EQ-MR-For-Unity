
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

        //��ǰ����ʶ��
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
        /// ��ʾ���ڵ�����
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
        /// ���ظ��ڵ�����
        /// </summary>
        public void HiddenRootNode()
        {
            rootNode.SetActive(false);
        }

        /// <summary>
        /// ���
        /// </summary>
        void Detect()
        {
            tagDetection = AprilTag.StartDetector(tagGroupName, size);//���� ģʽ
                                                                      //tagDetection = AprilTag.StartRgbDetector("36h11", 0.024);//rgb ģʽ-

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

                //����������������߱�ʶ�ȵ���̬��Ϣ
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
                        idText.text = "��־ ID:" + id.ToString() + " ��ʶ�ȣ�" + currentConfidence.ToString("0.000");
                    }
                    isFound = true;
                }

            }

        }

        public void ResetCurrentConfidence()
        {
            //����Ϊ0��������¿�ʼʶ����̵���
            currentConfidence = 0;
        }


    }
}
