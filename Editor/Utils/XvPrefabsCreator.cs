
using Holo.XR.Detect;
using Holo.XR.Core;
using UnityEngine;
using Holo.HUR;

namespace Holo.XR.Editor.Utils
{
    /// <summary>
    /// XVisio Ԥ�Ƽ����ع���
    /// </summary>
    class XvPrefabsCreator :BaseCreator
    {
        /// <summary>
        /// ����XvManager����
        /// </summary>
        public static void ImportXvManager()
        {
            CreateObject("Prefabs/Xvisio/XvXRManager").tag = CheckTag(MR_SystemTag);
        }

        /// <summary>
        /// ��������
        /// </summary>
        public static void ImportGesture()
        {
            CreateObject("Prefabs/Xvisio/MixedRealityToolkit").tag = CheckTag(MR_SystemTag);
            CreateObject("Prefabs/Xvisio/XvXRInput").tag = CheckTag(MR_SystemTag);
        }


        /// <summary>
        /// ����Xv Ͷ������
        /// </summary>
        public static void ImportXvThrowScene()
        {
            CreateObject("Prefabs/Xvisio/ThrowScene").tag = CheckTag(MR_SystemTag);
        }

        /// <summary>
        /// ���볡���ڵ�
        /// </summary>
        /// <param name="sceneName">��תĿ�곡��������</param>
        public static void ImportJumpSceneController(string sceneName,bool isAuto) {
            GameObject gameObject = new GameObject("To New Scene");
            //�鿴��ǰ���нڵ�
            GameObject[] gameObjects = GameObject.FindGameObjectsWithTag(MR_SystemTag);

            //Ĭ������
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
        /// ���볡��������
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
        /// ���볡��ɨ����
        /// </summary>
        public static void ImportMapScanner(string folderPath, string mapName)
        {
            GameObject[] obj = GetHoloRootNode(); 
            GameObject mapObj = obj[0];
            GameObject sceneNodeObj = obj[1];

            //���CSLAM��ͼɨ�����
            XvCslamMapScanner xvCslamMapScanner = mapObj.AddComponent<XvCslamMapScanner>();
            xvCslamMapScanner.content = mapObj;
            sceneNodeObj.transform.parent = mapObj.transform;

            //���Tag Detect���
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