using System;
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