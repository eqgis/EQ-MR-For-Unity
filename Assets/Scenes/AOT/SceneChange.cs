using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    public float fadeSpeed = 2.0f; // �����ٶ�
    public Color fadeColor = new Color(0, 0, 0, 1); // ������ɫ����ʼ͸����

    // ��ʼ��������
    public void LoadYourScene(string sceneName)
    {
        StartCoroutine(LoadScene(sceneName));
    }

    // ��������Э��
    IEnumerator LoadScene(string sceneName)
    {
        // ����
        fadeColor.a = 1;
        while (fadeColor.a > 0)
        {
            fadeColor.a -= Time.deltaTime / fadeSpeed;
            //Graphics.camera.backgroundColor = fadeColor;
            yield return null;
        }

        // �����³���
        SceneManager.LoadScene(sceneName);
    }
}
