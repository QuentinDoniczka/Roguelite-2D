using System.Collections;
using System.Globalization;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Economy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RogueliteAutoBattler.UI.Screens.SkillTree
{
    public class SkillTreeDetailPanel : MonoBehaviour
    {
        private const float SlideDuration = 0.25f;
        private static readonly Color GoldColor = new Color(1f, 0.843f, 0f);
        private static readonly Color SkillPointColor = new Color(0.671f, 0.278f, 0.737f);
        private static readonly Color DisabledColor = new Color(0.4f, 0.4f, 0.4f);

        [Header("Panel")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _panelRect;
        [SerializeField] private Image _topBorder;

        [Header("Header")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _statNameLabel;
        [SerializeField] private TMP_Text _statDescLabel;
        [SerializeField] private TMP_Text _levelLabel;
        [SerializeField] private TMP_Text _levelCaptionLabel;

        [Header("Detail")]
        [SerializeField] private TMP_Text _currentBonusLabel;
        [SerializeField] private TMP_Text _nextBonusLabel;

        [Header("Action")]
        [SerializeField] private TMP_Text _costLabel;
        [SerializeField] private Button _upgradeButton;
        [SerializeField] private TMP_Text _upgradeButtonLabel;
        [SerializeField] private Image _upgradeButtonImage;
        [SerializeField] private TMP_Text _deficitLabel;
        [SerializeField] private TMP_Text _costMultiplierLabel;

        private SkillTreeData _data;
        private SkillTreeProgress _progress;
        private GoldWallet _goldWallet;
        private SkillPointWallet _skillPointWallet;
        private int _currentNodeIndex = -1;
        private Coroutine _slideCoroutine;
        private bool _isSubscribed;
        private bool _isVisible;

        public bool IsVisible => _isVisible;

        private void Awake()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = false;
            }
        }

        public void Initialize(SkillTreeData data, SkillTreeProgress progress, GoldWallet goldWallet, SkillPointWallet spWallet)
        {
            _data = data;
            _progress = progress;
            _goldWallet = goldWallet;
            _skillPointWallet = spWallet;

            if (_levelCaptionLabel != null)
                _levelCaptionLabel.SetText("LEVEL");

            if (_upgradeButton != null && !_isSubscribed)
            {
                _upgradeButton.onClick.AddListener(OnUpgradeClicked);
                _isSubscribed = true;
            }
        }

        public void ShowNode(SkillTreeNode node)
        {
            _currentNodeIndex = node.NodeIndex;
            RefreshDisplay();

            if (_slideCoroutine != null)
                StopCoroutine(_slideCoroutine);
            _slideCoroutine = StartCoroutine(SlideUp());
        }

        public void Hide()
        {
            _currentNodeIndex = -1;

            if (_slideCoroutine != null)
                StopCoroutine(_slideCoroutine);
            _slideCoroutine = StartCoroutine(SlideDown());
        }

        private void RefreshDisplay()
        {
            if (_data == null || _currentNodeIndex < 0 || _currentNodeIndex >= _data.Nodes.Count)
                return;

            var entry = _data.Nodes[_currentNodeIndex];
            int level = _progress != null ? _progress.GetLevel(_currentNodeIndex) : 0;
            string statName = SkillTreeData.GetStatDisplayName(entry.statModifierType);
            string modeName = entry.statModifierMode.ToString();
            bool isMaxLevel = SkillTreeData.IsMaxLevel(entry, level);

            if (_statNameLabel != null)
                _statNameLabel.SetText(statName);

            if (_statDescLabel != null)
            {
                string perLevelBonus = SkillTreeData.FormatBonus(entry.statModifierValuePerLevel, entry.statModifierMode);
                _statDescLabel.SetText($"{perLevelBonus} {statName} per level ({modeName})");
            }

            if (_levelLabel != null)
                _levelLabel.SetText(level.ToString());

            if (_currentBonusLabel != null)
            {
                if (level == 0)
                {
                    _currentBonusLabel.SetText("None");
                }
                else
                {
                    float currentBonus = level * entry.statModifierValuePerLevel;
                    string formatted = SkillTreeData.FormatBonus(currentBonus, entry.statModifierMode);
                    _currentBonusLabel.SetText($"Current: {formatted} {statName}");
                }
            }

            if (_nextBonusLabel != null)
            {
                if (isMaxLevel)
                {
                    _nextBonusLabel.SetText("");
                }
                else
                {
                    float nextBonus = (level + 1) * entry.statModifierValuePerLevel;
                    string formattedNext = SkillTreeData.FormatBonus(nextBonus, entry.statModifierMode);
                    string formattedPerLevel = SkillTreeData.FormatBonus(entry.statModifierValuePerLevel, entry.statModifierMode);
                    _nextBonusLabel.SetText($"Next: {formattedNext} {statName} ({formattedPerLevel})");
                }
            }

            Color themeColor = entry.costType == SkillTreeData.CostType.Gold ? GoldColor : SkillPointColor;

            if (_topBorder != null)
                _topBorder.color = themeColor;

            if (isMaxLevel)
            {
                if (_upgradeButtonLabel != null)
                    _upgradeButtonLabel.SetText("MAX");
                if (_upgradeButton != null)
                    _upgradeButton.interactable = false;
                if (_upgradeButtonImage != null)
                    _upgradeButtonImage.color = DisabledColor;
                if (_costLabel != null)
                    _costLabel.gameObject.SetActive(false);
                if (_deficitLabel != null)
                    _deficitLabel.gameObject.SetActive(false);
                if (_costMultiplierLabel != null)
                    _costMultiplierLabel.gameObject.SetActive(false);
                return;
            }

            int cost = SkillTreeData.ComputeNodeCost(entry, level);
            bool canAfford = CanAffordCost(entry.costType, cost);

            if (_costLabel != null)
            {
                _costLabel.gameObject.SetActive(true);
                _costLabel.SetText(cost.ToString());
                _costLabel.color = themeColor;
            }

            if (_costMultiplierLabel != null)
            {
                float displayMultiplier = ((level + 1) % 2 == 1) ? entry.costMultiplierOdd : entry.costMultiplierEven;
                _costMultiplierLabel.gameObject.SetActive(true);
                _costMultiplierLabel.SetText($"cost x{displayMultiplier.ToString("0.##", CultureInfo.InvariantCulture)}");
            }

            if (_upgradeButtonLabel != null)
                _upgradeButtonLabel.SetText(level == 0 ? "UNLOCK" : "UPGRADE");

            if (canAfford)
            {
                if (_upgradeButton != null)
                    _upgradeButton.interactable = true;
                if (_upgradeButtonImage != null)
                    _upgradeButtonImage.color = themeColor;
                if (_deficitLabel != null)
                    _deficitLabel.gameObject.SetActive(false);
            }
            else
            {
                if (_upgradeButton != null)
                    _upgradeButton.interactable = false;
                if (_upgradeButtonImage != null)
                    _upgradeButtonImage.color = DisabledColor;
                if (_deficitLabel != null)
                {
                    int currentFunds = GetWalletBalance(entry.costType);
                    int missing = cost - currentFunds;
                    _deficitLabel.gameObject.SetActive(true);
                    _deficitLabel.SetText($"(missing {missing})");
                }
            }
        }

        private void OnUpgradeClicked()
        {
            if (_data == null || _progress == null || _currentNodeIndex < 0 || _currentNodeIndex >= _data.Nodes.Count)
                return;

            var entry = _data.Nodes[_currentNodeIndex];
            int level = _progress.GetLevel(_currentNodeIndex);

            if (SkillTreeData.IsMaxLevel(entry, level))
                return;

            int cost = SkillTreeData.ComputeNodeCost(entry, level);
            bool spent = SpendFromWallet(entry.costType, cost);

            if (!spent)
                return;

            _progress.SetLevel(_currentNodeIndex, level + 1);
            RefreshDisplay();
        }

        private bool CanAffordCost(SkillTreeData.CostType costType, int cost)
        {
            if (costType == SkillTreeData.CostType.Gold)
                return _goldWallet != null && _goldWallet.CanAfford(cost);
            return _skillPointWallet != null && _skillPointWallet.CanAfford(cost);
        }

        private int GetWalletBalance(SkillTreeData.CostType costType)
        {
            if (costType == SkillTreeData.CostType.Gold)
                return _goldWallet != null ? _goldWallet.Gold : 0;
            return _skillPointWallet != null ? _skillPointWallet.Points : 0;
        }

        private bool SpendFromWallet(SkillTreeData.CostType costType, int cost)
        {
            if (costType == SkillTreeData.CostType.Gold)
                return _goldWallet != null && _goldWallet.Spend(cost);
            return _skillPointWallet != null && _skillPointWallet.Spend(cost);
        }

        private IEnumerator SlideUp()
        {
            _isVisible = true;

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.blocksRaycasts = true;
            }

            if (_panelRect == null)
            {
                _slideCoroutine = null;
                yield break;
            }

            float panelHeight = _panelRect.rect.height;
            Vector2 startPos = _panelRect.anchoredPosition;
            startPos.y = -panelHeight;
            Vector2 endPos = startPos;
            endPos.y = 0f;

            float elapsed = 0f;
            while (elapsed < SlideDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / SlideDuration);
                float easeOut = 1f - (1f - t) * (1f - t);
                _panelRect.anchoredPosition = Vector2.LerpUnclamped(startPos, endPos, easeOut);
                yield return null;
            }

            _panelRect.anchoredPosition = endPos;
            _slideCoroutine = null;
        }

        private IEnumerator SlideDown()
        {
            if (_panelRect == null)
            {
                if (_canvasGroup != null)
                {
                    _canvasGroup.alpha = 0f;
                    _canvasGroup.blocksRaycasts = false;
                }
                _isVisible = false;
                _slideCoroutine = null;
                yield break;
            }

            float panelHeight = _panelRect.rect.height;
            Vector2 startPos = _panelRect.anchoredPosition;
            Vector2 endPos = startPos;
            endPos.y = -panelHeight;

            float elapsed = 0f;
            while (elapsed < SlideDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / SlideDuration);
                float easeOut = 1f - (1f - t) * (1f - t);
                _panelRect.anchoredPosition = Vector2.LerpUnclamped(startPos, endPos, easeOut);
                yield return null;
            }

            _panelRect.anchoredPosition = endPos;

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = false;
            }

            _isVisible = false;
            _slideCoroutine = null;
        }

        private void OnDestroy()
        {
            if (_upgradeButton != null && _isSubscribed)
            {
                _upgradeButton.onClick.RemoveListener(OnUpgradeClicked);
                _isSubscribed = false;
            }
        }

        internal float PanelAlpha => _canvasGroup != null ? _canvasGroup.alpha : 0f;
        internal string StatNameText => _statNameLabel != null ? _statNameLabel.text : "";
        internal string LevelText => _levelLabel != null ? _levelLabel.text : "";
        internal string CurrentBonusText => _currentBonusLabel != null ? _currentBonusLabel.text : "";
        internal string NextBonusText => _nextBonusLabel != null ? _nextBonusLabel.text : "";
        internal string CostText => _costLabel != null ? _costLabel.text : "";
        internal string UpgradeButtonLabelText => _upgradeButtonLabel != null ? _upgradeButtonLabel.text : "";
        internal bool IsUpgradeInteractable => _upgradeButton != null && _upgradeButton.interactable;
        internal string DeficitText => _deficitLabel != null ? _deficitLabel.text : "";

        internal void InitializeForTest(CanvasGroup cg, RectTransform rect, Image topBorder,
            Image icon, TMP_Text statName, TMP_Text statDesc, TMP_Text level, TMP_Text levelCaption,
            TMP_Text currentBonus, TMP_Text nextBonus, TMP_Text cost, Button upgradeBtn,
            TMP_Text upgradeBtnLabel, Image upgradeBtnImage, TMP_Text deficit, TMP_Text costMult)
        {
            _canvasGroup = cg;
            _panelRect = rect;
            _topBorder = topBorder;
            _iconImage = icon;
            _statNameLabel = statName;
            _statDescLabel = statDesc;
            _levelLabel = level;
            _levelCaptionLabel = levelCaption;
            _currentBonusLabel = currentBonus;
            _nextBonusLabel = nextBonus;
            _costLabel = cost;
            _upgradeButton = upgradeBtn;
            _upgradeButtonLabel = upgradeBtnLabel;
            _upgradeButtonImage = upgradeBtnImage;
            _deficitLabel = deficit;
            _costMultiplierLabel = costMult;
        }
    }
}
