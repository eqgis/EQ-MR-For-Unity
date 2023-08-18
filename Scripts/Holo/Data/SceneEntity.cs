using System.Collections.Generic;

namespace Holo.Data
{
    public class SceneEntity
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
    }
}