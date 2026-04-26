using System;
using RogueliteAutoBattler.Data;

namespace RogueliteAutoBattler.Combat.Core
{
    public sealed class AllyStatBonusService : IDisposable
    {
        private const float PercentInputToDecimal = 0.01f;

        private readonly TeamRoster _roster;
        private readonly SkillTreeData _data;
        private readonly SkillTreeProgress _progress;
        private bool _disposed;

        public AllyStatBonusService(TeamRoster roster, SkillTreeData data, SkillTreeProgress progress)
        {
            _roster = roster ?? throw new ArgumentNullException(nameof(roster));
            _data = data ?? throw new ArgumentNullException(nameof(data));
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));

            _roster.OnMemberSpawned += ApplyAndHealMember;
            _roster.OnMemberRevived += ApplyAndHealMember;
            _progress.OnLevelChanged += HandleLevelChanged;

            var existing = _roster.Members;
            for (int i = 0; i < existing.Count; i++)
                ApplyAndHealMember(existing[i]);
        }

        private void ApplyAndHealMember(TeamMember member)
        {
            ApplyAllToMember(member);
            if (member?.Stats != null && !member.Stats.IsDead)
                member.Stats.HealToFull();
        }

        private void HandleLevelChanged(int nodeIndex, int newLevel)
        {
            var members = _roster.Members;
            if (nodeIndex == SkillTreeProgress.BulkResetNodeIndex)
            {
                for (int i = 0; i < members.Count; i++)
                    ApplyAndHealMember(members[i]);
                return;
            }

            var nodes = _data.Nodes;
            if (nodeIndex >= nodes.Count) return;
            var node = nodes[nodeIndex];
            string source = ModifierSources.TechTreeNode(node.id);
            var resolved = ResolveNodeModifier(node, newLevel);

            for (int i = 0; i < members.Count; i++)
            {
                var member = members[i];
                if (member?.Stats == null) continue;
                ReapplyNodeModifierToStats(member.Stats, source, resolved);
                if (!member.Stats.IsDead) member.Stats.HealToFull();
            }
        }

        private static void ReapplyNodeModifierToStats(CombatStats stats, string source, (StatType stat, ModifierTier tier, float value)? resolved)
        {
            stats.RemoveModifiersFromSource(source);
            if (resolved.HasValue)
                stats.AddModifier(resolved.Value.stat, resolved.Value.tier, source, resolved.Value.value);
        }

        internal static (StatType stat, ModifierTier tier, float value)? ResolveNodeModifier(
            SkillTreeData.SkillNodeEntry node, int level)
        {
            if (level <= 0) return null;
            if (node.statModifierValuePerLevel == 0f) return null;

            bool isPercentMode = node.statModifierMode == SkillTreeData.StatModifierMode.Percent;
            float perLevelInPipelineUnits = isPercentMode
                ? node.statModifierValuePerLevel * PercentInputToDecimal
                : node.statModifierValuePerLevel;
            float value = perLevelInPipelineUnits * level;

            ModifierTier tier = isPercentMode ? ModifierTier.Percent : ModifierTier.Flat;
            return (node.statModifierType, tier, value);
        }

        private void ApplyAllToMember(TeamMember member)
        {
            if (member?.Stats == null) return;
            var nodes = _data.Nodes;
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                string source = ModifierSources.TechTreeNode(node.id);
                int level = _progress.GetLevel(i);
                var resolved = ResolveNodeModifier(node, level);
                ReapplyNodeModifierToStats(member.Stats, source, resolved);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _roster.OnMemberSpawned -= ApplyAndHealMember;
            _roster.OnMemberRevived -= ApplyAndHealMember;
            _progress.OnLevelChanged -= HandleLevelChanged;
        }
    }
}
