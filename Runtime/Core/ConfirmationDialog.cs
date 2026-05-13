using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnluMenu
{
    /// <summary>
    /// Reusable modal confirmation dialog. Call <see cref="Show"/> with a title,
    /// message, and callbacks. Wire the UI elements in the Inspector.
    /// </summary>
    /// <remarks>
    /// Place on a root-level GameObject in each scene where confirmations are needed.
    /// The dialog hides itself on Awake and re-hides after every Confirm/Cancel.
    /// </remarks>
    public class ConfirmationDialog : MonoBehaviour
    {
        public static ConfirmationDialog Instance { get; private set; }

        [Header("UI References")]
        [Tooltip("Root panel of the dialog. Toggled on/off.")]
        [SerializeField] private GameObject _root;

        [Tooltip("Title label (optional).")]
        [SerializeField] private TMP_Text _titleText;

        [Tooltip("Body message label.")]
        [SerializeField] private TMP_Text _messageText;

        [Tooltip("Confirm / OK button.")]
        [SerializeField] private Button _confirmButton;

        [Tooltip("Cancel / No button.")]
        [SerializeField] private Button _cancelButton;

        [Tooltip("Label on the confirm button (optional — set dynamically per call).")]
        [SerializeField] private TMP_Text _confirmLabel;

        [Tooltip("Label on the cancel button (optional — set dynamically per call).")]
        [SerializeField] private TMP_Text _cancelLabel;

        [Header("Audio")]
        [Tooltip("Optional MonoBehaviour implementing IUIAudio.")]
        [SerializeField] private MonoBehaviour _audioProvider;

        /// <summary>True while the dialog is visible.</summary>
        public bool IsOpen { get; private set; }

        private IUIAudio _audio = new NullUIAudio();
        private Action _onConfirm;
        private Action _onCancel;

        private void Awake()
        {
            Instance = this;
            if (_audioProvider is IUIAudio a) _audio = a;
            if (_root) _root.SetActive(false);

            _confirmButton?.onClick.AddListener(Confirm);
            _cancelButton?.onClick.AddListener(Cancel);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>
        /// Opens the dialog with the given content and callbacks.
        /// </summary>
        /// <param name="title">Dialog title (pass null to hide the title).</param>
        /// <param name="message">Body text.</param>
        /// <param name="onConfirm">Called when the player confirms.</param>
        /// <param name="onCancel">Called when the player cancels (optional).</param>
        /// <param name="confirmText">Label for the confirm button.</param>
        /// <param name="cancelText">Label for the cancel button.</param>
        public void Show(
            string title,
            string message,
            Action onConfirm,
            Action onCancel = null,
            string confirmText = "Confirm",
            string cancelText = "Cancel")
        {
            if (_titleText) _titleText.text = title ?? string.Empty;
            if (_messageText) _messageText.text = message;
            if (_confirmLabel) _confirmLabel.text = confirmText;
            if (_cancelLabel) _cancelLabel.text = cancelText;

            _onConfirm = onConfirm;
            _onCancel = onCancel;

            IsOpen = true;
            if (_root) _root.SetActive(true);
            _audio.PlayClick();
        }

        /// <summary>Confirms the dialog, fires the onConfirm callback, and hides.</summary>
        public void Confirm()
        {
            _audio.PlayClick();
            Hide();
            _onConfirm?.Invoke();
        }

        /// <summary>Cancels the dialog, fires the onCancel callback, and hides.</summary>
        public void Cancel()
        {
            _audio.PlayClick();
            Hide();
            _onCancel?.Invoke();
        }

        private void Hide()
        {
            IsOpen = false;
            if (_root) _root.SetActive(false);
            _onConfirm = null;
            _onCancel = null;
        }
    }
}
