using System.Collections;
using RogueliteAutoBattler.Combat.Visuals;
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
        private VisualElement _goldBadgeElement;
        private AllyStatsPanelController _allyStatsPanel;

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

            if (!TryQuery<VisualElement>(root, "info-panel-root", out VisualElement panelRoot)) return;
            if (!TryQuery<Label>(root, "info-empty-label", out Label emptyLabel)) return;
            if (!TryQuery<VisualElement>(root, "info-content", out VisualElement contentContainer)) return;
            if (!TryQuery<Label>(root, "info-name-label", out Label nameLabel)) return;
            if (!TryQuery<Label>(root, "info-team-pos-label", out Label teamPosLabel)) return;
            if (!TryQuery<Button>(root, "nav-prev-btn", out Button prevButton)) return;
            if (!TryQuery<Button>(root, "nav-next-btn", out Button nextButton)) return;
            if (!TryQuery<Button>(root, "info-tab-stats", out Button tabStats)) return;
            if (!TryQuery<Button>(root, "info-tab-traits", out Button tabTraits)) return;
            if (!TryQuery<Button>(root, "info-tab-loot", out Button tabLoot)) return;
            if (!TryQuery<ScrollView>(root, "info-tab-content-stats", out ScrollView statsScrollView)) return;
            if (!TryQuery<VisualElement>(root, "info-tab-content-traits", out VisualElement traitsContent)) return;
            if (!TryQuery<VisualElement>(root, "info-tab-content-loot", out VisualElement lootContent)) return;

            var tabButtons = new[] { tabStats, tabTraits, tabLoot };
            var tabContents = new VisualElement[] { statsScrollView, traitsContent, lootContent };

            _allyStatsPanel = new AllyStatsPanelController(
                panelRoot, emptyLabel, contentContainer,
                nameLabel, teamPosLabel,
                prevButton, nextButton,
                tabButtons, tabContents,
                statsScrollView, this);

            _goldBadgeElement.RegisterCallback<GeometryChangedEvent>(OnGoldBadgeGeometryChanged);
        }

        private void Start()
        {
            StartCoroutine(InitializeEndOfFrame());
        }

        private IEnumerator InitializeEndOfFrame()
        {
            yield return new WaitForEndOfFrame();
            _allyStatsPanel?.Initialize();
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
            if (_goldBadgeElement != null)
                _goldBadgeElement.UnregisterCallback<GeometryChangedEvent>(OnGoldBadgeGeometryChanged);

            _allyStatsPanel?.Dispose();
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
