using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class DebugDetailPanel : IDebugPanel
{
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

    public IEnumerator ProcessInit()
    {
        yield return null;
    }

    public IEnumerator ProcessAutoStart()
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

            if (DebugManager.IsActive())
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

    public void CreateUI()
    {
    }

    public void RefreshUI()
    {
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
}
