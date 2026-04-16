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

        public VisualElement GoldBadgeElement { get; private set; }

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

            VisualElement goldBadgeElement = root.Q<VisualElement>("gold-badge");
            if (goldBadgeElement == null)
            {
                Debug.LogWarning("[CombatHudController] Element 'gold-badge' not found.");
                return;
            }

            Label goldLabel = root.Q<Label>("gold-label");
            if (goldLabel == null)
            {
                Debug.LogWarning("[CombatHudController] Element 'gold-label' not found.");
                return;
            }

            Label battleCompactLabel = root.Q<Label>("battle-compact-label");
            if (battleCompactLabel == null)
            {
                Debug.LogWarning("[CombatHudController] Element 'battle-compact-label' not found.");
                return;
            }

            VisualElement announcementOverlay = root.Q<VisualElement>("announcement-overlay");
            if (announcementOverlay == null)
            {
                Debug.LogWarning("[CombatHudController] Element 'announcement-overlay' not found.");
                return;
            }

            Label announcementLabel = root.Q<Label>("announcement-label");
            if (announcementLabel == null)
            {
                Debug.LogWarning("[CombatHudController] Element 'announcement-label' not found.");
                return;
            }

            VisualElement stepProgressContainer = root.Q<VisualElement>("step-progress-container");
            if (stepProgressContainer == null)
            {
                Debug.LogWarning("[CombatHudController] Element 'step-progress-container' not found.");
                return;
            }

            GoldBadgeElement = goldBadgeElement;

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
    }
}
