using Holo.HUR;
using Holo.XR.Editor.Installer;
using Holo.XR.Editor.Utils;
using LitJson;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Holo.XR.Editor.UX
{
    public class SettingsWindow : EditorWindow
    {
        #region Other
        //��Unity�˵��д���һ���˵�·���������ú궨��
        [MenuItem("Holo-XR/Settings")]
        public static void Setting()
        {
            //Rect windowRect = new Rect(100, 100, 240, 180);
            //SettingsWindow win = EditorWindow.GetWindowWithRect<SettingsWindow>(windowRect, false, "Holo-Settings");
            SettingsWindow win = EditorWindow.GetWindow<SettingsWindow>(false, "Holo-Settings");
            //win.titleContent = new GUIContent("ȫ������");
            win.Show();
        }

        #endregion

        private List<MacorItem> m_List = new List<MacorItem>();

        private Dictionary<string, bool> m_Dic = new Dictionary<string, bool>();

        private string m_Macor = null;
        private SbcAuth sbcAuth;
        public void OnEnable()
        {
            //��ȡ��������
            sbcAuth = SbcAuthUtils.Read();

            m_Macor = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
            Debug.Log(m_Macor);
            m_List.Clear();
            //������������Լ���Ҫ����ĺ�
            m_List.Add(new MacorItem() { Name = "HYBIRDCLR_ENABLED", DisplayName = "�ȸ��� (���Ȱ�װHybirdCLR)", IsDebug = false, IsRelease = false });

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

            //if (GUILayout.Button("Install HybirdCLR", GUILayout.Width(120)))
            //{
            //    HybridCLRInstaller.Import();
            //}

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal("box");
            for (int i = 1; i < 3; i++)
            {
                m_Dic[m_List[i].Name] = GUILayout.Toggle(m_Dic[m_List[i].Name], m_List[i].DisplayName);
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10f);

            GUILayout.Label("ƽ̨ѡ��:", EditorStyles.boldLabel);

            //һ����װARCore���ӱ�������
            //if (GUILayout.Button("Install ARCore", GUILayout.Width(120)))
            //{
            //    ARCoreInstaller.Import();
            //}

            EditorGUILayout.BeginHorizontal("box");
            for (int i = 3; i < m_List.Count; i++)
            {
                m_Dic[m_List[i].Name] = GUILayout.Toggle(m_Dic[m_List[i].Name], m_List[i].DisplayName);
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10f);

            GUILayout.Label("˼�س�SDK����:", EditorStyles.boldLabel);

            sbcAuth.apiKey = EditorGUILayout.TextField("Api Key:", sbcAuth.apiKey);
            sbcAuth.productID = EditorGUILayout.TextField("Product ID:", sbcAuth.productID);
            sbcAuth.productKey = EditorGUILayout.TextField("Product Key:", sbcAuth.productKey);
            sbcAuth.productSecret = EditorGUILayout.TextField("Product Secret:", sbcAuth.productSecret);


            GUILayout.Space(10f);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace(); // ����һ�������ռ䣬����ť�Ƶ�ˮƽ����
            if (GUILayout.Button("�����޸�", GUILayout.Width(100)))
            {
                SaveMacor();

                SbcAuthUtils.SaveSbcAuth();

                PopWindow.Show("�޸����!", 200, 80);
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