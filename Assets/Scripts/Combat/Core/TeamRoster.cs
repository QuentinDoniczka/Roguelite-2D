using System;
using System.Collections.Generic;
using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.Combat.Core
{
    public class TeamRoster : MonoBehaviour
    {
        private readonly List<TeamMember> _members = new List<TeamMember>();

        public IReadOnlyList<TeamMember> Members => _members;

        public event Action<TeamMember> OnMemberSpawned;

        public void Spawn(TeamDatabase database, Transform teamContainer, Transform homeAnchor, float characterScale)
        {
            if (_members.Count > 0)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[{nameof(TeamRoster)}] Spawn called twice, ignoring");
#endif
                return;
            }

            if (database == null || database.Allies == null || database.Allies.Count == 0)
                return;

            Vector2 anchorPos = homeAnchor != null ? (Vector2)homeAnchor.position : Vector2.zero;

            var allies = database.Allies;
            Vector2[] positions = FormationLayout.GetPositions(anchorPos, allies.Count, facingRight: true, characterScale: characterScale);

            for (int i = 0; i < allies.Count; i++)
            {
                AllySpawnData data = allies[i];
                var member = new TeamMember(i, data);

                Vector2 spawnPos = positions[i];
                Vector2 homeOffset = positions[i] - anchorPos;

                GameObject spawnedGo = CombatSetupHelper.InstantiateAndAssembleAlly(
                    data,
                    teamContainer,
                    homeAnchor,
                    spawnPos,
                    homeOffset,
                    characterScale,
                    out CharacterComponents components);

                member.GameObject = spawnedGo;
                member.Stats = components.Stats;

                _members.Add(member);
                OnMemberSpawned?.Invoke(member);
            }
        }

        internal void InitializeForTest(List<TeamMember> members)
        {
            _members.Clear();
            if (members == null)
                return;

            _members.AddRange(members);
        }
    }
}
