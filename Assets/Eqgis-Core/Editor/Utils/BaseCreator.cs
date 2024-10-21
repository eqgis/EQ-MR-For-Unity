using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace Holo.XR.Editor.Utils
{ 
    class BaseCreator
    {
        protected const string SceneNodeTag = "SceneNode";
        protected const string MR_SystemTag = "MR_System";

        /// <summary>
        /// 获取HOLO场景的根节点
        /// </summary>
        /// <returns>[0]为世界节点，[1]为场景节点</returns>
        public static GameObject[] GetHoloRootNode()
        {
            string tag = CheckTag(SceneNodeTag);
            GameObject sceneNodeObj = GameObject.FindGameObjectWithTag(SceneNodeTag);
            GameObject mapObj;
            if (sceneNodeObj == null)
            {
                sceneNodeObj = new GameObject(SceneNodeTag);
                sceneNodeObj.tag = tag;

                mapObj = new GameObject("Holo XR World");
                sceneNodeObj.transform.parent = mapObj.transform;
            }
            else
            {
                mapObj = sceneNodeObj.transform.parent.gameObject;
            }

            return new GameObject[] { mapObj, sceneNodeObj };
        }

        /// <summary>
        /// 创建场景节点
        /// </summary>
        /// <returns>若场景中已存在，则返回false。若新建成功，则返回true</returns>
        public static bool CreateSceneNode()
        {
            string tag = CheckTag(SceneNodeTag);
            GameObject sceneNodeObj = GameObject.FindGameObjectWithTag(SceneNodeTag);
            GameObject mapObj;
            if (sceneNodeObj == null)
            {
                sceneNodeObj = new GameObject(SceneNodeTag);
                sceneNodeObj.tag = tag;

                mapObj = new GameObject("Holo XR World");
                sceneNodeObj.transform.parent = mapObj.transform;
                return true;
            }

            //已存在，返回false
            return false;
        }

        /// <summary>
        /// 创建游戏对象
        /// </summary>
        /// <param name="path">Resrouce路径下的预制件</param>
        /// <returns>游戏对象</returns>
        protected static GameObject CreateObject(string path)
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
        protected static string CheckTag(string newTag)
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