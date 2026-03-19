using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RogueliteAutoBattler.UI.Core
{
    /// <summary>
    /// Singleton that manages tab navigation.
    /// Combat screen is the default (no tab selected).
    /// Clicking a tab shows that panel; clicking it again returns to combat.
    /// </summary>
    public class NavigationManager : MonoBehaviour
    {
        /// <summary>
        /// Singleton instance.
        /// Single-scene architecture — no DontDestroyOnLoad needed.
        /// </summary>
        public static NavigationManager Instance { get; private set; }

        [Header("Default Screen")]
        [SerializeField]
        [Tooltip("The screen shown when no tab is selected (Combat/Battle).")]
        private UIScreen _defaultScreen;

        [Header("Tab Setup")]
        [SerializeField]
        [Tooltip("Tab buttons in order.")]
        private TabButton[] _tabButtons;

        [SerializeField]
        [Tooltip("Root screens in order (same order as tab buttons).")]
        private UIScreen[] _rootScreens;

        [Header("Input")]
        [SerializeField]
        [Tooltip("Input action for back/cancel navigation (typically Escape key).")]
        private InputActionReference _cancelAction;

        private ScreenStack[] _stacks;
        private int _currentTabIndex = -1;

        /// <summary>
        /// Index of the currently active tab. -1 means no tab (default/combat screen).
        /// </summary>
        public int CurrentTab => _currentTabIndex;

        /// <summary>
        /// Fired when the active tab changes. -1 means returned to default screen.
        /// </summary>
        public event Action<int> OnTabChanged;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (_tabButtons == null || _rootScreens == null ||
                _rootScreens.Length != _tabButtons.Length)
            {
                Debug.LogError("[NavigationManager] Tab buttons and root screens must match.", this);
                enabled = false;
                return;
            }

            if (_rootScreens.Length == 0)
            {
                Debug.LogError("[NavigationManager] No root screens assigned.", this);
                enabled = false;
                return;
            }

            _stacks = new ScreenStack[_rootScreens.Length];
            for (int i = 0; i < _rootScreens.Length; i++)
            {
                _rootScreens[i].OnHide();
                _stacks[i] = new ScreenStack(_rootScreens[i]);
            }
        }

        private void Start()
        {
            if (_stacks == null) return;

            // Show default screen (combat), no tab selected
            if (_defaultScreen != null)
                _defaultScreen.OnShow();
        }

        private void OnEnable()
        {
            if (_cancelAction != null && _cancelAction.action != null)
            {
                _cancelAction.action.Enable();
                _cancelAction.action.performed += OnCancelPerformed;
            }
        }

        private void OnDisable()
        {
            if (_cancelAction != null && _cancelAction.action != null)
            {
                _cancelAction.action.performed -= OnCancelPerformed;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void OnCancelPerformed(InputAction.CallbackContext context)
        {
            if (_stacks == null) return;

            // If a tab is active and has sub-screens, pop
            if (_currentTabIndex >= 0 && _stacks[_currentTabIndex].Count > 1)
            {
                PopScreen();
                return;
            }

            // If a tab is active, return to default (combat)
            if (_currentTabIndex >= 0)
            {
                ReturnToDefault();
            }
        }

        /// <summary>
        /// Switches to the tab at the given index.
        /// If the tab is already active, returns to the default screen (toggle behavior).
        /// </summary>
        public void SwitchTab(int index)
        {
            if (_stacks == null || index < 0 || index >= _stacks.Length)
                return;

            // Toggle: clicking the active tab returns to default
            if (index == _currentTabIndex)
            {
                ReturnToDefault();
                return;
            }

            // Hide current tab if any
            if (_currentTabIndex >= 0)
            {
                _tabButtons[_currentTabIndex].Deselect();
                _stacks[_currentTabIndex].HideCurrent();
            }
            else if (_defaultScreen != null)
            {
                // Hide default screen (combat)
                _defaultScreen.OnHide();
            }

            _currentTabIndex = index;
            _stacks[_currentTabIndex].ShowCurrent();
            _tabButtons[_currentTabIndex].Select();

            OnTabChanged?.Invoke(_currentTabIndex);
        }

        /// <summary>
        /// Returns to the default screen (combat). Deselects all tabs.
        /// </summary>
        public void ReturnToDefault()
        {
            if (_currentTabIndex >= 0)
            {
                _tabButtons[_currentTabIndex].Deselect();
                _stacks[_currentTabIndex].HideCurrent();
            }

            _currentTabIndex = -1;

            if (_defaultScreen != null)
                _defaultScreen.OnShow();

            OnTabChanged?.Invoke(-1);
        }

        /// <summary>
        /// Pushes a sub-screen onto the current tab's stack.
        /// </summary>
        public void PushScreen(UIScreen screen)
        {
            if (_stacks == null || _currentTabIndex < 0) return;
            _stacks[_currentTabIndex].Push(screen);
        }

        /// <summary>
        /// Pops the top screen from the current tab's stack.
        /// Returns null if only the root remains.
        /// </summary>
        public UIScreen PopScreen()
        {
            if (_stacks == null || _currentTabIndex < 0) return null;
            return _stacks[_currentTabIndex].Pop();
        }
    }
}
