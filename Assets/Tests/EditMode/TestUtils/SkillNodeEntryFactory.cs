using System.Collections.Generic;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode.TestUtils
{
    internal static class SkillNodeEntryFactory
    {
        internal static SkillTreeData.SkillNodeEntry Default(int id, Vector2 position)
        {
            return new SkillTreeData.SkillNodeEntry
            {
                id = id,
                position = position,
                connectedNodeIds = new List<int>(),
                costType = SkillTreeData.CostType.Gold,
                maxLevel = 1,
                baseCost = 1,
                costMultiplierOdd = 1f,
                costMultiplierEven = 1f,
                costAdditivePerLevel = 0,
                statModifierType = StatType.Hp,
                statModifierMode = SkillTreeData.StatModifierMode.Flat,
                statModifierValuePerLevel = 1f
            };
        }

        internal static SkillTreeData.SkillNodeEntry At(Vector2 position)
        {
            return Default(0, position);
        }
    }
}
