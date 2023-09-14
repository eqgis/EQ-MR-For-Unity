using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Holo.XR.Editor.Utils
{
    /// <summary>
    /// 思必驰单项技术授权信息
    /// </summary>
    class SbcAuth
    {
        public string apiKey;
        public string productID;
        public string productKey;
        public string productSecret;
    }

    class SbcAuthUtils
    {
        private static SbcAuth sbcAuth = new SbcAuth();

        /// <summary>
        /// 保存sbc语音授权信息
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public static void SaveSbcAuth()
        {
            string authJson = EditorJsonUtility.ToJson(sbcAuth);
            string filePath = Application.dataPath + "/../Sbc/auth.json";
            if (!File.Exists(filePath))
            {
                Directory.CreateDirectory(Application.dataPath + "/../Sbc/");
            }
            File.WriteAllText(filePath, authJson);
        }

        public static SbcAuth Read()
        {
            string filePath = Application.dataPath + "/../Sbc/auth.json";
            if (File.Exists(filePath))
            {
                try
                {
                    EditorJsonUtility.FromJsonOverwrite(File.ReadAllText(filePath), sbcAuth);
                }catch(Exception e)
                {
                    Debug.LogException(e);
                }
            }
            return sbcAuth;
        }
    }
}