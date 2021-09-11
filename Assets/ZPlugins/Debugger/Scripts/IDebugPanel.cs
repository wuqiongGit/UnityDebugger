using System.Collections;

namespace ZPlugins
{
    public interface IDebugPanel
    {
        IEnumerator ProcessInit();
        IEnumerator ProcessAutoStart();

        void RefreshUI();
        void CreateUI();
    }
}