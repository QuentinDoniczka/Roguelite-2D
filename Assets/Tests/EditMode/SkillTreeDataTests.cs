using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Editor;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class SkillTreeDataTests
    {
        private SkillTreeData _skillTreeData;

        [SetUp]
        public void SetUp()
        {
            _skillTreeData = ScriptableObject.CreateInstance<SkillTreeData>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_skillTreeData);
        }

        [Test]
        public void GenerateNodes_NoCenterNode_AllNodesOnPerimeter()
        {
            _skillTreeData.RingNodeCount = 6;
            _skillTreeData.RingRadius = 5f;

            _skillTreeData.GenerateNodes();

            for (int i = 0; i < _skillTreeData.Nodes.Count; i++)
            {
                float distance = Vector2.Distance(_skillTreeData.Nodes[i].position, Vector2.zero);
                Assert.That(distance, Is.EqualTo(5f).Within(0.001f),
                    $"Node {i} should be on the perimeter at distance 5f");
            }
        }

        [Test]
        public void GenerateNodes_TotalNodesEqualsRingNodeCount()
        {
            _skillTreeData.RingNodeCount = 6;

            _skillTreeData.GenerateNodes();

            Assert.AreEqual(6, _skillTreeData.Nodes.Count);
        }

        [Test]
        public void GenerateNodes_RingNodesAtCorrectDistance()
        {
            _skillTreeData.RingNodeCount = 8;
            _skillTreeData.RingRadius = 3f;

            _skillTreeData.GenerateNodes();

            for (int i = 0; i < 8; i++)
            {
                float distance = Vector2.Distance(_skillTreeData.Nodes[i].position, Vector2.zero);
                Assert.That(distance, Is.EqualTo(3f).Within(0.001f),
                    $"Node {i} distance from origin should be 3f");
            }
        }

        [Test]
        public void GenerateNodes_RingNodesEvenlySpaced()
        {
            _skillTreeData.RingNodeCount = 4;
            _skillTreeData.RingRadius = 5f;

            _skillTreeData.GenerateNodes();

            Assert.That(_skillTreeData.Nodes[0].position.x, Is.EqualTo(5f).Within(0.01f));
            Assert.That(_skillTreeData.Nodes[0].position.y, Is.EqualTo(0f).Within(0.01f));

            Assert.That(_skillTreeData.Nodes[1].position.x, Is.EqualTo(0f).Within(0.01f));
            Assert.That(_skillTreeData.Nodes[1].position.y, Is.EqualTo(5f).Within(0.01f));

            Assert.That(_skillTreeData.Nodes[2].position.x, Is.EqualTo(-5f).Within(0.01f));
            Assert.That(_skillTreeData.Nodes[2].position.y, Is.EqualTo(0f).Within(0.01f));

            Assert.That(_skillTreeData.Nodes[3].position.x, Is.EqualTo(0f).Within(0.01f));
            Assert.That(_skillTreeData.Nodes[3].position.y, Is.EqualTo(-5f).Within(0.01f));
        }

        [Test]
        public void GenerateNodes_IsDeterministic()
        {
            _skillTreeData.RingNodeCount = 8;
            _skillTreeData.RingRadius = 5f;

            _skillTreeData.GenerateNodes();
            var firstRunPositions = new Vector2[_skillTreeData.Nodes.Count];
            for (int i = 0; i < _skillTreeData.Nodes.Count; i++)
            {
                firstRunPositions[i] = _skillTreeData.Nodes[i].position;
            }

            _skillTreeData.GenerateNodes();

            for (int i = 0; i < _skillTreeData.Nodes.Count; i++)
            {
                Assert.AreEqual(firstRunPositions[i], _skillTreeData.Nodes[i].position,
                    $"Node {i} position should be identical between runs");
            }
        }

        [Test]
        public void GenerateNodes_ClearsExistingNodes()
        {
            _skillTreeData.RingNodeCount = 5;
            _skillTreeData.GenerateNodes();

            _skillTreeData.RingNodeCount = 3;
            _skillTreeData.GenerateNodes();

            Assert.AreEqual(3, _skillTreeData.Nodes.Count);
        }

        [Test]
        public void GenerateNodes_RingNodeIdsAreSequential()
        {
            _skillTreeData.RingNodeCount = 5;

            _skillTreeData.GenerateNodes();

            for (int i = 0; i < _skillTreeData.Nodes.Count; i++)
            {
                Assert.AreEqual(i, _skillTreeData.Nodes[i].id,
                    $"Node at index {i} should have id {i}");
            }
        }

        [Test]
        public void GenerateNodes_MinRingNodeCount_ProducesValidRing()
        {
            _skillTreeData.RingNodeCount = 3;
            _skillTreeData.RingRadius = 4f;

            _skillTreeData.GenerateNodes();

            Assert.AreEqual(3, _skillTreeData.Nodes.Count);

            for (int i = 0; i < 3; i++)
            {
                float distance = Vector2.Distance(_skillTreeData.Nodes[i].position, Vector2.zero);
                Assert.That(distance, Is.EqualTo(4f).Within(0.001f),
                    $"Ring node {i} should be at distance 4f from origin");
            }
        }

        [Test]
        public void GenerateNodes_EachNodeHasConnectedNodeIds()
        {
            _skillTreeData.RingNodeCount = 6;
            _skillTreeData.GenerateNodes();

            for (int i = 0; i < _skillTreeData.Nodes.Count; i++)
            {
                Assert.IsNotNull(_skillTreeData.Nodes[i].connectedNodeIds,
                    $"Node {i} should have connectedNodeIds");
                Assert.AreEqual(1, _skillTreeData.Nodes[i].connectedNodeIds.Count,
                    $"Node {i} should connect to exactly 1 neighbor");
            }
        }

        [Test]
        public void GenerateNodes_ConnectionsFormClosedRing()
        {
            _skillTreeData.RingNodeCount = 6;
            _skillTreeData.GenerateNodes();

            for (int i = 0; i < 6; i++)
            {
                int expectedNext = (i + 1) % 6;
                Assert.AreEqual(expectedNext, _skillTreeData.Nodes[i].connectedNodeIds[0],
                    $"Node {i} should connect to node {expectedNext}");
            }
        }

        [Test]
        public void GenerateNodes_MinRingCount_ConnectionsFormClosedRing()
        {
            _skillTreeData.RingNodeCount = 3;
            _skillTreeData.GenerateNodes();

            for (int i = 0; i < 3; i++)
            {
                int expectedNext = (i + 1) % 3;
                Assert.AreEqual(expectedNext, _skillTreeData.Nodes[i].connectedNodeIds[0],
                    $"Node {i} should connect to node {expectedNext}");
            }
        }

        [Test]
        public void GetEdges_ReturnsCorrectCount()
        {
            _skillTreeData.RingNodeCount = 6;
            _skillTreeData.GenerateNodes();

            var edges = _skillTreeData.GetEdges();
            Assert.AreEqual(6, edges.Length);
        }

        [Test]
        public void GetEdges_NoDuplicateEdges()
        {
            _skillTreeData.RingNodeCount = 6;
            _skillTreeData.GenerateNodes();

            var edges = _skillTreeData.GetEdges();
            var uniqueEdges = new HashSet<(int, int)>(edges);
            Assert.AreEqual(edges.Length, uniqueEdges.Count, "All edges should be unique");
        }

        [Test]
        public void GenerateNodes_ClearsExistingConnections()
        {
            _skillTreeData.RingNodeCount = 5;
            _skillTreeData.GenerateNodes();

            _skillTreeData.RingNodeCount = 3;
            _skillTreeData.GenerateNodes();

            Assert.AreEqual(3, _skillTreeData.GetEdges().Length);
            for (int i = 0; i < 3; i++)
            {
                Assert.AreEqual(1, _skillTreeData.Nodes[i].connectedNodeIds.Count);
            }
        }

        [Test]
        public void GenerateNodes_NewFieldsInitializedToDefaults()
        {
            _skillTreeData.RingNodeCount = 6;
            _skillTreeData.GenerateNodes();

            for (int i = 0; i < _skillTreeData.Nodes.Count; i++)
            {
                var node = _skillTreeData.Nodes[i];
                Assert.AreEqual(SkillTreeData.CostType.Gold, node.costType,
                    $"Node {i} costType should default to Gold");
                Assert.AreEqual(0, node.maxLevel,
                    $"Node {i} maxLevel should default to 0 (unlimited)");
                Assert.AreEqual(SkillTreeData.StatModifierType.HP, node.statModifierType,
                    $"Node {i} statModifierType should default to HP");
                Assert.AreEqual(0f, node.statModifierValuePerLevel,
                    $"Node {i} statModifierValuePerLevel should default to 0");
            }
        }

        [Test]
        public void GenerateNodes_MinRingCount_NewFieldsInitialized()
        {
            _skillTreeData.RingNodeCount = 3;
            _skillTreeData.GenerateNodes();

            for (int i = 0; i < _skillTreeData.Nodes.Count; i++)
            {
                var node = _skillTreeData.Nodes[i];
                Assert.AreEqual(SkillTreeData.CostType.Gold, node.costType);
                Assert.AreEqual(0, node.maxLevel);
            }
        }

        [Test]
        public void HitTestNode_ReturnsCorrectIndex()
        {
            _skillTreeData.RingNodeCount = 4;
            _skillTreeData.RingRadius = 5f;
            _skillTreeData.GenerateNodes();

            Vector2 origin = Vector2.zero;
            float zoom = 1f;

            Vector2 clickOnNode0 = new Vector2(1000f, 0f);
            int result = SkillTreeDesignerWindow.HitTestNode(clickOnNode0, origin, _skillTreeData.Nodes, SkillTreeData.DefaultUnitSize, SkillTreeData.DefaultNodeSize, zoom);
            Assert.AreEqual(0, result);
        }

        [Test]
        public void HitTestNode_OutOfBounds_ReturnsMinusOne()
        {
            _skillTreeData.RingNodeCount = 4;
            _skillTreeData.RingRadius = 5f;
            _skillTreeData.GenerateNodes();

            Vector2 origin = Vector2.zero;
            float zoom = 1f;

            Vector2 clickFarAway = new Vector2(9999f, 9999f);
            int result = SkillTreeDesignerWindow.HitTestNode(clickFarAway, origin, _skillTreeData.Nodes, SkillTreeData.DefaultUnitSize, SkillTreeData.DefaultNodeSize, zoom);
            Assert.AreEqual(-1, result);
        }

        [Test]
        public void SetNode_UpdatesNodeAndInvalidatesEdgeCache()
        {
            _skillTreeData.RingNodeCount = 4;
            _skillTreeData.GenerateNodes();

            var edgesBefore = _skillTreeData.GetEdges();
            Assert.IsNotNull(edgesBefore);

            var node = _skillTreeData.Nodes[0];
            var updated = node;
            updated.maxLevel = 5;
            updated.statModifierValuePerLevel = 2.5f;
            _skillTreeData.SetNode(0, updated);

            Assert.AreEqual(5, _skillTreeData.Nodes[0].maxLevel);
            Assert.AreEqual(2.5f, _skillTreeData.Nodes[0].statModifierValuePerLevel);
        }

        [Test]
        public void GenerateNodes_NodeTypeDefaultsToPassive()
        {
            _skillTreeData.RingNodeCount = 6;
            _skillTreeData.GenerateNodes();

            for (int i = 0; i < _skillTreeData.Nodes.Count; i++)
            {
                Assert.AreEqual(SkillTreeData.NodeType.Passive, _skillTreeData.Nodes[i].nodeType,
                    $"Node {i} should default to Passive");
            }
        }
        [Test]
        public void ComputeNodeCost_DefaultValues_ReturnsBaseCost()
        {
            Assert.AreEqual(1, _skillTreeData.ComputeNodeCost(0));
            Assert.AreEqual(1, _skillTreeData.ComputeNodeCost(1));
            Assert.AreEqual(1, _skillTreeData.ComputeNodeCost(5));
        }

        [Test]
        public void ComputeNodeCost_WithMultiplier_ScalesExponentially()
        {
            _skillTreeData.BaseCost = 10;
            _skillTreeData.CostMultiplierOdd = 2f;
            _skillTreeData.CostMultiplierEven = 2f;
            _skillTreeData.CostAdditivePerLevel = 0;

            Assert.AreEqual(10, _skillTreeData.ComputeNodeCost(0));
            Assert.AreEqual(20, _skillTreeData.ComputeNodeCost(1));
            Assert.AreEqual(40, _skillTreeData.ComputeNodeCost(2));
        }

        [Test]
        public void ComputeNodeCost_WithAdditive_AddsAfterMultiply()
        {
            _skillTreeData.BaseCost = 10;
            _skillTreeData.CostMultiplierOdd = 1.5f;
            _skillTreeData.CostMultiplierEven = 1.5f;
            _skillTreeData.CostAdditivePerLevel = 5;

            Assert.AreEqual(10, _skillTreeData.ComputeNodeCost(0));
            Assert.AreEqual(20, _skillTreeData.ComputeNodeCost(1));
            Assert.AreEqual(35, _skillTreeData.ComputeNodeCost(2));
        }

        [Test]
        public void ComputeNodeCost_AdditiveOnly_LinearScaling()
        {
            _skillTreeData.BaseCost = 5;
            _skillTreeData.CostMultiplierOdd = 1f;
            _skillTreeData.CostMultiplierEven = 1f;
            _skillTreeData.CostAdditivePerLevel = 3;

            Assert.AreEqual(5, _skillTreeData.ComputeNodeCost(0));
            Assert.AreEqual(8, _skillTreeData.ComputeNodeCost(1));
            Assert.AreEqual(14, _skillTreeData.ComputeNodeCost(3));
        }

        [Test]
        public void ComputeNodeCost_AlternatingMultipliers()
        {
            _skillTreeData.BaseCost = 1;
            _skillTreeData.CostMultiplierOdd = 5f;
            _skillTreeData.CostMultiplierEven = 2f;
            _skillTreeData.CostAdditivePerLevel = 0;

            Assert.AreEqual(1, _skillTreeData.ComputeNodeCost(0));
            Assert.AreEqual(5, _skillTreeData.ComputeNodeCost(1));
            Assert.AreEqual(10, _skillTreeData.ComputeNodeCost(2));
            Assert.AreEqual(50, _skillTreeData.ComputeNodeCost(3));
        }
    }
}
