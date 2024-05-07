using Holo.XR.Editor.Utils;
using Holo.XR.Editor.UX;
using UnityEditor;
using UnityEngine;

namespace Holo.XR.Editor
{
    public class CustomHierarchyMenu
    {
#if ENGINE_XVISIO
        [MenuItem("GameObject/Holo XR/Camera", false, 11)]
        static void XvManager(MenuCommand menuCommand)
        {
            GameObject selectedObject = menuCommand.context as GameObject;
            // Implement the action for Option 1 here
            XvPrefabsCreator.ImportXvManager();
        }

        [MenuItem("GameObject/Holo XR/Gesture", false, 12)]
        static void Option2(MenuCommand menuCommand)
        {
            GameObject selectedObject = menuCommand.context as GameObject;
            // Implement the action for Option 2 here
            XvPrefabsCreator.ImportGesture();
        }

        [MenuItem("GameObject/Holo XR/ThrowScreenObj", false, 13)]
        static void ThrowScreenObj(MenuCommand menuCommand)
        {
            GameObject selectedObject = menuCommand.context as GameObject;
            // Implement the action for Option 2 here
            XvPrefabsCreator.ImportXvThrowScene();
        }


#endif
        [MenuItem("GameObject/Holo/SceneNode", false, 13)]
        static void CreateSceneNode(MenuCommand menuCommand)
        {
            if (!BaseCreator.CreateSceneNode())
            {
                Debug.LogWarning("SceneNode 已存在，请勿重复创建");
            }
        }

        [MenuItem("GameObject/Holo/ExSceneTransition", false, 14)]
        static void JumpSceneController(MenuCommand menuCommand)
        {
            // Implement the action for Option 2 here
            // 创建自定义弹窗并显示
            // 创建自定义弹窗并设置尺寸
            Rect windowRect = new Rect(100, 100, 260, 120);
            EditWindowJumpSceneController window = EditorWindow.GetWindowWithRect<EditWindowJumpSceneController>(windowRect, true, "输入目标场景名称");
            window.Show();
        }

        [MenuItem("GameObject/Holo/VoiceAssistant", false, 15)]
        static void CreateVoiceAssistant(MenuCommand menuCommand)
        {
            GameObject selectedObject = menuCommand.context as GameObject;
            //创建语音助手

            VoiceAssistantCreator.CreateVoiceAssistant();

        }


#if ENGINE_ARCORE
        [MenuItem("GameObject/Holo XR/Camera", false, 11)]
        static void XvManager(MenuCommand menuCommand)
        {
            GameObject selectedObject = menuCommand.context as GameObject;
            //创建ARCamera(包含ARSession和ARSession Origin)
            ARCorePrefabsCreator.ImportARCoreCamera();
        }

#endif
    }

}
