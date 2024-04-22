using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainSceneGUI : MonoBehaviour
{
    public GUISkin guiSkin;


    Rect windowRect = new Rect(0, 0, 300, 200);
    private bool show = true;

    void Start()
    {
        windowRect.x = (Screen.width - windowRect.width) / 2;
        windowRect.y = (Screen.height - windowRect.height) / 2;
    }


    void OnGUI()
    {
        if (show)
        {
            GUI.skin = guiSkin;
            windowRect = GUI.Window(0, windowRect, DoMyWindow, "提示");
        }
    }



    void DoMyWindow(int windowID)
    {

        GUI.Label(new Rect(40, 40, 200, 40), "1、移动设备进行图片识别");
        GUI.Label(new Rect(40, 90, 200, 40), "2、点击模型载入对应场景");
        
        if (GUI.Button(new Rect(60, 140, 200, 60), "确定"))
        {
            show = false;
        }
    }

    public void ShowTips()
    {
        show = true;
    }
}
