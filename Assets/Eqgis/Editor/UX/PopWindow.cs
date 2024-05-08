
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Eqgis.Editor
{

    /// <summary>
    /// 提示内容弹窗
    /// </summary>
    public class PopWindow : EditorWindow
    {
        public static string msg;

        public static void Show(string message, int width, int height)
        {
            msg = message;
            Rect windowRect = new Rect(0, 0, width, height);
            PopWindow window = GetWindowWithRect<PopWindow>(windowRect, true, "提示");
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            GUILayout.TextArea(msg, EditorStyles.boldLabel);
            GUILayout.Space(10);
        }
        private void OnEnable()
        {
            EditorApplication.delayCall = async () =>
            {
                await Task.Delay(3000);

                Close();
            };
        }
    }
}