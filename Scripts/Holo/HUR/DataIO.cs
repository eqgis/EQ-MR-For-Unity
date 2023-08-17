using System;
using System.Reflection;

namespace Holo.HUR
{
    /// <summary>
    /// 数据IO，先提到此处，便于后续引入加密
    /// </summary>
    public class DataIO
    {
        /// <summary>
        /// 从指定路径读取data
        /// </summary>
        /// <param name="path">指定文件路径</param>
        /// <param name="key">密钥</param>
        /// <returns></returns>
        public static byte[] Read(string path,string key)
        {
            //return File.ReadAllBytes(path);
            return ReadFromPath(path);
        }

        ///// <summary>
        ///// 将data写入指定路径
        ///// </summary>
        ///// <param name="path">指定文件路径</param>
        ///// <param name="data">写入数据</param>
        ///// <param name="key">密钥</param>
        //public static void Write(string path, byte[] data,string key)
        //{
        //    File.WriteAllBytes(path, data); 
        //}

        /// <summary>
        /// 从文件中读取数据
        /// </summary>
        /// <param name="srcFilePath"></param>
        /// <returns></returns>
        public static byte[] ReadFromPath(string srcFilePath)
        {
            // 解密
            return DataUtils.Instance.Decrypt(srcFilePath);
        }

        /// <summary>
        /// 拷贝文件
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