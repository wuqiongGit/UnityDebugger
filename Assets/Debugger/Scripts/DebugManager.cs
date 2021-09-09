using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DebugManager : MonoBehaviour
{
    #region Init

    static DebugManager m_instance;

    //    [RuntimeInitializeOnLoadMethod]
    //    static void AutoInit()
    //    {
    //#if DEBUG || UNITY_EDITOR
    //        Init();
    //#endif
    //    }

    public static void Init()
    {
        if (m_instance) return;
        var dm = Resources.Load<DebugManager>("DebugManager");
        if (dm)
        {
            m_instance = Instantiate(dm);
            m_instance.name = "DebugManager";
            DontDestroyOnLoad(m_instance.gameObject);
        }
        else
        {
            Debug.LogWarning("DebugManager Load Failed!");
        }

        if (EventSystem.current == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }
    }

    #endregion

    #region Mono

    void Awake()
    {
        if (m_instance != null && m_instance != this)
        {
            if (Application.isPlaying) Destroy(gameObject);
            else DestroyImmediate(gameObject);

            return;
        }

        m_instance = this;
        DontDestroyOnLoad(gameObject);
    }

    IEnumerator Start()
    {
        yield return m_logPanel.ProcessInit();
        yield return m_optionPanel.ProcessInit();
        yield return m_detailPanel.ProcessInit();

        m_panels = new List<IDebugPanel>();
        m_panels.Add(m_logPanel);
        m_panels.Add(m_optionPanel);
        m_panels.Add(m_detailPanel);

        CreateUI();
    }

    void OnEnable()
    {
        if (m_panels != null)
        {
            foreach (var panel in m_panels)
            {
                StartCoroutine(panel.ProcessAutoStart());
            }
        }
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.D) && (Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.LeftControl)))
        {
            ShowDebugUI();
        }
#elif UNITY_IOS || UNITY_ANDROID
        if (Input.touchSupported && Input.touchCount > 3)
        {
            ShowDebugUI();
        }
#endif
    }

    #endregion

    #region GUI
    [Serializable]
    public class TabGroup
    {
        public Button tab;
        public GameObject view;
    }

    [SerializeField]
    GameObject m_root;

    [SerializeField]
    RectTransform m_safeArea;

    [SerializeField]
    Button m_btnClose;

    [SerializeField]
    List<TabGroup> m_tabList;

    [Header("Panels")]
    [SerializeField]
    DebugOptionPanel m_optionPanel;

    [SerializeField]
    DebugLogPanel m_logPanel;

    [SerializeField]
    DebugDetailPanel m_detailPanel;

    List<IDebugPanel> m_panels;

    public static bool IsActive()
    {
        return m_instance && m_instance.m_root && m_instance.m_root.activeInHierarchy;
    }

    public static void ShowDebugUI()
    {
        if (!m_instance) return;

        m_instance.RefreshUI();
        m_instance.m_root.SetActive(true);

        var fullSize = m_instance.m_root.GetComponent<RectTransform>().sizeDelta;

        var safeArea = Screen.safeArea;
        //safeArea.x += 80;
        //safeArea.width -= 80;

        var posX = Mathf.Round(fullSize.x * safeArea.position.x / Screen.width);
        var posW = fullSize.x * safeArea.size.x / Screen.width;
        var posY = Mathf.Round(fullSize.y * safeArea.position.y / Screen.height);
        var posH = fullSize.y * safeArea.size.y / Screen.height;

        m_instance.m_safeArea.anchorMin = Vector2.zero;
        m_instance.m_safeArea.anchorMax = Vector2.one;

        var deltaX = Mathf.Round(fullSize.x - posW);
        var deltaY = Mathf.Round(fullSize.y - posH);

        m_instance.m_safeArea.anchoredPosition = new Vector2(posX - deltaX / 2, posY - deltaY / 2);
        m_instance.m_safeArea.sizeDelta = new Vector2(-deltaX, -deltaY);
    }

    public static void HideDebugUI()
    {
        if (!m_instance) return;
        m_instance.m_root.SetActive(false);
    }

    void RefreshUI()
    {
        foreach (var panel in m_panels)
        {
            panel.RefreshUI();
        }
    }

    void CreateUI()
    {
        {
            m_btnClose.onClick.RemoveAllListeners();
            m_btnClose.onClick.AddListener(HideDebugUI);

            foreach (var tabData in m_tabList)
            {
                tabData.tab.onClick.RemoveAllListeners();
                tabData.tab.onClick.AddListener(() => ShowTabContent(tabData.view));
            }
            m_tabList[0].tab.onClick.Invoke();
        }

        foreach (var panel in m_panels)
        {
            panel.CreateUI();
            StartCoroutine(panel.ProcessAutoStart());
        }

        RefreshUI();
    }

    void ShowTabContent(GameObject view)
    {
        foreach (var tabData in m_tabList)
        {
            tabData.tab.interactable = view != tabData.view;
            tabData.view.SetActive(view == tabData.view);
        }
    }

    #endregion
}
