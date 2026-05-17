using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnluMenu
{
    /// <summary>
    /// Generic tab/panel switcher. One button per tab, one panel per tab, only one
    /// panel visible at a time. Pure presentation — no history stack, no scene
    /// loading, no quit. Use anywhere you need tabs (settings, inventory, pause,
    /// in-game HUDs).
    /// </summary>
    public class TabController : MonoBehaviour
    {
        [Serializable]
        public class Tab
        {
            [Tooltip("Button that activates this tab.")]
            public Button Button;

            [Tooltip("Panel shown while this tab is active.")]
            public GameObject Panel;

            [Tooltip("Optional graphic toggled to mark the active tab (e.g. underline, highlight).")]
            public GameObject ActiveIndicator;
        }

        [Tooltip("Tabs in display order.")]
        [SerializeField] private List<Tab> _tabs = new();

        [Tooltip("Index of the tab shown on Awake.")]
        [SerializeField] private int _initialIndex = 0;

        [Tooltip("If true, auto-selects the first Selectable in the active panel (gamepad/keyboard).")]
        [SerializeField] private bool _autoSelectFirstInPanel = false;

        [Tooltip("Optional audio output. Leave empty for silent tabs.")]
        [SerializeField] private MonoBehaviour _audioProvider;

        private IUIAudio _audio = new NullUIAudio();

        public int CurrentIndex { get; private set; } = -1;

        /// <summary>Fires after a tab change with the new index.</summary>
        public event Action<int> OnTabChanged;

        private void Awake()
        {
            if (_audioProvider is IUIAudio a) _audio = a;

            for (int i = 0; i < _tabs.Count; i++)
            {
                int index = i;
                if (_tabs[i].Button != null)
                    _tabs[i].Button.onClick.AddListener(() => ShowTab(index));
            }

            int initial = Mathf.Clamp(_initialIndex, 0, Mathf.Max(0, _tabs.Count - 1));
            ShowTab(initial, playSound: false);
        }

        /// <summary>Activates the given tab index. No-op if already active or out of range.</summary>
        public void ShowTab(int index) => ShowTab(index, playSound: true);

        private void ShowTab(int index, bool playSound)
        {
            if (index < 0 || index >= _tabs.Count) return;
            if (index == CurrentIndex) return;

            for (int i = 0; i < _tabs.Count; i++)
            {
                var t = _tabs[i];
                bool active = i == index;
                if (t.Panel != null) t.Panel.SetActive(active);
                if (t.ActiveIndicator != null) t.ActiveIndicator.SetActive(active);
            }

            CurrentIndex = index;
            if (playSound) _audio.PlayClick();

            if (_autoSelectFirstInPanel && _tabs[index].Panel != null && EventSystem.current != null)
            {
                var selectable = _tabs[index].Panel.GetComponentInChildren<Selectable>(false);
                if (selectable != null) EventSystem.current.SetSelectedGameObject(selectable.gameObject);
            }

            OnTabChanged?.Invoke(index);
        }
    }
}
