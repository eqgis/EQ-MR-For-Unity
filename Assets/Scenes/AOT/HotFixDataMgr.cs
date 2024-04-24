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
/// �ȸ����ݹ���
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
        /// ������
        /// </summary>
        UPDATING,

        /// <summary>
        /// ������
        /// </summary>
        LOADING,

        /// <summary>
        /// ����
        /// </summary>
        READY
    }

    private void Awake()
    {
        //�Զ���ȡ���ݱ��Ϊtrue
        dllLoader.autoReadData = true;
        //�����ݸ�����ɺ������������
        dataDownLoader.updateDataCompleted.AddListener(ReloadScene);
        dllLoader.loadComplete.AddListener(LoadDllCompleted);

        //���ݴ���ʧ��ʱ����
        dataDownLoader.OnError += HandleError;
        dllLoader.OnError += HandleError;

        //���ȸ���
        dllLoader.OnProgressUpdate += HandleProgressFromDllLoader;
        dataDownLoader.OnProgressUpdate += HandleProgressFromDataDownLoader;
    }

    private void HandleProgressFromDataDownLoader(float progress)
    {
        int pro = (int)(progress * 100);
        Debug.Log("����ͬ�����ȣ�  " + pro + " %");
    }

    private void HandleProgressFromDllLoader(float progress)
    {
        int pro = (int)(progress * 100);
        Debug.Log("���ݼ��ؽ��ȣ�  " + pro + " %");
    }

    private void Start()
    {
        windowRect.x = (Screen.width - windowRect.width) / 2;
        windowRect.y = (Screen.height - windowRect.height) / 2;
    }

    /// <summary>
    /// ������Ϸ
    /// </summary>
    public void StartGame()
    {
        if(m_Status != Status.READY)
        {
            AndroidUtils.Toast("���ݼ����У����Ժ�...");
            return;
        }

        //��ȡ��ڳ�������
        string mainSceneName = dllLoader.getEntrance();
        if (mainSceneName == null)
        {
            AndroidUtils.Toast("���ȸ������ݣ���������¡�");
            return;
        }

        SceneManager.LoadSceneAsync(mainSceneName, LoadSceneMode.Single);
    }

    /// <summary>
    /// ��������
    /// </summary>
    public void UpdateData()
    {
        if (m_Status != Status.READY)
        {
            AndroidUtils.Toast("����δ���������Ժ����ԣ�");
            return;
        }

        _logStr = "";
        if (dataDownLoader != null)
        {
            m_Status = Status.UPDATING;//��ʼ���£��л�Ϊ����״̬
            if (m_PanelManager)
            {
                m_PanelManager.CloseCurrent();
            }
            //��ʼ��������
            dataDownLoader.StartDownload();
        }

        Debug.Log("��ʼ����");

    }


    #region �ڲ�����
    private void LoadDllCompleted()
    {
        m_Status = Status.READY;//���ݼ�����ɣ��л�ΪREADY
        Debug.Log("���ݼ�����ɣ�");
    }

    /// <summary>
    /// �������
    /// </summary>
    /// <param name="message"></param>
    private void HandleError(string message)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            AndroidUtils.Toast("���ݰ汾У��ʧ��");
        }
#if DEBUG_LOG
        Debug.Log("���ݰ汾У��ʧ��");
#endif
        m_Status = Status.READY;
    }

    // ���¼��ص�ǰ����
    private void ReloadScene()
    {
        m_Status = Status.READY;
        StartCoroutine(ReloadThisScene());
    }

    private IEnumerator ReloadThisScene()
    {
        AndroidUtils.Toast("������ɣ�������...");
        yield return new WaitForSeconds(1.0f);
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        //����ֱ������Ӧ�õķ�ʽ����ΪAB����dll�޷���һ���������ظ���ӣ�
        //��˼�ʹ��������������Ҳδ�������롣��˸���ֱ��������׿Ӧ��
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
