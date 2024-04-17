using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Holo.Data
{
    /// <summary>
    /// AB��������
    /// </summary>
    public class AssetBundleManager
    {

        private static readonly object lockObject = new object();
        private static AssetBundleManager instance = null;

        private Dictionary<string, AssetBundle> _bundles = new Dictionary<string, AssetBundle>();

        private AssetBundleManager()
        {

        }

        /// <summary>
        /// ��ȡ����
        /// </summary>
        public static AssetBundleManager Instance 
        {
            get
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new AssetBundleManager();
                    }
                    return instance;
                }
            }
        }

        /// <summary>
        /// ��ȡ���ع���AB��
        /// </summary>
        /// <returns></returns>
        public AssetBundle GetLoadedAssetBundle(string name)
        {
            AssetBundle bundle;
            if (_bundles.TryGetValue(name, out bundle))
            {
                return bundle;
            }
            return null;
        }

        /// <summary>
        /// ����AB��Դ
        /// </summary>
        /// <param name="data">AB����</param>
        public IEnumerator LoadAB(string name,byte[] data)
        {
            //AssetBundle assetBundle = AssetBundle.LoadFromMemory(data);
            //_bundles.Add(name, assetBundle);

            // �첽����AssetBundle
            AssetBundleCreateRequest assetBundleCreateRequest = AssetBundle.LoadFromMemoryAsync(data);
            yield return assetBundleCreateRequest;

            // ��ȡ������ɵ�AssetBundle
            AssetBundle assetBundle = assetBundleCreateRequest.assetBundle;
            _bundles.Add(name, assetBundle);
        }

        /// <summary>
        /// ж�ص�ǰAB��
        /// </summary>
        /// <param name="unloadAllLoadedObjects">�Ƿ�ж���Ѽ��ص�ʵ��</param>
        public void UnLoadCurrentAB(string name,bool unloadAllLoadedObjects)
        {
            AssetBundle currentAssetBundle = GetLoadedAssetBundle(name);
            if (currentAssetBundle != null)
            {
                currentAssetBundle.Unload(unloadAllLoadedObjects);
                //�Ƴ�ab
                _bundles.Remove(name);
            }
        }

        /// <summary>
        /// ж������AB����Դ
        /// </summary>
        public void UnloadAll() {
            foreach (string name in _bundles.Keys) {
                UnLoadCurrentAB(name, true);
            }
        }
    }
}