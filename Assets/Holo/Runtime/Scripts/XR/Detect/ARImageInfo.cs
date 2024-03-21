using System.Collections.Generic;
using UnityEngine;

namespace Holo.XR.Detect
{
    /// <summary>
    /// AR图像信息
    /// </summary>
    public class ARImageInfo
    {
        private string m_Name;
        private Vector2 m_Extents;
        private Vector2 m_Size;
        
        private Transform m_Transform;

        private Dictionary<string, string> m_Exif;

        public string name { get => m_Name; set => m_Name = value; }
        public Vector2 extents { get => m_Extents; set => m_Extents = value; }
        public Vector2 size { get => m_Size; set => m_Size = value; }
        public Transform transform { get => m_Transform; set => m_Transform = value; }


        /// <summary>
        /// 添加属性信息
        /// </summary>
        /// <param name="key">字段名</param>
        /// <param name="value">属性信息</param>
        public void addExif(string key, string value)
        {
            if (m_Exif == null)
            {
                m_Exif = new Dictionary<string, string>();
            }
            if (m_Exif.ContainsKey(key))
            {
                //不重复添加，二次添加，则直接修改原值
                m_Exif[key] = value;
            }
            else
            {
                m_Exif.Add(key, value);
            }
        }

        /// <summary>
        /// 移除指定字段的属性信息
        /// </summary>
        /// <param name="key">字段名</param>
        public void removeExif(string key) {  
            if (m_Exif != null) { m_Exif.Remove(key); }
        }

        /// <summary>
        /// 获取指定字段的属性信息
        /// </summary>
        /// <param name="key">字段名</param>
        /// <returns></returns>
        public string getExif(string key)
        {
            if (m_Exif == null || !m_Exif.ContainsKey(key)) 
                return null; 
            return m_Exif[key];
        }
    }


}