using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDebugPanel
{
    IEnumerator ProcessInit();
    IEnumerator ProcessAutoStart();

    void RefreshUI();
    void CreateUI();
}
