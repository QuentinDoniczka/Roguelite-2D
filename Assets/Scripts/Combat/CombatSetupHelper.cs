using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    /// <summary>
    /// Components returned by <see cref="CombatSetupHelper.AssembleCharacter"/> so callers
    /// can perform team-specific wiring (death tracking, target assignment, etc.).
    /// </summary>
    public struct CharacterComponents
    {
        public CombatStats Stats;
        public CharacterMover Mover;
        public CombatController Controller;
    }

    /// <summary>
    /// Shared setup utilities used by both <see cref="CombatSpawnManager"/> (allies)
    /// and <see cref="LevelManager"/> (enemies) to avoid code duplication.
    /// </summary>
    public static class CombatSetupHelper
    {
        public const string TeamContainerName = "Team";
        public const string EnemiesContainerName = "Enemies";
        public const string TeamHomeAnchorName = "TeamHomeAnchor";
        public const string EnemiesHomeAnchorName = "EnemiesHomeAnchor";
        public const string CombatTriggerZoneName = "CombatTriggerZone";

        /// <summary>
        /// Adds all combat components to a freshly instantiated character:
        /// CombatStats, HealthBar, CharacterMover, CircleCollider2D radius,
        /// CombatController, AnimationEventRelay, and CharacterAppearance.
        /// Returns the key components so the caller can do team-specific wiring.
        /// </summary>
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
            string callerName)
        {
            // CombatStats — direct initialization from spawn data values.
            var combatStats = character.AddComponent<CombatStats>();
            combatStats.InitializeDirect(maxHp, atk, attackSpeed, regenHpPerSecond);

            // HealthBar — must be added after CombatStats (reads it in Awake).
            character.AddComponent<HealthBar>();

            // CharacterMover — set speed, home anchor, and home offset.
            var mover = character.AddComponent<CharacterMover>();
            mover.SetMoveSpeed(moveSpeed);
            if (homeAnchor != null)
                mover.HomeAnchor = homeAnchor;
            mover.SetHomeOffset(homeOffset);

            // Collider radius — set after AddComponent<CharacterMover> which auto-adds CircleCollider2D.
            var col = character.GetComponent<CircleCollider2D>();
            if (col != null)
                col.radius = colliderRadius;

            // CombatController + AnimationEventRelay wiring.
            var controller = character.AddComponent<CombatController>();
            WireAnimationRelay(character, controller, callerName);

            // Visual appearance.
            var appearanceComp = character.AddComponent<CharacterAppearance>();
            appearanceComp.ApplyAppearance(appearance);

            return new CharacterComponents
            {
                Stats = combatStats,
                Mover = mover,
                Controller = controller
            };
        }

        /// <summary>
        /// Finds an <see cref="AnimationEventRelay"/> on the character's Animator child
        /// and wires it to the given <see cref="CombatController"/>.
        /// </summary>
        public static void WireAnimationRelay(GameObject character, CombatController controller, string callerName)
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

        /// <summary>
        /// Resolves Team and Enemies child containers under the given parent transform,
        /// filling in any null references.
        /// </summary>
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
