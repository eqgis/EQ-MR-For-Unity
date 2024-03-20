using System.Collections.Generic;

namespace Holo.Data
{
    /// <summary>
    /// ��������
    /// </summary>
    public class HoloSceneConfig
    {
        /// <summary>
        /// ����������
        /// <code>�ȸ���ʱ����ڳ���</code>
        /// </summary>
        public string MainScene { get; set; }

        /// <summary>
        /// �ļ��嵥
        /// </summary>
        public List<string> FileList { get; set; }

        //�ȸ�DLL���Ƽ���
        public List<string> HotUpdateAssemblies { get; set; }

        //AOT����Ԫ���ݼ���
        public List<string> AotMetaAssemblies { get; set; }

        //AB������
        public List<string> AssetsBundleList { get; set; }
    }
}