using System;
using System.Collections.Generic;
using System.Text;
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
        private const string TabActiveClass = "info-tab--active";
        private const string TabInactiveClass = "info-tab--inactive";
        private const string BreakdownVisibleClass = "info-breakdown--visible";
        private const string BreakdownSeparator = "\u2500\u2500\u2500\u2500\u2500\u2500";
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

        private readonly List<GameObject> _teamRoster = new List<GameObject>();
        private Transform _teamContainer;
        private int _currentRosterIndex = -1;
        private bool _isSyncingSelection;
        private bool _displayingDeadUnit;

        private UnitSelectionManager _selectionManager;
        private CombatStats _trackedStats;
        private int _cachedAllyLayer;
        private int _activeTabIndex;
        private int _expandedRowIndex = -1;
        private GameObject _selectedUnit;
        private Coroutine _runningCoroutine;
        private readonly StringBuilder _stringBuilder = new StringBuilder(128);

        internal bool IsVisible => _infoArea.ClassListContains(VisibleClass);
        internal bool IsEmptyStateLabelVisible => !_emptyLabel.ClassListContains(HiddenClass);
        internal int ActiveTabIndex => _activeTabIndex;
        internal string NameLabelText => _nameLabel.text;
        internal string TeamPosLabelText => _positionLabel.text;
        internal int CurrentRosterIndex => _currentRosterIndex;
        internal int TeamRosterCount => _teamRoster.Count;
        internal bool IsDisplayingDeadUnit => _displayingDeadUnit;

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
            RegisterTabCallbacks();
            SwitchTab(0);
            Hide();
        }

        internal void InitializeForTest(UnitSelectionManager selectionManager)
        {
            _selectionManager = selectionManager;
            _cachedAllyLayer = PhysicsLayers.AllyLayer;
            SubscribeToEvents();
            BuildStatRows();
            RegisterTabCallbacks();
            SwitchTab(0);
            Hide();
        }

        internal void InitializeForTest(UnitSelectionManager selectionManager, Transform teamContainer)
        {
            InitializeForTest(selectionManager);
            SetTeamContainer(teamContainer);
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
            if (_expandedRowIndex >= 0)
            {
                CollapseBreakdown(_expandedRowIndex);
                _expandedRowIndex = -1;
            }

            _infoArea.AddToClassList(VisibleClass);
            _header.RemoveFromClassList(HiddenClass);
            _tabBar.RemoveFromClassList(HiddenClass);
            _scrollView.RemoveFromClassList(HiddenClass);
            _emptyLabel.AddToClassList(HiddenClass);
            SwitchTab(0);
        }

        private void Hide()
        {
            if (_expandedRowIndex >= 0)
            {
                CollapseBreakdown(_expandedRowIndex);
                _expandedRowIndex = -1;
            }

            _infoArea.RemoveFromClassList(VisibleClass);
            _header.AddToClassList(HiddenClass);
            _tabBar.AddToClassList(HiddenClass);
            _scrollView.AddToClassList(HiddenClass);
            _emptyLabel.RemoveFromClassList(HiddenClass);
        }

        internal void SwitchTab(int tabIndex)
        {
            if (tabIndex < 0 || tabIndex >= _tabButtons.Length)
                return;

            for (int i = 0; i < _tabButtons.Length; i++)
            {
                _tabButtons[i].RemoveFromClassList(TabActiveClass);
                _tabButtons[i].AddToClassList(TabInactiveClass);
            }

            _tabButtons[tabIndex].RemoveFromClassList(TabInactiveClass);
            _tabButtons[tabIndex].AddToClassList(TabActiveClass);
            _activeTabIndex = tabIndex;

            bool isStatsTab = tabIndex == 0;

            if (!isStatsTab && _expandedRowIndex >= 0)
            {
                CollapseBreakdown(_expandedRowIndex);
                _expandedRowIndex = -1;
            }

            for (int i = 0; i < StatRowCount; i++)
            {
                if (isStatsTab)
                {
                    _statRowElements[i].RemoveFromClassList(HiddenClass);
                }
                else
                {
                    _statRowElements[i].AddToClassList(HiddenClass);
                    _breakdownContainers[i].AddToClassList(HiddenClass);
                }
            }

            if (isStatsTab)
                UpdateDisplay();
        }

        private void RegisterTabCallbacks()
        {
            for (int i = 0; i < _tabButtons.Length; i++)
            {
                int capturedIndex = i;
                _tabButtons[i].clicked += () => SwitchTab(capturedIndex);
            }

            _prevButton.clicked += NavigateToPreviousAlly;
            _nextButton.clicked += NavigateToNextAlly;
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

            if (!_isSyncingSelection)
            {
                int index = FindRosterIndex(unit);
                if (index >= 0)
                    _currentRosterIndex = index;
                _displayingDeadUnit = false;
                UpdateNameLabel();
                UpdateTeamPositionLabel();
            }
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

                int capturedIndex = i;
                row.RegisterCallback<ClickEvent>(evt => ToggleBreakdown(capturedIndex));

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

            if (_expandedRowIndex >= 0)
                PopulateBreakdownText(_expandedRowIndex);
        }

        internal void ToggleBreakdown(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= StatRowCount)
                return;

            if (_expandedRowIndex == rowIndex)
            {
                CollapseBreakdown(rowIndex);
                _expandedRowIndex = -1;
            }
            else
            {
                if (_expandedRowIndex >= 0)
                    CollapseBreakdown(_expandedRowIndex);

                _breakdownContainers[rowIndex].RemoveFromClassList(HiddenClass);
                _breakdownContainers[rowIndex].AddToClassList(BreakdownVisibleClass);
                PopulateBreakdownText(rowIndex);
                _expandedRowIndex = rowIndex;
            }
        }

        private void CollapseBreakdown(int rowIndex)
        {
            _breakdownContainers[rowIndex].AddToClassList(HiddenClass);
            _breakdownContainers[rowIndex].RemoveFromClassList(BreakdownVisibleClass);
        }

        private void PopulateBreakdownText(int rowIndex)
        {
            if (_trackedStats == null)
                return;

            IReadOnlyList<StatType> displayOrder = CombatStats.DisplayOrder;
            StatBreakdownData breakdown = _trackedStats.GetBreakdown(displayOrder[rowIndex]);

            _stringBuilder.Clear();

            if (breakdown.Modifiers != null)
            {
                for (int i = 0; i < breakdown.Modifiers.Length; i++)
                {
                    StatModifierEntry modifier = breakdown.Modifiers[i];
                    _stringBuilder.Append(modifier.Source).Append(": ").AppendLine(modifier.Value);
                }
            }

            _stringBuilder.AppendLine(BreakdownSeparator);
            _stringBuilder.Append("Total: ").Append(breakdown.FinalValue);

            _breakdownLabels[rowIndex].text = _stringBuilder.ToString();
        }

        internal bool IsBreakdownExpanded(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= StatRowCount)
                return false;
            return !_breakdownContainers[rowIndex].ClassListContains(HiddenClass);
        }

        internal string BreakdownText(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= StatRowCount)
                return string.Empty;
            return _breakdownLabels[rowIndex].text;
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

        public void SetTeamContainer(Transform teamContainer)
        {
            _teamContainer = teamContainer;
            RefreshTeamRoster();
            if (_teamRoster.Count > 0)
                DisplayTeamMember(0);
        }

        private void RefreshTeamRoster()
        {
            _teamRoster.Clear();

            if (_teamContainer == null)
                return;

            for (int i = 0; i < _teamContainer.childCount; i++)
            {
                var child = _teamContainer.GetChild(i);
                if (child.TryGetComponent<CombatStats>(out _))
                    _teamRoster.Add(child.gameObject);
            }
        }

        internal void NavigateToNextAlly()
        {
            if (_teamRoster.Count == 0)
                return;

            int nextIndex = (_currentRosterIndex + 1) % _teamRoster.Count;
            DisplayTeamMember(nextIndex);
        }

        internal void NavigateToPreviousAlly()
        {
            if (_teamRoster.Count == 0)
                return;

            int prevIndex = (_currentRosterIndex - 1 + _teamRoster.Count) % _teamRoster.Count;
            DisplayTeamMember(prevIndex);
        }

        private void DisplayTeamMember(int rosterIndex)
        {
            _currentRosterIndex = rosterIndex;
            _displayingDeadUnit = false;

            var unit = _teamRoster[rosterIndex];

            bool isDeadOrDestroyed = unit == null
                || (unit.TryGetComponent<CombatStats>(out var stats) && stats.IsDead);

            if (isDeadOrDestroyed)
            {
                _displayingDeadUnit = true;
                UntrackStats();
                Show();
                if (_selectionManager != null)
                    _selectionManager.ForceDeselect();
            }
            else
            {
                _isSyncingSelection = true;
                if (_selectionManager != null)
                    _selectionManager.ForceSelect(unit);
                _isSyncingSelection = false;
            }

            UpdateNameLabel();
            UpdateTeamPositionLabel();
        }

        private int FindRosterIndex(GameObject unit)
        {
            for (int i = 0; i < _teamRoster.Count; i++)
            {
                if (_teamRoster[i] == unit)
                    return i;
            }

            return -1;
        }

        private void UpdateNameLabel()
        {
            if (_displayingDeadUnit)
            {
                _nameLabel.text = string.Empty;
                return;
            }

            if (_currentRosterIndex >= 0 && _currentRosterIndex < _teamRoster.Count
                && _teamRoster[_currentRosterIndex] != null)
            {
                _nameLabel.text = _teamRoster[_currentRosterIndex].name;
                return;
            }

            _nameLabel.text = string.Empty;
        }

        private void UpdateTeamPositionLabel()
        {
            _stringBuilder.Clear();
            _stringBuilder.Append(_currentRosterIndex + 1).Append('/').Append(_teamRoster.Count);
            _positionLabel.text = _stringBuilder.ToString();
        }
    }
}
