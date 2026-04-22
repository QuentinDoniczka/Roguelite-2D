#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Economy;
using RogueliteAutoBattler.UI.Toolkit.SkillTree;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class SkillTreeScreenControllerTests : PlayModeTestBase
    {
        private const string PanelSettingsPath = "Assets/UI/MainPanelSettings.asset";
        private const string MainLayoutPath = "Assets/UI/Layouts/MainLayout.uxml";
        private const string NodesLayerElementName = "skilltree-nodes-layer";
        private const string DetailPanelElementName = "skilltree-detail-panel";
        private const string UpgradeButtonElementName = "skilltree-detail-upgrade-btn";
        private const string CloseButtonElementName = "skilltree-detail-close-btn";
        private const string HiddenClassName = "hidden";
        private const string UiDocumentFieldName = "_uiDocument";
        private const string DataFieldName = "_data";
        private const string ProgressFieldName = "_progress";
        private const string GoldWalletFieldName = "_goldWallet";
        private const string SkillPointWalletFieldName = "_skillPointWallet";
        private const int NoSelectedNodeIndex = -1;
        private const int AffordableGoldAmount = 10000;

        private static void InjectPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"{target.GetType().Name} must expose private field {fieldName}.");
            field.SetValue(target, value);
        }

        private static SkillTreeData CreateFourNodeChainSkillTreeData()
        {
            SkillTreeData data = ScriptableObject.CreateInstance<SkillTreeData>();
            var entries = new List<SkillTreeData.SkillNodeEntry>
            {
                new SkillTreeData.SkillNodeEntry
                {
                    id = 0,
                    position = new Vector2(0f, 0f),
                    connectedNodeIds = new List<int>(),
                    costType = SkillTreeData.CostType.Gold,
                    maxLevel = 3,
                    baseCost = 10,
                    costMultiplierOdd = 1f,
                    costMultiplierEven = 1f,
                    costAdditivePerLevel = 0,
                    statModifierType = SkillTreeData.StatModifierType.HP,
                    statModifierMode = SkillTreeData.StatModifierMode.Flat,
                    statModifierValuePerLevel = 5f
                },
                new SkillTreeData.SkillNodeEntry
                {
                    id = 1,
                    position = new Vector2(1f, 0f),
                    connectedNodeIds = new List<int> { 0 },
                    costType = SkillTreeData.CostType.Gold,
                    maxLevel = 3,
                    baseCost = 10,
                    costMultiplierOdd = 1f,
                    costMultiplierEven = 1f,
                    costAdditivePerLevel = 0,
                    statModifierType = SkillTreeData.StatModifierType.Attack,
                    statModifierMode = SkillTreeData.StatModifierMode.Flat,
                    statModifierValuePerLevel = 1f
                },
                new SkillTreeData.SkillNodeEntry
                {
                    id = 2,
                    position = new Vector2(2f, 0f),
                    connectedNodeIds = new List<int> { 1 },
                    costType = SkillTreeData.CostType.Gold,
                    maxLevel = 3,
                    baseCost = 10,
                    costMultiplierOdd = 1f,
                    costMultiplierEven = 1f,
                    costAdditivePerLevel = 0,
                    statModifierType = SkillTreeData.StatModifierType.Defense,
                    statModifierMode = SkillTreeData.StatModifierMode.Flat,
                    statModifierValuePerLevel = 1f
                },
                new SkillTreeData.SkillNodeEntry
                {
                    id = 3,
                    position = new Vector2(3f, 0f),
                    connectedNodeIds = new List<int> { 2 },
                    costType = SkillTreeData.CostType.Gold,
                    maxLevel = 3,
                    baseCost = 10,
                    costMultiplierOdd = 1f,
                    costMultiplierEven = 1f,
                    costAdditivePerLevel = 0,
                    statModifierType = SkillTreeData.StatModifierType.Mana,
                    statModifierMode = SkillTreeData.StatModifierMode.Flat,
                    statModifierValuePerLevel = 1f
                }
            };

            FieldInfo nodesField = typeof(SkillTreeData).GetField("nodes", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(nodesField, "SkillTreeData must expose private nodes field.");
            nodesField.SetValue(data, entries);
            return data;
        }

        private SkillTreeScreenController BuildController(
            SkillTreeData data,
            SkillTreeProgress progress,
            GoldWallet goldWallet,
            SkillPointWallet skillPointWallet,
            out UIDocument uiDocument,
            bool activate = true)
        {
            PanelSettings panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            Assert.IsNotNull(panelSettings, $"MainPanelSettings must exist at {PanelSettingsPath}.");

            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(MainLayoutPath);
            Assert.IsNotNull(visualTree, $"MainLayout must exist at {MainLayoutPath}.");

            GameObject controllerGo = Track(new GameObject("SkillTreeScreenController"));
            controllerGo.SetActive(false);

            uiDocument = controllerGo.AddComponent<UIDocument>();
            uiDocument.panelSettings = panelSettings;
            uiDocument.visualTreeAsset = visualTree;

            SkillTreeScreenController controller = controllerGo.AddComponent<SkillTreeScreenController>();
            InjectPrivateField(controller, UiDocumentFieldName, uiDocument);
            if (data != null) InjectPrivateField(controller, DataFieldName, data);
            if (progress != null) InjectPrivateField(controller, ProgressFieldName, progress);
            if (goldWallet != null) InjectPrivateField(controller, GoldWalletFieldName, goldWallet);
            if (skillPointWallet != null) InjectPrivateField(controller, SkillPointWalletFieldName, skillPointWallet);

            if (activate) controllerGo.SetActive(true);
            return controller;
        }

        private GoldWallet CreateGoldWalletWithBalance(int startingBalance)
        {
            GameObject walletGo = Track(new GameObject("GoldWallet"));
            GoldWallet wallet = walletGo.AddComponent<GoldWallet>();
            wallet.Add(startingBalance);
            return wallet;
        }

        private SkillPointWallet CreateSkillPointWallet()
        {
            GameObject walletGo = Track(new GameObject("SkillPointWallet"));
            return walletGo.AddComponent<SkillPointWallet>();
        }

        [UnityTest]
        public IEnumerator Awake_SpawnsNodeElementPerDataNode()
        {
            SkillTreeData data = CreateFourNodeChainSkillTreeData();
            SkillTreeProgress progress = ScriptableObject.CreateInstance<SkillTreeProgress>();
            GoldWallet goldWallet = CreateGoldWalletWithBalance(AffordableGoldAmount);
            SkillPointWallet skillPointWallet = CreateSkillPointWallet();

            SkillTreeScreenController controller = BuildController(data, progress, goldWallet, skillPointWallet, out _);
            yield return null;

            Assert.AreEqual(4, controller.NodeElements.Count,
                "Controller must spawn one SkillTreeNodeElement per SkillTreeData node.");
        }

        [UnityTest]
        public IEnumerator Awake_NodesAreChildrenOfNodesLayer()
        {
            SkillTreeData data = CreateFourNodeChainSkillTreeData();
            SkillTreeProgress progress = ScriptableObject.CreateInstance<SkillTreeProgress>();
            GoldWallet goldWallet = CreateGoldWalletWithBalance(AffordableGoldAmount);
            SkillPointWallet skillPointWallet = CreateSkillPointWallet();

            SkillTreeScreenController controller = BuildController(data, progress, goldWallet, skillPointWallet, out UIDocument uiDocument);
            yield return null;

            VisualElement nodesLayer = uiDocument.rootVisualElement.Q<VisualElement>(NodesLayerElementName);
            Assert.IsNotNull(nodesLayer, $"{NodesLayerElementName} must be present in MainLayout.uxml.");
            foreach (SkillTreeNodeElement nodeElement in controller.NodeElements)
            {
                Assert.AreSame(nodesLayer, nodeElement.parent,
                    "Every spawned node element must be a child of the skilltree-nodes-layer VisualElement.");
            }
        }

        [UnityTest]
        public IEnumerator Awake_RootNodeWithNoPrereqs_IsAvailable()
        {
            SkillTreeData data = CreateFourNodeChainSkillTreeData();
            SkillTreeProgress progress = ScriptableObject.CreateInstance<SkillTreeProgress>();
            GoldWallet goldWallet = CreateGoldWalletWithBalance(AffordableGoldAmount);
            SkillPointWallet skillPointWallet = CreateSkillPointWallet();

            SkillTreeScreenController controller = BuildController(data, progress, goldWallet, skillPointWallet, out _);
            yield return null;

            Assert.AreEqual(SkillTreeNodeVisualState.Available, controller.NodeElements[0].CurrentState,
                "Root node with no prerequisites must start in Available state.");
        }

        [UnityTest]
        public IEnumerator Awake_NonRootNode_WithoutUnlockedPrereqs_IsLocked()
        {
            SkillTreeData data = CreateFourNodeChainSkillTreeData();
            SkillTreeProgress progress = ScriptableObject.CreateInstance<SkillTreeProgress>();
            GoldWallet goldWallet = CreateGoldWalletWithBalance(AffordableGoldAmount);
            SkillPointWallet skillPointWallet = CreateSkillPointWallet();

            SkillTreeScreenController controller = BuildController(data, progress, goldWallet, skillPointWallet, out _);
            yield return null;

            Assert.AreEqual(SkillTreeNodeVisualState.Locked, controller.NodeElements[1].CurrentState,
                "Non-root node whose prerequisites are still at level 0 must start in Locked state.");
        }

        [UnityTest]
        public IEnumerator NodeClick_SetsSelectedNodeIndex_AndShowsDetailPanel()
        {
            SkillTreeData data = CreateFourNodeChainSkillTreeData();
            SkillTreeProgress progress = ScriptableObject.CreateInstance<SkillTreeProgress>();
            GoldWallet goldWallet = CreateGoldWalletWithBalance(AffordableGoldAmount);
            SkillPointWallet skillPointWallet = CreateSkillPointWallet();

            SkillTreeScreenController controller = BuildController(data, progress, goldWallet, skillPointWallet, out UIDocument uiDocument);
            yield return null;

            SkillTreeNodeElement firstNode = controller.NodeElements[0];
            using (ClickEvent clickEvent = ClickEvent.GetPooled())
            {
                clickEvent.target = firstNode;
                firstNode.SendEvent(clickEvent);
            }

            yield return null;

            Assert.AreEqual(0, controller.SelectedNodeIndex,
                "Clicking a node must set SelectedNodeIndex to that node's index.");

            VisualElement detailPanel = uiDocument.rootVisualElement.Q<VisualElement>(DetailPanelElementName);
            Assert.IsNotNull(detailPanel, $"{DetailPanelElementName} must be present in MainLayout.uxml.");
            Assert.IsFalse(detailPanel.ClassListContains(HiddenClassName),
                "Detail panel must be visible (no hidden class) after a node click.");
        }

        [UnityTest]
        public IEnumerator UpgradeNode_RefreshesNodeState_FromAvailable_ToPurchased()
        {
            SkillTreeData data = CreateFourNodeChainSkillTreeData();
            SkillTreeProgress progress = ScriptableObject.CreateInstance<SkillTreeProgress>();
            GoldWallet goldWallet = CreateGoldWalletWithBalance(AffordableGoldAmount);
            SkillPointWallet skillPointWallet = CreateSkillPointWallet();

            SkillTreeScreenController controller = BuildController(data, progress, goldWallet, skillPointWallet, out UIDocument uiDocument);
            yield return null;

            SkillTreeNodeElement firstNode = controller.NodeElements[0];
            using (ClickEvent nodeClickEvent = ClickEvent.GetPooled())
            {
                nodeClickEvent.target = firstNode;
                firstNode.SendEvent(nodeClickEvent);
            }

            yield return null;

            Button upgradeButton = uiDocument.rootVisualElement.Q<Button>(UpgradeButtonElementName);
            Assert.IsNotNull(upgradeButton, $"{UpgradeButtonElementName} must be present in MainLayout.uxml.");

            using (ClickEvent upgradeClickEvent = ClickEvent.GetPooled())
            {
                upgradeClickEvent.target = upgradeButton;
                upgradeButton.SendEvent(upgradeClickEvent);
            }

            yield return null;

            Assert.AreEqual(1, progress.GetLevel(0),
                "Progress level for upgraded node must be 1 after one Upgrade click.");
            Assert.AreEqual(SkillTreeNodeVisualState.Purchased, controller.NodeElements[0].CurrentState,
                "Node state must transition to Purchased after upgrade.");
        }

        [UnityTest]
        public IEnumerator CloseDetail_ClearsSelectedNodeIndex()
        {
            SkillTreeData data = CreateFourNodeChainSkillTreeData();
            SkillTreeProgress progress = ScriptableObject.CreateInstance<SkillTreeProgress>();
            GoldWallet goldWallet = CreateGoldWalletWithBalance(AffordableGoldAmount);
            SkillPointWallet skillPointWallet = CreateSkillPointWallet();

            SkillTreeScreenController controller = BuildController(data, progress, goldWallet, skillPointWallet, out UIDocument uiDocument);
            yield return null;

            SkillTreeNodeElement firstNode = controller.NodeElements[0];
            using (ClickEvent nodeClickEvent = ClickEvent.GetPooled())
            {
                nodeClickEvent.target = firstNode;
                firstNode.SendEvent(nodeClickEvent);
            }

            yield return null;

            Assert.AreEqual(0, controller.SelectedNodeIndex,
                "Precondition: node click must select the clicked node.");

            Button closeButton = uiDocument.rootVisualElement.Q<Button>(CloseButtonElementName);
            Assert.IsNotNull(closeButton, $"{CloseButtonElementName} must be present in MainLayout.uxml.");

            using (ClickEvent closeClickEvent = ClickEvent.GetPooled())
            {
                closeClickEvent.target = closeButton;
                closeButton.SendEvent(closeClickEvent);
            }

            yield return null;

            Assert.AreEqual(NoSelectedNodeIndex, controller.SelectedNodeIndex,
                "Closing the detail panel must clear SelectedNodeIndex back to -1.");
        }

        [UnityTest]
        public IEnumerator Awake_WithMissingData_LogsError_AndDoesNotThrow()
        {
            GoldWallet goldWallet = CreateGoldWalletWithBalance(AffordableGoldAmount);
            SkillPointWallet skillPointWallet = CreateSkillPointWallet();

            LogAssert.Expect(LogType.Error,
                "SkillTreeScreenController requires SkillTreeData and SkillTreeProgress references.");

            SkillTreeScreenController controller = BuildController(null, null, goldWallet, skillPointWallet, out _);
            yield return null;

            Assert.IsNotNull(controller,
                "Controller GameObject must survive Awake even when SkillTreeData is missing.");
            Assert.AreEqual(0, controller.NodeElements.Count,
                "Controller must not spawn any node elements when SkillTreeData is missing.");
        }
    }
}
#endif
