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
    /// 点云采集器
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
                //获取点云渲染抽象类,执行清除方法
                FindFirstObjectByType<ARPointCloud>().GetComponent<BasePointCloudVisualizer>().ClearPoints();
            }

        }


        public void SavePointCloud()
        {
#if DEBUG_LOG
            AndroidUtils.Toast("saving...");
#endif
            // 获取当前时间
            DateTime currentTime = DateTime.Now;

            // 格式化时间字符串，作为文件名的一部分
            string fileName = "Data_" + currentTime.ToString("yyyy_MM_dd_HH_mm_ss") + ".pts";

            string persistentDataPath = Application.persistentDataPath;
            string filePath = Path.Combine(persistentDataPath, "PointCloud", fileName);
            // 创建保存网格数据的文件
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            bool hasData = false;

            StringBuilder strBuilder = new StringBuilder();
            
            // 执行不同的操作
            GameObject pointCloudPrefab = m_PointCloudManager.pointCloudPrefab;
            if (pointCloudPrefab != null)
            {
                ParticleSystem aRPointCloudParticleVisualizer = pointCloudPrefab.GetComponent<ParticleSystem>();
                if (aRPointCloudParticleVisualizer != null)
                {
#if DEBUG_LOG
                    Debug.Log("Using ParticleSystem");
#endif
                    //查找场景中带ARPointCloud的游戏对象，然后获取它的ParticleSystem
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
                        //查找场景中带ARPointCloud的游戏对象，然后获取它的MeshFilter
                        Extract(strBuilder, 
                            FindFirstObjectByType<ARPointCloud>().gameObject.GetComponent<MeshFilter>());
                        hasData = true;
                    }
                }
            }

            if (hasData)
            {
                // 创建保存网格数据的文件
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


        #region 内部方法
        private void Extract(StringBuilder strBuilder, MeshFilter meshFilter)
        {
            Mesh mesh = meshFilter.sharedMesh;

            //// 获取网格数据
            Vector3[] vertices = mesh.vertices;
            ////int[] triangles = mesh.triangles;//点云无需三角顶点索引，
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
            // 写入顶点数据
            for (int i = 0; i < minCount; i++)
            {
                //PTS格式前三个是 （x，y，z） 坐标 其中，第四个是“强度”值，最后三个是“颜色值”（R，G，B） 
                Vector3 vertex = vertices[i];
                Color c = colors[i];
                //这里暂用ALPHA表示强度值
                strBuilder.AppendLine(vertex.x + " " + vertex.y + " " + vertex.z
                    + " " + (int)(c.a * 255) + " " + (int)(c.r * 255) + " " + (int)(c.g * 255) + " " + (int)(c.b * 255));
            }
        }

        /// <summary>
        /// 提取点云数据
        /// </summary>
        /// <param name="strBuilder"></param>
        /// <param name="visualizer"></param>
        private void Extract(StringBuilder strBuilder, ParticleSystem particleSystem)
        {
            // 获取 ParticleSystem 中的所有粒子
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[particleSystem.main.maxParticles];
            int particleCount = particleSystem.GetParticles(particles);

#if DEBUG_LOG
            AndroidUtils.Toast("count: " + particleCount);
#endif
            // 遍历粒子数组并打印每个粒子的位置
            for (int i = 0; i < particleCount; i++)
            {
                Vector3 vertex = particles[i].position;
                Color32 startColor = particles[i].startColor;
                //PTS格式前三个是 （x，y，z） 坐标 其中，第四个是“强度”值，最后三个是“颜色值”（R，G，B） 
                strBuilder.AppendLine(vertex.x + " " + vertex.y + " " + vertex.z
                    + " " + startColor.a + " " + startColor.r + " " + startColor.g + " " + startColor.b);
            }
        }
        #endregion

    }
}

#endif