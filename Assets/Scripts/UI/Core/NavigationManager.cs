using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RogueliteAutoBattler.UI.Core
{
    /// <summary>
    /// Singleton that manages 5-tab navigation and per-tab screen stacks.
    /// </summary>
    public class NavigationManager : MonoBehaviour
    {
        private const int MinTabCount = 1;

        /// <summary>
        /// Singleton instance.
        /// Single-scene architecture — no DontDestroyOnLoad needed.
        /// </summary>
        public static NavigationManager Instance { get; private set; }

        [Header("Tab Setup")]
        [SerializeField]
        [Tooltip("Tab buttons in order: Village, SkillTree, Combat, Guild, Shop.")]
        private TabButton[] _tabButtons;

        [SerializeField]
        [Tooltip("Root screens in order: Village, SkillTree, Combat, Guild, Shop.")]
        private UIScreen[] _rootScreens;

        [SerializeField]
        [Tooltip("Default tab index to show on startup (0-4). 2 = Combat.")]
        private int _defaultTabIndex = 2;

        [Header("Input")]
        [SerializeField]
        [Tooltip("Input action for back/cancel navigation (typically Escape key).")]
        private InputActionReference _cancelAction;

        private ScreenStack[] _stacks;
        private int _currentTabIndex = -1;

        /// <summary>
        /// Fired when the active tab changes. Provides the new tab index.
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

            if (_rootScreens.Length != _tabButtons.Length)
            {
                Debug.LogError(
                    $"[NavigationManager] Mismatched arrays: {_rootScreens.Length} screens vs {_tabButtons.Length} tab buttons.",
                    this);
                enabled = false;
                return;
            }

            if (_rootScreens.Length < MinTabCount)
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

        private void Start()
        {
            if (_stacks == null)
            {
                return;
            }

            int startIndex = Mathf.Clamp(_defaultTabIndex, 0, _stacks.Length - 1);
            SwitchTab(startIndex);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void OnCancelPerformed(InputAction.CallbackContext context)
        {
            if (_stacks == null || _currentTabIndex < 0)
            {
                return;
            }

            if (_stacks[_currentTabIndex].Count > 1)
            {
                PopScreen();
            }
        }

        /// <summary>
        /// Switches to the tab at the given index (0-4).
        /// </summary>
        public void SwitchTab(int index)
        {
            if (_stacks == null || index < 0 || index >= _stacks.Length)
            {
                return;
            }

            if (index == _currentTabIndex)
            {
                return;
            }

            if (_currentTabIndex >= 0)
            {
                _tabButtons[_currentTabIndex].Deselect();
                _stacks[_currentTabIndex].HideCurrent();
            }

            _currentTabIndex = index;

            _stacks[_currentTabIndex].ShowCurrent();
            _tabButtons[_currentTabIndex].Select();

            OnTabChanged?.Invoke(_currentTabIndex);
        }

        /// <summary>
        /// Pushes a sub-screen onto the current tab's stack.
        /// </summary>
        public void PushScreen(UIScreen screen)
        {
            if (_stacks == null || _currentTabIndex < 0)
            {
                return;
            }

            _stacks[_currentTabIndex].Push(screen);
        }

        /// <summary>
        /// Pops the top screen from the current tab's stack.
        /// Returns null if only the root screen remains.
        /// </summary>
        public UIScreen PopScreen()
        {
            if (_stacks == null || _currentTabIndex < 0)
            {
                return null;
            }

            return _stacks[_currentTabIndex].Pop();
        }
    }
}
