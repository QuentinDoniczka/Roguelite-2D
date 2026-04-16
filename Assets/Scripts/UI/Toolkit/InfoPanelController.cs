using System;
using System.Collections.Generic;
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
        private const int StatRowCount = 6;

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

        private readonly VisualElement[] _statRowElements = new VisualElement[StatRowCount];
        private readonly Label[] _statNameLabels = new Label[StatRowCount];
        private readonly Label[] _statValueLabels = new Label[StatRowCount];
        private readonly VisualElement[] _breakdownContainers = new VisualElement[StatRowCount];
        private readonly Label[] _breakdownLabels = new Label[StatRowCount];

        private UnitSelectionManager _selectionManager;
        private CombatStats _trackedStats;
        private int _cachedAllyLayer;
        private GameObject _selectedUnit;
        private Coroutine _runningCoroutine;

        internal bool IsVisible => _infoArea.ClassListContains(VisibleClass);
        internal bool IsEmptyStateLabelVisible => !_emptyLabel.ClassListContains(HiddenClass);

        internal string StatValueText(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= StatRowCount)
                return string.Empty;
            return _statValueLabels[rowIndex].text;
        }

        internal string StatNameText(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= StatRowCount)
                return string.Empty;
            return _statNameLabels[rowIndex].text;
        }

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
            BuildStatRows();
            Hide();
        }

        internal void InitializeForTest(UnitSelectionManager selectionManager)
        {
            _selectionManager = selectionManager;
            _cachedAllyLayer = PhysicsLayers.AllyLayer;
            SubscribeToEvents();
            BuildStatRows();
            Hide();
        }

        public void Dispose()
        {
            UntrackStats();

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

            UntrackStats();

            if (!unit.TryGetComponent<CombatStats>(out var stats))
            {
                Hide();
                return;
            }

            _trackedStats = stats;
            _trackedStats.OnDamageTaken += HandleHpChanged;
            _trackedStats.OnHealed += HandleHpChanged;
            UpdateDisplay();

            _selectedUnit = unit;
            Show();
        }

        private void HandleUnitDeselected()
        {
        }

        private void BuildStatRows()
        {
            IReadOnlyList<StatType> displayOrder = CombatStats.DisplayOrder;

            for (int i = 0; i < StatRowCount; i++)
            {
                var row = new VisualElement();
                row.AddToClassList("info-stat-row");
                _tabContent.Add(row);

                var nameLabel = new Label();
                nameLabel.AddToClassList("info-stat-name");
                row.Add(nameLabel);

                var valueLabel = new Label();
                valueLabel.AddToClassList("info-stat-value");
                row.Add(valueLabel);

                var breakdownContainer = new VisualElement();
                breakdownContainer.AddToClassList("info-breakdown");
                breakdownContainer.AddToClassList(HiddenClass);
                _tabContent.Add(breakdownContainer);

                var breakdownLabel = new Label();
                breakdownLabel.AddToClassList("info-breakdown-text");
                breakdownContainer.Add(breakdownLabel);

                _statRowElements[i] = row;
                _statNameLabels[i] = nameLabel;
                _statValueLabels[i] = valueLabel;
                _breakdownContainers[i] = breakdownContainer;
                _breakdownLabels[i] = breakdownLabel;
            }
        }

        private void UpdateDisplay()
        {
            if (_trackedStats == null)
                return;

            IReadOnlyList<StatType> displayOrder = CombatStats.DisplayOrder;

            for (int i = 0; i < StatRowCount; i++)
            {
                StatBreakdownData breakdown = _trackedStats.GetBreakdown(displayOrder[i]);
                _statNameLabels[i].text = breakdown.StatName;
                _statValueLabels[i].text = breakdown.FinalValue;
            }
        }

        private void UntrackStats()
        {
            if (_trackedStats == null)
                return;

            _trackedStats.OnDamageTaken -= HandleHpChanged;
            _trackedStats.OnHealed -= HandleHpChanged;
            _trackedStats = null;
        }

        private void HandleHpChanged(int amount, int currentHp)
        {
            UpdateDisplay();
        }
    }
}
