using System.Collections.Generic;

namespace Holo.Data
{
    public class SceneEntity
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
    }
}