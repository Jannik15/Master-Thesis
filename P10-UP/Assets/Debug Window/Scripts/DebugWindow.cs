using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class DebugWindow : MonoBehaviour
{
    private string _dLog;
    private Queue _dLogQueue = new Queue();
    public Text debugText;
    public GameObject debugPanel;
    public ScrollRect scrollRect;

    void Awake()
    {
        Application.logMessageReceived += Log;
    }

    void Start()
    {
        if (debugText == null)
            return;

        if (scrollRect == null)
            scrollRect = debugText.GetComponentInParent<ScrollRect>();

        InvokeRepeating("CustomUpdate", 0.4f, 3.0f);
    }

    void OnDisable()
    {
        Application.logMessageReceived -= Log;
    }

    void Log(string logString, string stackTrace, LogType type)
    {
        _dLog = logString;
        string newString = "\n [" + type + "] : " + _dLog;
        _dLogQueue.Enqueue(newString);
        if (type == LogType.Exception)
        {
            newString = "\n" + stackTrace;
            _dLogQueue.Enqueue(newString);
        }
        _dLog = string.Empty;
        foreach (string dLog in _dLogQueue)
        {
            _dLog += dLog;
        }
    }

    private void CustomUpdate()
    {
        debugText.text = _dLog;
    }

    private void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.Four))
        {
            if (debugPanel != null)
                debugPanel.SetActive(!debugPanel.activeSelf);
        }

        if (OVRInput.GetDown(OVRInput.Button.SecondaryThumbstickUp))
            ScrollToTop();
        else if (OVRInput.GetDown(OVRInput.Button.SecondaryThumbstickDown))
            ScrollToBottom();
    }

    public void ScrollToTop()
    {
        if (scrollRect.verticalNormalizedPosition <= 1.0f)
            scrollRect.verticalNormalizedPosition += 0.02f;
    }
    public void ScrollToBottom()
    {
        if (scrollRect.verticalNormalizedPosition >= 0.0f)
            scrollRect.verticalNormalizedPosition -= 0.02f;
    }
}