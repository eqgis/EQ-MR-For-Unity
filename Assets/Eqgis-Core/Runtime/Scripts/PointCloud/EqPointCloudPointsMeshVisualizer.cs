#if ENGINE_ARCORE

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Holo.PointCloud
{
    /// <summary>
    /// 点云可视化工具
    /// </summary>
    public sealed class EqPointCloudPointsMeshVisualizer : BasePointCloudVisualizer
    {

        private MeshFilter m_MeshFilter;

        private ARPointCloud m_PointCloud;

        private Mesh mesh;

        private List<Vector3> vertices;
        private List<int> indices;

        private void Start()
        {
            vertices = new List<Vector3>(MAX_COUNT);
            indices = new List<int>(MAX_COUNT);
        }

        void OnPointCloudChanged(ARPointCloudUpdatedEventArgs eventArgs)
        {
            RenderPoints();
        }

        public override void RenderPoints()
        {
            if (!m_PointCloud.positions.HasValue)
                return;


            if (mode == VisualizationMode.CurrentFrame)
            {
                m_Points.Clear();
                vertices.Clear();
                indices.Clear();
            }
            //处理mesh
            mesh.Clear();

            var positions = m_PointCloud.positions.Value;
            if (m_PointCloud.identifiers.HasValue)
            {
                //Vector3 camPostion = _cameraManager.transform.position;
                var identifiers = m_PointCloud.identifiers.Value;
                for (int i = 0; i < positions.Length; ++i)
                {
                    if (m_Points.ContainsKey(identifiers[i]))
                    {
                        continue;
                    }

                    m_Points[identifiers[i]] = positions[i];
                    vertices.Add(positions[i]);
                    indices.Add(vertices.Count - 1);
                }

                mesh.SetVertices(vertices);
                mesh.SetIndices(indices, MeshTopology.Points, 0);
            }

            m_MeshFilter.sharedMesh = mesh;

        }


        void Awake()
        {
            mesh = new Mesh();
            m_PointCloud = GetComponent<ARPointCloud>();
            m_MeshFilter = GetComponent<MeshFilter>();
        }

        void OnEnable()
        {
            m_PointCloud.updated += OnPointCloudChanged;
            UpdateVisibility();
        }

        void OnDisable()
        {
            m_PointCloud.updated -= OnPointCloudChanged;
            UpdateVisibility();
        }

        void Update()
        {
            UpdateVisibility();
        }

        void UpdateVisibility()
        {
            var visible =
                 enabled &&
                 (m_PointCloud.trackingState != TrackingState.None);

            SetVisible(visible);
        }

        void SetVisible(bool visible)
        {
            var meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null)
                meshRenderer.enabled = visible;
        }

        /// <summary>
        /// 清除点云
        /// </summary>
        public override void ClearPoints()
        {
            m_Points.Clear();
            vertices.Clear();
            indices.Clear();
            mesh.Clear();
            m_MeshFilter.sharedMesh = mesh;
        }

    }
}

#endif