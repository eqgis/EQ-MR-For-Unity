using Holo.XR.Android;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectClick : MonoBehaviour
{
    // Start is called before the first frame update
    public void OnClick()
    {
        AndroidUtils.Toast("µ„÷–¡À\'"+this.name+"\'");
    }
}
