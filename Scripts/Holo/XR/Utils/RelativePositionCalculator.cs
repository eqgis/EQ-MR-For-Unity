
using UnityEngine;
using UnityEngine.UI;

namespace Holo.XR.Utils
{
    public class RelativePositionCalculator : MonoBehaviour
    {
        [Header("Core"), Tooltip("���ڷ�ʵ�ʸ��ӹ�ϵ�Ľڵ㣬ģ�����������ڸ���λ�ù�ϵ")]
        public Transform parent;
        public Transform child;

        private Vector3 relativePosition = Vector3.zero;

        public Text outPutText;

        /// <summary>
        /// ��ȡ�ӽڵ�����λ��
        /// </summary>
        /// <returns></returns>
        public Vector3 GetReativePosition()
        {
            return this.relativePosition;
        }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            Vector3 childPostion = child.transform.localPosition;
            Vector3 parentPosition = parent.transform.localPosition;

            relativePosition.x = childPostion.x - parentPosition.x;
            relativePosition.y = childPostion.y - parentPosition.y;
            relativePosition.z = childPostion.z - parentPosition.z;

            if (outPutText != null)
            {
                outPutText.text = "Current Position:" + relativePosition.ToString();
            }
        }
    }
}