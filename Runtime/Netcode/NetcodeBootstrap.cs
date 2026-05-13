using AnluMenu;
using UnityEngine;

namespace AnluMenu.Netcode
{
    /// <summary>
    /// Auto-registers <see cref="NetcodeSceneLoadStrategy"/> as the active strategy
    /// before the first scene loads. Only compiled when the NGO package is present
    /// (see Menu.Netcode.asmdef versionDefines).
    /// </summary>
    public static class NetcodeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterStrategy()
        {
            SceneLoader.Strategy = new NetcodeSceneLoadStrategy();
        }
    }
}
