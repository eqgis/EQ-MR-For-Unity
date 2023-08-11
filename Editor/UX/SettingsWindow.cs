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
            //在这里加入你自己想要定义的宏
            m_List.Add(new MacorItem() { Name = "HYBIRDCLR_ENABLED", DisplayName = "热更新 (需先导入HybirdCLR)", IsDebug = false, IsRelease = false });

            m_List.Add(new MacorItem() { Name = "DEBUG_MODEL", DisplayName = "调试模式", IsDebug = true, IsRelease = false });
            m_List.Add(new MacorItem() { Name = "DEBUG_LOG", DisplayName = "打印日志", IsDebug = true, IsRelease = false });

            //引擎选择
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
            //GUILayout.Label("快速设置:", EditorStyles.boldLabel);
            //if (GUILayout.Button("调试模式", GUILayout.Width(100)))
            //{
            //    for (int i = 0; i < m_List.Count; i++)
            //    {
            //        m_Dic[m_List[i].Name] = m_List[i].IsDebug;
            //    }
            //    SaveMacor();
            //}

            //if (GUILayout.Button("发布模式", GUILayout.Width(100)))
            //{
            //    for (int i = 0; i < m_List.Count; i++)
            //    {
            //        m_Dic[m_List[i].Name] = m_List[i].IsRelease;
            //    }
            //    SaveMacor();
            //}
            //EditorGUILayout.EndHorizontal();

            GUILayout.Space(10f);
            GUILayout.Label("开发设置:", EditorStyles.boldLabel);

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

            GUILayout.Label("平台选择:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal("box");
            for (int i = 3; i < m_List.Count; i++)
            {
                m_Dic[m_List[i].Name] = GUILayout.Toggle(m_Dic[m_List[i].Name], m_List[i].DisplayName);
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10f);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace(); // 创建一个伸缩空间，将按钮推到水平中心
            if (GUILayout.Button("保存修改", GUILayout.Width(100)))
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
            /// 名称
            /// </summary>
            public string Name;
            /// <summary>
            /// 显示的名称
            /// </summary>
            public string DisplayName;
            /// <summary>
            /// 是否调试项
            /// </summary>
            public bool IsDebug;
            ///是否发布项
            public bool IsRelease;
        }
    }
}