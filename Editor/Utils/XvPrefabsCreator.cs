
using Holo.XR.Detect;
using Holo.XR.Core;
using UnityEngine;
using Holo.HUR;

namespace Holo.XR.Editor.Utils
{
    /// <summary>
    /// XVisio 预制件加载工具
    /// </summary>
    class XvPrefabsCreator :BaseCreator
    {
        /// <summary>
        /// 导入XvManager对象
        /// </summary>
        public static void ImportXvManager()
        {
            CreateObject("Prefabs/Xvisio/XvXRManager").tag = CheckTag(MR_SystemTag);
        }

        /// <summary>
        /// 导入手势
        /// </summary>
        public static void ImportGesture()
        {
            CreateObject("Prefabs/Xvisio/MixedRealityToolkit").tag = CheckTag(MR_SystemTag);
            CreateObject("Prefabs/Xvisio/XvXRInput").tag = CheckTag(MR_SystemTag);
        }


        /// <summary>
        /// 导入Xv 投屏对象
        /// </summary>
        public static void ImportXvThrowScene()
        {
            CreateObject("Prefabs/Xvisio/ThrowScene").tag = CheckTag(MR_SystemTag);
        }

        /// <summary>
        /// 导入场景节点
        /// </summary>
        /// <param name="sceneName">跳转目标场景的名称</param>
        public static void ImportJumpSceneController(string sceneName,bool isAuto) {
            GameObject gameObject = new GameObject("To New Scene");
            //查看当前所有节点
            GameObject[] gameObjects = GameObject.FindGameObjectsWithTag(MR_SystemTag);

            //默认设置
            JumpSceneController jumpSceneController = gameObject.AddComponent<JumpSceneController>();
            jumpSceneController.dontDestoryGroup = gameObjects;
            jumpSceneController.nextSceneNodeTransform = gameObject.transform;
            jumpSceneController.auto = isAuto;
            if (sceneName != null)
            {
                jumpSceneController.sceneName = sceneName;
            }
        }

        /// <summary>
        /// 导入场景加载器
        /// </summary>
        public static void ImportMapLoader(string folderPath, string mapName,CslamMapDataSource dataSourceType,string webUrl)
        {
            GameObject[] obj = GetHoloRootNode();
            GameObject mapObj = obj[0];

            XvCslamMapLoader xvCslamMapLoader = mapObj.AddComponent<XvCslamMapLoader>();
            xvCslamMapLoader.content = mapObj;
            xvCslamMapLoader.sourceType = dataSourceType;
            xvCslamMapLoader.webUrl = webUrl;

            if (folderPath != null)
            {
                xvCslamMapLoader.folderPath = folderPath;
            }

            if (mapName != null)
            {
                xvCslamMapLoader.mapName = mapName;
            }

        }

        /// <summary>
        /// 导入场景扫描器
        /// </summary>
        public static void ImportMapScanner(string folderPath, string mapName)
        {
            GameObject[] obj = GetHoloRootNode(); 
            GameObject mapObj = obj[0];
            GameObject sceneNodeObj = obj[1];

            //添加CSLAM地图扫描组件
            XvCslamMapScanner xvCslamMapScanner = mapObj.AddComponent<XvCslamMapScanner>();
            xvCslamMapScanner.content = mapObj;
            sceneNodeObj.transform.parent = mapObj.transform;

            //添加Tag Detect组件
            TagDetect tagDetect = mapObj.AddComponent<TagDetect>();
            tagDetect.rootNode = sceneNodeObj;

            if (folderPath != null)
            {
                xvCslamMapScanner.folderPath = folderPath;
            }

            if (mapName != null)
            {
                xvCslamMapScanner.mapName = mapName;
            }
        }

    }

}