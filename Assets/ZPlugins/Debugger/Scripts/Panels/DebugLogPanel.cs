using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace ZPlugins
{
    [Serializable]
    public class DebugLogPanel : IDebugPanel
    {
        const int DISPLAY_MAX = 65000 / 4 - 2;
        const int RAW_MAX = 50000;

        class LogData
        {
            public string log;
            public string logRaw;
            public LogType type;
            public int length;
        }

        [SerializeField]
        Text m_txtLog;

        [SerializeField]
        Button m_btnClearFilter;

        [SerializeField]
        InputField m_txtFilter;

        [SerializeField]
        Toggle m_checkLogFilter;

        [SerializeField]
        Toggle m_checkWarningFilter;

        [SerializeField]
        Toggle m_checkExceptionFilter;

        [SerializeField]
        Toggle m_checkAssertFilter;

        [SerializeField]
        Toggle m_checkErrorFilter;

        [SerializeField]
        Button m_btnClearLog;

        [SerializeField]
        Button m_btnCopyLog;

        List<LogData> m_logData;

        StringBuilder m_logList;
        StringBuilder m_logRawList;

        bool m_filterDirty;
        bool m_logDirty;

        public void CreateUI()
        {
            m_btnClearLog.onClick.RemoveAllListeners();
            m_btnCopyLog.onClick.RemoveAllListeners();

            m_btnClearFilter.onClick.RemoveAllListeners();
            m_txtFilter.onValueChanged.RemoveAllListeners();

            m_checkLogFilter.onValueChanged.RemoveAllListeners();
            m_checkWarningFilter.onValueChanged.RemoveAllListeners();
            m_checkExceptionFilter.onValueChanged.RemoveAllListeners();
            m_checkAssertFilter.onValueChanged.RemoveAllListeners();
            m_checkErrorFilter.onValueChanged.RemoveAllListeners();

            m_btnClearLog.onClick.AddListener(ClearLog);
            m_btnCopyLog.onClick.AddListener(() => GUIUtility.systemCopyBuffer = GetLogText());

            m_btnClearFilter.onClick.AddListener(() => m_txtFilter.text = "");
            m_txtFilter.onValueChanged.AddListener(v => m_filterDirty = true);

            m_checkLogFilter.onValueChanged.AddListener(v => m_filterDirty = true);
            m_checkWarningFilter.onValueChanged.AddListener(v => m_filterDirty = true);
            m_checkExceptionFilter.onValueChanged.AddListener(v => m_filterDirty = true);
            m_checkAssertFilter.onValueChanged.AddListener(v => m_filterDirty = true);
            m_checkErrorFilter.onValueChanged.AddListener(v => m_filterDirty = true);

            m_logDirty = true;
            m_filterDirty = true;
        }

        public IEnumerator ProcessInit()
        {
            m_logData = new List<LogData>();

            m_logList = new StringBuilder();
            m_logRawList = new StringBuilder();

            Application.logMessageReceived += OnLogMessageReceived;

            yield return null;
        }

        public IEnumerator ProcessAutoStart()
        {
            while (true)
            {
                if (m_txtLog.gameObject.activeInHierarchy)
                {
                    if (m_filterDirty)
                    {
                        UpdateFilter();
                    }

                    if (m_logDirty)
                    {
                        UpdateLog();
                    }
                }

                yield return null;
            }
        }

        public void RefreshUI()
        {
            m_logDirty = true;
        }

        void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            var s = $"[{type}] {condition}";
            var s2 = "";

            if (type != LogType.Log)
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

            var data = new LogData();
            data.type = type;
            data.log = s;
            data.logRaw = s2;
            data.length = s2.Length;

            m_logData.Add(data);

            if (IsLogDisplayable(data))
            {
                m_logList.AppendLine(data.log);
                m_logRawList.AppendLine(data.logRaw);
            }

            m_logDirty = true;
        }

        string GetLogText()
        {
            return m_logRawList.ToString();
        }

        void ClearLog()
        {
            m_logData.Clear();

            m_logRawList.Clear();
            m_logList.Clear();

            m_txtLog.text = "";
        }

        void UpdateLog()
        {
            if (m_logList.Length > DISPLAY_MAX)
            {
                m_logList.Remove(0, m_logList.Length - DISPLAY_MAX);
            }
            if (m_logRawList.Length > RAW_MAX)
            {
                m_logRawList.Remove(0, m_logRawList.Length - RAW_MAX);
            }
            m_txtLog.text = m_logList.ToString() + "\n\n";

            m_txtLog.rectTransform.anchoredPosition = new Vector2(m_txtLog.rectTransform.anchoredPosition.x, m_txtLog.preferredHeight);
            m_logDirty = false;
        }

        void UpdateFilter()
        {
            m_logRawList.Clear();
            m_logList.Clear();

            foreach (var data in m_logData)
            {
                if (IsLogDisplayable(data))
                {
                    m_logList.AppendLine(data.log);
                    m_logRawList.AppendLine(data.logRaw);
                }
            }

            m_logDirty = true;
            m_filterDirty = false;
        }

        bool IsLogDisplayable(LogData data)
        {
            if (data == null) return false;

            switch (data.type)
            {
                case LogType.Log:
                    if (!m_checkLogFilter.isOn) return false;
                    break;
                case LogType.Warning:
                    if (!m_checkWarningFilter.isOn) return false;
                    break;
                case LogType.Exception:
                    if (!m_checkExceptionFilter.isOn) return false;
                    break;
                case LogType.Assert:
                    if (!m_checkAssertFilter.isOn) return false;
                    break;
                case LogType.Error:
                    if (!m_checkErrorFilter.isOn) return false;
                    break;
            }

            if (!string.IsNullOrEmpty(m_txtFilter.text))
            {
                if (!Regex.IsMatch(data.logRaw, m_txtFilter.text, RegexOptions.IgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }
    }
}