using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Editor.Tools;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class SkillTreeDataSnapNormalizationTests
    {
        private const float Tolerance = 1e-4f;

        [Test]
        public void OnEnable_PristineNode_SnapEnabledAndDefaultThreshold()
        {
            var data = ScriptableObject.CreateInstance<SkillTreeData>();
            try
            {
                var centralNode = data.Nodes[0];

                Assert.That(centralNode.snapEnabled, Is.True);
                Assert.That(centralNode.snapThresholdUnits, Is.EqualTo(SkillTreeData.DefaultSnapThresholdUnits).Within(Tolerance));
            }
            finally
            {
                Object.DestroyImmediate(data);
            }
        }

        [Test]
        public void OnEnable_NodeWithSnapEnabledTrue_NotOverwritten()
        {
            var data = ScriptableObject.CreateInstance<SkillTreeData>();
            try
            {
                var branchEntry = SkillTreeNodeFactory.CreateBranchNode(1, new Vector2(1f, 0f));
                data.AddBranchNode(branchEntry, SkillTreeData.CentralNodeId);

                var addedNode = data.Nodes[data.Nodes.Count - 1];

                Assert.That(addedNode.snapEnabled, Is.True);
                Assert.That(addedNode.snapThresholdUnits, Is.EqualTo(SkillTreeData.DefaultSnapThresholdUnits).Within(Tolerance));
            }
            finally
            {
                Object.DestroyImmediate(data);
            }
        }

        [Test]
        public void OnEnable_NodeWithExplicitThreshold_NotOverwritten()
        {
            var data = ScriptableObject.CreateInstance<SkillTreeData>();
            try
            {
                var nodeWithExplicitThreshold = new SkillTreeData.SkillNodeEntry
                {
                    id = 2,
                    position = new Vector2(2f, 0f),
                    connectedNodeIds = new List<int>(),
                    costType = SkillTreeData.CostType.SkillPoint,
                    maxLevel = 1,
                    baseCost = 1,
                    costMultiplierOdd = 1f,
                    costMultiplierEven = 1f,
                    costAdditivePerLevel = 0,
                    statModifierType = StatType.Hp,
                    statModifierMode = SkillTreeData.StatModifierMode.Flat,
                    statModifierValuePerLevel = 5f,
                    snapEnabled = false,
                    snapThresholdUnits = 0.5f
                };
                data.AddNode(nodeWithExplicitThreshold);

                var retrievedNode = data.Nodes[data.Nodes.Count - 1];

                Assert.That(retrievedNode.snapEnabled, Is.False,
                    "Node with explicit (false, 0.5f) must not be overwritten by normalization — threshold is non-zero so heuristic does not fire.");
                Assert.That(retrievedNode.snapThresholdUnits, Is.EqualTo(0.5f).Within(Tolerance));
            }
            finally
            {
                Object.DestroyImmediate(data);
            }
        }
    }
}
