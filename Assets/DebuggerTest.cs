using System;
using UnityEngine;
using ZPlugins;

public class DebuggerTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        DebugManager.Init();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log($"{UnityEngine.Random.value}");
    }

    [DebugMethod(DebugMethodAttribute.ParamType.String, "DebugLog")]
    public static void DebugLog(string v)
    {
        Debug.Log(v);
    }

    [DebugMethod(DebugMethodAttribute.ParamType.String, "DebugWarning")]
    public static void DebugWarning(string v)
    {
        Debug.LogWarning(v);
    }

    [DebugMethod(DebugMethodAttribute.ParamType.String, "DebugException")]
    public static void DebugException(string v)
    {
        Debug.LogException(new Exception(v));
    }

    [DebugMethod(DebugMethodAttribute.ParamType.String, "DebugAssert")]
    public static void DebugAssert(string v)
    {
        Debug.Assert(false, v);
    }

    [DebugMethod(DebugMethodAttribute.ParamType.String, "DebugError")]
    public static void DebugError(string v)
    {
        Debug.LogError(v);
    }
}
