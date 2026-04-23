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
        private const string ViewportElementName = "skilltree-viewport";
        private const string UpgradeButtonElementName = "skilltree-detail-upgrade-btn";
        private const string CloseButtonElementName = "skilltree-detail-close-btn";
        private const string HiddenClassName = "hidden";
        private const string UiDocumentFieldName = "_uiDocument";
        private const string DataFieldName = "_data";
        private const string ProgressFieldName = "_progress";
        private const string GoldWalletFieldName = "_goldWallet";
        private const string SkillPointWalletFieldName = "_skillPointWallet";
        private const string PanZoomManipulatorFieldName = "_panZoomManipulator";
        private const int NoSelectedNodeIndex = -1;
        private const int AffordableGoldAmount = 10000;
        private const int PrimaryPointerId = 0;

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
        public IEnumerator Click_OnNode_ManipulatorNeitherDragsNorCaptures_PreservingClickSynthesisPath()
        {
            SkillTreeData data = CreateFourNodeChainSkillTreeData();
            SkillTreeProgress progress = ScriptableObject.CreateInstance<SkillTreeProgress>();
            GoldWallet goldWallet = CreateGoldWalletWithBalance(AffordableGoldAmount);
            SkillPointWallet skillPointWallet = CreateSkillPointWallet();

            SkillTreeScreenController controller = BuildController(data, progress, goldWallet, skillPointWallet, out UIDocument uiDocument);
            yield return null;
            yield return null;

            SkillTreeNodeElement firstNode = controller.NodeElements[0];
            VisualElement viewport = uiDocument.rootVisualElement.Q<VisualElement>(ViewportElementName);
            Assert.IsNotNull(viewport, $"{ViewportElementName} must be present in MainLayout.uxml.");

            FieldInfo manipulatorField = typeof(SkillTreeScreenController).GetField(
                PanZoomManipulatorFieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(manipulatorField,
                $"{nameof(SkillTreeScreenController)} must expose private field {PanZoomManipulatorFieldName}.");
            SkillTreePanZoomManipulator manipulator = manipulatorField.GetValue(controller) as SkillTreePanZoomManipulator;
            Assert.IsNotNull(manipulator,
                "Pan-zoom manipulator must be instantiated after controller Build for this assertion to be meaningful.");

            Vector2 clickPosition = firstNode.worldBound.center;

            using (PointerDownEvent downEvent = PointerDownEvent.GetPooled())
            {
                PopulatePointerEventOnTarget(downEvent, clickPosition, PrimaryPointerId, viewport);
                viewport.SendEvent(downEvent);
            }

            using (PointerUpEvent upEvent = PointerUpEvent.GetPooled())
            {
                PopulatePointerEventOnTarget(upEvent, clickPosition, PrimaryPointerId, viewport);
                viewport.SendEvent(upEvent);
            }

            yield return null;

            Assert.IsFalse(manipulator.ExceededClickVersusDragThreshold,
                "A stationary Down+Up gesture must not flag the manipulator as having exceeded the click-versus-drag threshold.");
            Assert.IsFalse(viewport.HasPointerCapture(PrimaryPointerId),
                "Viewport must not retain pointer capture after a stationary Down+Up gesture, so UI Toolkit can synthesize a ClickEvent on the underlying node.");
        }

        private static void PopulatePointerEventOnTarget(EventBase evt, Vector2 position, int pointerId, VisualElement eventTarget)
        {
            Vector3 position3 = new Vector3(position.x, position.y, 0f);
            SetEventProperty(evt, "position", position3);
            SetEventProperty(evt, "localPosition", position3);
            SetEventProperty(evt, "pointerId", pointerId);
            evt.target = eventTarget;
        }

        private static void SetEventProperty(EventBase evt, string propertyName, object value)
        {
            System.Type type = evt.GetType();
            while (type != null)
            {
                PropertyInfo property = type.GetProperty(
                    propertyName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (property != null && property.CanWrite)
                {
                    property.SetValue(evt, value);
                    return;
                }

                FieldInfo field = type.GetField(
                    propertyName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (field != null)
                {
                    field.SetValue(evt, value);
                    return;
                }

                type = type.BaseType;
            }
        }

        [UnityTest]
        public IEnumerator Awake_CentersContentOnFirstGeometryEvent()
        {
            const float ViewportWidthPixels = 400f;
            const float ViewportHeightPixels = 300f;
            const float ExpectedCenterX = ViewportWidthPixels * 0.5f;
            const float ExpectedCenterY = ViewportHeightPixels * 0.5f;
            const float CenterPositionTolerancePixels = 0.01f;

            SkillTreeData data = CreateFourNodeChainSkillTreeData();
            SkillTreeProgress progress = ScriptableObject.CreateInstance<SkillTreeProgress>();
            GoldWallet goldWallet = CreateGoldWalletWithBalance(AffordableGoldAmount);
            SkillPointWallet skillPointWallet = CreateSkillPointWallet();

            SkillTreeScreenController controller = BuildController(data, progress, goldWallet, skillPointWallet, out _);
            yield return null;

            VisualElement viewport = controller.Viewport;
            Assert.IsNotNull(viewport, "Controller must expose its viewport after Build for centering assertions.");
            viewport.style.width = ViewportWidthPixels;
            viewport.style.height = ViewportHeightPixels;

            yield return null;

            using (GeometryChangedEvent firstGeometryEvent = GeometryChangedEvent.GetPooled(
                Rect.zero, new Rect(0f, 0f, ViewportWidthPixels, ViewportHeightPixels)))
            {
                firstGeometryEvent.target = viewport;
                viewport.SendEvent(firstGeometryEvent);
            }

            yield return null;

            Assert.IsTrue(controller.HasCenteredContent,
                "Controller must mark content as centered after the first GeometryChangedEvent fires with a positive viewport size.");

            Vector3 expectedCenter = new Vector3(ExpectedCenterX, ExpectedCenterY, 0f);
            float distanceFromExpected = Vector3.Distance(controller.ContentTargetPosition, expectedCenter);
            Assert.Less(distanceFromExpected, CenterPositionTolerancePixels,
                $"Content target position must be centered at the viewport midpoint ({ExpectedCenterX}, {ExpectedCenterY}); got {controller.ContentTargetPosition}.");
        }

        [UnityTest]
        public IEnumerator Center_IsIdempotent_AfterMultipleGeometryEvents()
        {
            const float InitialViewportWidthPixels = 400f;
            const float InitialViewportHeightPixels = 300f;
            const float ResizedViewportWidthPixels = 800f;
            const float ResizedViewportHeightPixels = 600f;
            const float InitialCenterX = InitialViewportWidthPixels * 0.5f;
            const float InitialCenterY = InitialViewportHeightPixels * 0.5f;
            const float CenterPositionTolerancePixels = 0.01f;

            SkillTreeData data = CreateFourNodeChainSkillTreeData();
            SkillTreeProgress progress = ScriptableObject.CreateInstance<SkillTreeProgress>();
            GoldWallet goldWallet = CreateGoldWalletWithBalance(AffordableGoldAmount);
            SkillPointWallet skillPointWallet = CreateSkillPointWallet();

            SkillTreeScreenController controller = BuildController(data, progress, goldWallet, skillPointWallet, out _);
            yield return null;

            VisualElement viewport = controller.Viewport;
            Assert.IsNotNull(viewport, "Controller must expose its viewport after Build for centering assertions.");
            viewport.style.width = InitialViewportWidthPixels;
            viewport.style.height = InitialViewportHeightPixels;

            yield return null;

            using (GeometryChangedEvent firstGeometryEvent = GeometryChangedEvent.GetPooled(
                Rect.zero, new Rect(0f, 0f, InitialViewportWidthPixels, InitialViewportHeightPixels)))
            {
                firstGeometryEvent.target = viewport;
                viewport.SendEvent(firstGeometryEvent);
            }

            yield return null;

            Assert.IsTrue(controller.HasCenteredContent,
                "Precondition: content must be centered after the first GeometryChangedEvent.");
            Vector3 initialCenter = controller.ContentTargetPosition;

            viewport.style.width = ResizedViewportWidthPixels;
            viewport.style.height = ResizedViewportHeightPixels;

            yield return null;

            using (GeometryChangedEvent secondGeometryEvent = GeometryChangedEvent.GetPooled(
                new Rect(0f, 0f, InitialViewportWidthPixels, InitialViewportHeightPixels),
                new Rect(0f, 0f, ResizedViewportWidthPixels, ResizedViewportHeightPixels)))
            {
                secondGeometryEvent.target = viewport;
                viewport.SendEvent(secondGeometryEvent);
            }

            yield return null;

            Assert.IsTrue(controller.HasCenteredContent,
                "Centered flag must remain true after subsequent GeometryChangedEvents.");
            float distanceFromInitial = Vector3.Distance(controller.ContentTargetPosition, initialCenter);
            Assert.Less(distanceFromInitial, CenterPositionTolerancePixels,
                $"Centering must run only once: position must remain at the initial center ({InitialCenterX}, {InitialCenterY}) and not move when the viewport is resized.");
        }

        [UnityTest]
        public IEnumerator EmptyViewportClick_AfterNodeSelection_DeselectsNodeAndHidesDetail()
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
                "Precondition: node click must set SelectedNodeIndex to 0.");

            VisualElement viewport = uiDocument.rootVisualElement.Q<VisualElement>(ViewportElementName);
            Assert.IsNotNull(viewport, $"{ViewportElementName} must be present in MainLayout.uxml.");

            using (ClickEvent viewportClickEvent = ClickEvent.GetPooled())
            {
                viewportClickEvent.target = viewport;
                viewport.SendEvent(viewportClickEvent);
            }

            yield return null;

            Assert.AreEqual(NoSelectedNodeIndex, controller.SelectedNodeIndex,
                "Clicking the viewport (not a node) must deselect and reset SelectedNodeIndex to -1.");

            VisualElement detailPanel = uiDocument.rootVisualElement.Q<VisualElement>(DetailPanelElementName);
            Assert.IsNotNull(detailPanel, $"{DetailPanelElementName} must be present in MainLayout.uxml.");
            Assert.IsTrue(detailPanel.ClassListContains(HiddenClassName),
                "Detail panel must be hidden after an empty viewport click deselects the node.");
        }

        [UnityTest]
        public IEnumerator ViewportClick_AfterDrag_DoesNotDeselect()
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
                "Precondition: node click must set SelectedNodeIndex to 0.");

            VisualElement viewport = uiDocument.rootVisualElement.Q<VisualElement>(ViewportElementName);
            Assert.IsNotNull(viewport, $"{ViewportElementName} must be present in MainLayout.uxml.");

            FieldInfo manipulatorField = typeof(SkillTreeScreenController).GetField(
                PanZoomManipulatorFieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(manipulatorField,
                $"{nameof(SkillTreeScreenController)} must expose private field {PanZoomManipulatorFieldName}.");
            SkillTreePanZoomManipulator manipulator = manipulatorField.GetValue(controller) as SkillTreePanZoomManipulator;
            Assert.IsNotNull(manipulator, "Pan-zoom manipulator must be instantiated after Build.");

            FieldInfo clickVsDragField = typeof(SkillTreePanZoomManipulator).GetField(
                "_clickVsDragExceededThisGesture", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(clickVsDragField,
                "SkillTreePanZoomManipulator must expose private field _clickVsDragExceededThisGesture.");
            clickVsDragField.SetValue(manipulator, true);

            using (ClickEvent viewportClickEvent = ClickEvent.GetPooled())
            {
                viewportClickEvent.target = viewport;
                viewport.SendEvent(viewportClickEvent);
            }

            yield return null;

            Assert.AreEqual(0, controller.SelectedNodeIndex,
                "A viewport click that follows a drag exceeding the threshold must not deselect the current node.");

            VisualElement detailPanel = uiDocument.rootVisualElement.Q<VisualElement>(DetailPanelElementName);
            Assert.IsNotNull(detailPanel, $"{DetailPanelElementName} must be present in MainLayout.uxml.");
            Assert.IsFalse(detailPanel.ClassListContains(HiddenClassName),
                "Detail panel must remain visible when the viewport click was preceded by a drag gesture.");
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
