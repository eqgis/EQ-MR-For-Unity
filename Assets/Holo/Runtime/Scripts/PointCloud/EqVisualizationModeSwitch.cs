#if ENGINE_ARCORE
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

namespace Holo.PointCloud
{
    /// <summary>
    /// 点云可视化渲染开关
    /// </summary>

    [RequireComponent(typeof(ARPointCloudManager))]
    public class EqVisualizationModeSwitch : MonoBehaviour
    {
        [SerializeField]
        Button m_ToggleButton;

        public Button toggleButton
        {
            get => m_ToggleButton;
            set => m_ToggleButton = value;
        }

        [SerializeField]
        Text m_Log;

        public Text log
        {
            get => m_Log;
            set => m_Log = value;
        }

        [Header("VisualizationMode")]
        [SerializeField]
        VisualizationMode m_Mode = VisualizationMode.All;

        public VisualizationMode mode
        {
            get => m_Mode;
            set => SetMode(value);
        }

        void OnEnable()
        {
            SetMode(m_Mode);
            GetComponent<ARPointCloudManager>().pointCloudsChanged += OnPointCloudsChanged;
        }

        StringBuilder m_StringBuilder = new StringBuilder();

        void OnPointCloudsChanged(ARPointCloudChangedEventArgs eventArgs)
        {
            m_StringBuilder.Clear();
            foreach (var pointCloud in eventArgs.updated)
            {
                //m_StringBuilder.Append($"\n{pointCloud.trackableId}: ");
                if (m_Mode == VisualizationMode.CurrentFrame)
                {
                    var visualizer = pointCloud.GetComponent<BasePointCloudVisualizer>();
                    if (visualizer && (visualizer.mode != m_Mode))
                    {
                        SetMode(m_Mode);
                    }

                    if (pointCloud.positions.HasValue)
                    {
                        m_StringBuilder.Append($"{pointCloud.positions.Value.Length}");
                    }
                    else
                    {
                        m_StringBuilder.Append("0");
                    }

                    m_StringBuilder.Append(" points in current frame.");
                }
                else
                {
                    var visualizer = pointCloud.GetComponent<BasePointCloudVisualizer>();
                    if (visualizer)
                    {
                        m_StringBuilder.Append($"{visualizer.totalPointCount} total points");
                    }
                }
            }
            if (log)
            {
                log.text = m_StringBuilder.ToString();
            }
        }

        void SetMode(VisualizationMode mode)
        {
            m_Mode = mode;
            var manager = GetComponent<ARPointCloudManager>();
            foreach (var pointCloud in manager.trackables)
            {
                var visualizer = pointCloud.GetComponent<BasePointCloudVisualizer>();
                if (visualizer)
                {
                    visualizer.mode = mode;
                }
            }

        }
    }
}

#endif