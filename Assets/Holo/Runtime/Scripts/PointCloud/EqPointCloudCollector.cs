#if ENGINE_ARCORE
using Holo.XR.Android;
using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace Holo.PointCloud
{
    /// <summary>
    /// ���Ʋɼ���
    /// </summary>
    public sealed class EqPointCloudCollector : MonoBehaviour, PointCloudCollect
    {
        [Header("Point Cloud Manager")]
        public ARPointCloudManager m_PointCloudManager;

        public void ClearPointCloud()
        {
            GameObject pointCloudPrefab = m_PointCloudManager.pointCloudPrefab;
            if (pointCloudPrefab != null)
            {
                //��ȡ������Ⱦ������,ִ���������
                FindFirstObjectByType<ARPointCloud>().GetComponent<BasePointCloudVisualizer>().ClearPoints();
            }

        }


        public void SavePointCloud()
        {
#if DEBUG_LOG
            AndroidUtils.Toast("saving...");
#endif
            // ��ȡ��ǰʱ��
            DateTime currentTime = DateTime.Now;

            // ��ʽ��ʱ���ַ�������Ϊ�ļ�����һ����
            string fileName = "Data_" + currentTime.ToString("yyyy_MM_dd_HH_mm_ss") + ".pts";

            string persistentDataPath = Application.persistentDataPath;
            string filePath = Path.Combine(persistentDataPath, "PointCloud", fileName);
            // ���������������ݵ��ļ�
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            bool hasData = false;

            StringBuilder strBuilder = new StringBuilder();
            
            // ִ�в�ͬ�Ĳ���
            GameObject pointCloudPrefab = m_PointCloudManager.pointCloudPrefab;
            if (pointCloudPrefab != null)
            {
                ParticleSystem aRPointCloudParticleVisualizer = pointCloudPrefab.GetComponent<ParticleSystem>();
                if (aRPointCloudParticleVisualizer != null)
                {
#if DEBUG_LOG
                    Debug.Log("Using ParticleSystem");
#endif
                    //���ҳ����д�ARPointCloud����Ϸ����Ȼ���ȡ����ParticleSystem
                    Extract(strBuilder, 
                        FindFirstObjectByType<ARPointCloud>().gameObject.GetComponent<ParticleSystem>());
                    hasData = true;
                }
                else
                {
                    MeshFilter aRPointCloudMeshVisualizer = pointCloudPrefab.GetComponent<MeshFilter>();
                    if (aRPointCloudMeshVisualizer != null)
                    {
#if DEBUG_LOG
                        Debug.Log("Using MeshFilter");
#endif
                        //���ҳ����д�ARPointCloud����Ϸ����Ȼ���ȡ����MeshFilter
                        Extract(strBuilder, 
                            FindFirstObjectByType<ARPointCloud>().gameObject.GetComponent<MeshFilter>());
                        hasData = true;
                    }
                }
            }

            if (hasData)
            {
                // ���������������ݵ��ļ�
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                using (StreamWriter wr = new StreamWriter(filePath, false))
                {
                    wr.Write(strBuilder.ToString());
                    wr.Flush();
                }
#if DEBUG_LOG
                AndroidUtils.Toast("success!");
#endif
            }
            else
            {
#if DEBUG_LOG
                AndroidUtils.Toast("Error Visualizer!");
#endif
            }
        }


        #region �ڲ�����
        private void Extract(StringBuilder strBuilder, MeshFilter meshFilter)
        {
            Mesh mesh = meshFilter.sharedMesh;

            //// ��ȡ��������
            Vector3[] vertices = mesh.vertices;
            ////int[] triangles = mesh.triangles;//�����������Ƕ���������
            Color[] colors = mesh.colors;
            if (colors.Length == 0)
            {
                colors = new Color[vertices.Length];
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = new Color(1, 1, 1, 1);
                }
            }

            int minCount = Math.Min(vertices.Length, colors.Length);
#if DEBUG_LOG
            AndroidUtils.Toast("count: " + minCount);
#endif
            // д�붥������
            for (int i = 0; i < minCount; i++)
            {
                //PTS��ʽǰ������ ��x��y��z�� ���� ���У����ĸ��ǡ�ǿ�ȡ�ֵ����������ǡ���ɫֵ����R��G��B�� 
                Vector3 vertex = vertices[i];
                Color c = colors[i];
                //��������ALPHA��ʾǿ��ֵ
                strBuilder.AppendLine(vertex.x + " " + vertex.y + " " + vertex.z
                    + " " + (int)(c.a * 255) + " " + (int)(c.r * 255) + " " + (int)(c.g * 255) + " " + (int)(c.b * 255));
            }
        }

        /// <summary>
        /// ��ȡ��������
        /// </summary>
        /// <param name="strBuilder"></param>
        /// <param name="visualizer"></param>
        private void Extract(StringBuilder strBuilder, ParticleSystem particleSystem)
        {
            // ��ȡ ParticleSystem �е���������
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[particleSystem.main.maxParticles];
            int particleCount = particleSystem.GetParticles(particles);

#if DEBUG_LOG
            AndroidUtils.Toast("count: " + particleCount);
#endif
            // �����������鲢��ӡÿ�����ӵ�λ��
            for (int i = 0; i < particleCount; i++)
            {
                Vector3 vertex = particles[i].position;
                Color32 startColor = particles[i].startColor;
                //PTS��ʽǰ������ ��x��y��z�� ���� ���У����ĸ��ǡ�ǿ�ȡ�ֵ����������ǡ���ɫֵ����R��G��B�� 
                strBuilder.AppendLine(vertex.x + " " + vertex.y + " " + vertex.z
                    + " " + startColor.a + " " + startColor.r + " " + startColor.g + " " + startColor.b);
            }
        }
        #endregion

    }
}

#endif