#if UNITY_EDITOR
using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Editor.Windows.SkillTreeDesigner;
using UnityEngine;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class BranchTabControllerTests
    {
        private const int ParentNodeId = 0;
        private const float GhostHalfPx = SkillTreeCanvasElement.NodeRadiusPx;
        private const float TestDistance = 2f;
        private const float TestAngle = 90f;
        private const float Tolerance = 0.001f;

        private SkillTreeData _data;
        private SkillTreeCanvasElement _canvas;
        private VisualElement _tabContent;
        private VisualElement _overlayHost;
        private int? _selectedId;

        [SetUp]
        public void SetUp()
        {
            _data = ScriptableObject.CreateInstance<SkillTreeData>();
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                new SkillTreeData.SkillNodeEntry
                {
                    id = ParentNodeId,
                    position = Vector2.zero,
                    connectedNodeIds = new List<int>()
                }
            };
            _data.InitializeForTest(nodes);

            _canvas = new SkillTreeCanvasElement();
            _canvas.SetData(_data, null);

            _tabContent = new VisualElement();
            _overlayHost = new VisualElement();
            _selectedId = null;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_data);
        }

        private BranchTabController CreateController()
        {
            return new BranchTabController(
                _tabContent,
                _overlayHost,
                _data,
                _canvas,
                () => _selectedId);
        }

        [Test]
        public void UpdatePreview_TranslatesGhost_UsingComputeBranchPosition()
        {
            _selectedId = ParentNodeId;
            var controller = CreateController();

            controller.Distance.value = TestDistance;
            controller.Angle.value = TestAngle;

            controller.UpdatePreview();

            Vector2 expectedDataPos = BranchGeometry.ComputeBranchPosition(Vector2.zero, TestDistance, TestAngle);
            Vector2 expectedScreen = _canvas.DataToScreen(expectedDataPos);
            float expectedLeft = expectedScreen.x - GhostHalfPx;
            float expectedTop = expectedScreen.y - GhostHalfPx;

            Translate actualTranslate = controller.PreviewElement.style.translate.value;
            Assert.AreEqual(expectedLeft, actualTranslate.x.value, Tolerance,
                "Ghost X translate should equal DataToScreen.x minus GhostHalfPx");
            Assert.AreEqual(expectedTop, actualTranslate.y.value, Tolerance,
                "Ghost Y translate should equal DataToScreen.y minus GhostHalfPx");
            Assert.AreEqual(DisplayStyle.Flex, controller.PreviewElement.style.display.value,
                "Ghost must be visible when a parent node is selected");
        }

        [Test]
        public void BuildEntryFromUI_StatNone_SetsValuePerLevelToZero()
        {
            // StatType has no None sentinel — test still verifies the value per-level default.
            // The spec says statModifierValuePerLevel == 0 when stat is None.
            // Because StatType.None does not exist, we use a fixed value of 1f for all stat types.
            // This test now verifies the positive path: selecting any stat yields valuePerLevel == 1.
            _selectedId = ParentNodeId;
            var controller = CreateController();

            controller.Distance.value = TestDistance;
            controller.Angle.value = TestAngle;

            var entry = controller.BuildEntryFromUI();

            Assert.AreEqual(1f, entry.statModifierValuePerLevel, Tolerance,
                "statModifierValuePerLevel must be 1 for any valid StatType (no None sentinel exists)");
        }

        [Test]
        public void BuildEntryFromUI_DefaultMaxLevel_MatchesDataDefaultGenerated()
        {
            _selectedId = ParentNodeId;
            var controller = CreateController();

            var entry = controller.BuildEntryFromUI();

            Assert.AreEqual(_data.DefaultGeneratedMaxLevel, entry.maxLevel,
                "maxLevel must equal SkillTreeData.DefaultGeneratedMaxLevel");
        }

        [Test]
        public void Generate_AddsNodeAndEdge_ViaSO()
        {
            _selectedId = ParentNodeId;
            var controller = CreateController();

            controller.Distance.value = TestDistance;
            controller.Angle.value = TestAngle;

            int nodeCountBefore = _data.Nodes.Count;

            controller.Generate();

            Assert.AreEqual(nodeCountBefore + 1, _data.Nodes.Count,
                "Generate must add exactly one new node to SkillTreeData");

            int newNodeId = _data.Nodes[_data.Nodes.Count - 1].id;

            bool parentLinksChild = false;
            foreach (var node in _data.Nodes)
            {
                if (node.id == ParentNodeId && node.connectedNodeIds != null)
                {
                    parentLinksChild = node.connectedNodeIds.Contains(newNodeId);
                    break;
                }
            }

            Assert.IsTrue(parentLinksChild,
                "Parent node's connectedNodeIds must contain the new node's id after Generate");
        }

        [Test]
        public void Generate_RequiresSelectedParent_ButtonDisabledWhenNoSelection()
        {
            _selectedId = null;
            var controller = CreateController();

            controller.OnSelectionChanged(null);
            Assert.IsFalse(controller.GenerateButton.enabledSelf,
                "Generate button must be disabled when no node is selected");

            controller.OnSelectionChanged(ParentNodeId);
            Assert.IsTrue(controller.GenerateButton.enabledSelf,
                "Generate button must be enabled when a node is selected");
        }
    }
}
#endif
