using System.IO;
using System.Security.Cryptography;
using UnityEngine;

namespace Holo.XR.Utils
{
    internal class EqAesUtils
    {
        //Ω‚√‹
        private static byte[] Decrypt(string srcFilePath)
        {
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

        /// <summary>
        /// º”√‹
        /// </summary>
        /// <param name="srcFilePath"></param>
        /// <param name="encryptedFilePath"></param>
        private static void Encrypt(string srcFilePath, string encryptedFilePath)
        {
            try
            {
                // º”√‹
                using (Aes aesAlg = Aes.Create())
                {
                    byte[] key = aesAlg.Key;
                    byte[] iv = aesAlg.IV;
                    using (FileStream fsSrc = new FileStream(srcFilePath, FileMode.Open))
                    using (FileStream fsEncrypted = new FileStream(encryptedFilePath, FileMode.Create))
                    {
                        fsEncrypted.Write(key, 0, key.Length);
                        fsEncrypted.Write(iv, 0, iv.Length);
                        {

                            using (ICryptoTransform encryptor = aesAlg.CreateEncryptor())
                            using (CryptoStream csEncrypt = new CryptoStream(fsEncrypted, encryptor, CryptoStreamMode.Write))
                            {
                                byte[] buffer = new byte[4096];
                                int bytesRead;
                                while ((bytesRead = fsSrc.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    csEncrypt.Write(buffer, 0, bytesRead);
                                }
                            }
                        }
                    }
                }
            }
            catch (IOException ex)
            {
                Debug.LogWarning(ex);
            }
        }
    }
}