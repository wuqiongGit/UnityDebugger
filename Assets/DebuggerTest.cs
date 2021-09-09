using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    }

    [DebugMethod(DebugMethodAttribute.ParamType.Float, "0")]
    public static void DebugTest(float v)
    {
        Debug.Log(v);
    }

}
