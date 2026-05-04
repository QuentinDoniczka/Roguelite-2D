using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Editor.Tools;
using RogueliteAutoBattler.Editor.Windows;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class SkillTreeDesignerPickMirrorSourceTests
    {
        private SkillTreeData _data;
        private SkillTreeDesignerWindow _window;

        [SetUp]
        public void SetUp()
        {
            _data = ScriptableObject.CreateInstance<SkillTreeData>();
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, Vector2.zero),
                MakeNode(1, new Vector2(2f, 0f)),
                MakeNode(2, new Vector2(0f, 3f))
            });
            _window = ScriptableObject.CreateInstance<SkillTreeDesignerWindow>();
            _window.SetDataForTests(_data);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_window);
            Object.DestroyImmediate(_data);
        }

        private static SkillTreeData.SkillNodeEntry MakeNode(int id, Vector2 position)
        {
            return new SkillTreeData.SkillNodeEntry
            {
                id = id,
                position = position,
                connectedNodeIds = new List<int>(),
                costType = SkillTreeData.CostType.Gold,
                maxLevel = 1,
                baseCost = 1,
                costMultiplierOdd = 1f,
                costMultiplierEven = 1f,
                costAdditivePerLevel = 0,
                statModifierType = StatType.Hp,
                statModifierMode = SkillTreeData.StatModifierMode.Flat,
                statModifierValuePerLevel = 1f
            };
        }

        [Test]
        public void StartMirrorSourcePickMode_WhenMirrorEnabled_SetsActive()
        {
            _window.BeginBranchPreview(parentIndex: 1);
            _window.SetMirrorEnabled(true);

            _window.StartMirrorSourcePickMode();

            Assert.IsTrue(_window.IsMirrorSourcePickModeActiveForTests);
        }

        [Test]
        public void StartMirrorSourcePickMode_WhenMirrorDisabled_RemainsInactive()
        {
            _window.BeginBranchPreview(parentIndex: 1);
            _window.SetMirrorEnabled(false);

            _window.StartMirrorSourcePickMode();

            Assert.IsFalse(_window.IsMirrorSourcePickModeActiveForTests);
        }

        [Test]
        public void StartMirrorSourcePickMode_WhenPreviewInactive_RemainsInactive()
        {
            _window.StartMirrorSourcePickMode();

            Assert.IsFalse(_window.IsMirrorSourcePickModeActiveForTests);
        }

        [Test]
        public void CancelMirrorSourcePickMode_WhenActive_DeactivatesIt()
        {
            _window.BeginBranchPreview(parentIndex: 1);
            _window.SetMirrorEnabled(true);
            _window.StartMirrorSourcePickMode();

            _window.CancelMirrorSourcePickMode();

            Assert.IsFalse(_window.IsMirrorSourcePickModeActiveForTests);
        }

        [Test]
        public void EndBranchPreview_WhenPickModeActive_DeactivatesIt()
        {
            _window.BeginBranchPreview(parentIndex: 1);
            _window.SetMirrorEnabled(true);
            _window.MirrorSourceNodeIndexForTests = 2;
            _window.StartMirrorSourcePickMode();

            _window.EndBranchPreview();

            Assert.IsFalse(_window.IsMirrorSourcePickModeActiveForTests);
            Assert.AreEqual(BranchPlacement.NoMirrorSourceOverride, _window.MirrorSourceNodeIndexForTests);
        }

        [Test]
        public void SetMirrorEnabled_False_WhenPickModeActive_DeactivatesIt()
        {
            _window.BeginBranchPreview(parentIndex: 1);
            _window.SetMirrorEnabled(true);
            _window.StartMirrorSourcePickMode();

            _window.SetMirrorEnabled(false);

            Assert.IsFalse(_window.IsMirrorSourcePickModeActiveForTests);
        }

        [Test]
        public void BeginBranchPreview_ResetsPickMode()
        {
            _window.BeginBranchPreview(parentIndex: 1);
            _window.SetMirrorEnabled(true);
            _window.StartMirrorSourcePickMode();

            _window.BeginBranchPreview(parentIndex: 2);

            Assert.IsFalse(_window.IsMirrorSourcePickModeActiveForTests);
        }
    }
}
