using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    /// <summary>
    /// Shared setup utilities used by both <see cref="CombatSpawnManager"/> (allies)
    /// and <see cref="LevelManager"/> (enemies) to avoid code duplication.
    /// </summary>
    public static class CombatSetupHelper
    {
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
                teamContainer = parent.Find(CombatSpawnManager.TeamContainerName);
            if (enemiesContainer == null)
                enemiesContainer = parent.Find(CombatSpawnManager.EnemiesContainerName);

            if (teamContainer == null)
                Debug.LogWarning($"[{callerName}] '{CombatSpawnManager.TeamContainerName}' container not found!");
            if (enemiesContainer == null)
                Debug.LogWarning($"[{callerName}] '{CombatSpawnManager.EnemiesContainerName}' container not found!");
        }
    }
}
