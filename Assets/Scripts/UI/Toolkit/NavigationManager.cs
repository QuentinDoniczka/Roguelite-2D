using System;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.UI.Toolkit
{
    public class NavigationManager : IDisposable
    {
        private const string TAB_ACTIVE_CLASS = "tab-active";
        private const string TAB_INACTIVE_CLASS = "tab-inactive";

        private readonly Button[] _tabButtons;
        private readonly IScreen _defaultScreen;
        private readonly ScreenStack[] _stacks;
        private readonly Action[] _clickCallbacks;
        private int _currentTabIndex = -1;

        public int CurrentTab => _currentTabIndex;

        public event Action<int> OnTabChanged;

        public NavigationManager(Button[] tabButtons, IScreen defaultScreen)
        {
            _tabButtons = tabButtons;
            _defaultScreen = defaultScreen;
            _stacks = new ScreenStack[tabButtons.Length];
            _clickCallbacks = new Action[tabButtons.Length];

            for (int i = 0; i < _tabButtons.Length; i++)
            {
                int capturedIndex = i;
                _clickCallbacks[i] = () => SwitchTab(capturedIndex);
                _tabButtons[i].clicked += _clickCallbacks[i];
            }

            _defaultScreen.OnShow();
        }

        public void RegisterTab(int index, IScreen rootScreen)
        {
            if (index < 0 || index >= _stacks.Length)
            {
                return;
            }

            _stacks[index] = new ScreenStack(rootScreen);
        }

        public void SwitchTab(int index)
        {
            if (index < 0 || index >= _tabButtons.Length)
            {
                return;
            }

            if (index == _currentTabIndex)
            {
                ReturnToDefault();
                return;
            }

            if (_currentTabIndex >= 0)
            {
                DeactivateTab(_currentTabIndex);
                if (_stacks[_currentTabIndex] != null)
                {
                    _stacks[_currentTabIndex].HideCurrent();
                }
            }
            else
            {
                _defaultScreen.OnHide();
            }

            _currentTabIndex = index;
            ActivateTab(_currentTabIndex);

            if (_stacks[_currentTabIndex] != null)
            {
                _stacks[_currentTabIndex].ShowCurrent();
            }

            OnTabChanged?.Invoke(_currentTabIndex);
        }

        public void ReturnToDefault()
        {
            if (_currentTabIndex >= 0)
            {
                DeactivateTab(_currentTabIndex);
                if (_stacks[_currentTabIndex] != null)
                {
                    _stacks[_currentTabIndex].HideCurrent();
                }
            }

            _currentTabIndex = -1;
            _defaultScreen.OnShow();
            OnTabChanged?.Invoke(-1);
        }

        public void PushScreen(IScreen screen)
        {
            if (_currentTabIndex < 0 || _stacks[_currentTabIndex] == null)
            {
                return;
            }

            _stacks[_currentTabIndex].Push(screen);
        }

        public IScreen PopScreen()
        {
            if (_currentTabIndex < 0 || _stacks[_currentTabIndex] == null)
            {
                return null;
            }

            return _stacks[_currentTabIndex].Pop();
        }

        public void HandleCancel()
        {
            if (_currentTabIndex < 0)
            {
                return;
            }

            if (_stacks[_currentTabIndex] != null && _stacks[_currentTabIndex].Count > 1)
            {
                PopScreen();
                return;
            }

            ReturnToDefault();
        }

        public void Dispose()
        {
            for (int i = 0; i < _tabButtons.Length; i++)
            {
                _tabButtons[i].clicked -= _clickCallbacks[i];
            }
        }

        private void ActivateTab(int index)
        {
            _tabButtons[index].RemoveFromClassList(TAB_INACTIVE_CLASS);
            _tabButtons[index].AddToClassList(TAB_ACTIVE_CLASS);
        }

        private void DeactivateTab(int index)
        {
            _tabButtons[index].RemoveFromClassList(TAB_ACTIVE_CLASS);
            _tabButtons[index].AddToClassList(TAB_INACTIVE_CLASS);
        }
    }
}
