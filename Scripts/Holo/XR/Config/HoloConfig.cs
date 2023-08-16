
namespace Holo.XR.Config
{
    public class HoloConfig
    {
        #region CSLAM地图数据读写

        /// <summary>
        /// CSLAM 地图包后缀
        /// </summary>
        public static string mapPackageSuffix { get; } = ".homap";

        //地图存储解压的临时文件
        public static string cslamMapSuffix { get; } = ".bin";
        public static string tagPoseSuffix { get; } = ".eq";

        //cache相对路径
        public static string cacheFolder { get; } = "/cache/";

        #endregion

        #region 热更新配置
        //热更新时数据包名称
        public static string hotUpdateDataName { get; } = "HoloData";

        //热更时AB包名
        public static string hotUpdateAbName { get; } = "hur";

        //热更数据相对文件夹
        public static string hotUpdateDataFolder { get; } = "/data/";

        //热更主场景配置文件
        public static string sceneConfig { get; } = "scene.cfg";
        #endregion
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


        #region hotFix
        /// <summary>
        /// 获取场景配置文件的名称
        /// </summary>
        /// <returns></returns>
        public static string GetSceneConfigName()
        {
            return HoloConfig.sceneConfig;
        }


        /// <summary>
        /// 获取热更时AB包名
        /// </summary>
        /// <returns></returns>
        public static string GetHotUpdateAbName()
        {
            return HoloConfig.hotUpdateAbName;
        }

        /// <summary>
        /// 获取热更数据名称
        /// </summary>
        /// <returns></returns>
        public static string GetHotDataName()
        {
            return HoloConfig.hotUpdateDataName;
        }
        #endregion
    }
}