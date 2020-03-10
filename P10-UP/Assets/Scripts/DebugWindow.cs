using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class DebugWindow : MonoBehaviour
{
    private string _dLog;
    private Queue _dLogQueue = new Queue();
    private bool active;
    public Text debugText;
    public GameObject debugPanel;
    private ScrollRect scrollRect;

    public GameObject testPrefab;

    void Awake()
    {
        //if (OVRInput.IsControllerConnected(OVRInput.Controller.RTrackedRemote))
        active = true;

        if (!active || debugText == null)
            return;

        scrollRect = debugText.gameObject.GetComponent<ScrollRect>();
        InvokeRepeating("CustomUpdate", 1.0f, 3.0f);
    }

    void OnEnable()
    {
        Application.logMessageReceived += Log;
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
        if (OVRInput.GetDown(OVRInput.Button.One))
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
        if (scrollRect.normalizedPosition.y >= 0 && scrollRect.normalizedPosition.y <= 1)
            scrollRect.normalizedPosition = new Vector2(scrollRect.normalizedPosition.x, scrollRect.normalizedPosition.y + 0.05f * Time.deltaTime);
    }
    public void ScrollToBottom()
    {
        if (scrollRect.normalizedPosition.y >= 0 && scrollRect.normalizedPosition.y <= 1)
            scrollRect.normalizedPosition = new Vector2(scrollRect.normalizedPosition.x, scrollRect.normalizedPosition.y - 0.05f * Time.deltaTime);
    }
}