using UnityEngine;
using System.IO;
using Holo.XR.Config;
using Holo.XR.Android;

namespace Holo.XR.Utils
{

    /// <summary>
    /// IO 工具类
    /// </summary>
    public class IoUtils
    {
        public static string CopyMapToPersistentDataPath(string relativePath,bool forceUpdate)
        {
            //格式校验
            relativePath.Replace("\\", "/");
            if (!relativePath.StartsWith("/"))
            {
                relativePath = "/" + relativePath;
            }

            //示例数据路径:"/homap/test.homap"
            //如果持久化目录下没有文件，先从streamingAssets里复制一份到持久化目录
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
