
using Holo.XR.Android;
using Holo.XR.Detect;
using Holo.XR.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Holo.SceneScanner
{
    /// <summary>
    /// CSlam场景控制器
    /// </summary>
    public class CslamSceneController : MonoBehaviour
    {
        public XvCslamMapScanner mapScanner;
        public XvCslamMapLoader mapLoader;
        public TagDetect tagDetect;

        [Header("输出文件名称的控件")]
        public InputField outputFileName;

        public void ExecSaveMap()
        {
            string name = outputFileName.text.Trim();
            if (name.Equals("") || "" == name)
            {
                AndroidUtils.GetInstance().ShowToast("文件名不能为空");
                return;
            }

            //更新名称，文件夹采用默认
            mapScanner.mapName = name;
            mapScanner.SaveMap();
        }

        public void ExecLoadMap()
        {
            string name = outputFileName.text.Trim();
            if (name.Equals("") || "" == name)
            {
                AndroidUtils.GetInstance().ShowToast("文件名不能为空");
                return;
            }

            //更新名称，文件夹采用默认
            mapLoader.mapName = name;
            mapLoader.LoadMap();
        }

        /// <summary>
        /// 重置TAG识别的阈值
        /// </summary>
        public void ExecResetTagScan()
        {
            tagDetect.ResetCurrentConfidence();
        }


        /// <summary>
        /// 设置Tag识别启用状态
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
        /// 设置地图扫描器启用状态
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
