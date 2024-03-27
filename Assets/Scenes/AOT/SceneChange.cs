using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    public float fadeSpeed = 2.0f; // 渐变速度
    public Color fadeColor = new Color(0, 0, 0, 1); // 渐变颜色和起始透明度

    // 开始场景加载
    public void LoadYourScene(string sceneName)
    {
        StartCoroutine(LoadScene(sceneName));
    }

    // 场景加载协程
    IEnumerator LoadScene(string sceneName)
    {
        // 淡出
        fadeColor.a = 1;
        while (fadeColor.a > 0)
        {
            fadeColor.a -= Time.deltaTime / fadeSpeed;
            //Graphics.camera.backgroundColor = fadeColor;
            yield return null;
        }

        // 加载新场景
        SceneManager.LoadScene(sceneName);
    }
}
