using Holo.XR.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace Holo.XR.Editor
{
    public class CustomHierarchyMenu
    {
        [MenuItem("GameObject/Holo XR", false, 10)]
        static void CreateCustomObject(MenuCommand menuCommand)
        {
            GameObject newObject = new GameObject("CustomObject");
            // Add other components or set object properties
            // newObject.AddComponent<YourCustomComponent>();

            if (menuCommand.context is GameObject contextObject)
            {
                GameObjectUtility.SetParentAndAlign(newObject, contextObject);
            }

            Undo.RegisterCreatedObjectUndo(newObject, "Create " + newObject.name);
            Selection.activeObject = newObject;
        }

        [MenuItem("GameObject/Holo XR/Camera", false, 11)]
        static void XvManager(MenuCommand menuCommand)
        {
            GameObject selectedObject = menuCommand.context as GameObject;
            // Implement the action for Option 1 here
            XvPrefabsUtils.ImportXvManager();
        }

        [MenuItem("GameObject/Holo XR/Gesture", false, 12)]
        static void Option2(MenuCommand menuCommand)
        {
            GameObject selectedObject = menuCommand.context as GameObject;
            // Implement the action for Option 2 here
            XvPrefabsUtils.ImportGesture();
        }

        [MenuItem("GameObject/Holo XR/ThrowScreenObj", false, 13)]
        static void ThrowScreenObj(MenuCommand menuCommand)
        {
            GameObject selectedObject = menuCommand.context as GameObject;
            // Implement the action for Option 2 here
            XvPrefabsUtils.ImportXvThrowScene();
        }

        [MenuItem("GameObject/Holo XR/JumpSceneController", false, 13)]
        static void JumpSceneController(MenuCommand menuCommand)
        {
            // Implement the action for Option 2 here
            // �����Զ��嵯������ʾ
            // �����Զ��嵯�������óߴ�
            Rect windowRect = new Rect(100, 100, 240, 80);
            EditWindowJumpSceneController window = EditorWindow.GetWindowWithRect<EditWindowJumpSceneController>(windowRect, true, "����Ŀ�곡������");
            window.Show();
        }
    }

}
