
using UnityEngine;

namespace Holo.XR.Editor.Utils
{
    /// <summary>
    /// XVisio Ԥ�Ƽ����ع���
    /// </summary>
    class ARCorePrefabsCreator : BaseCreator
    {
        /// <summary>
        /// ����ARCore Camera���󣨰�����AR Session Origin���͡�AR Session����
        /// </summary>
        public static void ImportARCoreCamera()
        {
            CreateObject("Prefabs/ARCore/ARCore Session").tag = CheckTag(MR_SystemTag);
        }

    }

}