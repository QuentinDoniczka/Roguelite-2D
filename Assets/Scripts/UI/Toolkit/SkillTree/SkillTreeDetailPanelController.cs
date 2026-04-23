using System;
using UnityEngine.UIElements;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Economy;

namespace RogueliteAutoBattler.UI.Toolkit.SkillTree
{
    public sealed class SkillTreeDetailPanelController : IDisposable
    {
        private const string HiddenClassName = "hidden";
        private const int NoSelectedNodeIndex = -1;

        private readonly VisualElement _panelRoot;
        private readonly Label _nameLabel;
        private readonly Label _levelLabel;
        private readonly Label _costLabel;
        private readonly Label _bonusLabel;
        private readonly Label _bonusCurrentLabel;
        private readonly Label _bonusNextLabel;
        private readonly Button _upgradeButton;
        private readonly Button _closeButton;
        private readonly SkillTreeData _data;
        private readonly SkillTreeProgress _progress;
        private readonly GoldWallet _goldWallet;
        private readonly SkillPointWallet _skillPointWallet;

        private int _currentNodeIndex = NoSelectedNodeIndex;
        private bool _isShowing;

        public event Action<int> NodeUpgraded;
        public event Action Closed;

        public bool IsShowing => _isShowing;
        public int CurrentNodeIndex => _currentNodeIndex;

        public SkillTreeDetailPanelController(
            VisualElement panelRoot,
            Label nameLabel,
            Label levelLabel,
            Label costLabel,
            Label bonusLabel,
            Label bonusCurrentLabel,
            Label bonusNextLabel,
            Button upgradeButton,
            Button closeButton,
            SkillTreeData data,
            SkillTreeProgress progress,
            GoldWallet goldWallet,
            SkillPointWallet skillPointWallet)
        {
            _panelRoot = panelRoot ?? throw new ArgumentNullException(nameof(panelRoot));
            _nameLabel = nameLabel ?? throw new ArgumentNullException(nameof(nameLabel));
            _levelLabel = levelLabel ?? throw new ArgumentNullException(nameof(levelLabel));
            _costLabel = costLabel ?? throw new ArgumentNullException(nameof(costLabel));
            _bonusLabel = bonusLabel ?? throw new ArgumentNullException(nameof(bonusLabel));
            _bonusCurrentLabel = bonusCurrentLabel ?? throw new ArgumentNullException(nameof(bonusCurrentLabel));
            _bonusNextLabel = bonusNextLabel ?? throw new ArgumentNullException(nameof(bonusNextLabel));
            _upgradeButton = upgradeButton ?? throw new ArgumentNullException(nameof(upgradeButton));
            _closeButton = closeButton ?? throw new ArgumentNullException(nameof(closeButton));
            _data = data ?? throw new ArgumentNullException(nameof(data));
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));
            _goldWallet = goldWallet ?? throw new ArgumentNullException(nameof(goldWallet));
            _skillPointWallet = skillPointWallet ?? throw new ArgumentNullException(nameof(skillPointWallet));

            _upgradeButton.RegisterCallback<ClickEvent>(OnUpgradeClicked);
            _closeButton.RegisterCallback<ClickEvent>(OnCloseClicked);
        }

        public void Show(int nodeIndex)
        {
            _currentNodeIndex = nodeIndex;
            _isShowing = true;
            SubscribeToWalletEvents();
            _panelRoot.RemoveFromClassList(HiddenClassName);
            Refresh();
        }

        public void Hide()
        {
            _isShowing = false;
            UnsubscribeFromWalletEvents();
            _panelRoot.AddToClassList(HiddenClassName);
            _currentNodeIndex = NoSelectedNodeIndex;
        }

        public void Refresh()
        {
            if (!_isShowing || _currentNodeIndex < 0 || _currentNodeIndex >= _data.Nodes.Count)
            {
                return;
            }

            var node = _data.Nodes[_currentNodeIndex];
            var currentLevel = _progress.GetLevel(_currentNodeIndex);
            var nextCost = SkillTreeData.ComputeNodeCost(node, currentLevel);
            var isMax = SkillTreeData.IsMaxLevel(node, currentLevel);

            var maxLevelDisplay = node.maxLevel > 0 ? node.maxLevel.ToString() : "∞";
            _nameLabel.text = SkillTreeData.GetStatDisplayName(node.statModifierType);
            _levelLabel.text = $"Lvl {currentLevel} / {maxLevelDisplay}";
            _costLabel.text = isMax ? "MAX" : FormatCostLabel(nextCost, node.costType);
            _bonusLabel.text = $"Per level: {SkillTreeData.FormatBonus(node.statModifierValuePerLevel, node.statModifierMode)}";
            _bonusCurrentLabel.text = $"Current: {SkillTreeData.FormatBonus(node.statModifierValuePerLevel * currentLevel, node.statModifierMode)}";
            _bonusNextLabel.text = isMax ? "Next: —" : $"Next: {SkillTreeData.FormatBonus(node.statModifierValuePerLevel * (currentLevel + 1), node.statModifierMode)}";
            _upgradeButton.SetEnabled(!isMax && CanAfford(node.costType, nextCost));
        }

        private bool CanAfford(SkillTreeData.CostType costType, int cost)
            => costType == SkillTreeData.CostType.Gold
                ? _goldWallet.CanAfford(cost)
                : _skillPointWallet.CanAfford(cost);

        private static string FormatCostLabel(int cost, SkillTreeData.CostType costType)
            => costType == SkillTreeData.CostType.Gold ? $"{cost} Gold" : $"{cost} SP";

        private void OnUpgradeClicked(ClickEvent _)
        {
            if (!_isShowing || _currentNodeIndex < 0)
            {
                return;
            }

            var node = _data.Nodes[_currentNodeIndex];
            var currentLevel = _progress.GetLevel(_currentNodeIndex);

            if (SkillTreeData.IsMaxLevel(node, currentLevel))
            {
                return;
            }

            var cost = SkillTreeData.ComputeNodeCost(node, currentLevel);
            var success = node.costType == SkillTreeData.CostType.Gold
                ? _goldWallet.Spend(cost)
                : _skillPointWallet.Spend(cost);

            if (!success)
            {
                return;
            }

            _progress.SetLevel(_currentNodeIndex, currentLevel + 1);
            NodeUpgraded?.Invoke(_currentNodeIndex);
            Refresh();
        }

        private void OnCloseClicked(ClickEvent _)
        {
            Closed?.Invoke();
            Hide();
        }

        private void SubscribeToWalletEvents()
        {
            _goldWallet.OnGoldChanged += OnWalletBalanceChanged;
            _skillPointWallet.OnPointsChanged += OnWalletBalanceChanged;
        }

        private void UnsubscribeFromWalletEvents()
        {
            _goldWallet.OnGoldChanged -= OnWalletBalanceChanged;
            _skillPointWallet.OnPointsChanged -= OnWalletBalanceChanged;
        }

        private void OnWalletBalanceChanged(int _) => Refresh();

        public void Dispose()
        {
            _upgradeButton.UnregisterCallback<ClickEvent>(OnUpgradeClicked);
            _closeButton.UnregisterCallback<ClickEvent>(OnCloseClicked);
            UnsubscribeFromWalletEvents();
        }
    }
}
