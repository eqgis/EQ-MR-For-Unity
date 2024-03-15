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
            //设置标签宽度
            EditorGUIUtility.labelWidth = 60.0f;
            // 显示文本输入框
            inputText = EditorGUILayout.TextField("场景名称:", inputText);

            GUILayout.Space(5f);
            selected = GUILayout.Toggle(selected, "自动跳转");
            GUILayout.Label("注：勾选后，场景启动后自动跳转目标场景)", EditorStyles.boldLabel);
            // 在文本输入框和按钮之间增加间距
            GUILayout.Space(10f);
            if (GUILayout.Button("创建对象"))
            {
                XvPrefabsCreator.ImportJumpSceneController(inputText, selected);
                // 关闭弹窗
                this.Close();
            }
        }
    }

}