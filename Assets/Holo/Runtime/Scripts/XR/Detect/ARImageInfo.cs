using System.Collections.Generic;
using UnityEngine;

namespace Holo.XR.Detect
{
    /// <summary>
    /// ARͼ����Ϣ
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
        /// ���������Ϣ
        /// </summary>
        /// <param name="key">�ֶ���</param>
        /// <param name="value">������Ϣ</param>
        public void addExif(string key, string value)
        {
            if (m_Exif == null)
            {
                m_Exif = new Dictionary<string, string>();
            }
            if (m_Exif.ContainsKey(key))
            {
                //���ظ���ӣ�������ӣ���ֱ���޸�ԭֵ
                m_Exif[key] = value;
            }
            else
            {
                m_Exif.Add(key, value);
            }
        }

        /// <summary>
        /// �Ƴ�ָ���ֶε�������Ϣ
        /// </summary>
        /// <param name="key">�ֶ���</param>
        public void removeExif(string key) {  
            if (m_Exif != null) { m_Exif.Remove(key); }
        }

        /// <summary>
        /// ��ȡָ���ֶε�������Ϣ
        /// </summary>
        /// <param name="key">�ֶ���</param>
        /// <returns></returns>
        public string getExif(string key)
        {
            if (m_Exif == null || !m_Exif.ContainsKey(key)) 
                return null; 
            return m_Exif[key];
        }
    }


}