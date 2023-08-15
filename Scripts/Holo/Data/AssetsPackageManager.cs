namespace Holo.HUR
{
    /// <summary>
    /// 资源包管理器
    /// <code>
    /// 目的:方便管理<see cref="DllLoader"/>中文件路径，不采用单例模式
    /// </code>
    /// </summary>
    class AssetsPackageManager
    {
        public string folderPath;

        //数据包名称
        public const string dataName = "content.bin";


    }
}