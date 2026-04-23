#if UNITY_EDITOR
using NUnit.Framework;
using UnityEditor;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class SkillTreeLayoutTests
    {
        private const string MainLayoutPath = "Assets/UI/Layouts/MainLayout.uxml";

        private const string ScreenSkillTreeName = "screen-skilltree";
        private const string ViewportName = "skilltree-viewport";
        private const string ContentName = "skilltree-content";
        private const string EdgeLayerName = "skilltree-edge-layer";
        private const string NodesLayerName = "skilltree-nodes-layer";
        private const string DetailPanelName = "skilltree-detail-panel";
        private const string DetailNameName = "skilltree-detail-name";
        private const string DetailLevelName = "skilltree-detail-level";
        private const string DetailCostName = "skilltree-detail-cost";
        private const string DetailBonusName = "skilltree-detail-bonus";
        private const string DetailUpgradeButtonName = "skilltree-detail-upgrade-btn";
        private const string DetailCloseButtonName = "skilltree-detail-close-btn";
        private const string HiddenClassName = "hidden";

        private VisualElement _clonedTreeRoot;

        [SetUp]
        public void Setup()
        {
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(MainLayoutPath);
            Assert.IsNotNull(visualTree, $"MainLayout must exist at {MainLayoutPath}.");

            _clonedTreeRoot = new VisualElement();
            visualTree.CloneTree(_clonedTreeRoot);
        }

        [Test]
        public void ScreenSkillTree_Exists()
        {
            VisualElement screenSkillTree = _clonedTreeRoot.Q<VisualElement>(ScreenSkillTreeName);
            Assert.IsNotNull(screenSkillTree, $"{ScreenSkillTreeName} must be present in MainLayout.uxml.");
        }

        [Test]
        public void ScreenSkillTree_HasViewportAndContentHierarchy()
        {
            VisualElement viewport = _clonedTreeRoot.Q<VisualElement>(ViewportName);
            VisualElement content = _clonedTreeRoot.Q<VisualElement>(ContentName);
            VisualElement edgeLayer = _clonedTreeRoot.Q<VisualElement>(EdgeLayerName);
            VisualElement nodesLayer = _clonedTreeRoot.Q<VisualElement>(NodesLayerName);

            Assert.IsNotNull(viewport, $"{ViewportName} must be present in MainLayout.uxml.");
            Assert.IsNotNull(content, $"{ContentName} must be present in MainLayout.uxml.");
            Assert.IsNotNull(edgeLayer, $"{EdgeLayerName} must be present in MainLayout.uxml.");
            Assert.IsNotNull(nodesLayer, $"{NodesLayerName} must be present in MainLayout.uxml.");

            Assert.IsTrue(IsAncestorOf(viewport, content),
                $"{ViewportName} must be an ancestor of {ContentName}.");
            Assert.IsTrue(IsAncestorOf(content, edgeLayer),
                $"{ContentName} must be an ancestor of {EdgeLayerName}.");
            Assert.IsTrue(IsAncestorOf(content, nodesLayer),
                $"{ContentName} must be an ancestor of {NodesLayerName}.");
        }

        [Test]
        public void ScreenSkillTree_EdgeLayer_IsNotPickable()
        {
            VisualElement edgeLayer = _clonedTreeRoot.Q<VisualElement>(EdgeLayerName);
            Assert.IsNotNull(edgeLayer, $"{EdgeLayerName} must be present in MainLayout.uxml.");
            Assert.AreEqual(PickingMode.Ignore, edgeLayer.pickingMode,
                $"{EdgeLayerName} must have pickingMode=Ignore so clicks pass through to nodes.");
        }

        [Test]
        public void ScreenSkillTree_DetailPanel_StartsHidden()
        {
            VisualElement detailPanel = _clonedTreeRoot.Q<VisualElement>(DetailPanelName);
            Assert.IsNotNull(detailPanel, $"{DetailPanelName} must be present in MainLayout.uxml.");
            Assert.IsTrue(detailPanel.ClassListContains(HiddenClassName),
                $"{DetailPanelName} must start with the '{HiddenClassName}' class.");
        }

        [Test]
        public void ScreenSkillTree_DetailPanel_HasAllLeafChildren()
        {
            VisualElement detailName = _clonedTreeRoot.Q<VisualElement>(DetailNameName);
            VisualElement detailLevel = _clonedTreeRoot.Q<VisualElement>(DetailLevelName);
            VisualElement detailCost = _clonedTreeRoot.Q<VisualElement>(DetailCostName);
            VisualElement detailBonus = _clonedTreeRoot.Q<VisualElement>(DetailBonusName);
            VisualElement detailUpgradeButton = _clonedTreeRoot.Q<VisualElement>(DetailUpgradeButtonName);
            VisualElement detailCloseButton = _clonedTreeRoot.Q<VisualElement>(DetailCloseButtonName);

            Assert.IsNotNull(detailName, $"{DetailNameName} must be present in MainLayout.uxml.");
            Assert.IsNotNull(detailLevel, $"{DetailLevelName} must be present in MainLayout.uxml.");
            Assert.IsNotNull(detailCost, $"{DetailCostName} must be present in MainLayout.uxml.");
            Assert.IsNotNull(detailBonus, $"{DetailBonusName} must be present in MainLayout.uxml.");
            Assert.IsNotNull(detailUpgradeButton, $"{DetailUpgradeButtonName} must be present in MainLayout.uxml.");
            Assert.IsNotNull(detailCloseButton, $"{DetailCloseButtonName} must be present in MainLayout.uxml.");
        }

        private static bool IsAncestorOf(VisualElement ancestor, VisualElement descendant)
        {
            VisualElement current = descendant?.parent;
            while (current != null)
            {
                if (current == ancestor)
                {
                    return true;
                }
                current = current.parent;
            }
            return false;
        }
    }
}
#endif
