using UnityEngine;
using UnityEditor;
using Holo.XR.Editor.Utils;

public class EditWindowJumpSceneController : EditorWindow
{
    private string inputText = "";

    private void OnGUI()
    {
        GUILayout.Space(15f);
        //设置标签宽度
        EditorGUIUtility.labelWidth = 60.0f;
        // 显示文本输入框
        inputText = EditorGUILayout.TextField("场景名称:", inputText);

        // 在文本输入框和按钮之间增加间距
        GUILayout.Space(15f);
        if (GUILayout.Button("创建对象"))
        {
            XvPrefabsUtils.ImportJumpSceneController(inputText);
            // 关闭弹窗
            this.Close();
        }
    }
}
