using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Economy;
using RogueliteAutoBattler.UI.Toolkit.SkillTree;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class SkillTreeDetailPanelControllerTests : PlayModeTestBase
    {
        private const string HiddenClassName = "hidden";

        private SkillTreeDetailPanelController _controller;
        private SkillTreeData _data;
        private SkillTreeProgress _progress;
        private GoldWallet _goldWallet;
        private SkillPointWallet _skillPointWallet;

        private VisualElement _panelRoot;
        private Label _nameLabel;
        private Label _levelLabel;
        private Label _costLabel;
        private Label _bonusLabel;
        private Button _upgradeButton;
        private Button _closeButton;

        [SetUp]
        public void SetUp()
        {
            _panelRoot = new VisualElement();
            _panelRoot.AddToClassList(HiddenClassName);
            _nameLabel = new Label();
            _levelLabel = new Label();
            _costLabel = new Label();
            _bonusLabel = new Label();
            _upgradeButton = new Button();
            _closeButton = new Button();

            _panelRoot.Add(_nameLabel);
            _panelRoot.Add(_levelLabel);
            _panelRoot.Add(_costLabel);
            _panelRoot.Add(_bonusLabel);
            _panelRoot.Add(_upgradeButton);
            _panelRoot.Add(_closeButton);

            _data = ScriptableObject.CreateInstance<SkillTreeData>();
            _data.RingNodeCount = 4;
            _data.RingRadius = 5f;
            _data.BaseCost = 10;
            _data.CostMultiplierOdd = 1.5f;
            _data.CostMultiplierEven = 1.5f;
            _data.GenerateNodes();

            ConfigureGoldNode(nodeIndex: 0, maxLevel: 5, statType: SkillTreeData.StatModifierType.Attack, statValue: 3f);
            ConfigureSkillPointNode(nodeIndex: 1, maxLevel: 5, statType: SkillTreeData.StatModifierType.HP, statValue: 10f);
            ConfigureGoldNode(nodeIndex: 2, maxLevel: 5, statType: SkillTreeData.StatModifierType.Defense, statValue: 2f);
            ConfigureGoldNode(nodeIndex: 3, maxLevel: 3, statType: SkillTreeData.StatModifierType.Mana, statValue: 5f);

            _progress = ScriptableObject.CreateInstance<SkillTreeProgress>();

            var walletHost = Track(new GameObject("WalletHost"));
            _goldWallet = walletHost.AddComponent<GoldWallet>();
            _skillPointWallet = walletHost.AddComponent<SkillPointWallet>();

            _controller = new SkillTreeDetailPanelController(
                _panelRoot,
                _nameLabel,
                _levelLabel,
                _costLabel,
                _bonusLabel,
                _upgradeButton,
                _closeButton,
                _data,
                _progress,
                _goldWallet,
                _skillPointWallet);
        }

        public override void TearDown()
        {
            _controller?.Dispose();
            _controller = null;
            if (_data != null) Object.DestroyImmediate(_data);
            if (_progress != null) Object.DestroyImmediate(_progress);
            base.TearDown();
        }

        private void ConfigureGoldNode(int nodeIndex, int maxLevel, SkillTreeData.StatModifierType statType, float statValue)
        {
            var node = _data.Nodes[nodeIndex];
            node.costType = SkillTreeData.CostType.Gold;
            node.maxLevel = maxLevel;
            node.statModifierType = statType;
            node.statModifierMode = SkillTreeData.StatModifierMode.Flat;
            node.statModifierValuePerLevel = statValue;
            _data.SetNode(nodeIndex, node);
        }

        private void ConfigureSkillPointNode(int nodeIndex, int maxLevel, SkillTreeData.StatModifierType statType, float statValue)
        {
            var node = _data.Nodes[nodeIndex];
            node.costType = SkillTreeData.CostType.SkillPoint;
            node.maxLevel = maxLevel;
            node.statModifierType = statType;
            node.statModifierMode = SkillTreeData.StatModifierMode.Flat;
            node.statModifierValuePerLevel = statValue;
            _data.SetNode(nodeIndex, node);
        }

        private void InvokeButtonClick(Button button)
        {
            using (var clickEvent = ClickEvent.GetPooled())
            {
                clickEvent.target = button;
                button.SendEvent(clickEvent);
            }
        }

        [UnityTest]
        public IEnumerator Show_RemovesHiddenClass()
        {
            yield return null;
            Assert.IsTrue(_panelRoot.ClassListContains(HiddenClassName), "Panel should start hidden");

            _controller.Show(0);

            Assert.IsFalse(_panelRoot.ClassListContains(HiddenClassName));
            Assert.IsTrue(_controller.IsShowing);
        }

        [UnityTest]
        public IEnumerator Hide_AddsHiddenClass()
        {
            yield return null;
            _controller.Show(0);
            Assert.IsFalse(_panelRoot.ClassListContains(HiddenClassName));

            _controller.Hide();

            Assert.IsTrue(_panelRoot.ClassListContains(HiddenClassName));
            Assert.IsFalse(_controller.IsShowing);
        }

        [UnityTest]
        public IEnumerator Show_SetsCurrentNodeIndex()
        {
            yield return null;
            _controller.Show(2);
            Assert.AreEqual(2, _controller.CurrentNodeIndex);
        }

        [UnityTest]
        public IEnumerator Refresh_PopulatesLabels()
        {
            yield return null;
            _goldWallet.Add(1000);

            _controller.Show(0);

            Assert.IsFalse(string.IsNullOrEmpty(_nameLabel.text));
            Assert.IsFalse(string.IsNullOrEmpty(_levelLabel.text));
            Assert.IsFalse(string.IsNullOrEmpty(_costLabel.text));
            Assert.IsFalse(string.IsNullOrEmpty(_bonusLabel.text));

            Assert.AreEqual("Attack", _nameLabel.text);
            StringAssert.Contains("0", _levelLabel.text);
            StringAssert.Contains("5", _levelLabel.text);
            StringAssert.Contains("Gold", _costLabel.text);
            StringAssert.Contains("+3", _bonusLabel.text);
        }

        [UnityTest]
        public IEnumerator Refresh_UpgradeButtonDisabled_WhenCantAffordGold()
        {
            yield return null;
            Assert.AreEqual(0, _goldWallet.Gold);

            _controller.Show(0);

            Assert.IsFalse(_upgradeButton.enabledSelf);
        }

        [UnityTest]
        public IEnumerator Refresh_UpgradeButtonEnabled_WhenAffordableGold()
        {
            yield return null;
            _goldWallet.Add(1000);

            _controller.Show(0);

            Assert.IsTrue(_upgradeButton.enabledSelf);
        }

        [UnityTest]
        public IEnumerator Refresh_UpgradeButtonDisabled_AtMaxLevel()
        {
            yield return null;
            _goldWallet.Add(999999);
            _progress.SetLevel(0, 5);

            _controller.Show(0);

            Assert.IsFalse(_upgradeButton.enabledSelf);
            Assert.AreEqual("MAX", _costLabel.text);
        }

        [UnityTest]
        public IEnumerator UpgradeClick_DebitsGoldWallet_AndIncrementsProgress()
        {
            yield return null;
            _goldWallet.Add(1000);
            var goldBefore = _goldWallet.Gold;
            var levelBefore = _progress.GetLevel(0);

            int upgradedIndex = -1;
            _controller.NodeUpgraded += index => upgradedIndex = index;

            _controller.Show(0);
            InvokeButtonClick(_upgradeButton);

            Assert.AreEqual(0, upgradedIndex);
            Assert.AreEqual(levelBefore + 1, _progress.GetLevel(0));
            Assert.Less(_goldWallet.Gold, goldBefore);
        }

        [UnityTest]
        public IEnumerator UpgradeClick_DebitsSkillPointWallet_ForSkillPointCostNode()
        {
            yield return null;
            _skillPointWallet.Add(1000);
            var pointsBefore = _skillPointWallet.Points;
            var goldBefore = _goldWallet.Gold;
            var levelBefore = _progress.GetLevel(1);

            _controller.Show(1);
            InvokeButtonClick(_upgradeButton);

            Assert.AreEqual(levelBefore + 1, _progress.GetLevel(1));
            Assert.Less(_skillPointWallet.Points, pointsBefore);
            Assert.AreEqual(goldBefore, _goldWallet.Gold);
        }

        [UnityTest]
        public IEnumerator CloseClick_RaisesClosedEvent_AndHidesPanel()
        {
            yield return null;
            _controller.Show(0);
            bool closedFired = false;
            _controller.Closed += () => closedFired = true;

            InvokeButtonClick(_closeButton);

            Assert.IsTrue(closedFired);
            Assert.IsTrue(_panelRoot.ClassListContains(HiddenClassName));
            Assert.IsFalse(_controller.IsShowing);
        }

        [UnityTest]
        public IEnumerator WalletEvent_WhenShowing_RefreshesButton()
        {
            yield return null;
            _controller.Show(0);
            Assert.IsFalse(_upgradeButton.enabledSelf, "Precondition: button disabled with 0 gold");

            _goldWallet.Add(1000);

            Assert.IsTrue(_upgradeButton.enabledSelf);
        }

        [UnityTest]
        public IEnumerator Dispose_UnregistersWalletEvents()
        {
            yield return null;
            _goldWallet.Add(500);
            _controller.Show(0);
            var costTextBeforeDispose = _costLabel.text;

            _controller.Dispose();
            _controller = null;

            _costLabel.text = "SENTINEL";
            _goldWallet.Add(10000);

            Assert.AreEqual("SENTINEL", _costLabel.text);
        }

        [UnityTest]
        public IEnumerator Dispose_UnregistersButtonClicks()
        {
            yield return null;
            _goldWallet.Add(1000);
            _controller.Show(0);
            var levelBeforeDispose = _progress.GetLevel(0);

            _controller.Dispose();
            _controller = null;

            InvokeButtonClick(_upgradeButton);

            Assert.AreEqual(levelBeforeDispose, _progress.GetLevel(0));
        }
    }
}
