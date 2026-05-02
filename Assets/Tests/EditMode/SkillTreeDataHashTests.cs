using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class SkillTreeDataHashTests
    {
        private SkillTreeData _treeA;
        private SkillTreeData _treeB;

        [TearDown]
        public void TearDown()
        {
            if (_treeA != null) Object.DestroyImmediate(_treeA);
            if (_treeB != null) Object.DestroyImmediate(_treeB);
        }

        private static SkillTreeData.SkillNodeEntry MakeNode(
            int id,
            Vector2 position,
            SkillTreeData.CostType costType = SkillTreeData.CostType.Gold,
            int maxLevel = 3,
            int baseCost = 10,
            float multOdd = 1f,
            float multEven = 1f,
            int additive = 0,
            StatType statType = StatType.Hp,
            SkillTreeData.StatModifierMode statMode = SkillTreeData.StatModifierMode.Flat,
            float statValue = 5f)
        {
            return new SkillTreeData.SkillNodeEntry
            {
                id = id,
                position = position,
                connectedNodeIds = new List<int>(),
                costType = costType,
                maxLevel = maxLevel,
                baseCost = baseCost,
                costMultiplierOdd = multOdd,
                costMultiplierEven = multEven,
                costAdditivePerLevel = additive,
                statModifierType = statType,
                statModifierMode = statMode,
                statModifierValuePerLevel = statValue
            };
        }

        private static SkillTreeData CreateTree(List<SkillTreeData.SkillNodeEntry> nodes)
        {
            var tree = ScriptableObject.CreateInstance<SkillTreeData>();
            tree.InitializeForTest(nodes);
            return tree;
        }

        [Test]
        public void ComputeGameplayHash_SameData_ReturnsSameHash()
        {
            _treeA = CreateTree(new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, Vector2.zero),
                MakeNode(1, new Vector2(1f, 0f)),
                MakeNode(2, new Vector2(2f, 0f), SkillTreeData.CostType.SkillPoint, 5, 3, 1.5f, 1.2f, 1)
            });

            string hashFirst = SkillTreeData.ComputeGameplayHash(_treeA);
            string hashSecond = SkillTreeData.ComputeGameplayHash(_treeA);

            Assert.IsNotEmpty(hashFirst);
            Assert.AreEqual(hashFirst, hashSecond);
        }

        [Test]
        public void ComputeGameplayHash_VisualOnlyChange_ReturnsSameHash()
        {
            _treeA = CreateTree(new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, Vector2.zero),
                MakeNode(1, new Vector2(1f, 0f))
            });
            _treeA.UnitSize = 40f;
            _treeA.NodeSize = 48f;
            _treeA.NodeColor = Color.red;
            _treeA.BorderNormalColor = Color.gray;
            _treeA.EdgeColor = Color.white;
            _treeA.EdgeThickness = 4f;

            string hashA = SkillTreeData.ComputeGameplayHash(_treeA);

            _treeB = CreateTree(new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(5f, 5f)),
                MakeNode(1, new Vector2(99f, 99f))
            });
            _treeB.UnitSize = 80f;
            _treeB.NodeSize = 96f;
            _treeB.NodeColor = Color.blue;
            _treeB.BorderNormalColor = Color.green;
            _treeB.EdgeColor = Color.yellow;
            _treeB.EdgeThickness = 12f;

            string hashB = SkillTreeData.ComputeGameplayHash(_treeB);

            Assert.AreEqual(hashA, hashB);
        }

        [Test]
        public void ComputeGameplayHash_GameplayChange_ReturnsDifferentHash()
        {
            _treeA = CreateTree(new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, Vector2.zero),
                MakeNode(1, new Vector2(1f, 0f), baseCost: 10)
            });

            _treeB = CreateTree(new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, Vector2.zero),
                MakeNode(1, new Vector2(1f, 0f), baseCost: 999)
            });

            string hashA = SkillTreeData.ComputeGameplayHash(_treeA);
            string hashB = SkillTreeData.ComputeGameplayHash(_treeB);

            Assert.AreNotEqual(hashA, hashB);
        }

        [Test]
        public void ComputeGameplayHash_NodeOrderChanged_ReturnsSameHash()
        {
            var nodeZero = MakeNode(0, Vector2.zero);
            var nodeOne = MakeNode(1, new Vector2(1f, 0f), baseCost: 25);
            var nodeTwo = MakeNode(2, new Vector2(2f, 0f), SkillTreeData.CostType.SkillPoint, 4, 7);

            _treeA = CreateTree(new List<SkillTreeData.SkillNodeEntry> { nodeZero, nodeOne, nodeTwo });
            _treeB = CreateTree(new List<SkillTreeData.SkillNodeEntry> { nodeTwo, nodeZero, nodeOne });

            string hashA = SkillTreeData.ComputeGameplayHash(_treeA);
            string hashB = SkillTreeData.ComputeGameplayHash(_treeB);

            Assert.AreEqual(hashA, hashB);
        }
    }
}
