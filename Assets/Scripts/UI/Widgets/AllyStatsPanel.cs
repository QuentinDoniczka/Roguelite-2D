using System.Collections;
using System.Collections.Generic;
using System.Text;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Common;
using RogueliteAutoBattler.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RogueliteAutoBattler.UI.Widgets
{
    public class AllyStatsPanel : MonoBehaviour
    {
        [Header("Visibility")]
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Empty State")]
        [SerializeField] private TMP_Text _emptyStateLabel;

        [Header("Tabs")]
        [SerializeField] private GameObject _tabHeaderContainer;
        [SerializeField] private GameObject[] _tabContents;
        [SerializeField] private Image[] _tabButtonImages;
        [SerializeField] private Color _tabActiveColor = new Color(0.24f, 0.24f, 0.31f, 1f);
        [SerializeField] private Color _tabInactiveColor = new Color(0.14f, 0.14f, 0.20f, 1f);

        [Header("Stat Rows")]
        [SerializeField] private TMP_Text[] _statValueLabels;
        [SerializeField] private TMP_Text[] _statNameLabels;
        [SerializeField] private GameObject[] _breakdownContainers;
        [SerializeField] private TMP_Text[] _breakdownTexts;
        [SerializeField] private CanvasGroup[] _statRowGroups;

        [Header("Scroll")]
        [SerializeField] private ScrollRect _scrollRect;

        [Header("Team Navigation")]
        [SerializeField] private TMP_Text _nameLabel;
        [SerializeField] private TMP_Text _teamPosLabel;
        [SerializeField] private Transform _teamContainer;

        [Header("Animation")]
        [SerializeField] private float _staggerDelay = 0.05f;
        [SerializeField] private float _fadeDuration = 0.15f;

        private UnitSelectionManager _selectionManager;
        private CombatStats _trackedStats;
        private bool _initializedForTest;
        private int _cachedAllyLayer;
        private Coroutine _fadeCoroutine;
        private int _activeTabIndex;
        private int _expandedRowIndex = -1;

        private readonly List<GameObject> _teamRoster = new List<GameObject>();
        private StatSnapshot[] _snapshots;
        private int _currentRosterIndex = -1;
        private bool _isSyncingSelection;
        private bool _displayingDeadUnit;

        private const string BreakdownSeparator = "\u2500\u2500\u2500\u2500\u2500\u2500";
        private readonly StringBuilder _stringBuilder = new StringBuilder(128);

        private struct StatSnapshot
        {
            internal string UnitName;
            internal string[] StatValues;
            internal bool IsValid;
        }

        private IEnumerator Start()
        {
            if (_initializedForTest) yield break;

            _cachedAllyLayer = PhysicsLayers.AllyLayer;

            _selectionManager = UnitSelectionManager.Instance;
            if (_selectionManager == null)
            {
                Debug.LogWarning($"[{nameof(AllyStatsPanel)}] UnitSelectionManager.Instance not found — panel will not track selections.", this);
                Hide();
                yield break;
            }

            _selectionManager.OnUnitSelected += HandleUnitSelected;
            _selectionManager.OnUnitDeselected += HandleUnitDeselected;

            Hide();

            if (_emptyStateLabel != null)
                _emptyStateLabel.gameObject.SetActive(true);

            if (_teamContainer == null)
            {
                var combatWorld = GameBootstrap.CombatWorld;
                if (combatWorld != null)
                    _teamContainer = combatWorld.Find(CombatSetupHelper.TeamContainerName);
            }

            if (_teamContainer != null)
            {
                yield return null;
                RefreshTeamRoster();
                if (_teamRoster.Count > 0)
                    DisplayTeamMember(0);
            }
        }

        private void OnDestroy()
        {
            if (_selectionManager != null)
            {
                _selectionManager.OnUnitSelected -= HandleUnitSelected;
                _selectionManager.OnUnitDeselected -= HandleUnitDeselected;
            }

            UntrackStats();
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

        private void HandleUnitDeselected()
        {
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
            _selectionManager?.ForceDeselect();
        }

        private void UpdateDisplay()
        {
            if (_trackedStats == null) return;

            var displayOrder = CombatStats.DisplayOrder;
            for (int i = 0; i < _statValueLabels.Length && i < displayOrder.Count; i++)
            {
                if (_statValueLabels[i] != null)
                {
                    var breakdown = _trackedStats.GetBreakdown(displayOrder[i]);
                    _statValueLabels[i].SetText(breakdown.FinalValue);
                }
            }

            if (_expandedRowIndex >= 0)
                PopulateBreakdownText(_expandedRowIndex);
        }

        private void Show()
        {
            if (_canvasGroup == null) return;

            if (_fadeCoroutine != null)
                StopCoroutine(_fadeCoroutine);

            if (_emptyStateLabel != null)
                _emptyStateLabel.gameObject.SetActive(false);

            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;

            _fadeCoroutine = StartCoroutine(StaggeredFadeInCoroutine());

            if (_tabContents != null)
                SwitchTab(0);

            if (_expandedRowIndex >= 0)
                CollapseBreakdown(_expandedRowIndex);
        }

        private IEnumerator StaggeredFadeInCoroutine()
        {
            if (_statRowGroups == null || _statRowGroups.Length == 0)
                yield break;

            for (int i = 0; i < _statRowGroups.Length; i++)
                if (_statRowGroups[i] != null)
                    _statRowGroups[i].alpha = 0f;

            for (int i = 0; i < _statRowGroups.Length; i++)
            {
                if (_statRowGroups[i] == null) continue;

                float staggerElapsed = 0f;
                while (staggerElapsed < _staggerDelay)
                {
                    staggerElapsed += Time.deltaTime;
                    yield return null;
                }

                if (_fadeDuration > 0f)
                {
                    float elapsed = 0f;
                    while (elapsed < _fadeDuration)
                    {
                        elapsed += Time.deltaTime;
                        float t = Mathf.Clamp01(elapsed / _fadeDuration);
                        float easeOut = 1f - (1f - t) * (1f - t);
                        _statRowGroups[i].alpha = easeOut;
                        yield return null;
                    }
                }
                _statRowGroups[i].alpha = 1f;
            }
            _fadeCoroutine = null;
        }

        private void Hide()
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }

            if (_canvasGroup == null) return;
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;

            if (_emptyStateLabel != null)
                _emptyStateLabel.gameObject.SetActive(true);

            _expandedRowIndex = -1;
            if (_breakdownContainers != null)
            {
                for (int i = 0; i < _breakdownContainers.Length; i++)
                    if (_breakdownContainers[i] != null)
                        _breakdownContainers[i].SetActive(false);
            }

            if (_statRowGroups != null)
            {
                for (int i = 0; i < _statRowGroups.Length; i++)
                    if (_statRowGroups[i] != null)
                        _statRowGroups[i].alpha = 0f;
            }
        }

        public void SwitchTab(int tabIndex)
        {
            if (_tabContents == null || tabIndex < 0 || tabIndex >= _tabContents.Length) return;

            for (int i = 0; i < _tabContents.Length; i++)
                if (_tabContents[i] != null)
                    _tabContents[i].SetActive(false);

            if (_tabContents[tabIndex] != null)
                _tabContents[tabIndex].SetActive(true);

            if (_tabButtonImages != null)
            {
                for (int i = 0; i < _tabButtonImages.Length; i++)
                    if (_tabButtonImages[i] != null)
                        _tabButtonImages[i].color = _tabInactiveColor;

                if (tabIndex < _tabButtonImages.Length && _tabButtonImages[tabIndex] != null)
                    _tabButtonImages[tabIndex].color = _tabActiveColor;
            }

            if (tabIndex == 0)
                UpdateDisplay();

            _activeTabIndex = tabIndex;
        }

        public void ToggleBreakdown(int rowIndex)
        {
            if (_breakdownContainers == null || rowIndex < 0 || rowIndex >= _breakdownContainers.Length) return;

            if (_expandedRowIndex == rowIndex)
            {
                CollapseBreakdown(rowIndex);
                _expandedRowIndex = -1;
            }
            else
            {
                if (_expandedRowIndex >= 0)
                    CollapseBreakdown(_expandedRowIndex);

                if (_breakdownContainers[rowIndex] != null)
                    _breakdownContainers[rowIndex].SetActive(true);

                PopulateBreakdownText(rowIndex);
                _expandedRowIndex = rowIndex;
            }

            if (_scrollRect != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);
        }

        private void CollapseBreakdown(int rowIndex)
        {
            if (_breakdownContainers != null && rowIndex >= 0 && rowIndex < _breakdownContainers.Length
                && _breakdownContainers[rowIndex] != null)
                _breakdownContainers[rowIndex].SetActive(false);
        }

        private void PopulateBreakdownText(int rowIndex)
        {
            if (_breakdownTexts == null || rowIndex < 0 || rowIndex >= _breakdownTexts.Length
                || _breakdownTexts[rowIndex] == null || _trackedStats == null)
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

            _stringBuilder.AppendLine(BreakdownSeparator);
            _stringBuilder.Append("Total: ").Append(breakdown.FinalValue);

            _breakdownTexts[rowIndex].SetText(_stringBuilder.ToString());
        }

        public void NavigateToNextAlly()
        {
            if (_teamRoster.Count == 0) return;

            int nextIndex = (_currentRosterIndex + 1) % _teamRoster.Count;
            DisplayTeamMember(nextIndex);
        }

        public void NavigateToPreviousAlly()
        {
            if (_teamRoster.Count == 0) return;

            int previousIndex = (_currentRosterIndex - 1 + _teamRoster.Count) % _teamRoster.Count;
            DisplayTeamMember(previousIndex);
        }

        private void RefreshTeamRoster()
        {
            _teamRoster.Clear();

            if (_teamContainer == null) return;

            for (int i = 0; i < _teamContainer.childCount; i++)
            {
                var child = _teamContainer.GetChild(i);
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
            if (_teamPosLabel == null) return;

            _stringBuilder.Clear();
            _stringBuilder.Append(_currentRosterIndex + 1).Append('/').Append(_teamRoster.Count);
            _teamPosLabel.SetText(_stringBuilder.ToString());
        }

        private void UpdateNameLabel()
        {
            if (_nameLabel == null) return;

            if (_displayingDeadUnit && _currentRosterIndex >= 0
                && _snapshots != null && _currentRosterIndex < _snapshots.Length
                && _snapshots[_currentRosterIndex].IsValid)
            {
                _nameLabel.SetText(_snapshots[_currentRosterIndex].UnitName);
                return;
            }

            if (_currentRosterIndex >= 0 && _currentRosterIndex < _teamRoster.Count
                && _teamRoster[_currentRosterIndex] != null)
            {
                _nameLabel.SetText(_teamRoster[_currentRosterIndex].name);
                return;
            }

            _nameLabel.SetText("");
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

            for (int i = 0; i < _statValueLabels.Length && i < snapshot.StatValues.Length; i++)
            {
                if (_statValueLabels[i] != null)
                    _statValueLabels[i].SetText(snapshot.StatValues[i]);
            }
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
                _selectionManager?.ForceDeselect();
            }
            else
            {
                _isSyncingSelection = true;
                _selectionManager?.ForceSelect(unit);
                _isSyncingSelection = false;
            }

            UpdateNameLabel();
            UpdateTeamPositionLabel();
        }

        internal int ActiveTabIndex => _activeTabIndex;

        internal bool IsBreakdownExpanded(int rowIndex) =>
            _breakdownContainers != null && rowIndex >= 0 && rowIndex < _breakdownContainers.Length
            && _breakdownContainers[rowIndex] != null && _breakdownContainers[rowIndex].activeSelf;

        internal string BreakdownText(int rowIndex) =>
            _breakdownTexts != null && rowIndex >= 0 && rowIndex < _breakdownTexts.Length && _breakdownTexts[rowIndex] != null
                ? _breakdownTexts[rowIndex].text : "";

        internal string StatValueText(int rowIndex) =>
            _statValueLabels != null && rowIndex >= 0 && rowIndex < _statValueLabels.Length && _statValueLabels[rowIndex] != null
                ? _statValueLabels[rowIndex].text : "";

        internal string StatNameText(int rowIndex) =>
            _statNameLabels != null && rowIndex >= 0 && rowIndex < _statNameLabels.Length && _statNameLabels[rowIndex] != null
                ? _statNameLabels[rowIndex].text : "";

        internal float PanelAlpha => _canvasGroup != null ? _canvasGroup.alpha : 0f;
        internal bool IsVisible => _canvasGroup != null && _canvasGroup.alpha > 0f;

        internal bool IsEmptyStateLabelActive => _emptyStateLabel != null && _emptyStateLabel.gameObject.activeSelf;

        internal float StatRowAlpha(int index) =>
            _statRowGroups != null && index >= 0 && index < _statRowGroups.Length && _statRowGroups[index] != null
                ? _statRowGroups[index].alpha
                : -1f;

        internal string NameLabelText => _nameLabel != null ? _nameLabel.text : "";
        internal string TeamPosLabelText => _teamPosLabel != null ? _teamPosLabel.text : "";
        internal int CurrentRosterIndex => _currentRosterIndex;
        internal int TeamRosterCount => _teamRoster.Count;
        internal bool IsDisplayingDeadUnit => _displayingDeadUnit;

        internal void InitializeForTest(
            UnitSelectionManager selectionManager,
            CanvasGroup canvasGroup,
            int allyLayer,
            TMP_Text emptyStateLabel,
            TMP_Text[] statValueLabels,
            TMP_Text[] statNameLabels,
            CanvasGroup[] statRowGroups,
            GameObject[] breakdownContainers,
            TMP_Text[] breakdownTexts,
            GameObject tabHeaderContainer,
            GameObject[] tabContents,
            Image[] tabButtonImages,
            Color tabActiveColor,
            Color tabInactiveColor,
            Transform teamContainer = null,
            TMP_Text nameLabel = null,
            TMP_Text teamPosLabel = null,
            ScrollRect scrollRect = null)
        {
            _selectionManager = selectionManager;
            _canvasGroup = canvasGroup;
            _cachedAllyLayer = allyLayer;
            _emptyStateLabel = emptyStateLabel;
            _statValueLabels = statValueLabels;
            _statNameLabels = statNameLabels;
            _statRowGroups = statRowGroups;
            _breakdownContainers = breakdownContainers;
            _breakdownTexts = breakdownTexts;
            _tabHeaderContainer = tabHeaderContainer;
            _tabContents = tabContents;
            _tabButtonImages = tabButtonImages;
            _tabActiveColor = tabActiveColor;
            _tabInactiveColor = tabInactiveColor;
            _teamContainer = teamContainer;
            _nameLabel = nameLabel;
            _teamPosLabel = teamPosLabel;
            _scrollRect = scrollRect;

            FinalizeTestInitialization();
        }

        private void FinalizeTestInitialization()
        {
            _initializedForTest = true;

            _selectionManager.OnUnitSelected += HandleUnitSelected;
            _selectionManager.OnUnitDeselected += HandleUnitDeselected;

            Hide();

            if (_emptyStateLabel != null)
                _emptyStateLabel.gameObject.SetActive(true);

            RefreshTeamRoster();
            if (_teamRoster.Count > 0)
                DisplayTeamMember(0);
        }
    }
}
