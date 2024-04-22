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
            windowRect = GUI.Window(0, windowRect, DoMyWindow, "��ʾ");
        }
    }



    void DoMyWindow(int windowID)
    {

        GUI.Label(new Rect(40, 40, 200, 40), "1���ƶ��豸����ͼƬʶ��");
        GUI.Label(new Rect(40, 90, 200, 40), "2�����ģ�������Ӧ����");
        
        if (GUI.Button(new Rect(60, 140, 200, 60), "ȷ��"))
        {
            show = false;
        }
    }

    public void ShowTips()
    {
        show = true;
    }
}
