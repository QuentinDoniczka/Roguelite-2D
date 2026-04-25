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
        private const float StaggerDelay = 0.025f;
        private const float FadeDuration = 0.075f;
        private const string BreakdownSectionDividerLine = "\u2500\u2500\u2500\u2500\u2500\u2500";

        private static class UssClasses
        {
            internal const string TabActive = "info-tab--active";
            internal const string TabContentHidden = "info-tab-content--hidden";
            internal const string BreakdownHidden = "stat-breakdown--hidden";
            internal const string EmptyLabelHidden = "info-empty-label--hidden";
            internal const string NameLabelDead = "info-name-label--dead";
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
        private readonly StringBuilder _stringBuilder = new StringBuilder(128);

        private UnitSelectionManager _selectionManager;
        private TeamRoster _teamRosterRef;
        private IReadOnlyList<TeamMember> _members;
        private CombatStats _trackedStats;
        private int _currentRosterIndex = -1;
        private int _activeTabIndex;
        private int _expandedRowIndex = -1;
        private int _cachedAllyLayer;
        private bool _isSyncingSelection;
        private bool _isDisplayingDeadUnit;
        private Coroutine _fadeCoroutine;

        private struct StatRowView
        {
            internal VisualElement Root;
            internal Label NameLabel;
            internal Label ValueLabel;
            internal VisualElement BreakdownContainer;
            internal Label BreakdownText;
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

            var teamRoster = GameBootstrap.TeamRoster;
            if (teamRoster == null)
                Debug.LogError($"[{nameof(AllyStatsPanelController)}] TeamRoster not found on GameBootstrap.");

            InitializeCore(selectionManager, teamRoster);
        }

        internal void InitializeForTest(UnitSelectionManager selectionManager, TeamRoster roster = null)
        {
            InitializeCore(selectionManager, roster);
        }

        private void InitializeCore(UnitSelectionManager selectionManager, TeamRoster roster)
        {
            _cachedAllyLayer = PhysicsLayers.AllyLayer;
            _selectionManager = selectionManager;
            _selectionManager.OnUnitSelected += HandleUnitSelected;

            Hide();

            BindRoster(roster);

            if (_members != null && _members.Count > 0)
                DisplayTeamMember(0);
        }

        public void BindRoster(TeamRoster roster)
        {
            if (_teamRosterRef != null)
            {
                _teamRosterRef.OnMemberRevived -= OnMemberRevived;
                _teamRosterRef.OnMemberDied -= OnMemberDiedRefreshPanel;
            }

            _teamRosterRef = roster;
            _members = roster?.Members;

            if (roster != null)
            {
                roster.OnMemberRevived += OnMemberRevived;
                roster.OnMemberDied += OnMemberDiedRefreshPanel;
            }
        }

        public void Dispose()
        {
            UntrackStats();

            if (_selectionManager != null)
                _selectionManager.OnUnitSelected -= HandleUnitSelected;

            if (_teamRosterRef != null)
            {
                _teamRosterRef.OnMemberRevived -= OnMemberRevived;
                _teamRosterRef.OnMemberDied -= OnMemberDiedRefreshPanel;
            }

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
                _isDisplayingDeadUnit = false;
                UpdateNameLabel(unit.name, isDead: false);
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
            UntrackStats();
            _isDisplayingDeadUnit = true;
            if (_selectionManager != null)
                _selectionManager.ForceDeselect();
        }

        private void OnMemberRevived(TeamMember member)
        {
            if (_members == null || member == null) return;

            if (member.Index == _currentRosterIndex)
                DisplayTeamMember(_currentRosterIndex);
        }

        private void OnMemberDiedRefreshPanel(TeamMember member)
        {
            if (_members == null || member == null) return;

            if (member.Index == _currentRosterIndex)
                DisplayTeamMember(_currentRosterIndex);
        }

        private void UpdateDisplay()
        {
            if (_trackedStats == null) return;

            WriteStatsToRows(_trackedStats);

            if (_expandedRowIndex >= 0)
                PopulateBreakdownText(_expandedRowIndex);
        }

        private void WriteStatsToRows(CombatStats stats)
        {
            if (stats == null) return;

            var displayOrder = CombatStats.DisplayOrder;
            int count = _statRows.Length < displayOrder.Count ? _statRows.Length : displayOrder.Count;
            for (int i = 0; i < count; i++)
            {
                var breakdown = stats.GetBreakdown(displayOrder[i]);
                _statRows[i].NameLabel.text = breakdown.StatName;
                _statRows[i].ValueLabel.text = breakdown.FinalValue;
            }
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

            _stringBuilder.AppendLine(BreakdownSectionDividerLine);
            _stringBuilder.Append("Total: ").Append(breakdown.FinalValue);

            _statRows[rowIndex].BreakdownText.text = _stringBuilder.ToString();
        }

        internal void NavigateToNextAlly()
        {
            if (_members == null || _members.Count == 0) return;

            int nextIndex = (_currentRosterIndex + 1) % _members.Count;
            DisplayTeamMember(nextIndex);
        }

        internal void NavigateToPreviousAlly()
        {
            if (_members == null || _members.Count == 0) return;

            int previousIndex = (_currentRosterIndex - 1 + _members.Count) % _members.Count;
            DisplayTeamMember(previousIndex);
        }

        private int FindRosterIndex(GameObject unit)
        {
            if (_members == null) return -1;

            for (int i = 0; i < _members.Count; i++)
            {
                if (_members[i].GameObject == unit)
                    return i;
            }

            return -1;
        }

        private void UpdateTeamPositionLabel()
        {
            int count = _members?.Count ?? 0;
            _stringBuilder.Clear();
            _stringBuilder.Append(_currentRosterIndex + 1).Append('/').Append(count);
            _teamPosLabel.text = _stringBuilder.ToString();
        }

        private void UpdateNameLabel(string name, bool isDead = false)
        {
            _nameLabel.text = name;
            _nameLabel.EnableInClassList(UssClasses.NameLabelDead, isDead);
        }

        private void DisplayTeamMember(int rosterIndex)
        {
            _currentRosterIndex = rosterIndex;

            var member = _members[rosterIndex];
            bool isDead = member.IsDead;

            if (isDead)
            {
                _isDisplayingDeadUnit = true;
                UntrackStats();
                WriteStatsToRows(member.Stats);
                Show();
                UpdateNameLabel(member.GameObject != null ? member.GameObject.name : "", isDead: true);
                if (_selectionManager != null)
                    _selectionManager.ForceDeselect();
            }
            else
            {
                _isDisplayingDeadUnit = false;
                _isSyncingSelection = true;
                if (_selectionManager != null)
                    _selectionManager.ForceSelect(member.GameObject);
                _isSyncingSelection = false;
                UpdateNameLabel(member.GameObject != null ? member.GameObject.name : "", isDead: false);
            }

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
        internal int TeamRosterCount => _members?.Count ?? 0;
        internal bool IsDisplayingDeadUnit => _isDisplayingDeadUnit;
        internal bool IsNameLabelDeadMarked => _nameLabel.ClassListContains(UssClasses.NameLabelDead);
    }
}
