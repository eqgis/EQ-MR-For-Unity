using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Holo.XR.Editor.UX
{

    public class SettingsWindow : EditorWindow
    {
        private List<MacorItem> m_List = new List<MacorItem>();

        private Dictionary<string, bool> m_Dic = new Dictionary<string, bool>();

        private string m_Macor = null;
        public void OnEnable()
        {
            m_Macor = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
            Debug.Log(m_Macor);
            m_List.Clear();
            //������������Լ���Ҫ����ĺ�
            m_List.Add(new MacorItem() { Name = "HYBIRDCLR_ENABLED", DisplayName = "�ȸ��� (���ȵ���HybirdCLR)", IsDebug = false, IsRelease = false });

            m_List.Add(new MacorItem() { Name = "DEBUG_MODEL", DisplayName = "����ģʽ", IsDebug = true, IsRelease = false });
            m_List.Add(new MacorItem() { Name = "DEBUG_LOG", DisplayName = "��ӡ��־", IsDebug = true, IsRelease = false });

            //����ѡ��
            m_List.Add(new MacorItem() { Name = "ENGINE_XVISIO", DisplayName = "XVisio", IsDebug = false, IsRelease = true });
            m_List.Add(new MacorItem() { Name = "ENGINE_ARCORE", DisplayName = "ARCore", IsDebug = false, IsRelease = true });
            m_List.Add(new MacorItem() { Name = "ENGINE_NREAL", DisplayName = "NReal", IsDebug = false, IsRelease = true });
            for (int i = 0; i < m_List.Count; i++)
            {
                if ("".Equals(m_Macor) || m_Macor == null || m_Macor.IndexOf(m_List[i].Name) == -1)
                {
                    m_Dic[m_List[i].Name] = false;
                }
                else
                {
                    m_Dic[m_List[i].Name] = true;
                }
            }
        }

        void OnGUI()
        {
            //EditorGUILayout.BeginHorizontal("box");
            //EditorGUIUtility.labelWidth = 40;
            //GUILayout.Label("��������:", EditorStyles.boldLabel);
            //if (GUILayout.Button("����ģʽ", GUILayout.Width(100)))
            //{
            //    for (int i = 0; i < m_List.Count; i++)
            //    {
            //        m_Dic[m_List[i].Name] = m_List[i].IsDebug;
            //    }
            //    SaveMacor();
            //}

            //if (GUILayout.Button("����ģʽ", GUILayout.Width(100)))
            //{
            //    for (int i = 0; i < m_List.Count; i++)
            //    {
            //        m_Dic[m_List[i].Name] = m_List[i].IsRelease;
            //    }
            //    SaveMacor();
            //}
            //EditorGUILayout.EndHorizontal();

            GUILayout.Space(10f);
            GUILayout.Label("��������:", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal("box");
            m_Dic[m_List[0].Name] = GUILayout.Toggle(m_Dic[m_List[0].Name], m_List[0].DisplayName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal("box");
            for (int i = 1; i < 3; i++)
            {
                m_Dic[m_List[i].Name] = GUILayout.Toggle(m_Dic[m_List[i].Name], m_List[i].DisplayName);
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10f);

            GUILayout.Label("ƽ̨ѡ��:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal("box");
            for (int i = 3; i < m_List.Count; i++)
            {
                m_Dic[m_List[i].Name] = GUILayout.Toggle(m_Dic[m_List[i].Name], m_List[i].DisplayName);
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10f);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace(); // ����һ�������ռ䣬����ť�Ƶ�ˮƽ����
            if (GUILayout.Button("�����޸�", GUILayout.Width(100)))
            {
                SaveMacor();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        private void SaveMacor()
        {
            m_Macor = string.Empty;
            foreach (var item in m_Dic)
            {
                if (item.Value)
                {
                    m_Macor += string.Format("{0};", item.Key);

                }
            }
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, m_Macor);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, m_Macor);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, m_Macor);
        }
        public class MacorItem
        {
            /// <summary>
            /// ����
            /// </summary>
            public string Name;
            /// <summary>
            /// ��ʾ������
            /// </summary>
            public string DisplayName;
            /// <summary>
            /// �Ƿ������
            /// </summary>
            public bool IsDebug;
            ///�Ƿ񷢲���
            public bool IsRelease;
        }
    }
}