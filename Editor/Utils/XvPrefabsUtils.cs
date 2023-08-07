
using Holo.XR.Detect;
using Holo.XR.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Holo.XR.Editor.Utils
{
    /// <summary>
    /// XVisio 预制件加载工具
    /// </summary>
    public class XvPrefabsUtils
    {
        private const string SceneNodeTag = "SceneNode";
        private const string MR_SystemTag = "MR_System";

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
        /// 导入场景扫描器
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
        /// 检查场景中是否存在目标tag，若没有则创建
        /// </summary>
        /// <param name="newTag">目标Tag</param>
        /// <returns></returns>
        private static string CheckTag(string newTag)
        {
            bool tagExists = false;
            // 检查标签是否已经存在
            for (int i = 0; i < UnityEditorInternal.InternalEditorUtility.tags.Length; i++)
            {
                if (UnityEditorInternal.InternalEditorUtility.tags[i] == newTag)
                {
                    tagExists = true;
                    break;
                }
            }

            // 检查标签是否已经存在
            if (!tagExists)
            {
                // 使用SerializedObject来修改内置标签数据库
                SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                SerializedProperty tagsProp = tagManager.FindProperty("tags");

                // 添加新标签
                int index = tagsProp.arraySize;
                tagsProp.InsertArrayElementAtIndex(index);
                SerializedProperty newTagProp = tagsProp.GetArrayElementAtIndex(index);
                newTagProp.stringValue = newTag;

                // 应用修改
                tagManager.ApplyModifiedProperties();
            }
            return newTag;
        }

    }

}