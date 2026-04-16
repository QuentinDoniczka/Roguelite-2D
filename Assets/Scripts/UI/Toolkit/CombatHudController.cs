using UnityEngine;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.UI.Toolkit
{
    public class CombatHudController : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;

        private GoldBadgeController _goldBadge;
        private BattleIndicatorController _battleIndicator;
        private StepProgressBarController _stepProgressBar;

        internal GoldBadgeController GoldBadge => _goldBadge;
        internal BattleIndicatorController BattleIndicator => _battleIndicator;
        internal StepProgressBarController StepProgressBar => _stepProgressBar;

        private void Awake()
        {
            if (_uiDocument == null)
            {
                Debug.LogWarning("[CombatHudController] UIDocument is not assigned.");
                return;
            }

            VisualElement root = _uiDocument.rootVisualElement;
            if (root == null)
            {
                Debug.LogWarning("[CombatHudController] rootVisualElement is null.");
                return;
            }

            if (!TryQuery<VisualElement>(root, "gold-badge", out VisualElement goldBadgeElement)) return;
            if (!TryQuery<Label>(root, "gold-label", out Label goldLabel)) return;
            if (!TryQuery<Label>(root, "battle-compact-label", out Label battleCompactLabel)) return;
            if (!TryQuery<VisualElement>(root, "announcement-overlay", out VisualElement announcementOverlay)) return;
            if (!TryQuery<Label>(root, "announcement-label", out Label announcementLabel)) return;
            if (!TryQuery<VisualElement>(root, "step-progress-container", out VisualElement stepProgressContainer)) return;

            _goldBadge = new GoldBadgeController(goldBadgeElement, goldLabel, this);
            _battleIndicator = new BattleIndicatorController(battleCompactLabel, announcementOverlay, announcementLabel, this);
            _stepProgressBar = new StepProgressBarController(stepProgressContainer);

            _goldBadge.Initialize();
            _battleIndicator.Initialize();
            _stepProgressBar.Initialize();
        }

        private void OnDestroy()
        {
            _goldBadge?.Dispose();
            _battleIndicator?.Dispose();
            _stepProgressBar?.Dispose();
        }

        private void Update()
        {
            _stepProgressBar?.UpdateDotPosition();
        }

        private static bool TryQuery<T>(VisualElement root, string elementName, out T result)
            where T : VisualElement
        {
            result = root.Q<T>(elementName);
            if (result != null) return true;

            Debug.LogWarning($"[CombatHudController] Element '{elementName}' not found.");
            return false;
        }
    }
}
