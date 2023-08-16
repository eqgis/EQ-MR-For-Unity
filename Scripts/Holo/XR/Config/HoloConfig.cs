
namespace Holo.XR.Config
{
    public class HoloConfig
    {
        #region CSLAM��ͼ���ݶ�д

        /// <summary>
        /// CSLAM ��ͼ����׺
        /// </summary>
        public static string mapPackageSuffix { get; } = ".homap";

        //��ͼ�洢��ѹ����ʱ�ļ�
        public static string cslamMapSuffix { get; } = ".bin";
        public static string tagPoseSuffix { get; } = ".eq";

        //cache���·��
        public static string cacheFolder { get; } = "/cache/";

        #endregion

        #region �ȸ�������
        //�ȸ���ʱ���ݰ�����
        public static string hotUpdateDataName { get; } = "HoloData";

        //�ȸ�ʱAB����
        public static string hotUpdateAbName { get; } = "hur";

        //�ȸ���������ļ���
        public static string hotUpdateDataFolder { get; } = "/data/";

        //�ȸ������������ļ�
        public static string sceneConfig { get; } = "scene.cfg";
        #endregion
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


        #region hotFix
        /// <summary>
        /// ��ȡ���������ļ�������
        /// </summary>
        /// <returns></returns>
        public static string GetSceneConfigName()
        {
            return HoloConfig.sceneConfig;
        }


        /// <summary>
        /// ��ȡ�ȸ�ʱAB����
        /// </summary>
        /// <returns></returns>
        public static string GetHotUpdateAbName()
        {
            return HoloConfig.hotUpdateAbName;
        }

        /// <summary>
        /// ��ȡ�ȸ���������
        /// </summary>
        /// <returns></returns>
        public static string GetHotDataName()
        {
            return HoloConfig.hotUpdateDataName;
        }
        #endregion
    }
}