using UnityEngine;
using UnityEngine.Events;

namespace AnluMenu
{
    /// <summary>
    /// Drop on a button to prepend a confirmation popup before the real action.
    /// Wire the button's OnClick to <see cref="Trigger"/>, then put the real action
    /// in <see cref="OnConfirmed"/>. Skips the dialog (and runs OnConfirmed directly)
    /// if no <see cref="ConfirmationDialog"/> instance is present in the scene.
    /// </summary>
    public class ConfirmAction : MonoBehaviour
    {
        [Header("Dialog Content")]
        [SerializeField] private string _title = "Confirm";
        [TextArea(2, 4)]
        [SerializeField] private string _message = "Are you sure?";
        [SerializeField] private string _confirmText = "Yes";
        [SerializeField] private string _cancelText = "No";

        [Header("Callbacks")]
        [Tooltip("Fired after the player confirms.")]
        [SerializeField] private UnityEvent _onConfirmed;

        [Tooltip("Fired after the player cancels (optional).")]
        [SerializeField] private UnityEvent _onCancelled;

        /// <summary>Opens the confirmation dialog. Wire to UnityEvent (Button.OnClick).</summary>
        public void Trigger()
        {
            if (ConfirmationDialog.Instance == null)
            {
                Debug.LogWarning("[ConfirmAction] No ConfirmationDialog.Instance in scene. Running OnConfirmed directly.");
                _onConfirmed?.Invoke();
                return;
            }

            ConfirmationDialog.Instance.Show(
                title: _title,
                message: _message,
                onConfirm: () => _onConfirmed?.Invoke(),
                onCancel:  () => _onCancelled?.Invoke(),
                confirmText: _confirmText,
                cancelText:  _cancelText);
        }
    }
}
