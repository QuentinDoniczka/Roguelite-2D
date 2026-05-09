#if UNITY_EDITOR
using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Editor.Windows;
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
        public void BuildRoot_Creates4StateRows()
        {
            var panel = new SkillTreePreviewPanel(_data, _palette);
            var root = panel.BuildRoot();

            var labels = root.Query<Label>().ToList();
            var labelTexts = new System.Collections.Generic.HashSet<string>();
            foreach (var label in labels)
                labelTexts.Add(label.text);

            Assert.IsTrue(labelTexts.Contains("Locked"), "Expected a Label with text 'Locked'");
            Assert.IsTrue(labelTexts.Contains("Available"), "Expected a Label with text 'Available'");
            Assert.IsTrue(labelTexts.Contains("Purchased"), "Expected a Label with text 'Purchased'");
            Assert.IsTrue(labelTexts.Contains("Max"), "Expected a Label with text 'Max'");
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
    }
}
#endif
