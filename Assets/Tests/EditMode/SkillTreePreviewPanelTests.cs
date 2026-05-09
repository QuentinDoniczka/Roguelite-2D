#if UNITY_EDITOR
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
        public void ClickNode_CyclesState()
        {
            var panel = new SkillTreePreviewPanel(_data, _palette);
            panel.BuildRoot();

            panel.CycleNodeState(0);
            Assert.AreEqual(SkillTreeNodeVisualState.Available, GetFirstNodeElement(panel).CurrentState,
                "First cycle: Locked -> Available");

            panel.CycleNodeState(0);
            Assert.AreEqual(SkillTreeNodeVisualState.Purchased, GetFirstNodeElement(panel).CurrentState,
                "Second cycle: Available -> Purchased");

            panel.CycleNodeState(0);
            Assert.AreEqual(SkillTreeNodeVisualState.Max, GetFirstNodeElement(panel).CurrentState,
                "Third cycle: Purchased -> Max");

            panel.CycleNodeState(0);
            Assert.AreEqual(SkillTreeNodeVisualState.Locked, GetFirstNodeElement(panel).CurrentState,
                "Fourth cycle: Max -> Locked (wrap around)");
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

        private static SkillTreeNodeElement GetFirstNodeElement(SkillTreePreviewPanel panel)
        {
            var rootField = typeof(SkillTreePreviewPanel).GetField("_root",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var root = rootField?.GetValue(panel) as VisualElement;
            Assert.IsNotNull(root, "Could not access _root via reflection.");
            var elements = root.Query<SkillTreeNodeElement>().ToList();
            Assert.IsTrue(elements.Count > 0, "No SkillTreeNodeElement found in panel.");
            return elements[0];
        }
    }
}
#endif
