
using Holo.XR.Android;
using Holo.XR.Detect;
using Holo.XR.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Holo.SceneScanner
{
    /// <summary>
    /// CSlam����������
    /// </summary>
    public class CslamSceneController : MonoBehaviour
    {
        public XvCslamMapScanner mapScanner;
        public XvCslamMapLoader mapLoader;
        public TagDetect tagDetect;

        [Header("����ļ����ƵĿؼ�")]
        public InputField outputFileName;

        public void ExecSaveMap()
        {
            string name = outputFileName.text.Trim();
            if (name.Equals("") || "" == name)
            {
                AndroidUtils.GetInstance().ShowToast("�ļ�������Ϊ��");
                return;
            }

            //�������ƣ��ļ��в���Ĭ��
            mapScanner.mapName = name;
            mapScanner.SaveMap();
        }

        public void ExecLoadMap()
        {
            string name = outputFileName.text.Trim();
            if (name.Equals("") || "" == name)
            {
                AndroidUtils.GetInstance().ShowToast("�ļ�������Ϊ��");
                return;
            }

            //�������ƣ��ļ��в���Ĭ��
            mapLoader.mapName = name;
            mapLoader.LoadMap();
        }

        /// <summary>
        /// ����TAGʶ�����ֵ
        /// </summary>
        public void ExecResetTagScan()
        {
            tagDetect.ResetCurrentConfidence();
        }


        /// <summary>
        /// ����Tagʶ������״̬
        /// </summary>
        /// <param name="enable"></param>
        public void SetTagDetectEnable(bool enable)
        {
            if (enable)
            {
                tagDetect.StartDetect();
            }
            else
            {
                tagDetect.StopDetect();
            }
        }

        /// <summary>
        /// ���õ�ͼɨ��������״̬
        /// </summary>
        /// <param name="enable"></param>
        public void SetMapScannerEnable(bool enable)
        {
            if (enable)
            {
                mapScanner.StartMap();
            }
            else
            {
                mapScanner.StopMap();
            }
        }
    }
}
