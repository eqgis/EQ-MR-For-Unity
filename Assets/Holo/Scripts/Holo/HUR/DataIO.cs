using System;
using System.IO;
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

        /// <summary>
        /// 写入版本信息
        /// </summary>
        /// <param name="targetFilePath">输出文件夹路径</param>
        /// <param name="dataVersionInfo">内容</param>
        public static void WriteVersionFile(string targetFilePath,string dataVersionInfo,bool overrideFile)
        {
            string filePath = targetFilePath + "/" + XR.Config.HoloConfig.versionFileName;
            //判断文件是否存在
            if (File.Exists(filePath))
            {
                if(overrideFile)
                {
                    File.Delete(filePath);
                    //在本地路径写入当前数据版本信息
                    File.WriteAllText(filePath, "###Data Version###\n" + dataVersionInfo);
                }
                else
                {
                    File.AppendAllText(filePath, "\n" + dataVersionInfo);
                }
            }
            else
            {
                //在本地路径写入当前数据版本信息
                File.WriteAllText(filePath, "###Data Version###\n" + dataVersionInfo);
            }
        }


        /// <summary>
        /// 读取新建的版本号
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

            //版本内容
            string versionContent = File.ReadAllText(filePath);

            string[] versionList = versionContent.Split('\n');
            float maxVersion = 0;
            foreach (string item in versionList)
            {
                string fileFullName = item.Trim();
                string content = Path.GetFileNameWithoutExtension(fileFullName); // 去除空格和换行符

                //“#”开头则跳过该行
                if (!content.StartsWith("#"))
                {
                    string[] parts = content.Split(new string[] { "_v" }, StringSplitOptions.None);

                    if (parts.Length == 2)
                    {
                        string version = parts[1];
                        float versionValue = float.Parse(version);
                        if (maxVersion < versionValue)
                        {
                            //记录最大值
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