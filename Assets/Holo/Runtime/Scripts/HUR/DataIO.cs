using System;
using System.IO;
using System.Reflection;

namespace Holo.HUR
{
    /// <summary>
    /// ����IO�����ᵽ�˴������ں����������
    /// </summary>
    public class DataIO
    {
        /// <summary>
        /// ��ָ��·����ȡdata
        /// </summary>
        /// <param name="path">ָ���ļ�·��</param>
        /// <param name="key">��Կ</param>
        /// <returns></returns>
        public static byte[] Read(string path,string key)
        {
            //return File.ReadAllBytes(path);
            return ReadFromPath(path);
        }

        ///// <summary>
        ///// ��dataд��ָ��·��
        ///// </summary>
        ///// <param name="path">ָ���ļ�·��</param>
        ///// <param name="data">д������</param>
        ///// <param name="key">��Կ</param>
        //public static void Write(string path, byte[] data,string key)
        //{
        //    File.WriteAllBytes(path, data); 
        //}

        /// <summary>
        /// ���ļ��ж�ȡ����
        /// </summary>
        /// <param name="srcFilePath"></param>
        /// <returns></returns>
        public static byte[] ReadFromPath(string srcFilePath)
        {
            // ����
            return DataUtils.Instance.Decrypt(srcFilePath);
        }

        /// <summary>
        /// �����ļ�
        /// </summary>
        /// <param name="srcFilePath"></param>
        /// <param name="targetFilePath"></param>
        public static void Copy(string srcFilePath, string targetFilePath) {
            DataUtils.Instance.Encrypt(srcFilePath, targetFilePath);
        }

        /// <summary>
        /// д��汾��Ϣ
        /// </summary>
        /// <param name="targetFilePath">����ļ���·��</param>
        /// <param name="dataVersionInfo">����</param>
        public static void WriteVersionFile(string targetFilePath,string dataVersionInfo,bool overrideFile)
        {
            string filePath = targetFilePath + "/" + XR.Config.HoloConfig.versionFileName;
            //�ж��ļ��Ƿ����
            if (File.Exists(filePath))
            {
                if(overrideFile)
                {
                    File.Delete(filePath);
                    //�ڱ���·��д�뵱ǰ���ݰ汾��Ϣ
                    File.WriteAllText(filePath, "###Data Version###\n" + dataVersionInfo);
                }
                else
                {
                    File.AppendAllText(filePath, "\n" + dataVersionInfo);
                }
            }
            else
            {
                //�ڱ���·��д�뵱ǰ���ݰ汾��Ϣ
                File.WriteAllText(filePath, "###Data Version###\n" + dataVersionInfo);
            }
        }


        /// <summary>
        /// ��ȡ�½��İ汾��
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string ReadNewVersion(string folderPath)
        {
            string filePath = folderPath + "/" + XR.Config.HoloConfig.versionFileName;
            if (!File.Exists(filePath))
            {
                return "1";
            }

            //�汾����
            string versionContent = File.ReadAllText(filePath);

            string[] versionList = versionContent.Split('\n');
            float maxVersion = 0;
            foreach (string item in versionList)
            {
                string fileFullName = item.Trim();
                string content = Path.GetFileNameWithoutExtension(fileFullName); // ȥ���ո�ͻ��з�

                //��#����ͷ����������
                if (!content.StartsWith("#"))
                {
                    string[] parts = content.Split(new string[] { "_v" }, StringSplitOptions.None);

                    if (parts.Length == 2)
                    {
                        string version = parts[1];
                        float versionValue = float.Parse(version);
                        if (maxVersion < versionValue)
                        {
                            //��¼���ֵ
                            maxVersion = versionValue;
                        }
                    }
                }
            }

            return (maxVersion + 1).ToString();
        }

    }

    internal class DataUtils
    {
        private static readonly object lockObject = new object();
        private MethodInfo encrypt;
        private MethodInfo decrypt;
        private static DataUtils instance = null;

        private DataUtils()
        {
            Type type = Type.GetType("Holo.XR.Utils.EqAesUtils");
            if (type != null)
            {
                encrypt = type.GetMethod("Encrypt", BindingFlags.NonPublic | BindingFlags.Static);
                decrypt = type.GetMethod("Decrypt", BindingFlags.NonPublic | BindingFlags.Static);
            }
        }
        public static DataUtils Instance {
            get
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new DataUtils();
                    }
                    return instance;
                }
            }
        }

        public void Encrypt(string srcFilePath, string encryptedFilePath)
        {
            encrypt.Invoke(this, new object[] { srcFilePath, encryptedFilePath });
        }

        public byte[] Decrypt(string srcFilePath) { 
            return (byte[])decrypt.Invoke(this, new object[] {srcFilePath});
        }
    }

}