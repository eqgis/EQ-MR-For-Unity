using System.Collections.Generic;

namespace Holo.Data
{
    /// <summary>
    /// 场景配置
    /// </summary>
    public class HoloSceneConfig
    {
        /// <summary>
        /// 主场景名称
        /// <code>热更新时的入口场景</code>
        /// </summary>
        public string MainScene { get; set; }

        /// <summary>
        /// 文件清单
        /// </summary>
        public List<string> FileList { get; set; }

        //热更DLL名称集合
        public List<string> HotUpdateAssemblies { get; set; }

        //AOT补充元数据集合
        public List<string> AotMetaAssemblies { get; set; }

        //AB包集合
        public List<string> AssetsBundleList { get; set; }
    }
}