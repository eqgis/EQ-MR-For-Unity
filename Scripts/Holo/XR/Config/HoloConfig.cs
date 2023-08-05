
namespace Holo.XR.Config
{
    class HoloConfig
    {
        /// <summary>
        /// CSLAM ��ͼ����׺
        /// </summary>
        public static string mapPackageSuffix = ".homap";

        //��ͼ�洢��ѹ����ʱ�ļ�
        public static string cslamMapSuffix = ".bin";
        public static string tagPoseSuffix = ".eq";

        //cache���·��
        public static string cacheFolder = "/cache/";
    }

    public class EditorConfig
    {
        /// <summary>
        /// ��ȡ��ͼ���ݰ���׺����
        /// </summary>
        /// <returns>����</returns>
        public static string GetMapPackageSuffix()
        {
            string suffix = HoloConfig.mapPackageSuffix.TrimStart('.');
            return suffix;
        }
    }
}