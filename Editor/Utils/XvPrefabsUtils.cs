
using Holo.XR.Detect;
using Holo.XR.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Holo.XR.Editor.Utils
{
    /// <summary>
    /// XVisio Ԥ�Ƽ����ع���
    /// </summary>
    public class XvPrefabsUtils
    {
        private const string SceneNodeTag = "SceneNode";
        private const string MR_SystemTag = "MR_System";

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
        public static void ImportMapLoader(string folderPath, string mapName,bool streaminAssets)
        {
            GameObject sceneNodeObj = GameObject.FindGameObjectWithTag(SceneNodeTag);
            GameObject mapLoaderObj;
            if (sceneNodeObj == null)
            {
                sceneNodeObj = new GameObject(SceneNodeTag);
                sceneNodeObj.tag = CheckTag(SceneNodeTag);

                mapLoaderObj = new GameObject("Holo XR World");
                sceneNodeObj.transform.parent = mapLoaderObj.transform;
            }
            else
            {
                mapLoaderObj = sceneNodeObj.transform.parent.gameObject;
            }

            XvCslamMapLoader xvCslamMapLoader = mapLoaderObj.AddComponent<XvCslamMapLoader>();
            xvCslamMapLoader.content = mapLoaderObj;
            xvCslamMapLoader.streamingAssets = streaminAssets;

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
            GameObject sceneNodeObj = GameObject.FindGameObjectWithTag(SceneNodeTag);
            GameObject mapObj;
            if (sceneNodeObj == null)
            {
                sceneNodeObj = new GameObject(SceneNodeTag);
                sceneNodeObj.tag = CheckTag(SceneNodeTag);

                mapObj = new GameObject("Holo XR World");
                sceneNodeObj.transform.parent = mapObj.transform;
            }
            else
            {
                mapObj = sceneNodeObj.transform.parent.gameObject;
            }

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

        private static GameObject CreateObject(string path)
        {
            GameObject instance = PrefabUtility.InstantiatePrefab(Resources.Load(path), SceneManager.GetActiveScene()) as GameObject;
            if (instance != null)
            {
                // Optionally, you can set the instance's position and rotation in the scene.
                instance.transform.position = Vector3.zero;
                instance.transform.rotation = Quaternion.identity;

                Debug.Log("Prefab instantiated in the scene!");
            }
            else
            {
                Debug.LogError("Failed to instantiate the prefab!");
            }
            return instance;
        }

        /// <summary>
        /// ��鳡�����Ƿ����Ŀ��tag����û���򴴽�
        /// </summary>
        /// <param name="newTag">Ŀ��Tag</param>
        /// <returns></returns>
        private static string CheckTag(string newTag)
        {
            bool tagExists = false;
            // ����ǩ�Ƿ��Ѿ�����
            for (int i = 0; i < UnityEditorInternal.InternalEditorUtility.tags.Length; i++)
            {
                if (UnityEditorInternal.InternalEditorUtility.tags[i] == newTag)
                {
                    tagExists = true;
                    break;
                }
            }

            // ����ǩ�Ƿ��Ѿ�����
            if (!tagExists)
            {
                // ʹ��SerializedObject���޸����ñ�ǩ���ݿ�
                SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                SerializedProperty tagsProp = tagManager.FindProperty("tags");

                // ����±�ǩ
                int index = tagsProp.arraySize;
                tagsProp.InsertArrayElementAtIndex(index);
                SerializedProperty newTagProp = tagsProp.GetArrayElementAtIndex(index);
                newTagProp.stringValue = newTag;

                // Ӧ���޸�
                tagManager.ApplyModifiedProperties();
            }
            return newTag;
        }

    }

}