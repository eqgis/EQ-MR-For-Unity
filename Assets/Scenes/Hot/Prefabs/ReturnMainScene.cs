using Holo.XR.Android;
using Holo.XR.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �����������ű�
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
        //��100ms
        AndroidUtils.vibrate(50);

        if (mExSceneTransition != null)
        {
            mExSceneTransition.LoadScene();
        }
    }
}
