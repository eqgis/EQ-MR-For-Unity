using UnityEngine;
using UnityEditor;
using Holo.XR.Editor.Utils;

namespace Holo.XR.Editor.UX
{
    public class EditWindowJumpSceneController : EditorWindow
    {
        private string inputText = "";
        private bool selected = true;

        private void OnGUI()
        {
            GUILayout.Space(10f);
            //���ñ�ǩ���
            EditorGUIUtility.labelWidth = 60.0f;
            // ��ʾ�ı������
            inputText = EditorGUILayout.TextField("��������:", inputText);

            GUILayout.Space(5f);
            selected = GUILayout.Toggle(selected, "�Զ���ת");
            GUILayout.Label("ע����ѡ�󣬳����������Զ���תĿ�곡��)", EditorStyles.boldLabel);
            // ���ı������Ͱ�ť֮�����Ӽ��
            GUILayout.Space(10f);
            if (GUILayout.Button("��������"))
            {
                XvPrefabsCreator.ImportJumpSceneController(inputText, selected);
                // �رյ���
                this.Close();
            }
        }
    }

}