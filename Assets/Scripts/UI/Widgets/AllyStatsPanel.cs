using System.Collections;
using System.Text;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RogueliteAutoBattler.UI.Widgets
{
    public class AllyStatsPanel : MonoBehaviour
    {
        [Header("Labels")]
        [SerializeField] private TMP_Text _hpLabel;
        [SerializeField] private TMP_Text _atkLabel;
        [SerializeField] private TMP_Text _attackSpeedLabel;

        [Header("Visibility")]
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Empty State")]
        [SerializeField] private TMP_Text _emptyStateLabel;

        [Header("Stat Cards")]
        [SerializeField] private CanvasGroup[] _statCardGroups;

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

        private UnitSelectionManager _selectionManager;
        private CombatStats _trackedStats;
        private bool _initializedForTest;
        private int _cachedAllyLayer;
        private Coroutine _fadeCoroutine;
        private int _activeTabIndex;
        private int _expandedRowIndex = -1;

        private const string BreakdownSeparator = "\u2500\u2500\u2500\u2500\u2500\u2500";
        private readonly StringBuilder _stringBuilder = new StringBuilder(128);

        private void Start()
        {
            if (_initializedForTest) return;

            _cachedAllyLayer = PhysicsLayers.AllyLayer;

            _selectionManager = UnitSelectionManager.Instance;
            if (_selectionManager == null)
            {
                Debug.LogWarning($"[{nameof(AllyStatsPanel)}] UnitSelectionManager.Instance not found — panel will not track selections.", this);
                Hide();
                return;
            }

            _selectionManager.OnUnitSelected += HandleUnitSelected;
            _selectionManager.OnUnitDeselected += HandleUnitDeselected;

            Hide();

            if (_emptyStateLabel != null)
                _emptyStateLabel.gameObject.SetActive(true);
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
        }

        private void HandleUnitDeselected()
        {
            UntrackStats();
            Hide();
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
            Hide();
        }

        private void UpdateDisplay()
        {
            if (_trackedStats == null) return;

            if (_hpLabel != null)
                _hpLabel.SetText("{0} / {1}", _trackedStats.CurrentHp, _trackedStats.MaxHp);

            if (_atkLabel != null)
                _atkLabel.SetText("{0}", _trackedStats.Atk);

            if (_attackSpeedLabel != null)
                _attackSpeedLabel.SetText("{0:1}", _trackedStats.AttackSpeed);

            if (_statValueLabels != null)
            {
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
            var groups = (_statRowGroups != null && _statRowGroups.Length > 0) ? _statRowGroups : _statCardGroups;

            if (groups == null || groups.Length == 0)
                yield break;

            for (int i = 0; i < groups.Length; i++)
                if (groups[i] != null)
                    groups[i].alpha = 0f;

            const float staggerDelay = 0.05f;
            const float fadeDuration = 0.15f;

            for (int i = 0; i < groups.Length; i++)
            {
                if (groups[i] == null) continue;

                float staggerElapsed = 0f;
                while (staggerElapsed < staggerDelay)
                {
                    staggerElapsed += Time.deltaTime;
                    yield return null;
                }

                float elapsed = 0f;
                while (elapsed < fadeDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / fadeDuration);
                    float easeOut = 1f - (1f - t) * (1f - t);
                    groups[i].alpha = easeOut;
                    yield return null;
                }
                groups[i].alpha = 1f;
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

            if (_statCardGroups != null)
            {
                for (int i = 0; i < _statCardGroups.Length; i++)
                    if (_statCardGroups[i] != null)
                        _statCardGroups[i].alpha = 0f;
            }

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

        internal string HpText => _hpLabel != null ? _hpLabel.text : "";
        internal string AtkText => _atkLabel != null ? _atkLabel.text : "";
        internal string AttackSpeedText => _attackSpeedLabel != null ? _attackSpeedLabel.text : "";
        internal float PanelAlpha => _canvasGroup != null ? _canvasGroup.alpha : 0f;
        internal bool IsVisible => _canvasGroup != null && _canvasGroup.alpha > 0f;

        internal bool IsEmptyStateLabelActive => _emptyStateLabel != null && _emptyStateLabel.gameObject.activeSelf;

        internal float StatCardAlpha(int index) =>
            _statCardGroups != null && index >= 0 && index < _statCardGroups.Length && _statCardGroups[index] != null
                ? _statCardGroups[index].alpha
                : -1f;

        internal void InitializeForTest(UnitSelectionManager selectionManager,
            TMP_Text hpLabel, TMP_Text atkLabel, TMP_Text attackSpeedLabel,
            CanvasGroup canvasGroup, int allyLayer,
            TMP_Text emptyStateLabel = null, CanvasGroup[] statCardGroups = null)
        {
            _selectionManager = selectionManager;
            _hpLabel = hpLabel;
            _atkLabel = atkLabel;
            _attackSpeedLabel = attackSpeedLabel;
            _canvasGroup = canvasGroup;
            _cachedAllyLayer = allyLayer;
            _emptyStateLabel = emptyStateLabel;
            _statCardGroups = statCardGroups;

            FinalizeTestInitialization();
        }

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
        }
    }
}
