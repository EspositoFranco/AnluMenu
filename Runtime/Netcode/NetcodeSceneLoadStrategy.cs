using AnluMenu;
using Unity.Netcode;
using UnityEngine.SceneManagement;

namespace AnluMenu.Netcode
{
    /// <summary>
    /// NGO-aware scene load strategy. When the local peer is the active server in a
    /// running NetworkManager session, scene loads are delegated to the network
    /// scene manager so all connected clients load the same scene in lockstep.
    /// </summary>
    /// <remarks>
    /// Clients never trigger a load directly through this strategy — they receive
    /// the load command from the server via NGO and Unity handles their loading
    /// transition automatically.
    /// </remarks>
    public sealed class NetcodeSceneLoadStrategy : ISceneLoadStrategy
    {
        public bool ShouldLoadNetworked()
        {
            var nm = NetworkManager.Singleton;
            return nm != null && nm.IsListening && nm.IsServer;
        }

        public void LoadNetworked(string sceneName)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }
}
