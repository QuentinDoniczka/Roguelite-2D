using System;
using System.Linq;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Data;
using UnityEditor;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class SkillTreeAssetIntegrityTests
    {
        private const string AssetPath = "Assets/Data/SkillTreeData.asset";

        private SkillTreeData _asset;

        [SetUp]
        public void SetUp()
        {
            _asset = AssetDatabase.LoadAssetAtPath<SkillTreeData>(AssetPath);
        }

        [Test]
        public void Asset_LoadsSuccessfully()
        {
            Assert.IsNotNull(_asset, $"Failed to load SkillTreeData at {AssetPath}");
        }

        [Test]
        public void Asset_AllNodesHaveValidStatType()
        {
            Assert.IsNotNull(_asset, $"Failed to load SkillTreeData at {AssetPath}");

            foreach (var node in _asset.Nodes)
            {
                Assert.IsTrue(Enum.IsDefined(typeof(StatType), node.statModifierType),
                    $"Node {node.id} has invalid statModifierType={(int)node.statModifierType}");
            }
        }

        [Test]
        public void Asset_AllNodesHaveValidModifierMode()
        {
            Assert.IsNotNull(_asset, $"Failed to load SkillTreeData at {AssetPath}");

            foreach (var node in _asset.Nodes)
            {
                Assert.IsTrue(Enum.IsDefined(typeof(SkillTreeData.StatModifierMode), node.statModifierMode),
                    $"Node {node.id} has invalid statModifierMode={(int)node.statModifierMode}");
            }
        }

        [Test]
        public void Asset_PreservesZeroMigrationIndexMapping()
        {
            Assert.IsNotNull(_asset, $"Failed to load SkillTreeData at {AssetPath}");

            var nodes = _asset.Nodes;
            Assert.GreaterOrEqual(nodes.Count, 6,
                "SkillTreeData.asset should contain at least 6 nodes (ring layout)");

            Assert.AreEqual(StatType.Atk, nodes[0].statModifierType,
                "Node 0 (YAML index 2) must map to Atk after zero-migration");
            Assert.AreEqual(StatType.Def, nodes[1].statModifierType,
                "Node 1 (YAML index 3) must map to Def");
            Assert.AreEqual(StatType.Mana, nodes[2].statModifierType,
                "Node 2 (YAML index 4) must map to Mana");
            Assert.AreEqual(StatType.Power, nodes[3].statModifierType,
                "Node 3 (YAML index 5) must map to Power");
            Assert.AreEqual(StatType.Hp, nodes[4].statModifierType,
                "Node 4 (YAML index 0) must map to Hp");
            Assert.AreEqual(StatType.RegenHp, nodes[5].statModifierType,
                "Node 5 (YAML index 1) must map to RegenHp");
        }
    }
}
