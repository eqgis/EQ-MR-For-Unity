using Holo.XR.Android;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 提示弹窗
/// </summary>
public class TipsUI : MonoBehaviour
{
    //由于.SetActive(false);后，场景中无法找到这个对象，因此这里采用static
    //也可通过父物体或其他物体上的脚本开关active
    private static GameObject instance;
    public TextMeshProUGUI m_Content;

    public Button m_CloseButton;


    void Awake()
    {
        m_CloseButton.onClick.AddListener(/*关闭弹窗提示*/CloseTips);
        instance = this.gameObject;
    }

    public void CloseTips()
    {
        AndroidUtils.vibrate(30);
        if (this.gameObject.activeSelf)
        {
            this.gameObject.SetActive(false);
        }
        //this.transform.GetChild(0).gameObject.SetActive(false);
    }


    public void ShowTips(string tips)
    {
        instance.SetActive(true);
        m_Content.text = tips;
    }    

}
