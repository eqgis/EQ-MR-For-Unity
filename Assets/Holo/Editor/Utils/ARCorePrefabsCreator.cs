
using UnityEngine;

namespace Holo.XR.Editor.Utils
{
    /// <summary>
    /// XVisio 预制件加载工具
    /// </summary>
    class ARCorePrefabsCreator : BaseCreator
    {
        /// <summary>
        /// 导入ARCore Camera对象（包含“AR Session Origin”和“AR Session”）
        /// </summary>
        public static void ImportARCoreCamera()
        {
            CreateObject("Prefabs/ARCore/ARCore Session").tag = CheckTag(MR_SystemTag);
        }

    }

}