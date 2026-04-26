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

            _roster.OnMemberSpawned += HandleMemberSpawned;
            _roster.OnMemberRevived += HandleMemberRevived;
            _progress.OnLevelChanged += HandleLevelChanged;

            var existing = _roster.Members;
            for (int i = 0; i < existing.Count; i++)
            {
                var member = existing[i];
                ApplyAllToMember(member);
                if (member?.Stats != null && !member.Stats.IsDead)
                    member.Stats.HealToFull();
            }
        }

        private void HandleMemberSpawned(TeamMember member)
        {
            ApplyAllToMember(member);
            if (member?.Stats != null && !member.Stats.IsDead)
                member.Stats.HealToFull();
        }

        private void HandleMemberRevived(TeamMember member)
        {
            ApplyAllToMember(member);
            if (member?.Stats != null && !member.Stats.IsDead)
                member.Stats.HealToFull();
        }

        private void HandleLevelChanged(int nodeIndex, int newLevel)
        {
            var members = _roster.Members;
            if (nodeIndex < 0)
            {
                for (int i = 0; i < members.Count; i++)
                {
                    var member = members[i];
                    if (member?.Stats == null) continue;
                    RemoveAllFromMember(member);
                    ApplyAllToMember(member);
                    if (!member.Stats.IsDead) member.Stats.HealToFull();
                }
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
                member.Stats.RemoveModifiersFromSource(source);
                if (resolved.HasValue)
                    member.Stats.AddModifier(resolved.Value.stat, resolved.Value.tier, source, resolved.Value.value);
                if (!member.Stats.IsDead) member.Stats.HealToFull();
            }
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
            _roster.OnMemberSpawned -= HandleMemberSpawned;
            _roster.OnMemberRevived -= HandleMemberRevived;
            _progress.OnLevelChanged -= HandleLevelChanged;
        }
    }
}
