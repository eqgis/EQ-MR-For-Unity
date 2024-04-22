using Holo.XR.Android;
using Holo.XR.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 返回主场景脚本
/// </summary>
public class ReturnMainScene : MonoBehaviour
{

    private ExSceneTransition mExSceneTransition;

    void Awake()
    {
        mExSceneTransition = GetComponent<ExSceneTransition>();
    }

    public void exec()
    {
        //震动100ms
        AndroidUtils.vibrate(50);

        if (mExSceneTransition != null)
        {
            mExSceneTransition.LoadScene();
        }
    }
}
