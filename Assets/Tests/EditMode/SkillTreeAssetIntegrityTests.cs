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
    }
}
