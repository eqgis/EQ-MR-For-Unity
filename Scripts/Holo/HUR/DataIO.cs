using System.IO;
using System.Security.Cryptography;
using System.Text;
using Unity;

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
        private static byte[] ReadFromPath(string srcFilePath)
        {
            // 解密
            using (Aes aesAlg = Aes.Create())
            {
                using (FileStream fsEncrypted = new FileStream(srcFilePath, FileMode.Open))
                using (MemoryStream fs = new MemoryStream())
                {
                    byte[] key2 = new byte[32];
                    byte[] iv2 = new byte[16];
                    fsEncrypted.Read(key2, 0, key2.Length);
                    fsEncrypted.Read(iv2, 0, iv2.Length);
                    using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(key2, iv2))
                    using (CryptoStream csDecrypt = new CryptoStream(fsEncrypted, decryptor, CryptoStreamMode.Read))
                    {
                        byte[] buffer = new byte[4096];
                        int bytesRead;
                        while ((bytesRead = csDecrypt.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            fs.Write(buffer, 0, bytesRead);
                        }
                        return fs.ToArray();
                    }
                }
            }
        }
    }


}