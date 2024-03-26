using System.Collections.Generic;
using UnityEngine;

namespace Holo.XR.Detect
{
    /// <summary>
    /// ARÕºœÒ–≈œ¢
    /// </summary>
    public class ARImageInfo
    {
        private string m_Name;
        private Vector2 m_Extents;
        private Vector2 m_Size;
        
        private Transform m_Transform;

        public string name { get => m_Name; set => m_Name = value; }
        public Vector2 extents { get => m_Extents; set => m_Extents = value; }
        public Vector2 size { get => m_Size; set => m_Size = value; }
        public Transform transform { get => m_Transform; set => m_Transform = value; }
    }


}