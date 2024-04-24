using Holo.Data;
using Holo.HUR;
using Holo.XR.Android;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// 热更数据管理
/// </summary>
public class HotFixDataMgr : MonoBehaviour
{
    public DllLoader dllLoader;
    public DataDownLoader dataDownLoader;

    [Header("GUI")]
    public GUISkin skin;

    private Status m_Status = Status.LOADING;
    private Rect windowRect = new Rect(0, 0, Screen.width - 60, Screen.height - 60);

    public PanelManager m_PanelManager;

    private enum Status
    {
        /// <summary>
        /// 更新中
        /// </summary>
        UPDATING,

        /// <summary>
        /// 加载中
        /// </summary>
        LOADING,

        /// <summary>
        /// 就绪
        /// </summary>
        READY
    }

    private void Awake()
    {
        //自动读取数据标记为true
        dllLoader.autoReadData = true;
        //在数据更新完成后，重启这个场景
        dataDownLoader.updateDataCompleted.AddListener(ReloadScene);
        dllLoader.loadComplete.AddListener(LoadDllCompleted);

        //数据处理失败时触发
        dataDownLoader.OnError += HandleError;
        dllLoader.OnError += HandleError;

        //进度更新
        dllLoader.OnProgressUpdate += HandleProgressFromDllLoader;
        dataDownLoader.OnProgressUpdate += HandleProgressFromDataDownLoader;
    }

    private void HandleProgressFromDataDownLoader(float progress)
    {
        int pro = (int)(progress * 100);
        Debug.Log("数据同步进度：  " + pro + " %");
    }

    private void HandleProgressFromDllLoader(float progress)
    {
        int pro = (int)(progress * 100);
        Debug.Log("数据加载进度：  " + pro + " %");
    }

    private void Start()
    {
        windowRect.x = (Screen.width - windowRect.width) / 2;
        windowRect.y = (Screen.height - windowRect.height) / 2;
    }

    /// <summary>
    /// 启动游戏
    /// </summary>
    public void StartGame()
    {
        if(m_Status != Status.READY)
        {
            AndroidUtils.Toast("数据加载中，请稍候...");
            return;
        }

        //获取入口场景名称
        string mainSceneName = dllLoader.getEntrance();
        if (mainSceneName == null)
        {
            AndroidUtils.Toast("请先更新数据，点击“更新”");
            return;
        }

        SceneManager.LoadSceneAsync(mainSceneName, LoadSceneMode.Single);
    }

    /// <summary>
    /// 更新数据
    /// </summary>
    public void UpdateData()
    {
        if (m_Status != Status.READY)
        {
            AndroidUtils.Toast("程序未就绪，请稍后再试！");
            return;
        }

        _logStr = "";
        if (dataDownLoader != null)
        {
            m_Status = Status.UPDATING;//开始更新，切换为更新状态
            if (m_PanelManager)
            {
                m_PanelManager.CloseCurrent();
            }
            //开始数据下载
            dataDownLoader.StartDownload();
        }

        Debug.Log("开始更新");

    }


    #region 内部方法
    private void LoadDllCompleted()
    {
        m_Status = Status.READY;//数据加载完成，切换为READY
        Debug.Log("数据加载完成！");
    }

    /// <summary>
    /// 处理错误
    /// </summary>
    /// <param name="message"></param>
    private void HandleError(string message)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            AndroidUtils.Toast("数据版本校验失败");
        }
#if DEBUG_LOG
        Debug.Log("数据版本校验失败");
#endif
        m_Status = Status.READY;
    }

    // 重新加载当前场景
    private void ReloadScene()
    {
        m_Status = Status.READY;
        StartCoroutine(ReloadThisScene());
    }

    private IEnumerator ReloadThisScene()
    {
        AndroidUtils.Toast("更新完成，重启中...");
        yield return new WaitForSeconds(1.0f);
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        //改用直接重启应用的方式，因为AB包和dll无法在一个进程中重复添加，
        //因此即使场景重启，数据也未重新载入。因此改用直接重启安卓应用
        AndroidUtils.RestartApplication();
    }
    #endregion

    #region Log Info
    public Color textColor = Color.white;
    const int maxLines = 3;
    const int maxLineLength = 50;
    private string _logStr = "";

    private readonly List<string> _lines = new List<string>();

    public int fontSize = 15;

    void OnEnable() { Application.logMessageReceived += Log; }
    void OnDisable() { Application.logMessageReceived -= Log; }

    public void Log(string logString, string stackTrace, LogType type)
    {
        if (type != LogType.Log)
        {
            return;
        }
        foreach (var line in logString.Split('\n'))
        {
            if (line.Length <= maxLineLength)
            {
                _lines.Add(line);
                continue;
            }
            var lineCount = line.Length / maxLineLength + 1;
            for (int i = 0; i < lineCount; i++)
            {
                if ((i + 1) * maxLineLength <= line.Length)
                {
                    _lines.Add(line.Substring(i * maxLineLength, maxLineLength));
                }
                else
                {
                    _lines.Add(line.Substring(i * maxLineLength, line.Length - i * maxLineLength));
                }
            }
        }
        if (_lines.Count > maxLines)
        {
            _lines.RemoveRange(0, _lines.Count - maxLines);
        }
        _logStr = string.Join("\n", _lines);
    }


    private void OnGUI()
    {
        if (m_Status == Status.READY)
        {
            return;
        }

        GUI.skin = skin;
        windowRect = GUI.Window(0, windowRect, DrawWindow, "");

    }

    private void DrawWindow(int id)
    {

        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity,
            new Vector3(Screen.width / 1200.0f, Screen.height / 800.0f, 1.0f));
        GUIStyle style = new GUIStyle() { fontSize = Math.Max(10, fontSize) };
        style.normal.textColor = textColor;


        GUI.Label(new Rect(150, 100, windowRect.width - 20, windowRect.height - 200), _logStr, style);
    }
    #endregion
}
