using UnityEngine;
using System.Collections.Generic;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Holo.XR.Detect
{
    public class ImageDataMatcher : MonoBehaviour, ISerializationCallbackReceiver
    {
        [SerializeField]
        private ARCoreImageDetector imageDetector;

        /// <summary>
        /// ƥ��������б�
        /// </summary>
        private Dictionary<string, string> m_matchedData = new Dictionary<string, string>();

        /// <summary>
        /// Used to associate an `XRReferenceImage` with a Prefab by using the `XRReferenceImage`'s guid as a unique identifier for a particular reference image.
        /// </summary>
        [Serializable]
        struct NamedPrefab
        {
            // System.Guid isn't serializable, so we store the Guid as a string. At runtime, this is converted back to a System.Guid
            public string imageName;
            public string imageData;

            public NamedPrefab(string name, string data)
            {
                imageName = name;
                imageData = data;
            }
        }

        /// <summary>
        /// ��ͬʱʹ�� [SerializeField] �� [HideInInspector] ��ǩʱ��
        /// �ֶν��ᱻ���л�������ڽű���ʵ������������϶���Prefab��ʱ��
        /// ���ֶε�ֵ�ᱻ���档
        /// ���ǣ���Inspector�У����ֶν������أ�����û�����ֱ����Inspector�б༭����
        /// ����һЩ�����к����ã�����˵��Ҫ����ĳ���ֶε�ֵ�����ֲ������û���Inspector�б༭����
        /// </summary>
        [SerializeField]
        [HideInInspector]
        List<NamedPrefab> m_PrefabsList = new List<NamedPrefab>();


        /// <summary>
        /// ��ȡͼ��ƥ�������
        /// </summary>
        /// <param name="referenceImage"></param>
        /// <returns></returns>

        public string Match(string imageName)
            => m_matchedData.TryGetValue(imageName, out var data) ? data : null;

        #region 
        //��������Ҫ
        public void OnBeforeSerialize()
        {
            m_PrefabsList.Clear();
            foreach (var kvp in m_matchedData)
            {
                m_PrefabsList.Add(new NamedPrefab(kvp.Key, kvp.Value));
            }
        }

        public void OnAfterDeserialize()
        {
            m_matchedData = new Dictionary<string, string>();
            foreach (var entry in m_PrefabsList)
            {
                m_matchedData.Add(entry.imageName, entry.imageData);
            }
        }
        #endregion


#if UNITY_EDITOR
        /// <summary>
        /// This customizes the inspector component and updates the prefab list when
        /// the reference image library is changed.
        /// �Զ������������ARCoreImageDetector�����ݸ���ʦ������������б�����Ӧ����
        /// </summary>
        [CustomEditor(typeof(ImageDataMatcher))]
        class ImageDataMatcherInspector : Editor
        {
            List<ImageData> m_ReferenceImages = new List<ImageData>();
            bool m_IsExpanded = true;

            bool HasLibraryChanged(ImageData[] imageDatas)
            {
                if (imageDatas == null)
                    return m_ReferenceImages.Count == 0;

                if (m_ReferenceImages.Count != imageDatas.Length)
                    return true;

                for (int i = 0; i < imageDatas.Length; i++)
                {
                    if (m_ReferenceImages[i] != imageDatas[i])
                        return true;
                }

                return false;
            }
            public override void OnInspectorGUI()
            {
                //�Զ���inspector
                var behaviour = serializedObject.targetObject as ImageDataMatcher;

                serializedObject.Update();
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
                }

                var libraryProperty = serializedObject.FindProperty(nameof(imageDetector));
                EditorGUILayout.PropertyField(libraryProperty);
                var detector = libraryProperty.objectReferenceValue as ARCoreImageDetector;

                if (detector == null) { return; }


                //��������Ƿ�仯
                ImageData[] imageDatas = detector.GetImageData();
                if (HasLibraryChanged(imageDatas))
                {
                    var tempDictionary = new Dictionary<string, string>();
                    foreach (var imageData in imageDatas)
                    {
                        if (tempDictionary.ContainsKey(imageData.name))
                        {
                            //ͼ�����ƿ����ظ�������Ͳ������
                            continue;
                        }
                        tempDictionary.Add(imageData.name, behaviour.Match(imageData.name));

                    }
                    behaviour.m_matchedData = tempDictionary;
                }
                // ����
                m_ReferenceImages.Clear();
                foreach (var referenceImage in imageDatas)
                {
                    m_ReferenceImages.Add(referenceImage);
                }

                //��ʾ�����б�����������ƥ��
                m_IsExpanded = EditorGUILayout.Foldout(m_IsExpanded, "Data List");
                if (m_IsExpanded)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUI.BeginChangeCheck();

                        var tempDictionary = new Dictionary<string, string>();
                        foreach (var imageData in imageDatas)
                        {
                            if (tempDictionary.ContainsKey(imageData.name))
                            {
                                //ͼ�����ƿ����ظ�������Ͳ������
                                continue;
                            }
                            EditorGUILayout.LabelField("Image Name: " + imageData.name);
                            var prefab = EditorGUILayout.TextField(behaviour.Match(imageData.name));
                            tempDictionary.Add(imageData.name, prefab);
                        }

                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(target, "Update Data");
                            behaviour.m_matchedData = tempDictionary;
                            EditorUtility.SetDirty(target);
                        }
                    }
                }

                serializedObject.ApplyModifiedProperties();
            }

        }

#endif

    }


    //namespace========
}