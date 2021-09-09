using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class DebugLogPanel : IDebugPanel
{
    [SerializeField]
    Text m_txtLog;

    [SerializeField]
    Button m_btnClearLog;

    [SerializeField]
    Button m_btnCopyLog;

    StringBuilder m_logList;
    StringBuilder m_logRawList;
    bool m_logDirty;

    public void CreateUI()
    {
        m_btnClearLog.onClick.RemoveAllListeners();
        m_btnCopyLog.onClick.RemoveAllListeners();

        m_btnClearLog.onClick.AddListener(() => { m_logList.Clear(); m_txtLog.text = ""; });
        m_btnCopyLog.onClick.AddListener(() => { GUIUtility.systemCopyBuffer = m_logRawList.ToString(); });
    }

    public IEnumerator ProcessInit()
    {
        m_logList = new StringBuilder();
        m_logRawList = new StringBuilder();
        Application.logMessageReceived += logMessageReceived;
        yield return null;
    }

    public IEnumerator ProcessAutoStart()
    {
        var displayMax = 65000 / 4 - 2;
        var rawMax = 50000;
        while (true)
        {
            if (m_logDirty && m_txtLog.gameObject.activeInHierarchy)
            {
                if (m_logList.Length > displayMax)
                {
                    m_logList.Remove(0, m_logList.Length - displayMax);
                }
                if (m_logRawList.Length > rawMax)
                {
                    m_logRawList.Remove(0, m_logRawList.Length - rawMax);
                }
                m_txtLog.text = m_logList.ToString() + "\n\n";

                m_txtLog.rectTransform.anchoredPosition = new Vector2(m_txtLog.rectTransform.anchoredPosition.x, m_txtLog.preferredHeight);
                m_logDirty = false;
            }

            yield return null;
        }
    }

    public void RefreshUI()
    {
        m_logDirty = true;
    }

    void logMessageReceived(string condition, string stackTrace, LogType type)
    {
        var s = $"[{type}] {condition}";
        var s2 = "";
        if (!type.HasFlag(LogType.Log))
        {
            stackTrace = stackTrace.TrimEnd('\r', '\n', '\t', ' ');
            stackTrace = stackTrace.Replace("\n", "\n\t");
            var color = "#FF0000";
            switch (type)
            {
                case LogType.Warning:
                case LogType.Exception:
                    color = "#669900";
                    break;
                case LogType.Assert:
                    color = "#000000";
                    break;
            }
            s2 = $"{s}\n\t{stackTrace}";
            s = $"<color={color}><b>{s}</b></color>\n\t<color=#999999><i>{stackTrace}</i></color>";
        }
        else
        {
            s2 = s;
        }

        m_logList.AppendLine(s);
        m_logRawList.AppendLine(s2);
        m_logDirty = true;
    }
}
