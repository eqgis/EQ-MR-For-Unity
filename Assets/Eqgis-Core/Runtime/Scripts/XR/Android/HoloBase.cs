using System.Collections.Generic;
using UnityEngine;

namespace Holo.XR.Android
{
    public class HoloBase : MonoBehaviour
    {
        private AndroidUtils androidUtils;

        public List<GameObject> hiddenObjectsInAndroid;

        private void Awake()
        {
            Application.targetFrameRate = 30;
            androidUtils = AndroidUtils.GetInstance();

#if DEBUG_MODEL
            EqLog.d("HoloBase", "---Awake---");
#endif

            if (Application.platform == RuntimePlatform.Android)
            {
                foreach (var item in hiddenObjectsInAndroid)
                {
                    //item.SetActive(false);
                    Destroy(item);
                }
            }
        }

        private void Start()
        {
#if DEBUG_MODEL
            EqLog.d("HoloBase", "---Start---");
            string sceneName = this.gameObject.scene.name;
            androidUtils.ShowToast("µ±Ç°³¡¾°£º" + sceneName);
#endif
        }

        private void OnDestroy()
        {
#if DEBUG_MODEL
                EqLog.d("HoloBase", "---OnDestroy---"); 
#endif
            androidUtils.Destroy();
        }
    }
}