using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Holo.Data
{
    /// <summary>
    /// AB包管理器
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
        /// 获取单例
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
        /// 获取加载过的AB包
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
        /// 加载AB资源
        /// </summary>
        /// <param name="data">AB数据</param>
        public IEnumerator LoadAB(string name,byte[] data)
        {
            //AssetBundle assetBundle = AssetBundle.LoadFromMemory(data);
            //_bundles.Add(name, assetBundle);

            // 异步加载AssetBundle
            AssetBundleCreateRequest assetBundleCreateRequest = AssetBundle.LoadFromMemoryAsync(data);
            yield return assetBundleCreateRequest;

            // 获取加载完成的AssetBundle
            AssetBundle assetBundle = assetBundleCreateRequest.assetBundle;
            _bundles.Add(name, assetBundle);
        }

        /// <summary>
        /// 卸载当前AB包
        /// </summary>
        /// <param name="unloadAllLoadedObjects">是否卸载已加载的实例</param>
        public void UnLoadCurrentAB(string name,bool unloadAllLoadedObjects)
        {
            AssetBundle currentAssetBundle = GetLoadedAssetBundle(name);
            if (currentAssetBundle != null)
            {
                currentAssetBundle.Unload(unloadAllLoadedObjects);
                //移除ab
                _bundles.Remove(name);
            }
        }

        /// <summary>
        /// 卸载所有AB包资源
        /// </summary>
        public void UnloadAll() {
            foreach (string name in _bundles.Keys) {
                UnLoadCurrentAB(name, true);
            }
        }
    }
}