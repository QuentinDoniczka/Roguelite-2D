using System.Collections.Generic;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.Editor.Tools
{
    internal static class SkillTreeNodeFactory
    {
        private const int DefaultBranchMaxLevel = 1;
        private const int DefaultBranchBaseCost = 1;
        private const float DefaultBranchCostMultiplierOdd = 1f;
        private const float DefaultBranchCostMultiplierEven = 1f;
        private const int DefaultBranchCostAdditivePerLevel = 0;
        private const float DefaultBranchStatValuePerLevel = 5f;

        public static SkillTreeData.SkillNodeEntry CreateBranchNode(int newId, Vector2 position)
        {
            return new SkillTreeData.SkillNodeEntry
            {
                id = newId,
                position = position,
                connectedNodeIds = new List<int>(),
                costType = SkillTreeData.CostType.SkillPoint,
                maxLevel = DefaultBranchMaxLevel,
                baseCost = DefaultBranchBaseCost,
                costMultiplierOdd = DefaultBranchCostMultiplierOdd,
                costMultiplierEven = DefaultBranchCostMultiplierEven,
                costAdditivePerLevel = DefaultBranchCostAdditivePerLevel,
                statModifierType = StatType.Hp,
                statModifierMode = SkillTreeData.StatModifierMode.Flat,
                statModifierValuePerLevel = DefaultBranchStatValuePerLevel
            };
        }
    }
}
