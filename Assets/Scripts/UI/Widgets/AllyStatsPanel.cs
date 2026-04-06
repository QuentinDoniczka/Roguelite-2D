using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Common;
using TMPro;
using UnityEngine;

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

        private UnitSelectionManager _selectionManager;
        private CombatStats _trackedStats;
        private bool _initializedForTest;
        private int _cachedAllyLayer;

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
        }

        private void Show()
        {
            if (_canvasGroup == null) return;
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
        }

        private void Hide()
        {
            if (_canvasGroup == null) return;
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
        }

        internal string HpText => _hpLabel != null ? _hpLabel.text : "";
        internal string AtkText => _atkLabel != null ? _atkLabel.text : "";
        internal string AttackSpeedText => _attackSpeedLabel != null ? _attackSpeedLabel.text : "";
        internal float PanelAlpha => _canvasGroup != null ? _canvasGroup.alpha : 0f;
        internal bool IsVisible => _canvasGroup != null && _canvasGroup.alpha > 0f;

        internal void InitializeForTest(UnitSelectionManager selectionManager,
            TMP_Text hpLabel, TMP_Text atkLabel, TMP_Text attackSpeedLabel,
            CanvasGroup canvasGroup, int allyLayer)
        {
            _initializedForTest = true;
            _selectionManager = selectionManager;
            _hpLabel = hpLabel;
            _atkLabel = atkLabel;
            _attackSpeedLabel = attackSpeedLabel;
            _canvasGroup = canvasGroup;
            _cachedAllyLayer = allyLayer;

            _selectionManager.OnUnitSelected += HandleUnitSelected;
            _selectionManager.OnUnitDeselected += HandleUnitDeselected;

            Hide();
        }
    }
}
