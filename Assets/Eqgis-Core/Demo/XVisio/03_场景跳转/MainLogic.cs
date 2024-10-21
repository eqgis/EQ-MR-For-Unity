using Holo.XR.Android;
using Holo.XR.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainLogic : MonoBehaviour
{
    public ExSceneTransition jumpSceneController;
    // Start is called before the first frame update
    void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        AndroidUtils.Toast("当前场景:"+sceneName);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        AndroidUtils.Toast("场景切换");
        jumpSceneController.LoadScene();
    }
}
