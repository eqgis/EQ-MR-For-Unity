using System.IO;

namespace Holo.HUR
{
    /// <summary>
    /// ����IO�����ᵽ�˴������ں����������
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