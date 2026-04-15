using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Economy;
using RogueliteAutoBattler.UI.Screens.SkillTree;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class SkillTreeDetailPanelTests : PlayModeTestBase
    {
        private SkillTreeDetailPanel _panel;
        private SkillTreeData _data;
        private SkillTreeProgress _progress;
        private GoldWallet _goldWallet;
        private SkillPointWallet _spWallet;
        private CanvasGroup _canvasGroup;
        private RectTransform _panelRect;

        [SetUp]
        public void SetUp()
        {
            var canvasGo = Track(new GameObject("Canvas"));
            canvasGo.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            var panelGo = new GameObject("DetailPanel");
            panelGo.transform.SetParent(canvasGo.transform, false);
            _panelRect = panelGo.AddComponent<RectTransform>();
            _panelRect.sizeDelta = new Vector2(400, 160);
            _canvasGroup = panelGo.AddComponent<CanvasGroup>();

            var topBorder = new GameObject("TopBorder").AddComponent<Image>();
            topBorder.transform.SetParent(panelGo.transform, false);

            var icon = new GameObject("Icon").AddComponent<Image>();
            icon.transform.SetParent(panelGo.transform, false);

            TMP_Text statName = CreateTMP(panelGo.transform, "StatName");
            TMP_Text statDesc = CreateTMP(panelGo.transform, "StatDesc");
            TMP_Text level = CreateTMP(panelGo.transform, "Level");
            TMP_Text levelCaption = CreateTMP(panelGo.transform, "LevelCaption");
            TMP_Text currentBonus = CreateTMP(panelGo.transform, "CurrentBonus");
            TMP_Text nextBonus = CreateTMP(panelGo.transform, "NextBonus");
            TMP_Text cost = CreateTMP(panelGo.transform, "Cost");
            TMP_Text deficit = CreateTMP(panelGo.transform, "Deficit");
            TMP_Text costMult = CreateTMP(panelGo.transform, "CostMult");

            var upgradeBtnGo = new GameObject("UpgradeBtn");
            upgradeBtnGo.transform.SetParent(panelGo.transform, false);
            upgradeBtnGo.AddComponent<RectTransform>();
            var upgradeBtnImage = upgradeBtnGo.AddComponent<Image>();
            var upgradeBtn = upgradeBtnGo.AddComponent<Button>();
            TMP_Text upgradeBtnLabel = CreateTMP(upgradeBtnGo.transform, "BtnLabel");

            panelGo.SetActive(false);
            _panel = panelGo.AddComponent<SkillTreeDetailPanel>();
            _panel.InitializeForTest(_canvasGroup, _panelRect, topBorder,
                icon, statName, statDesc, level, levelCaption,
                currentBonus, nextBonus, cost, upgradeBtn,
                upgradeBtnLabel, upgradeBtnImage, deficit, costMult);
            panelGo.SetActive(true);

            _data = ScriptableObject.CreateInstance<SkillTreeData>();
            _data.RingNodeCount = 6;
            _data.RingRadius = 5f;
            _data.BaseCost = 10;
            _data.CostMultiplierOdd = 1.5f;
            _data.CostMultiplierEven = 1.5f;
            _data.GenerateNodes();
            var node0 = _data.Nodes[0];
            node0.maxLevel = 5;
            node0.statModifierType = SkillTreeData.StatModifierType.Attack;
            node0.statModifierMode = SkillTreeData.StatModifierMode.Flat;
            node0.statModifierValuePerLevel = 3f;
            _data.SetNode(0, node0);

            _progress = ScriptableObject.CreateInstance<SkillTreeProgress>();

            var walletGo = Track(new GameObject("Wallets"));
            _goldWallet = walletGo.AddComponent<GoldWallet>();
            _spWallet = walletGo.AddComponent<SkillPointWallet>();

            _panel.Initialize(_data, _progress, _goldWallet, _spWallet);
        }

        public override void TearDown()
        {
            if (_data != null) Object.DestroyImmediate(_data);
            if (_progress != null) Object.DestroyImmediate(_progress);
            base.TearDown();
        }

        [UnityTest]
        public IEnumerator Awake_PanelHiddenByDefault()
        {
            yield return null;
            Assert.AreEqual(0f, _panel.PanelAlpha, 0.01f);
            Assert.IsFalse(_panel.IsVisible);
        }

        [UnityTest]
        public IEnumerator ShowNode_SetsPanelVisible()
        {
            yield return null;
            var node = CreateTestNode(0);
            _panel.ShowNode(node);
            yield return new WaitForSeconds(0.3f);
            Assert.IsTrue(_panel.IsVisible);
            Assert.AreEqual(1f, _panel.PanelAlpha, 0.01f);
            Object.DestroyImmediate(node.gameObject);
        }

        [UnityTest]
        public IEnumerator ShowNode_DisplaysStatName()
        {
            yield return null;
            var node = CreateTestNode(0);
            _panel.ShowNode(node);
            yield return null;
            Assert.AreEqual("Attack", _panel.StatNameText);
            Object.DestroyImmediate(node.gameObject);
        }

        [UnityTest]
        public IEnumerator ShowNode_AtLevel0_ShowsNoneBonus()
        {
            yield return null;
            var node = CreateTestNode(0);
            _panel.ShowNode(node);
            yield return null;
            Assert.AreEqual("None", _panel.CurrentBonusText);
            Assert.AreEqual("0", _panel.LevelText);
            Object.DestroyImmediate(node.gameObject);
        }

        [UnityTest]
        public IEnumerator Hide_SetsPanelInvisible()
        {
            yield return null;
            var node = CreateTestNode(0);
            _panel.ShowNode(node);
            yield return new WaitForSeconds(0.3f);
            _panel.Hide();
            yield return new WaitForSeconds(0.3f);
            Assert.IsFalse(_panel.IsVisible);
            Assert.AreEqual(0f, _panel.PanelAlpha, 0.01f);
            Object.DestroyImmediate(node.gameObject);
        }

        [UnityTest]
        public IEnumerator ShowNode_CanAfford_UpgradeInteractable()
        {
            yield return null;
            _goldWallet.Add(1000);
            var node = CreateTestNode(0);
            _panel.ShowNode(node);
            yield return null;
            Assert.IsTrue(_panel.IsUpgradeInteractable);
            Assert.AreEqual("UNLOCK", _panel.UpgradeButtonLabelText);
            Object.DestroyImmediate(node.gameObject);
        }

        [UnityTest]
        public IEnumerator ShowNode_CannotAfford_UpgradeDisabled()
        {
            yield return null;
            var node = CreateTestNode(0);
            _panel.ShowNode(node);
            yield return null;
            Assert.IsFalse(_panel.IsUpgradeInteractable);
            Object.DestroyImmediate(node.gameObject);
        }

        [UnityTest]
        public IEnumerator ShowNode_MaxLevel_ShowsMAX()
        {
            yield return null;
            _progress.SetLevel(0, 5);
            var node = CreateTestNode(0);
            _panel.ShowNode(node);
            yield return null;
            Assert.AreEqual("MAX", _panel.UpgradeButtonLabelText);
            Assert.IsFalse(_panel.IsUpgradeInteractable);
            Object.DestroyImmediate(node.gameObject);
        }

        private SkillTreeNode CreateTestNode(int index)
        {
            var nodeGo = new GameObject($"Node_{index}");
            nodeGo.AddComponent<RectTransform>();
            var borderImage = nodeGo.AddComponent<Image>();
            var node = nodeGo.AddComponent<SkillTreeNode>();
            node.Setup(borderImage, Color.gray, Color.yellow);
            node.Initialize(index);
            return node;
        }

        private static TMP_Text CreateTMP(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go.AddComponent<TextMeshProUGUI>();
        }
    }
}
