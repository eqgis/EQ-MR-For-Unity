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
        /// ��ȡHOLO�����ĸ��ڵ�
        /// </summary>
        /// <returns>[0]Ϊ����ڵ㣬[1]Ϊ�����ڵ�</returns>
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
        /// ���������ڵ�
        /// </summary>
        /// <returns>���������Ѵ��ڣ��򷵻�false�����½��ɹ����򷵻�true</returns>
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

            //�Ѵ��ڣ�����false
            return false;
        }

        /// <summary>
        /// ������Ϸ����
        /// </summary>
        /// <param name="path">Resrouce·���µ�Ԥ�Ƽ�</param>
        /// <returns>��Ϸ����</returns>
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
        /// ��鳡�����Ƿ����Ŀ��tag����û���򴴽�
        /// </summary>
        /// <param name="newTag">Ŀ��Tag</param>
        /// <returns></returns>
        protected static string CheckTag(string newTag)
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