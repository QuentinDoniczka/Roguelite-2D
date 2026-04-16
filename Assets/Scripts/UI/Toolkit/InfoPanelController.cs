using System;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.UI.Toolkit
{
    public class InfoPanelController
    {
        private const string VisibleClass = "info-area--visible";
        private const string HiddenClass = "hidden";

        private readonly VisualElement _infoArea;
        private readonly Label _emptyLabel;
        private readonly VisualElement _header;
        private readonly Label _nameLabel;
        private readonly Label _positionLabel;
        private readonly Button _prevButton;
        private readonly Button _nextButton;
        private readonly Button[] _tabButtons;
        private readonly VisualElement _tabBar;
        private readonly ScrollView _scrollView;
        private readonly VisualElement _tabContent;
        private readonly MonoBehaviour _coroutineHost;

        private UnitSelectionManager _selectionManager;
        private int _cachedAllyLayer;
        private GameObject _selectedUnit;
        private Coroutine _runningCoroutine;

        internal bool IsVisible => _infoArea.ClassListContains(VisibleClass);
        internal bool IsEmptyStateLabelVisible => !_emptyLabel.ClassListContains(HiddenClass);

        public InfoPanelController(
            VisualElement infoArea,
            Label emptyLabel,
            VisualElement header,
            Label nameLabel,
            Label positionLabel,
            Button prevButton,
            Button nextButton,
            Button[] tabButtons,
            VisualElement tabBar,
            ScrollView scrollView,
            VisualElement tabContent,
            MonoBehaviour coroutineHost)
        {
            _infoArea = infoArea;
            _emptyLabel = emptyLabel;
            _header = header;
            _nameLabel = nameLabel;
            _positionLabel = positionLabel;
            _prevButton = prevButton;
            _nextButton = nextButton;
            _tabButtons = tabButtons;
            _tabBar = tabBar;
            _scrollView = scrollView;
            _tabContent = tabContent;
            _coroutineHost = coroutineHost;
        }

        public void Initialize()
        {
            var managers = UnityEngine.Object.FindObjectsByType<UnitSelectionManager>(FindObjectsSortMode.None);
            if (managers.Length == 0)
                return;

            _selectionManager = managers[0];
            _cachedAllyLayer = PhysicsLayers.AllyLayer;
            SubscribeToEvents();
            Hide();
        }

        internal void InitializeForTest(UnitSelectionManager selectionManager)
        {
            _selectionManager = selectionManager;
            _cachedAllyLayer = PhysicsLayers.AllyLayer;
            SubscribeToEvents();
            Hide();
        }

        public void Dispose()
        {
            if (_selectionManager != null)
            {
                _selectionManager.OnUnitSelected -= HandleUnitSelected;
                _selectionManager.OnUnitDeselected -= HandleUnitDeselected;
            }

            if (_runningCoroutine != null && _coroutineHost != null)
            {
                _coroutineHost.StopCoroutine(_runningCoroutine);
                _runningCoroutine = null;
            }
        }

        private void SubscribeToEvents()
        {
            _selectionManager.OnUnitSelected += HandleUnitSelected;
            _selectionManager.OnUnitDeselected += HandleUnitDeselected;
        }

        private void Show()
        {
            _infoArea.AddToClassList(VisibleClass);
            _header.RemoveFromClassList(HiddenClass);
            _tabBar.RemoveFromClassList(HiddenClass);
            _scrollView.RemoveFromClassList(HiddenClass);
            _emptyLabel.AddToClassList(HiddenClass);
        }

        private void Hide()
        {
            _infoArea.RemoveFromClassList(VisibleClass);
            _header.AddToClassList(HiddenClass);
            _tabBar.AddToClassList(HiddenClass);
            _scrollView.AddToClassList(HiddenClass);
            _emptyLabel.RemoveFromClassList(HiddenClass);
        }

        private void HandleUnitSelected(GameObject unit)
        {
            if (unit.layer != _cachedAllyLayer)
            {
                Hide();
                return;
            }

            if (!unit.TryGetComponent<CombatStats>(out _))
            {
                Hide();
                return;
            }

            _selectedUnit = unit;
            Show();
        }

        private void HandleUnitDeselected()
        {
        }
    }
}
