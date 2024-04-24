using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainSceneGUI : MonoBehaviour
{
    public GUISkin guiSkin;


    Rect windowRect = new Rect(0, 0, 600, 400);
    private bool show = true;


    private GUIStyle m_TitleStyle;

    void Start()
    {
        windowRect.x = (Screen.width - windowRect.width) / 2;
        windowRect.y = (Screen.height - windowRect.height) / 2;

        m_TitleStyle = new GUIStyle();
        m_TitleStyle.alignment = TextAnchor.MiddleCenter;
        m_TitleStyle.fontSize = 60;
        m_TitleStyle.fontStyle = new FontStyle();
        m_TitleStyle.normal.textColor = Color.white;
    }


    void OnGUI()
    {
        if (show)
        {

            //GUIStyle style = new GUIStyle();
            //style.fontSize = 120;
            //style.alignment = TextAnchor.MiddleCenter;
            //style.normal.textColor = Color.white;
            GUI.skin = guiSkin;
            windowRect = GUI.Window(0, windowRect, DoMyWindow, "");
        }
    }



    void DoMyWindow(int windowID)
    {

        GUIStyle style = new GUIStyle();
        style.fontSize = 40;
        style.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(0, 20, 600, 100), "提示",m_TitleStyle);
        GUI.Label(new Rect(0, 100, 800, 100), "2、点击模型载入对应场景", style);

        if (GUI.Button(new Rect(450, 100, 200, 60), "关闭"))
        {
            show = false;
        }
    }

    public void ShowTips()
    {
        show = true;
    }
}
