using UnityEngine;
using System.IO;
using Holo.XR.Config;
using Holo.XR.Android;

namespace Holo.XR.Utils
{

    /// <summary>
    /// IO ������
    /// </summary>
    public class IoUtils
    {
        public static string CopyMapToPersistentDataPath(string relativePath,bool forceUpdate)
        {
            //��ʽУ��
            relativePath.Replace("\\", "/");
            if (!relativePath.StartsWith("/"))
            {
                relativePath = "/" + relativePath;
            }

            //ʾ������·��:"/homap/test.homap"
            //����־û�Ŀ¼��û���ļ����ȴ�streamingAssets�︴��һ�ݵ��־û�Ŀ¼
            string targetPersistentPath = Application.persistentDataPath + relativePath;
            if (forceUpdate || !File.Exists(targetPersistentPath))
            {
                EqLog.d("IKKYU", Application.streamingAssetsPath + relativePath);
                //WWW loadWWW = new WWW(Application.streamingAssetsPath + relativePath);
                //while (!loadWWW.isDone)
                //{
                //}
                //File.WriteAllBytes(targetPersistentPath, loadWWW.bytes);
                File.Copy(Application.streamingAssetsPath + relativePath, targetPersistentPath);
            }

            return targetPersistentPath;
        }
    }
}
