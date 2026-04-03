using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RogueliteAutoBattler.UI.Core
{
    public class NavigationManager : MonoBehaviour
    {
        public static NavigationManager Instance { get; private set; }

        [Header("Default Screens")]
        [SerializeField] private UIScreen _defaultScreen;
        [SerializeField] private UIScreen _defaultInfoScreen;

        [Header("Tab Setup")]
        [SerializeField] private TabButton[] _tabButtons;
        [SerializeField] private UIScreen[] _rootScreens;
        [SerializeField] private UIScreen[] _infoScreens;

        [Header("Input")]
        [SerializeField] private InputActionReference _cancelAction;

        private ScreenStack[] _stacks;
        private int _currentTabIndex = -1;

        public int CurrentTab => _currentTabIndex;

        public event Action<int> OnTabChanged;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnDomainReload()
        {
            Instance = null;
        }

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

            if (_infoScreens != null && _infoScreens.Length != _rootScreens.Length)
            {
                Debug.LogError("[NavigationManager] Info screens count must match root screens.", this);
                enabled = false;
                return;
            }

            _stacks = new ScreenStack[_rootScreens.Length];
            for (int i = 0; i < _rootScreens.Length; i++)
            {
                _rootScreens[i].OnHide();
                _stacks[i] = new ScreenStack(_rootScreens[i]);
            }

            if (_infoScreens != null)
            {
                for (int i = 0; i < _infoScreens.Length; i++)
                {
                    if (_infoScreens[i] != null)
                        _infoScreens[i].OnHide();
                }
            }
        }

        private void Start()
        {
            if (_stacks == null) return;

            if (_defaultScreen != null)
                _defaultScreen.OnShow();
            if (_defaultInfoScreen != null)
                _defaultInfoScreen.OnShow();
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

            if (_currentTabIndex >= 0 && _stacks[_currentTabIndex].Count > 1)
            {
                PopScreen();
                return;
            }

            if (_currentTabIndex >= 0)
            {
                ReturnToDefault();
            }
        }

        public void SwitchTab(int index)
        {
            if (_stacks == null || index < 0 || index >= _stacks.Length)
                return;

            if (index == _currentTabIndex)
            {
                ReturnToDefault();
                return;
            }

            if (_currentTabIndex >= 0)
            {
                _tabButtons[_currentTabIndex].Deselect();
                _stacks[_currentTabIndex].HideCurrent();
                HideInfoScreen(_currentTabIndex);
            }
            else
            {
                if (_defaultScreen != null)
                    _defaultScreen.OnHide();
                if (_defaultInfoScreen != null)
                    _defaultInfoScreen.OnHide();
            }

            _currentTabIndex = index;
            _stacks[_currentTabIndex].ShowCurrent();
            _tabButtons[_currentTabIndex].Select();
            ShowInfoScreen(_currentTabIndex);

            OnTabChanged?.Invoke(_currentTabIndex);
        }

        public void ReturnToDefault()
        {
            if (_currentTabIndex >= 0)
            {
                _tabButtons[_currentTabIndex].Deselect();
                _stacks[_currentTabIndex].HideCurrent();
                HideInfoScreen(_currentTabIndex);
            }

            _currentTabIndex = -1;

            if (_defaultScreen != null)
                _defaultScreen.OnShow();
            if (_defaultInfoScreen != null)
                _defaultInfoScreen.OnShow();

            OnTabChanged?.Invoke(-1);
        }

        public void PushScreen(UIScreen screen)
        {
            if (_stacks == null || _currentTabIndex < 0) return;
            _stacks[_currentTabIndex].Push(screen);
        }

        public UIScreen PopScreen()
        {
            if (_stacks == null || _currentTabIndex < 0) return null;
            return _stacks[_currentTabIndex].Pop();
        }

        private void ShowInfoScreen(int index)
        {
            if (_infoScreens != null && index >= 0 && index < _infoScreens.Length && _infoScreens[index] != null)
                _infoScreens[index].OnShow();
        }

        private void HideInfoScreen(int index)
        {
            if (_infoScreens != null && index >= 0 && index < _infoScreens.Length && _infoScreens[index] != null)
                _infoScreens[index].OnHide();
        }
    }
}
