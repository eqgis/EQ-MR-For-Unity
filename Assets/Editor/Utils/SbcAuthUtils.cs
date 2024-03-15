using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Holo.XR.Editor.Utils
{
    /// <summary>
    /// ˼�س۵������Ȩ��Ϣ
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
        /// ����sbc������Ȩ��Ϣ
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