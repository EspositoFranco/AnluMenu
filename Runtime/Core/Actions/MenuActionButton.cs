using UnityEngine;

namespace AnluMenu
{
    /// <summary>
    /// One component, many menu actions. Drop on a button, pick the action in the
    /// inspector, wire the button's OnClick to <see cref="Trigger"/>. Avoids per-button
    /// glue code and keeps every menu action discoverable from a single dropdown.
    /// </summary>
    /// <remarks>
    /// For confirmation popups (e.g. "Are you sure you want to quit?"), pair this
    /// with a <see cref="ConfirmAction"/> on the same button: wire OnClick to
    /// ConfirmAction.Trigger and route ConfirmAction.OnConfirmed to this Trigger.
    /// </remarks>
    public class MenuActionButton : MonoBehaviour
    {
        public enum ActionType
        {
            Quit,
            LoadScene,
            RestartCurrentScene,
            OpenSettings,
            CloseSettings,
            ToggleSettings,
            Pause,
            Resume,
            TogglePause
        }

        [Tooltip("Which menu action this button triggers.")]
        [SerializeField] private ActionType _action;

        [Tooltip("Scene name. Used only when Action is LoadScene.")]
        [SerializeField] private string _sceneName;

        /// <summary>Executes the configured action. Wire to UnityEvent (Button.OnClick).</summary>
        public void Trigger()
        {
            switch (_action)
            {
                case ActionType.Quit:                MenuActions.Quit(); break;
                case ActionType.LoadScene:           MenuActions.LoadScene(_sceneName); break;
                case ActionType.RestartCurrentScene: MenuActions.RestartCurrentScene(); break;
                case ActionType.OpenSettings:        MenuEvents.RaiseOpenSettings(); break;
                case ActionType.CloseSettings:       MenuEvents.RaiseCloseSettings(); break;
                case ActionType.ToggleSettings:      MenuEvents.RaiseToggleSettings(); break;
                case ActionType.Pause:               MenuEvents.RaisePause(); break;
                case ActionType.Resume:              MenuEvents.RaiseResume(); break;
                case ActionType.TogglePause:         MenuEvents.RaiseTogglePause(); break;
            }
        }
    }
}
