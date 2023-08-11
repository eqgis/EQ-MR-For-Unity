using System.IO;

namespace Holo.HUR
{
    /// <summary>
    /// 数据IO，先提到此处，便于后续引入加密
    /// </summary>
    class DataIO
    {
        internal static byte[] Read(string path,string key)
        {
            return File.ReadAllBytes(path);
        }

        internal static void Write(string path, byte[] data,string key)
        {
            File.WriteAllBytes(path, data); 
        }
    }
}