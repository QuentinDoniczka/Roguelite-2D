using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.UI.Toolkit.SkillTree;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class SkillTreeStateEvaluatorLockedTests
    {
        private SkillTreeData _data;
        private SkillTreeProgress _progress;

        [SetUp]
        public void SetUp()
        {
            _data = ScriptableObject.CreateInstance<SkillTreeData>();
            _progress = ScriptableObject.CreateInstance<SkillTreeProgress>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_data);
            Object.DestroyImmediate(_progress);
        }

        private static SkillTreeData.SkillNodeEntry MakeNode(int id, int maxLevel, List<int> children)
        {
            return new SkillTreeData.SkillNodeEntry
            {
                id = id,
                maxLevel = maxLevel,
                connectedNodeIds = children ?? new List<int>()
            };
        }

        private void InitializeData(int ringNodeCount, List<SkillTreeData.SkillNodeEntry> nodes)
        {
            _data.RingNodeCount = ringNodeCount;
            _data.InitializeForTest(nodes);
        }

        [Test]
        public void Root_AlwaysAvailable_WhenLevelZero()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, 5, null),
                MakeNode(1, 5, null)
            };
            InitializeData(ringNodeCount: 2, nodes);

            var evaluator = new SkillTreeStateEvaluator(_data, _progress);

            Assert.AreEqual(SkillTreeNodeVisualState.Available, evaluator.GetState(0));
            Assert.AreEqual(SkillTreeNodeVisualState.Available, evaluator.GetState(1));
        }

        [Test]
        public void Root_Purchased_WhenLevelOne()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, 5, null)
            };
            InitializeData(ringNodeCount: 1, nodes);
            _progress.SetLevel(0, 1);

            var evaluator = new SkillTreeStateEvaluator(_data, _progress);

            Assert.AreEqual(SkillTreeNodeVisualState.Purchased, evaluator.GetState(0));
        }

        [Test]
        public void BranchNode_Locked_WhenAllParentsLevelZero()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, 5, new List<int> { 1 }),
                MakeNode(1, 5, null)
            };
            InitializeData(ringNodeCount: 1, nodes);

            var evaluator = new SkillTreeStateEvaluator(_data, _progress);

            Assert.AreEqual(SkillTreeNodeVisualState.Locked, evaluator.GetState(1));
        }

        [Test]
        public void BranchNode_Available_WhenAnyParentLevelOne()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, 5, new List<int> { 2 }),
                MakeNode(1, 5, new List<int> { 2 }),
                MakeNode(2, 5, null)
            };
            InitializeData(ringNodeCount: 2, nodes);
            _progress.SetLevel(0, 1);

            var evaluator = new SkillTreeStateEvaluator(_data, _progress);

            Assert.AreEqual(SkillTreeNodeVisualState.Available, evaluator.GetState(2));
        }

        [Test]
        public void BranchNode_Purchased_StaysPurchased_EvenIf_ParentLevelZero()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, 5, new List<int> { 1 }),
                MakeNode(1, 5, null)
            };
            InitializeData(ringNodeCount: 1, nodes);
            _progress.SetLevel(1, 1);

            var evaluator = new SkillTreeStateEvaluator(_data, _progress);

            Assert.AreEqual(SkillTreeNodeVisualState.Purchased, evaluator.GetState(1));
        }

        [Test]
        public void OrphanNode_TreatedAsUnlocked()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, 5, null),
                MakeNode(1, 5, null)
            };
            InitializeData(ringNodeCount: 1, nodes);

            var evaluator = new SkillTreeStateEvaluator(_data, _progress);

            Assert.AreEqual(SkillTreeNodeVisualState.Available, evaluator.GetState(1));
        }

        [Test]
        public void MaxLevel_StillReturnsMax()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, 5, new List<int> { 1 }),
                MakeNode(1, 3, null)
            };
            InitializeData(ringNodeCount: 1, nodes);
            _progress.SetLevel(1, 3);

            var evaluator = new SkillTreeStateEvaluator(_data, _progress);

            Assert.AreEqual(SkillTreeNodeVisualState.Max, evaluator.GetState(1));
        }
    }
}
