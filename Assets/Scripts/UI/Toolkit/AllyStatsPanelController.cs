using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Common;
using RogueliteAutoBattler.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.UI.Toolkit
{
    public class AllyStatsPanelController
    {
        private const float StaggerDelay = 0.05f;
        private const float FadeDuration = 0.15f;

        private static class UssClasses
        {
            internal const string BreakdownSeparator = "\u2500\u2500\u2500\u2500\u2500\u2500";
            internal const string TabActive = "info-tab--active";
            internal const string TabContentHidden = "info-tab-content--hidden";
            internal const string BreakdownHidden = "stat-breakdown--hidden";
            internal const string EmptyLabelHidden = "info-empty-label--hidden";
        }

        private readonly VisualElement _panelRoot;
        private readonly Label _emptyLabel;
        private readonly VisualElement _contentContainer;
        private readonly Label _nameLabel;
        private readonly Label _teamPosLabel;
        private readonly Button _prevButton;
        private readonly Button _nextButton;
        private readonly Button[] _tabButtons;
        private readonly VisualElement[] _tabContents;
        private readonly ScrollView _statsScrollView;
        private readonly MonoBehaviour _coroutineHost;
        private readonly StatRowView[] _statRows;
        private readonly Action[] _tabClickHandlers;
        private readonly EventCallback<ClickEvent>[] _rowHeaderClickHandlers;
        private readonly VisualElement[] _rowHeaders;
        private readonly List<GameObject> _teamRoster = new();
        private readonly StringBuilder _stringBuilder = new StringBuilder(128);

        private UnitSelectionManager _selectionManager;
        private CombatStats _trackedStats;
        private StatSnapshot[] _snapshots;
        private int _currentRosterIndex = -1;
        private int _activeTabIndex;
        private int _expandedRowIndex = -1;
        private int _cachedAllyLayer;
        private bool _isSyncingSelection;
        private bool _displayingDeadUnit;
        private Coroutine _fadeCoroutine;

        private struct StatRowView
        {
            internal VisualElement Root;
            internal Label NameLabel;
            internal Label ValueLabel;
            internal VisualElement BreakdownContainer;
            internal Label BreakdownText;
        }

        private struct StatSnapshot
        {
            internal string UnitName;
            internal string[] StatValues;
            internal bool IsValid;
        }

        public AllyStatsPanelController(
            VisualElement panelRoot,
            Label emptyLabel,
            VisualElement contentContainer,
            Label nameLabel,
            Label teamPosLabel,
            Button prevButton,
            Button nextButton,
            Button[] tabButtons,
            VisualElement[] tabContents,
            ScrollView statsScrollView,
            MonoBehaviour coroutineHost)
        {
            _panelRoot = panelRoot;
            _emptyLabel = emptyLabel;
            _contentContainer = contentContainer;
            _nameLabel = nameLabel;
            _teamPosLabel = teamPosLabel;
            _prevButton = prevButton;
            _nextButton = nextButton;
            _tabButtons = tabButtons;
            _tabContents = tabContents;
            _statsScrollView = statsScrollView;
            _coroutineHost = coroutineHost;

            _statRows = new StatRowView[CombatStats.DisplayOrder.Count];
            _rowHeaders = new VisualElement[_statRows.Length];
            _rowHeaderClickHandlers = new EventCallback<ClickEvent>[_statRows.Length];
            BuildStatRows();

            _prevButton.clicked += NavigateToPreviousAlly;
            _nextButton.clicked += NavigateToNextAlly;

            _tabClickHandlers = new Action[_tabButtons.Length];
            for (int i = 0; i < _tabButtons.Length; i++)
            {
                int tabIndex = i;
                _tabClickHandlers[i] = () => SwitchTab(tabIndex);
                _tabButtons[i].clicked += _tabClickHandlers[i];
            }
        }

        private void BuildStatRows()
        {
            var displayOrder = CombatStats.DisplayOrder;
            for (int i = 0; i < displayOrder.Count; i++)
            {
                int rowIndex = i;
                var statRow = new VisualElement();
                statRow.AddToClassList("stat-row");
                statRow.style.opacity = 0f;

                var statRowHeader = new VisualElement();
                statRowHeader.AddToClassList("stat-row-header");
                EventCallback<ClickEvent> headerClickHandler = _ => ToggleBreakdown(rowIndex);
                statRowHeader.RegisterCallback(headerClickHandler);
                _rowHeaders[i] = statRowHeader;
                _rowHeaderClickHandlers[i] = headerClickHandler;

                var statName = new Label();
                statName.AddToClassList("stat-name");

                var statValue = new Label();
                statValue.AddToClassList("stat-value");

                statRowHeader.Add(statName);
                statRowHeader.Add(statValue);
                statRow.Add(statRowHeader);

                var breakdownContainer = new VisualElement();
                breakdownContainer.AddToClassList("stat-breakdown");
                breakdownContainer.AddToClassList(UssClasses.BreakdownHidden);

                var breakdownText = new Label();
                breakdownText.AddToClassList("breakdown-text");
                breakdownContainer.Add(breakdownText);
                statRow.Add(breakdownContainer);

                _statsScrollView.Add(statRow);

                _statRows[i] = new StatRowView
                {
                    Root = statRow,
                    NameLabel = statName,
                    ValueLabel = statValue,
                    BreakdownContainer = breakdownContainer,
                    BreakdownText = breakdownText
                };
            }
        }

        public void Initialize()
        {
            var selectionManager = UnitSelectionManager.Instance;
            if (selectionManager == null)
            {
                _cachedAllyLayer = PhysicsLayers.AllyLayer;
                Debug.LogWarning($"[{nameof(AllyStatsPanelController)}] UnitSelectionManager.Instance not found.");
                Hide();
                return;
            }

            Transform teamContainer = null;
            var combatWorld = GameBootstrap.CombatWorld;
            if (combatWorld != null)
                teamContainer = combatWorld.Find(CombatSetupHelper.TeamContainerName);

            InitializeCore(selectionManager, teamContainer);
        }

        internal void InitializeForTest(UnitSelectionManager selectionManager, Transform teamContainer = null)
        {
            InitializeCore(selectionManager, teamContainer);
        }

        private void InitializeCore(UnitSelectionManager selectionManager, Transform teamContainer)
        {
            _cachedAllyLayer = PhysicsLayers.AllyLayer;
            _selectionManager = selectionManager;
            _selectionManager.OnUnitSelected += HandleUnitSelected;

            Hide();

            if (teamContainer != null)
            {
                RefreshTeamRoster(teamContainer);
                if (_teamRoster.Count > 0)
                    DisplayTeamMember(0);
            }
        }

        public void Dispose()
        {
            UntrackStats();

            if (_selectionManager != null)
                _selectionManager.OnUnitSelected -= HandleUnitSelected;

            _prevButton.clicked -= NavigateToPreviousAlly;
            _nextButton.clicked -= NavigateToNextAlly;

            if (_tabClickHandlers != null)
            {
                for (int i = 0; i < _tabButtons.Length && i < _tabClickHandlers.Length; i++)
                {
                    if (_tabClickHandlers[i] != null)
                        _tabButtons[i].clicked -= _tabClickHandlers[i];
                }
            }

            if (_rowHeaders != null && _rowHeaderClickHandlers != null)
            {
                for (int i = 0; i < _rowHeaders.Length; i++)
                {
                    if (_rowHeaders[i] != null && _rowHeaderClickHandlers[i] != null)
                        _rowHeaders[i].UnregisterCallback(_rowHeaderClickHandlers[i]);
                }
            }

            if (_fadeCoroutine != null && _coroutineHost != null)
                _coroutineHost.StopCoroutine(_fadeCoroutine);
        }

        private void HandleUnitSelected(GameObject unit)
        {
            if (unit.layer != _cachedAllyLayer)
            {
                UntrackStats();
                Hide();
                return;
            }

            if (!unit.TryGetComponent<CombatStats>(out var stats))
            {
                Hide();
                return;
            }

            UntrackStats();
            _trackedStats = stats;
            _trackedStats.OnDamageTaken += HandleHpChanged;
            _trackedStats.OnHealed += HandleHpChanged;
            _trackedStats.OnDied += HandleDied;
            UpdateDisplay();
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

        private void UntrackStats()
        {
            if (_trackedStats == null) return;

            _trackedStats.OnDamageTaken -= HandleHpChanged;
            _trackedStats.OnHealed -= HandleHpChanged;
            _trackedStats.OnDied -= HandleDied;
            _trackedStats = null;
        }

        private void HandleHpChanged(int amount, int currentHp)
        {
            UpdateDisplay();
        }

        private void HandleDied()
        {
            CaptureSnapshot(_currentRosterIndex);
            UntrackStats();
            _displayingDeadUnit = true;
            if (_selectionManager != null)
                _selectionManager.ForceDeselect();
        }

        private void UpdateDisplay()
        {
            if (_trackedStats == null) return;

            var displayOrder = CombatStats.DisplayOrder;
            for (int i = 0; i < _statRows.Length && i < displayOrder.Count; i++)
            {
                var breakdown = _trackedStats.GetBreakdown(displayOrder[i]);
                _statRows[i].NameLabel.text = breakdown.StatName;
                _statRows[i].ValueLabel.text = breakdown.FinalValue;
            }

            if (_expandedRowIndex >= 0)
                PopulateBreakdownText(_expandedRowIndex);
        }

        private void Show()
        {
            if (_fadeCoroutine != null && _coroutineHost != null)
                _coroutineHost.StopCoroutine(_fadeCoroutine);

            _emptyLabel.AddToClassList(UssClasses.EmptyLabelHidden);
            _contentContainer.style.display = DisplayStyle.Flex;

            _fadeCoroutine = _coroutineHost.StartCoroutine(StaggeredFadeInCoroutine());

            SwitchTab(0);

            if (_expandedRowIndex >= 0)
                CollapseBreakdown(_expandedRowIndex);
        }

        private IEnumerator StaggeredFadeInCoroutine()
        {
            for (int i = 0; i < _statRows.Length; i++)
                _statRows[i].Root.style.opacity = 0f;

            for (int i = 0; i < _statRows.Length; i++)
            {
                float staggerElapsed = 0f;
                while (staggerElapsed < StaggerDelay)
                {
                    staggerElapsed += Time.deltaTime;
                    yield return null;
                }

                if (FadeDuration > 0f)
                {
                    float elapsed = 0f;
                    while (elapsed < FadeDuration)
                    {
                        elapsed += Time.deltaTime;
                        float t = Mathf.Clamp01(elapsed / FadeDuration);
                        float easeOut = 1f - (1f - t) * (1f - t);
                        _statRows[i].Root.style.opacity = easeOut;
                        yield return null;
                    }
                }

                _statRows[i].Root.style.opacity = 1f;
            }

            _fadeCoroutine = null;
        }

        private void Hide()
        {
            if (_fadeCoroutine != null && _coroutineHost != null)
            {
                _coroutineHost.StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }

            _emptyLabel.RemoveFromClassList(UssClasses.EmptyLabelHidden);
            _contentContainer.style.display = DisplayStyle.None;

            _expandedRowIndex = -1;
            for (int i = 0; i < _statRows.Length; i++)
            {
                _statRows[i].BreakdownContainer.AddToClassList(UssClasses.BreakdownHidden);
                _statRows[i].Root.style.opacity = 0f;
            }
        }

        internal void SwitchTab(int tabIndex)
        {
            if (tabIndex < 0 || tabIndex >= _tabButtons.Length) return;

            for (int i = 0; i < _tabButtons.Length; i++)
                _tabButtons[i].RemoveFromClassList(UssClasses.TabActive);

            _tabButtons[tabIndex].AddToClassList(UssClasses.TabActive);

            for (int i = 0; i < _tabContents.Length; i++)
            {
                if (i == tabIndex)
                    _tabContents[i].RemoveFromClassList(UssClasses.TabContentHidden);
                else
                    _tabContents[i].AddToClassList(UssClasses.TabContentHidden);
            }

            if (tabIndex == 0)
                UpdateDisplay();

            _activeTabIndex = tabIndex;
        }

        internal void ToggleBreakdown(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= _statRows.Length) return;

            if (_expandedRowIndex == rowIndex)
            {
                CollapseBreakdown(rowIndex);
                _expandedRowIndex = -1;
            }
            else
            {
                if (_expandedRowIndex >= 0)
                    CollapseBreakdown(_expandedRowIndex);

                _statRows[rowIndex].BreakdownContainer.RemoveFromClassList(UssClasses.BreakdownHidden);
                PopulateBreakdownText(rowIndex);
                _expandedRowIndex = rowIndex;
            }
        }

        private void CollapseBreakdown(int rowIndex)
        {
            if (rowIndex >= 0 && rowIndex < _statRows.Length)
                _statRows[rowIndex].BreakdownContainer.AddToClassList(UssClasses.BreakdownHidden);
        }

        private void PopulateBreakdownText(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= _statRows.Length || _trackedStats == null)
                return;

            var displayOrder = CombatStats.DisplayOrder;
            if (rowIndex >= displayOrder.Count) return;

            var breakdown = _trackedStats.GetBreakdown(displayOrder[rowIndex]);
            _stringBuilder.Clear();

            if (breakdown.Modifiers != null)
            {
                for (int i = 0; i < breakdown.Modifiers.Length; i++)
                {
                    var modifier = breakdown.Modifiers[i];
                    _stringBuilder.Append(modifier.Source).Append(": ").AppendLine(modifier.Value);
                }
            }

            _stringBuilder.AppendLine(UssClasses.BreakdownSeparator);
            _stringBuilder.Append("Total: ").Append(breakdown.FinalValue);

            _statRows[rowIndex].BreakdownText.text = _stringBuilder.ToString();
        }

        internal void NavigateToNextAlly()
        {
            if (_teamRoster.Count == 0) return;

            int nextIndex = (_currentRosterIndex + 1) % _teamRoster.Count;
            DisplayTeamMember(nextIndex);
        }

        internal void NavigateToPreviousAlly()
        {
            if (_teamRoster.Count == 0) return;

            int previousIndex = (_currentRosterIndex - 1 + _teamRoster.Count) % _teamRoster.Count;
            DisplayTeamMember(previousIndex);
        }

        private void RefreshTeamRoster(Transform teamContainer)
        {
            _teamRoster.Clear();

            if (teamContainer == null) return;

            for (int i = 0; i < teamContainer.childCount; i++)
            {
                var child = teamContainer.GetChild(i);
                if (child.TryGetComponent<CombatStats>(out _))
                    _teamRoster.Add(child.gameObject);
            }

            _snapshots = new StatSnapshot[_teamRoster.Count];
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

        private void UpdateTeamPositionLabel()
        {
            _stringBuilder.Clear();
            _stringBuilder.Append(_currentRosterIndex + 1).Append('/').Append(_teamRoster.Count);
            _teamPosLabel.text = _stringBuilder.ToString();
        }

        private void UpdateNameLabel()
        {
            if (_displayingDeadUnit && _currentRosterIndex >= 0
                && _snapshots != null && _currentRosterIndex < _snapshots.Length
                && _snapshots[_currentRosterIndex].IsValid)
            {
                _nameLabel.text = _snapshots[_currentRosterIndex].UnitName;
                return;
            }

            if (_currentRosterIndex >= 0 && _currentRosterIndex < _teamRoster.Count
                && _teamRoster[_currentRosterIndex] != null)
            {
                _nameLabel.text = _teamRoster[_currentRosterIndex].name;
                return;
            }

            _nameLabel.text = "";
        }

        private void CaptureSnapshot(int rosterIndex)
        {
            if (_trackedStats == null || rosterIndex < 0
                || _snapshots == null || rosterIndex >= _snapshots.Length)
                return;

            var displayOrder = CombatStats.DisplayOrder;
            var statValues = new string[displayOrder.Count];
            for (int i = 0; i < displayOrder.Count; i++)
                statValues[i] = _trackedStats.GetBreakdown(displayOrder[i]).FinalValue;

            _snapshots[rosterIndex] = new StatSnapshot
            {
                UnitName = _teamRoster[rosterIndex] != null ? _teamRoster[rosterIndex].name : "",
                StatValues = statValues,
                IsValid = true
            };
        }

        private void DisplaySnapshot(int rosterIndex)
        {
            if (_snapshots == null || rosterIndex < 0 || rosterIndex >= _snapshots.Length)
                return;

            var snapshot = _snapshots[rosterIndex];
            if (!snapshot.IsValid) return;

            for (int i = 0; i < _statRows.Length && i < snapshot.StatValues.Length; i++)
                _statRows[i].ValueLabel.text = snapshot.StatValues[i];
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
                DisplaySnapshot(rosterIndex);
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

        internal int ActiveTabIndex => _activeTabIndex;

        internal bool IsBreakdownExpanded(int rowIndex) =>
            rowIndex >= 0 && rowIndex < _statRows.Length
            && !_statRows[rowIndex].BreakdownContainer.ClassListContains(UssClasses.BreakdownHidden);

        internal string BreakdownText(int rowIndex) =>
            rowIndex >= 0 && rowIndex < _statRows.Length
                ? _statRows[rowIndex].BreakdownText.text : "";

        internal string StatValueText(int rowIndex) =>
            rowIndex >= 0 && rowIndex < _statRows.Length
                ? _statRows[rowIndex].ValueLabel.text : "";

        internal string StatNameText(int rowIndex) =>
            rowIndex >= 0 && rowIndex < _statRows.Length
                ? _statRows[rowIndex].NameLabel.text : "";

        internal bool IsVisible =>
            _contentContainer.resolvedStyle.display == DisplayStyle.Flex
            || _contentContainer.style.display == new StyleEnum<DisplayStyle>(DisplayStyle.Flex);

        internal bool IsEmptyStateLabelActive =>
            !_emptyLabel.ClassListContains(UssClasses.EmptyLabelHidden);

        internal float StatRowOpacity(int index) =>
            index >= 0 && index < _statRows.Length
                ? _statRows[index].Root.resolvedStyle.opacity
                : -1f;

        internal string NameLabelText => _nameLabel.text;
        internal string TeamPosLabelText => _teamPosLabel.text;
        internal int CurrentRosterIndex => _currentRosterIndex;
        internal int TeamRosterCount => _teamRoster.Count;
        internal bool IsDisplayingDeadUnit => _displayingDeadUnit;
    }
}
