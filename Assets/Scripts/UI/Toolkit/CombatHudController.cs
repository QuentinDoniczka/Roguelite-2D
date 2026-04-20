using System.Collections;
using RogueliteAutoBattler.Combat.Visuals;
using RogueliteAutoBattler.Economy;
using UnityEngine;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.UI.Toolkit
{
    public class CombatHudController : MonoBehaviour
    {
        private const string LogTag = "[CombatHudController]";
        private const string GoldBadgeElementName = "gold-badge";
        private const string GoldLabelElementName = "gold-label";
        private const string BattleCompactLabelElementName = "battle-compact-label";
        private const string AnnouncementOverlayElementName = "announcement-overlay";
        private const string AnnouncementLabelElementName = "announcement-label";
        private const string StepProgressContainerElementName = "step-progress-container";
        private const string InfoPanelRootElementName = "info-panel-root";
        private const string InfoEmptyLabelElementName = "info-empty-label";
        private const string InfoContentElementName = "info-content";
        private const string InfoNameLabelElementName = "info-name-label";
        private const string InfoTeamPosLabelElementName = "info-team-pos-label";
        private const string NavPrevButtonElementName = "nav-prev-btn";
        private const string NavNextButtonElementName = "nav-next-btn";
        private const string InfoTabStatsButtonElementName = "info-tab-stats";
        private const string InfoTabTraitsButtonElementName = "info-tab-traits";
        private const string InfoTabLootButtonElementName = "info-tab-loot";
        private const string InfoTabStatsContentElementName = "info-tab-content-stats";
        private const string InfoTabTraitsContentElementName = "info-tab-content-traits";
        private const string InfoTabLootContentElementName = "info-tab-content-loot";

        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private GoldWallet _goldWallet;

        private GoldBadgeController _goldBadge;
        private BattleIndicatorController _battleIndicator;
        private StepProgressBarController _stepProgressBar;
        private VisualElement _goldBadgeElement;
        private AllyStatsPanelController _allyStatsPanel;

        private void Awake()
        {
            if (_uiDocument == null)
            {
                Debug.LogWarning($"{LogTag} UIDocument is not assigned.");
                return;
            }

            VisualElement root = _uiDocument.rootVisualElement;
            if (root == null)
            {
                Debug.LogWarning($"{LogTag} rootVisualElement is null.");
                return;
            }

            if (!TryQuery<VisualElement>(root, GoldBadgeElementName, out VisualElement goldBadgeElement)) return;
            if (!TryQuery<Label>(root, GoldLabelElementName, out Label goldLabel)) return;
            if (!TryQuery<Label>(root, BattleCompactLabelElementName, out Label battleCompactLabel)) return;
            if (!TryQuery<VisualElement>(root, AnnouncementOverlayElementName, out VisualElement announcementOverlay)) return;
            if (!TryQuery<Label>(root, AnnouncementLabelElementName, out Label announcementLabel)) return;
            if (!TryQuery<VisualElement>(root, StepProgressContainerElementName, out VisualElement stepProgressContainer)) return;

            _goldBadgeElement = goldBadgeElement;

            _goldBadge = new GoldBadgeController(goldBadgeElement, goldLabel, this);
            _battleIndicator = new BattleIndicatorController(battleCompactLabel, announcementOverlay, announcementLabel, this);
            _stepProgressBar = new StepProgressBarController(stepProgressContainer);

            _goldBadge.Initialize(_goldWallet);
            _battleIndicator.Initialize();
            _stepProgressBar.Initialize();

            if (!TryQuery<VisualElement>(root, InfoPanelRootElementName, out VisualElement panelRoot)) return;
            if (!TryQuery<Label>(root, InfoEmptyLabelElementName, out Label emptyLabel)) return;
            if (!TryQuery<VisualElement>(root, InfoContentElementName, out VisualElement contentContainer)) return;
            if (!TryQuery<Label>(root, InfoNameLabelElementName, out Label nameLabel)) return;
            if (!TryQuery<Label>(root, InfoTeamPosLabelElementName, out Label teamPosLabel)) return;
            if (!TryQuery<Button>(root, NavPrevButtonElementName, out Button prevButton)) return;
            if (!TryQuery<Button>(root, NavNextButtonElementName, out Button nextButton)) return;
            if (!TryQuery<Button>(root, InfoTabStatsButtonElementName, out Button tabStats)) return;
            if (!TryQuery<Button>(root, InfoTabTraitsButtonElementName, out Button tabTraits)) return;
            if (!TryQuery<Button>(root, InfoTabLootButtonElementName, out Button tabLoot)) return;
            if (!TryQuery<ScrollView>(root, InfoTabStatsContentElementName, out ScrollView statsScrollView)) return;
            if (!TryQuery<VisualElement>(root, InfoTabTraitsContentElementName, out VisualElement traitsContent)) return;
            if (!TryQuery<VisualElement>(root, InfoTabLootContentElementName, out VisualElement lootContent)) return;

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
            if (panelBound.width <= 0f || panelBound.height <= 0f)
                return;

            Rect screenBound = PanelRectToScreenRect(panelBound);
            CoinFlyService.InitializeToolkitTarget(screenBound, () => _goldBadge.Punch());
            CoinFlyService.UpdateToolkitTargetBound(screenBound);
        }

        private Rect PanelRectToScreenRect(Rect panelRect)
        {
            VisualElement panelRoot = _goldBadgeElement.panel.visualTree;
            Rect fullPanel = panelRoot.worldBound;
            float panelToScreenScaleX = Screen.width / fullPanel.width;
            float panelToScreenScaleY = Screen.height / fullPanel.height;
            return new Rect(
                panelRect.x * panelToScreenScaleX,
                panelRect.y * panelToScreenScaleY,
                panelRect.width * panelToScreenScaleX,
                panelRect.height * panelToScreenScaleY);
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

            Debug.LogWarning($"{LogTag} Element '{elementName}' not found.");
            return false;
        }
    }
}
