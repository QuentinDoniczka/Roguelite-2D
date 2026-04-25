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

        private static readonly (int nodeIndex, StatType expected, int legacyYamlIndex)[] MigrationMap =
        {
            (0, StatType.Atk,     2),
            (1, StatType.Def,     3),
            (2, StatType.Mana,    4),
            (3, StatType.Power,   5),
            (4, StatType.Hp,      0),
            (5, StatType.RegenHp, 1),
        };

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
            Assert.GreaterOrEqual(nodes.Count, MigrationMap.Length,
                "SkillTreeData.asset should contain at least one node per MigrationMap entry (ring layout)");

            foreach (var (nodeIndex, expected, legacyYamlIndex) in MigrationMap)
                Assert.AreEqual(expected, nodes[nodeIndex].statModifierType,
                    $"Node {nodeIndex} (YAML index {legacyYamlIndex}) must map to {expected}");
        }
    }
}
