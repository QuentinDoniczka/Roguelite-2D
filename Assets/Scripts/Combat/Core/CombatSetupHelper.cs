using System.Collections.Generic;
using RogueliteAutoBattler.Combat.Visuals;
using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.Combat.Core
{
    public struct CharacterComponents
    {
        public CombatStats Stats;
        public CombatController Controller;
    }

    public static class CombatSetupHelper
    {
        public const string TeamContainerName = "Team";
        public const string EnemiesContainerName = "Enemies";
        public const string TeamHomeAnchorName = "TeamHomeAnchor";
        public const string EnemiesHomeAnchorName = "EnemiesHomeAnchor";
        public const string CombatTriggerZoneName = "CombatTriggerZone";

        public static CharacterComponents AssembleCharacter(
            GameObject character,
            int maxHp,
            int atk,
            float attackSpeed,
            float regenHpPerSecond,
            float moveSpeed,
            Transform homeAnchor,
            Vector2 homeOffset,
            float colliderRadius,
            AppearanceData appearance,
            string callerName,
            Color? healthBarFillColor = null,
            Color? healthBarTrailColor = null,
            bool isAlly = true,
            float characterScale = 1f)
        {
            var combatStats = character.AddComponent<CombatStats>();
            combatStats.InitializeDirect(maxHp, atk, attackSpeed, regenHpPerSecond);

            var healthBar = character.AddComponent<HealthBar>();
            if (healthBarFillColor.HasValue || healthBarTrailColor.HasValue)
            {
                healthBar.SetColors(
                    healthBarFillColor ?? HealthBar.AllyFillColor,
                    healthBarTrailColor ?? HealthBar.DefaultTrailColor);
            }

            var mover = character.AddComponent<CharacterMover>();
            mover.SetMoveSpeed(moveSpeed);
            if (homeAnchor != null)
                mover.HomeAnchor = homeAnchor;
            mover.SetHomeOffset(homeOffset);

            var col = character.GetComponent<CircleCollider2D>();
            if (col != null)
                col.radius = colliderRadius / characterScale;

            var controller = character.AddComponent<CombatController>();
            WireAnimationRelay(character, controller, callerName);

            var appearanceComp = character.AddComponent<CharacterAppearance>();
            appearanceComp.ApplyAppearance(appearance);

            var characterTransform = character.transform;
            bool ally = isAlly;
            combatStats.OnDamageTaken += (damage, _) =>
            {
                if (characterTransform != null)
                    DamageNumberService.Show(characterTransform.position, damage, ally);
            };

            return new CharacterComponents
            {
                Stats = combatStats,
                Controller = controller
            };
        }

        private static void WireAnimationRelay(GameObject character, CombatController controller, string callerName)
        {
            var animator = character.GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogWarning($"[{callerName}] No Animator found on {character.name} — AnimationEventRelay not added.");
                return;
            }

            var relay = animator.gameObject.AddComponent<AnimationEventRelay>();
            relay.Initialize(controller);
        }

        public static void RecalculateFormation(Transform container, Transform homeAnchor, bool facingRight, float characterScale = 1f)
        {
            if (container == null || homeAnchor == null)
                return;

            var aliveList = new List<CharacterMover>();

            for (int i = 0; i < container.childCount; i++)
            {
                Transform child = container.GetChild(i);
                if (child.TryGetComponent<CombatStats>(out var stats)
                    && !stats.IsDead
                    && child.TryGetComponent<CharacterMover>(out var mover))
                {
                    aliveList.Add(mover);
                }
            }

            if (aliveList.Count == 0)
                return;

            Vector2 anchorPos = (Vector2)homeAnchor.position;
            Vector2[] positions = FormationLayout.GetPositions(anchorPos, aliveList.Count, facingRight, characterScale: characterScale);

            for (int i = 0; i < aliveList.Count; i++)
            {
                Vector2 offset = positions[i] - anchorPos;
                aliveList[i].SetHomeOffset(offset);
            }
        }

        public static void DestroyAllChildren(Transform container)
        {
            if (container == null)
                return;

            for (int i = container.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(container.GetChild(i).gameObject);
            }
        }

        public static void FindContainersIfNeeded(Transform parent, ref Transform teamContainer, ref Transform enemiesContainer, string callerName)
        {
            if (teamContainer == null)
                teamContainer = parent.Find(TeamContainerName);
            if (enemiesContainer == null)
                enemiesContainer = parent.Find(EnemiesContainerName);

            if (teamContainer == null)
                Debug.LogWarning($"[{callerName}] '{TeamContainerName}' container not found!");
            if (enemiesContainer == null)
                Debug.LogWarning($"[{callerName}] '{EnemiesContainerName}' container not found!");
        }
    }
}
