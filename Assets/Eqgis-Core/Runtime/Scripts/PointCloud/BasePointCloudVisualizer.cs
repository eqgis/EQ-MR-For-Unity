using System.Collections.Generic;
using UnityEngine;

namespace Holo.PointCloud
{
    /// <summary>
    /// 点云可视化工具抽象类
    /// </summary>
    public abstract class BasePointCloudVisualizer : MonoBehaviour
    {
        protected const int MAX_COUNT = 1000000;

        [SerializeField]
        [Tooltip("Whether to draw all the feature points or only the ones from the current frame.")]
        public VisualizationMode m_Mode;

        protected Dictionary<ulong, Vector3> m_Points = new Dictionary<ulong, Vector3>(MAX_COUNT);


        public int totalPointCount => m_Points.Count;

        public VisualizationMode mode
        {
            get => m_Mode;
            set
            {
                m_Mode = value;
                RenderPoints();
            }
        }

        public abstract void RenderPoints();

        public abstract void ClearPoints();
    }

}