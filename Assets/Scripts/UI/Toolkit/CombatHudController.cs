using RogueliteAutoBattler.Combat.Visuals;
using UnityEngine;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.UI.Toolkit
{
    public class CombatHudController : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private VisualTreeAsset _infoPanelTemplate;

        private GoldBadgeController _goldBadge;
        private BattleIndicatorController _battleIndicator;
        private StepProgressBarController _stepProgressBar;
        private VisualElement _goldBadgeElement;
        private AllyStatsPanelController _allyStatsPanel;

        internal GoldBadgeController GoldBadge => _goldBadge;
        internal AllyStatsPanelController AllyStatsPanel => _allyStatsPanel;
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

            _goldBadgeElement = goldBadgeElement;

            _goldBadge = new GoldBadgeController(goldBadgeElement, goldLabel, this);
            _battleIndicator = new BattleIndicatorController(battleCompactLabel, announcementOverlay, announcementLabel, this);
            _stepProgressBar = new StepProgressBarController(stepProgressContainer);

            _goldBadge.Initialize();
            _battleIndicator.Initialize();
            _stepProgressBar.Initialize();

            if (!TryQuery<VisualElement>(root, "info-area", out VisualElement infoArea)) return;

            if (_infoPanelTemplate != null)
            {
                _infoPanelTemplate.CloneTree(infoArea);

                var panelRoot = infoArea.Q<VisualElement>("info-panel-root");
                var emptyLabel = infoArea.Q<Label>("info-empty-label");
                var contentContainer = infoArea.Q<VisualElement>("info-content");
                var nameLabel = infoArea.Q<Label>("info-name-label");
                var teamPosLabel = infoArea.Q<Label>("info-team-pos-label");
                var prevButton = infoArea.Q<Button>("nav-prev-btn");
                var nextButton = infoArea.Q<Button>("nav-next-btn");

                var tabButtons = new[]
                {
                    infoArea.Q<Button>("info-tab-stats"),
                    infoArea.Q<Button>("info-tab-traits"),
                    infoArea.Q<Button>("info-tab-loot")
                };

                var statsScrollView = infoArea.Q<ScrollView>("info-tab-content-stats");
                var tabContents = new VisualElement[]
                {
                    statsScrollView,
                    infoArea.Q<VisualElement>("info-tab-content-traits"),
                    infoArea.Q<VisualElement>("info-tab-content-loot")
                };

                _allyStatsPanel = new AllyStatsPanelController(
                    panelRoot, emptyLabel, contentContainer,
                    nameLabel, teamPosLabel,
                    prevButton, nextButton,
                    tabButtons, tabContents,
                    statsScrollView, this);

                _allyStatsPanel.Initialize();
            }

            _goldBadgeElement.RegisterCallback<GeometryChangedEvent>(OnGoldBadgeGeometryChanged);
        }

        private void OnGoldBadgeGeometryChanged(GeometryChangedEvent evt)
        {
            Rect panelBound = _goldBadgeElement.worldBound;
            if (panelBound.width <= 0 || panelBound.height <= 0)
                return;

            Rect screenBound = PanelToScreenRect(panelBound);
            CoinFlyService.InitializeToolkitTarget(screenBound, () => _goldBadge.Punch());
            CoinFlyService.UpdateToolkitTargetBound(screenBound);
        }

        private Rect PanelToScreenRect(Rect panelRect)
        {
            VisualElement panelRoot = _goldBadgeElement.panel.visualTree;
            Rect fullPanel = panelRoot.worldBound;
            float scaleX = Screen.width / fullPanel.width;
            float scaleY = Screen.height / fullPanel.height;
            return new Rect(
                panelRect.x * scaleX,
                panelRect.y * scaleY,
                panelRect.width * scaleX,
                panelRect.height * scaleY);
        }

        private void OnDestroy()
        {
            _allyStatsPanel?.Dispose();
            _goldBadge?.Dispose();
            _battleIndicator?.Dispose();
            _stepProgressBar?.Dispose();

            if (_goldBadgeElement != null)
                _goldBadgeElement.UnregisterCallback<GeometryChangedEvent>(OnGoldBadgeGeometryChanged);
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
