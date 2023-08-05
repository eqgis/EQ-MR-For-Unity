
namespace Holo.XR.Config
{
    class HoloConfig
    {
        /// <summary>
        /// CSLAM 地图包后缀
        /// </summary>
        public static string mapPackageSuffix = ".homap";

        //地图存储解压的临时文件
        public static string cslamMapSuffix = ".bin";
        public static string tagPoseSuffix = ".eq";

        //cache相对路径
        public static string cacheFolder = "/cache/";
    }

    public class EditorConfig
    {
        /// <summary>
        /// 获取地图数据包后缀名称
        /// </summary>
        /// <returns>名称</returns>
        public static string GetMapPackageSuffix()
        {
            string suffix = HoloConfig.mapPackageSuffix.TrimStart('.');
            return suffix;
        }
    }
}