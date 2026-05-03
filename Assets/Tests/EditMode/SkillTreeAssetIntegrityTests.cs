using System;
using System.Linq;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class SkillTreeAssetIntegrityTests
    {
        private SkillTreeData _asset;

        [SetUp]
        public void SetUp()
        {
            _asset = ActiveSkillTreeResolver.GetActive();
        }

        [Test]
        public void Asset_LoadsSuccessfully()
        {
            Assert.IsNotNull(_asset, "Failed to resolve active SkillTreeData via ActiveSkillTreeResolver");
        }

        [Test]
        public void Asset_AllNodesHaveValidStatType()
        {
            Assert.IsNotNull(_asset, "Failed to resolve active SkillTreeData via ActiveSkillTreeResolver");

            foreach (var node in _asset.Nodes)
            {
                Assert.IsTrue(Enum.IsDefined(typeof(StatType), node.statModifierType),
                    $"Node {node.id} has invalid statModifierType={(int)node.statModifierType}");
            }
        }

        [Test]
        public void Asset_AllNodesHaveValidModifierMode()
        {
            Assert.IsNotNull(_asset, "Failed to resolve active SkillTreeData via ActiveSkillTreeResolver");

            foreach (var node in _asset.Nodes)
            {
                Assert.IsTrue(Enum.IsDefined(typeof(SkillTreeData.StatModifierMode), node.statModifierMode),
                    $"Node {node.id} has invalid statModifierMode={(int)node.statModifierMode}");
            }
        }

        [Test]
        public void Asset_HasExactlyOneCentralNode_AtId0()
        {
            Assert.IsNotNull(_asset, "Failed to resolve active SkillTreeData via ActiveSkillTreeResolver");

            var centrals = _asset.Nodes.Where(n => n.id == SkillTreeData.CentralNodeId).ToList();
            Assert.AreEqual(1, centrals.Count, "Expected exactly one node with id == CentralNodeId.");
        }

        [Test]
        public void Asset_CentralNode_HasExpectedShape()
        {
            Assert.IsNotNull(_asset, "Failed to resolve active SkillTreeData via ActiveSkillTreeResolver");

            var central = _asset.Nodes.First(n => n.id == SkillTreeData.CentralNodeId);
            Assert.AreEqual(SkillTreeData.CostType.Gold, central.costType, "Central node costType should be Gold.");
            Assert.AreEqual(1, central.maxLevel, "Central node maxLevel should be 1.");
            Assert.AreEqual(_asset.CentralUnlockCost, central.baseCost, "Central node baseCost should match centralUnlockCost.");
            Assert.AreEqual(0f, central.statModifierValuePerLevel, "Central node should grant no stat bonus.");
        }

        [Test]
        public void Asset_AllNodePositions_QuantizedTo01Unit()
        {
            Assert.IsNotNull(_asset, "Failed to resolve active SkillTreeData via ActiveSkillTreeResolver");

            var nodes = _asset.Nodes;
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                float expectedX = Mathf.Round(node.position.x / SkillTreeGrid.Step) * SkillTreeGrid.Step;
                float expectedY = Mathf.Round(node.position.y / SkillTreeGrid.Step) * SkillTreeGrid.Step;

                Assert.AreEqual(expectedX, node.position.x, 1e-5f,
                    $"Asset '{_asset.name}' node[{i}] id={node.id}: position.x={node.position.x} is not quantized to grid step {SkillTreeGrid.Step}");
                Assert.AreEqual(expectedY, node.position.y, 1e-5f,
                    $"Asset '{_asset.name}' node[{i}] id={node.id}: position.y={node.position.y} is not quantized to grid step {SkillTreeGrid.Step}");
            }
        }
    }
}
