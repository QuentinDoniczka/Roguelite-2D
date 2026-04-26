using System;
using RogueliteAutoBattler.Data;

namespace RogueliteAutoBattler.Combat.Core
{
    public sealed class AllyStatBonusService : IDisposable
    {
        private readonly TeamRoster _roster;
        private readonly SkillTreeData _data;
        private readonly SkillTreeProgress _progress;
        private bool _disposed;

        public AllyStatBonusService(TeamRoster roster, SkillTreeData data, SkillTreeProgress progress)
        {
            _roster = roster ?? throw new ArgumentNullException(nameof(roster));
            _data = data ?? throw new ArgumentNullException(nameof(data));
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));
        }

        internal static (StatType stat, ModifierTier tier, float value)? ResolveNodeModifier(
            SkillTreeData.SkillNodeEntry node, int level)
        {
            if (level <= 0) return null;
            if (node.statModifierValuePerLevel == 0f) return null;
            float value = node.statModifierValuePerLevel * level;
            ModifierTier tier = node.statModifierMode == SkillTreeData.StatModifierMode.Percent
                ? ModifierTier.Percent
                : ModifierTier.Flat;
            return (node.statModifierType, tier, value);
        }

        internal void ApplyAllToMember(TeamMember member)
        {
            if (member?.Stats == null) return;
            var nodes = _data.Nodes;
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                string source = ModifierSources.TechTreeNode(node.id);
                member.Stats.RemoveModifiersFromSource(source);
                int level = _progress.GetLevel(i);
                var resolved = ResolveNodeModifier(node, level);
                if (resolved.HasValue)
                    member.Stats.AddModifier(resolved.Value.stat, resolved.Value.tier, source, resolved.Value.value);
            }
        }

        internal void RemoveAllFromMember(TeamMember member)
        {
            if (member?.Stats == null) return;
            var nodes = _data.Nodes;
            for (int i = 0; i < nodes.Count; i++)
                member.Stats.RemoveModifiersFromSource(ModifierSources.TechTreeNode(nodes[i].id));
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
        }
    }
}
