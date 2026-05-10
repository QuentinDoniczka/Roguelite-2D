using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Editor.Windows;
using RogueliteAutoBattler.UI.Toolkit.SkillTree;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class SkillTreePreviewPanelTests
    {
        private SkillTreeData _data;
        private SkillNodePalette _palette;

        [SetUp]
        public void SetUp()
        {
            _data = ScriptableObject.CreateInstance<SkillTreeData>();
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>
            {
                new SkillTreeData.SkillNodeEntry
                {
                    id = 0,
                    position = Vector2.zero,
                    connectedNodeIds = new List<int>(),
                    colorTag = NodeColorTag.Default
                }
            });
            _palette = ScriptableObject.CreateInstance<SkillNodePalette>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_data);
            Object.DestroyImmediate(_palette);
        }

        [Test]
        public void BuildRoot_CreatesSingleNodeContainer()
        {
            var panel = new SkillTreePreviewPanel(_data, _palette);
            var root = panel.BuildRoot();

            var nodeElements = root.Query<SkillTreeNodeElement>().ToList();
            Assert.IsTrue(nodeElements.Count >= 1, "Expected at least one SkillTreeNodeElement after BuildRoot.");
        }

        [Test]
        public void BuildRoot_LoadsMainStyleSheet()
        {
            var panel = new SkillTreePreviewPanel(_data, _palette);
            var root = panel.BuildRoot();

            Assert.IsTrue(root.styleSheets.count > 0, "Expected at least one stylesheet to be attached to the root.");
        }

        [Test]
        public void Rebuild_DoesNotLeakElements()
        {
            var panel = new SkillTreePreviewPanel(_data, _palette);
            var root = panel.BuildRoot();
            int childCountAfterBuild = root.childCount;

            panel.Rebuild();
            int childCountAfterRebuild = root.childCount;

            Assert.AreEqual(childCountAfterBuild, childCountAfterRebuild,
                "Rebuild should replace children without leaking extra elements.");
        }

        [Test]
        public void ClickNode_DoesNotCycleState()
        {
            var panel = new SkillTreePreviewPanel(_data, _palette);
            var root = panel.BuildRoot();

            var nodeElements = root.Query<SkillTreeNodeElement>().ToList();
            Assert.IsTrue(nodeElements.Count >= 1, "Designer Visual sub-tab must build at least one node element.");

            foreach (var element in nodeElements)
            {
                Assert.AreEqual(SkillTreeNodeVisualState.Locked, element.CurrentState,
                    "Designer Visual sub-tab keeps every node in Locked state.");
            }
        }

        [Test]
        public void DragNode_UpdatesPositionInData()
        {
            var panel = new SkillTreePreviewPanel(_data, _palette);
            panel.BuildRoot();

            var newPos = new Vector2(3f, 4f);
            panel.WriteNodePositionWithUndo(0, newPos);

            Assert.AreEqual(newPos, _data.Nodes[0].position, "Node position should be updated in SkillTreeData after WriteNodePositionWithUndo.");
            Assert.IsTrue(EditorUtility.IsDirty(_data), "SkillTreeData should be marked dirty after WriteNodePositionWithUndo.");
        }

        [Test]
        public void DragNode_RefreshesEdges_DuringAndAfterDrag()
        {
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>
            {
                new SkillTreeData.SkillNodeEntry
                {
                    id = 0,
                    position = Vector2.zero,
                    connectedNodeIds = new List<int> { 1 },
                    colorTag = NodeColorTag.Default
                },
                new SkillTreeData.SkillNodeEntry
                {
                    id = 1,
                    position = new Vector2(2f, 0f),
                    connectedNodeIds = new List<int>(),
                    colorTag = NodeColorTag.Default
                }
            });

            var panel = new SkillTreePreviewPanel(_data, _palette);
            panel.BuildRoot();

            var edgeLayer = panel.EdgeLayer;
            Assert.IsNotNull(edgeLayer, "EdgeLayer must be exposed for regression testing.");
            Assert.AreEqual(1, edgeLayer.EdgeCount, "Initial edge count must be 1 for the seeded two-node graph.");

            var liveDataPos = new Vector2(5f, 1f);
            panel.HandleDragStep(1, liveDataPos, commit: false);

            var expectedLive = SkillTreeGrid.Quantize(liveDataPos);
            Assert.AreEqual(expectedLive, _data.Nodes[1].position,
                "Live drag must update the data position so the edge layer reads the new endpoint immediately.");
            Assert.AreEqual(1, edgeLayer.EdgeCount,
                "Edge layer must still hold the rebuilt edge after the live drag refresh.");

            var finalDataPos = new Vector2(7f, 2f);
            panel.HandleDragStep(1, finalDataPos, commit: true);

            var expectedFinal = SkillTreeGrid.Quantize(finalDataPos);
            Assert.AreEqual(expectedFinal, _data.Nodes[1].position,
                "Drag end must commit the final data position so edges reconcile to the released endpoint.");
            Assert.AreEqual(1, edgeLayer.EdgeCount,
                "Edge layer must still hold the rebuilt edge after the drag-end refresh.");
        }
    }
}
