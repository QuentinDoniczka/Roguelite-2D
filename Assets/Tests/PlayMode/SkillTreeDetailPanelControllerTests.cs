#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Economy;
using RogueliteAutoBattler.UI.Toolkit.SkillTree;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class SkillTreeDetailPanelControllerTests : PlayModeTestBase
    {
        private const string HiddenClassName = "hidden";
        private const string PanelSettingsPath = "Assets/UI/MainPanelSettings.asset";

        private SkillTreeDetailPanelController _controller;
        private SkillTreeData _data;
        private SkillTreeProgress _progress;
        private GoldWallet _goldWallet;
        private SkillPointWallet _skillPointWallet;

        private UIDocument _uiDocument;
        private VisualElement _panelRoot;
        private Label _nameLabel;
        private Label _levelLabel;
        private Label _costLabel;
        private Label _bonusLabel;
        private Label _bonusCurrentLabel;
        private Label _bonusNextLabel;
        private Button _upgradeButton;
        private Button _closeButton;

        [SetUp]
        public void SetUp()
        {
            PanelSettings panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            Assert.IsNotNull(panelSettings, $"MainPanelSettings not found at {PanelSettingsPath}");

            var documentGo = Track(new GameObject("TestUIDocument"));
            documentGo.SetActive(false);
            _uiDocument = documentGo.AddComponent<UIDocument>();
            _uiDocument.panelSettings = panelSettings;
            documentGo.SetActive(true);

            _panelRoot = new VisualElement();
            _panelRoot.AddToClassList(HiddenClassName);
            _nameLabel = new Label();
            _levelLabel = new Label();
            _costLabel = new Label();
            _bonusLabel = new Label();
            _bonusCurrentLabel = new Label();
            _bonusNextLabel = new Label();
            _upgradeButton = new Button();
            _closeButton = new Button();

            _panelRoot.Add(_nameLabel);
            _panelRoot.Add(_levelLabel);
            _panelRoot.Add(_costLabel);
            _panelRoot.Add(_bonusLabel);
            _panelRoot.Add(_bonusCurrentLabel);
            _panelRoot.Add(_bonusNextLabel);
            _panelRoot.Add(_upgradeButton);
            _panelRoot.Add(_closeButton);

            _uiDocument.rootVisualElement.Add(_panelRoot);

            _data = ScriptableObject.CreateInstance<SkillTreeData>();
            _data.BaseCost = 10;
            _data.CostMultiplierOdd = 1.5f;
            _data.CostMultiplierEven = 1.5f;
            const int testNodeCount = 4;
            const float testRadius = 5f;
            var testNodes = new List<SkillTreeData.SkillNodeEntry>(testNodeCount);
            for (int i = 0; i < testNodeCount; i++)
            {
                float angle = i * (2f * Mathf.PI / testNodeCount);
                testNodes.Add(new SkillTreeData.SkillNodeEntry
                {
                    id = i,
                    position = new Vector2(testRadius * Mathf.Cos(angle), testRadius * Mathf.Sin(angle)),
                    connectedNodeIds = new List<int>(),
                    costType = SkillTreeData.CostType.Gold,
                    maxLevel = 5,
                    baseCost = 10,
                    costMultiplierOdd = 1.5f,
                    costMultiplierEven = 1.5f,
                    costAdditivePerLevel = 0,
                    statModifierType = StatType.Hp,
                    statModifierMode = SkillTreeData.StatModifierMode.Flat,
                    statModifierValuePerLevel = 0f
                });
            }

            ApplyGoldNodeConfig(testNodes, nodeIndex: 0, maxLevel: 5, statType: StatType.Atk, statValue: 3f);
            ApplySkillPointNodeConfig(testNodes, nodeIndex: 1, maxLevel: 5, statType: StatType.Hp, statValue: 10f);
            ApplyGoldNodeConfig(testNodes, nodeIndex: 2, maxLevel: 5, statType: StatType.Def, statValue: 2f);
            ApplyGoldNodeConfig(testNodes, nodeIndex: 3, maxLevel: 3, statType: StatType.Mana, statValue: 5f);

            _data.InitializeForTest(testNodes);

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
                _bonusCurrentLabel,
                _bonusNextLabel,
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

        private static void ApplyGoldNodeConfig(List<SkillTreeData.SkillNodeEntry> testNodes, int nodeIndex, int maxLevel, StatType statType, float statValue)
        {
            var node = testNodes[nodeIndex];
            node.costType = SkillTreeData.CostType.Gold;
            node.maxLevel = maxLevel;
            node.statModifierType = statType;
            node.statModifierMode = SkillTreeData.StatModifierMode.Flat;
            node.statModifierValuePerLevel = statValue;
            testNodes[nodeIndex] = node;
        }

        private static void ApplySkillPointNodeConfig(List<SkillTreeData.SkillNodeEntry> testNodes, int nodeIndex, int maxLevel, StatType statType, float statValue)
        {
            var node = testNodes[nodeIndex];
            node.costType = SkillTreeData.CostType.SkillPoint;
            node.maxLevel = maxLevel;
            node.statModifierType = statType;
            node.statModifierMode = SkillTreeData.StatModifierMode.Flat;
            node.statModifierValuePerLevel = statValue;
            testNodes[nodeIndex] = node;
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
            StringAssert.Contains("Lvl", _levelLabel.text);
            StringAssert.Contains("0", _levelLabel.text);
            StringAssert.Contains("5", _levelLabel.text);
            StringAssert.Contains("Gold", _costLabel.text);
            StringAssert.Contains("+3", _bonusLabel.text);
            StringAssert.Contains("Current", _bonusCurrentLabel.text);
            StringAssert.Contains("Next", _bonusNextLabel.text);
        }

        [UnityTest]
        public IEnumerator Refresh_NodeWithMaxLevelZero_LevelLabelShowsInfinity()
        {
            yield return null;
            var node = _data.Nodes[0];
            node.maxLevel = 0;
            _data.SetNode(0, node);

            _controller.Show(0);

            StringAssert.Contains("∞", _levelLabel.text);
        }

        [UnityTest]
        public IEnumerator Refresh_NodeAtMaxLevel_NextBonusShowsDash()
        {
            yield return null;
            _goldWallet.Add(999999);
            _progress.SetLevel(0, 5);

            _controller.Show(0);

            StringAssert.Contains("—", _bonusNextLabel.text);
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
#endif
