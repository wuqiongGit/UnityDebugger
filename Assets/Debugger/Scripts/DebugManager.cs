using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
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

    #region Data

    [SerializeField]
    DebugOptionPanel m_optionPanel;

    [SerializeField]
    DebugLogPanel m_logPanel;

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

        CreateUI();
    }

    #endregion

    #region Mono

    void OnEnable()
    {
        StartCoroutine(ProcessFPS());
        StartCoroutine(m_logPanel.ProcessAutoStart());
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

    [Header("Base")]
    [SerializeField]
    GameObject m_root;

    [SerializeField]
    Button m_btnClose;

    [SerializeField]
    List<TabGroup> m_tabList;


    [Header("Log View")]

    [Header("Info View")]
    [SerializeField]
    RawImage m_imgFPS;

    [SerializeField]
    Text m_txtFPS;

    [SerializeField, Range(0.01f, 2)]
    float m_fpsCheckTime = 0.5f;

    [SerializeField]
    Text m_txtDeviceInfo;

    [SerializeField]
    Text m_txtScreenInfo;

    Texture2D m_texture;

    public static void ShowDebugUI()
    {
        if (!m_instance) return;

        m_instance.RefreshUI();
        m_instance.m_root.SetActive(true);
    }

    public static void HideDebugUI()
    {
        if (!m_instance) return;
        m_instance.m_root.SetActive(false);
    }

    void RefreshUI()
    {
        m_optionPanel.RefreshUI();
        m_logPanel.RefreshUI();

        var sb = new StringBuilder();
        sb.Append($"Game\n");
        sb.Append($"\tPlatform\t : {Application.platform}\n");
        sb.Append($"\tIdentifier\t : {Application.identifier}\n");
        sb.Append($"\tVersion\t : {Application.version}\n");
        sb.Append($"\tUnity Version\t : {Application.unityVersion}\n");
        sb.Append($"\tIs 64Bit\t : {Environment.Is64BitProcess}\n");

        sb.Append($"System\n");
        sb.Append($"\tSystem\t : {SystemInfo.operatingSystem}\n");
        sb.Append($"\tVersion\t : {Environment.OSVersion}\n");
        sb.Append($"\tLanguage\t : {Application.systemLanguage}\n");
        sb.Append($"\tIs 64Bit\t : {Environment.Is64BitOperatingSystem}\n");

        sb.Append($"Device\n");
        sb.Append($"\tName\t : {SystemInfo.deviceName}\n");
        sb.Append($"\tType\t : {SystemInfo.deviceType}\n");
        sb.Append($"\tModel\t : {SystemInfo.deviceModel}\n");
        sb.Append($"\tMemory\t : {(float)SystemInfo.systemMemorySize / 1024:F2}GB\n");

        m_txtDeviceInfo.text = sb.ToString();
        sb = new StringBuilder();
        sb.Append($"Screen\n");
        sb.Append($"\tSize\t : {Screen.width}x{Screen.height}\n");
        sb.Append($"\tSafe Area\t : {Screen.safeArea.x},{Screen.safeArea.y},{Screen.safeArea.width},{Screen.safeArea.height}\n");
        sb.Append($"\tResolution\t : {Screen.currentResolution}\n");
        sb.Append($"\tTargetFrameRate\t : {Application.targetFrameRate}\n");
        sb.Append($"Graphics\n");
        sb.Append($"\tName\t : {SystemInfo.graphicsDeviceName}\n");
        sb.Append($"\tType\t : {SystemInfo.graphicsDeviceType}\n");
        sb.Append($"\tVendor\t : {SystemInfo.graphicsDeviceVendor}\n");
        sb.Append($"\tVersion\t : {SystemInfo.graphicsDeviceVersion}\n");
        sb.Append($"\tMemory\t : {(float)SystemInfo.graphicsMemorySize / 1024:F2}GB\n");
        sb.Append($"\tShader Level\t : {(float)SystemInfo.graphicsShaderLevel}\n");

        m_txtScreenInfo.text = sb.ToString();
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

        m_optionPanel.CreateUI();
        m_logPanel.CreateUI();


        RefreshUI();
    }

    IEnumerator ProcessFPS()
    {
        var lastFrame = Time.frameCount;
        var tw = 1024;
        var th = 128;
        var lastFps = 0f;
        var lastTimer = 0f;
        var fpsIndex = 0;
        var defaultColor = new Color(1, 1, 1, 0);
        var fpsColor = new Color(0.5f, 0.5f, 0.5f);
        var colorList = new Color[th];
        while (true)
        {
            var fps = (Time.frameCount - lastFrame) / (Time.realtimeSinceStartup - lastTimer);

            if (m_root.activeInHierarchy)
            {
                m_txtFPS.text = $"FPS : {fps:F1}";

                if (m_texture == null)
                {
                    m_texture = new Texture2D(tw, th, TextureFormat.RGBA32, true);
                    var fillColor = m_texture.GetPixels();
                    var s = tw * th;
                    for (int i = 0; i < s; i++) fillColor[i] = defaultColor;
                    m_texture.SetPixels(fillColor);
                    m_texture.Apply();
                    m_imgFPS.texture = m_texture;
                }

                var t1 = Mathf.Clamp((int)(lastFps / 60 * th / 2), 0, th - 1);
                var t2 = Mathf.Clamp((int)(fps / 60 * th / 2), 0, th - 1);
                if (t1 > t2)
                {
                    var t = t2;
                    t2 = t1;
                    t1 = t;
                }

                for (int i = 0; i < t1; i++) colorList[i] = defaultColor;
                for (int i = th - 1; i > t2; i--) colorList[i] = defaultColor;
                for (int i = t1; i <= t2; i++) colorList[i] = fpsColor;

                if (fpsIndex >= tw) fpsIndex = 0;

                m_texture.SetPixels(fpsIndex, 0, 1, th, colorList);
                m_texture.Apply();

                fpsIndex++;

                m_imgFPS.uvRect = new Rect(((float)fpsIndex / tw - 1), 0, 1, 1);
            }

            lastFrame = Time.frameCount;
            lastTimer = Time.realtimeSinceStartup;
            lastFps = fps;

            yield return new WaitForSeconds(m_fpsCheckTime);
        }
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
