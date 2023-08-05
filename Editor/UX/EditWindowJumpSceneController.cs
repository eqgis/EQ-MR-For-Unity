using UnityEngine;
using UnityEditor;
using Holo.XR.Editor.Utils;

public class EditWindowJumpSceneController : EditorWindow
{
    private string inputText = "";

    private void OnGUI()
    {
        GUILayout.Space(15f);
        //���ñ�ǩ���
        EditorGUIUtility.labelWidth = 60.0f;
        // ��ʾ�ı������
        inputText = EditorGUILayout.TextField("��������:", inputText);

        // ���ı������Ͱ�ť֮�����Ӽ��
        GUILayout.Space(15f);
        if (GUILayout.Button("��������"))
        {
            XvPrefabsUtils.ImportJumpSceneController(inputText);
            // �رյ���
            this.Close();
        }
    }
}
