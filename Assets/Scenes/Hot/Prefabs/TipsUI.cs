using Holo.XR.Android;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ��ʾ����
/// </summary>
public class TipsUI : MonoBehaviour
{
    //����.SetActive(false);�󣬳������޷��ҵ������������������static
    //Ҳ��ͨ������������������ϵĽű�����active
    private static GameObject instance;
    public TextMeshProUGUI m_Content;

    public Button m_CloseButton;


    void Awake()
    {
        m_CloseButton.onClick.AddListener(/*�رյ�����ʾ*/CloseTips);
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
