using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Data;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class AllyStatBonusServiceResolverTests
    {
        [Test]
        public void ResolveNodeModifier_LevelZero_ReturnsNull()
        {
            var node = new SkillTreeData.SkillNodeEntry
            {
                id = 0,
                statModifierType = StatType.Hp,
                statModifierMode = SkillTreeData.StatModifierMode.Flat,
                statModifierValuePerLevel = 5f
            };
            Assert.IsNull(AllyStatBonusService.ResolveNodeModifier(node, 0));
        }

        [Test]
        public void ResolveNodeModifier_ZeroValuePerLevel_ReturnsNull()
        {
            var node = new SkillTreeData.SkillNodeEntry
            {
                id = 1,
                statModifierType = StatType.Hp,
                statModifierMode = SkillTreeData.StatModifierMode.Flat,
                statModifierValuePerLevel = 0f
            };
            Assert.IsNull(AllyStatBonusService.ResolveNodeModifier(node, 5));
        }

        [Test]
        public void ResolveNodeModifier_FlatHpAtLevel3_ReturnsFlatTimesLevel()
        {
            var node = new SkillTreeData.SkillNodeEntry
            {
                id = 2,
                statModifierType = StatType.Hp,
                statModifierMode = SkillTreeData.StatModifierMode.Flat,
                statModifierValuePerLevel = 5f
            };
            var result = AllyStatBonusService.ResolveNodeModifier(node, 3);
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(StatType.Hp, result.Value.stat);
            Assert.AreEqual(ModifierTier.Flat, result.Value.tier);
            Assert.AreEqual(15f, result.Value.value, 0.0001f);
        }

        [Test]
        public void ResolveNodeModifier_PercentAtkAtLevel2_ReturnsPercentTimesLevel()
        {
            var node = new SkillTreeData.SkillNodeEntry
            {
                id = 3,
                statModifierType = StatType.Atk,
                statModifierMode = SkillTreeData.StatModifierMode.Percent,
                statModifierValuePerLevel = 0.10f
            };
            var result = AllyStatBonusService.ResolveNodeModifier(node, 2);
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(StatType.Atk, result.Value.stat);
            Assert.AreEqual(ModifierTier.Percent, result.Value.tier);
            Assert.AreEqual(0.20f, result.Value.value, 0.0001f);
        }
    }
}
