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
            AndroidUtils.Toast("数据未就绪！");
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
#if UNITY_EDITOR
        Debug.Log("开始更新");
#endif
    }



    private void OnGUI()
    {
        if (m_Status == Status.READY)
        {
            return;
        }

        GUI.skin = skin;
        windowRect = GUI.Window(0, windowRect, DrawWindow, "Loading...");



    }

    Vector2 scrollPosition = Vector2.zero;
    float hSbarValue;
    float vSbarValue;

    private void DrawWindow(int id)
    {

        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity,
            new Vector3(Screen.width / 1200.0f, Screen.height / 800.0f, 1.0f));
        GUIStyle style = new GUIStyle() { fontSize = Math.Max(10, fontSize) };
        style.normal.textColor = textColor;

        scrollPosition = GUI.BeginScrollView(windowRect, scrollPosition, new Rect(0, 0, 220, 200));
        
        GUI.Label(new Rect(150, 100, windowRect.width - 20, windowRect.height - 200), _logStr, style);

        GUI.EndScrollView();
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
        AndroidUtils.Toast("更新完成！");
        yield return new WaitForSeconds(1.0f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    #endregion


    public Color textColor = Color.white;
    const int maxLines = 30;
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

}
