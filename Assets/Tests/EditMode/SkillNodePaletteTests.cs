using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Data;
using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class SkillNodePaletteTests
    {
        private SkillNodePalette _palette;
        private SkillTreeData _treeA;
        private SkillTreeData _treeB;

        [TearDown]
        public void TearDown()
        {
            if (_palette != null) Object.DestroyImmediate(_palette);
            if (_treeA != null) Object.DestroyImmediate(_treeA);
            if (_treeB != null) Object.DestroyImmediate(_treeB);
            _palette = null;
            _treeA = null;
            _treeB = null;
        }

        [Test]
        public void GetColor_ReturnsConfiguredColor_AndFallsBackToWhiteForMissing()
        {
            _palette = ScriptableObject.CreateInstance<SkillNodePalette>();
            var serialized = new SerializedObject(_palette);
            var entriesProperty = serialized.FindProperty(SkillNodePalette.FieldNames.Entries);
            Assert.IsNotNull(entriesProperty, $"Field '{SkillNodePalette.FieldNames.Entries}' not found on SkillNodePalette.");

            entriesProperty.arraySize = 2;

            var entry0 = entriesProperty.GetArrayElementAtIndex(0);
            entry0.FindPropertyRelative("tag").enumValueIndex = (int)NodeColorTag.Red;
            entry0.FindPropertyRelative("color").colorValue = Color.red;

            var entry1 = entriesProperty.GetArrayElementAtIndex(1);
            entry1.FindPropertyRelative("tag").enumValueIndex = (int)NodeColorTag.Blue;
            entry1.FindPropertyRelative("color").colorValue = Color.blue;

            serialized.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(Color.red, _palette.GetColor(NodeColorTag.Red), "Red tag should return configured Color.red.");
            Assert.AreEqual(Color.blue, _palette.GetColor(NodeColorTag.Blue), "Blue tag should return configured Color.blue.");
            Assert.AreEqual(Color.white, _palette.GetColor(NodeColorTag.Green), "Missing tag (Green) should fall back to Color.white.");
            Assert.AreEqual(Color.white, _palette.GetColor(NodeColorTag.Default), "Default tag (unconfigured) should fall back to Color.white.");
        }

        [Test]
        public void ComputeGameplayHash_IsUnchanged_WhenOnlyColorTagDiffers()
        {
            _treeA = ScriptableObject.CreateInstance<SkillTreeData>();
            _treeB = ScriptableObject.CreateInstance<SkillTreeData>();

            _treeA.InitializeForTest(BuildIdenticalEntries(NodeColorTag.Default));
            _treeB.InitializeForTest(BuildIdenticalEntries(NodeColorTag.Default));

            var entriesB = new List<SkillTreeData.SkillNodeEntry>(_treeB.Nodes);
            var mutated = entriesB[1];
            mutated.colorTag = NodeColorTag.Red;
            entriesB[1] = mutated;
            _treeB.InitializeForTest(entriesB);

            string hashA = SkillTreeData.ComputeGameplayHash(_treeA);
            string hashB = SkillTreeData.ComputeGameplayHash(_treeB);

            Assert.AreEqual(hashA, hashB, "Gameplay hash must be invariant under colorTag-only changes (cosmetic).");
        }

        private static List<SkillTreeData.SkillNodeEntry> BuildIdenticalEntries(NodeColorTag tag)
        {
            return new List<SkillTreeData.SkillNodeEntry>
            {
                new SkillTreeData.SkillNodeEntry
                {
                    id = 0,
                    position = Vector2.zero,
                    connectedNodeIds = new List<int> { 1 },
                    costType = SkillTreeData.CostType.Gold,
                    maxLevel = 1,
                    baseCost = 100,
                    costMultiplierOdd = 1f,
                    costMultiplierEven = 1f,
                    costAdditivePerLevel = 0,
                    colorTag = tag
                },
                new SkillTreeData.SkillNodeEntry
                {
                    id = 1,
                    position = new Vector2(1f, 0f),
                    connectedNodeIds = new List<int>(),
                    costType = SkillTreeData.CostType.SkillPoint,
                    maxLevel = 5,
                    baseCost = 1,
                    costMultiplierOdd = 1f,
                    costMultiplierEven = 1f,
                    costAdditivePerLevel = 0,
                    colorTag = tag
                }
            };
        }
    }
}
