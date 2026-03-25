using RogueliteAutoBattler.Combat;
using UnityEngine;

namespace RogueliteAutoBattler.Tests
{
    /// <summary>
    /// Factory methods for creating lightweight combat GameObjects in tests.
    /// No real sprites or animators — just the minimum components needed for logic tests.
    /// </summary>
    public static class TestCharacterFactory
    {
        /// <summary>
        /// Creates a bare-bones combat character with Rigidbody2D + CombatStats initialized.
        /// Adds a child "Visual" with SpriteRenderer for CharacterMover compatibility.
        /// </summary>
        public static GameObject CreateCombatCharacter(
            string name = "TestChar",
            int maxHp = 100,
            int atk = 10,
            float attackSpeed = 1f,
            float regenHpPerSecond = 0f,
            Vector2? position = null)
        {
            var go = new GameObject(name);

            if (position.HasValue)
                go.transform.position = position.Value;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            // Visual child — CharacterMover.Awake() looks for SpriteRenderer in children.
            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            visual.AddComponent<SpriteRenderer>();

            var stats = go.AddComponent<CombatStats>();
            stats.InitializeDirect(maxHp, atk, attackSpeed, regenHpPerSecond);

            return go;
        }

        /// <summary>
        /// Creates a WorldConveyor host (Rigidbody2D Kinematic + WorldConveyor component).
        /// WorldConveyor.Awake() sets bodyType to Kinematic automatically.
        /// </summary>
        public static GameObject CreateConveyor(string name = "TestConveyor")
        {
            var go = new GameObject(name);

            // WorldConveyor has [RequireComponent(typeof(Rigidbody2D))],
            // so AddComponent<WorldConveyor> auto-adds Rigidbody2D.
            // Awake() will set bodyType = Kinematic.
            go.AddComponent<WorldConveyor>();

            return go;
        }

        /// <summary>
        /// Creates a character with CharacterMover, optionally parented under a conveyor.
        /// CharacterMover has [RequireComponent] for Rigidbody2D and CircleCollider2D.
        /// </summary>
        public static GameObject CreateMoverCharacter(
            string name = "TestMover",
            float moveSpeed = 2f,
            Transform parent = null,
            Vector2? position = null)
        {
            var go = new GameObject(name);

            if (parent != null)
                go.transform.SetParent(parent, false);

            if (position.HasValue)
                go.transform.position = position.Value;

            // Visual child — CharacterMover.Awake() calls GetComponentInChildren<Animator>()
            // and looks for SpriteRenderer for flipping.
            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            visual.AddComponent<SpriteRenderer>();

            // CharacterMover auto-adds Rigidbody2D and CircleCollider2D via RequireComponent.
            var mover = go.AddComponent<CharacterMover>();
            mover.SetMoveSpeed(moveSpeed);

            // Configure Rigidbody2D for test use.
            var rb = go.GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;

            return go;
        }

        /// <summary>
        /// Creates a full combat character with all components needed for auto-battle:
        /// CombatStats, CharacterMover (with Rigidbody2D + CircleCollider2D), and CombatController.
        /// </summary>
        public static GameObject CreateFullCombatCharacter(
            string name,
            int maxHp,
            int atk,
            float attackSpeed,
            float moveSpeed,
            float attackRange,
            Transform parent = null,
            Vector2? position = null)
        {
            var go = new GameObject(name);

            if (parent != null)
                go.transform.SetParent(parent, false);

            if (position.HasValue)
                go.transform.position = position.Value;

            // Visual child — CharacterMover.Awake() calls GetComponentInChildren<Animator>()
            // and looks for SpriteRenderer for flipping.
            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            visual.AddComponent<SpriteRenderer>();

            // CombatStats — must be added and initialized before CombatController.Awake() runs.
            var stats = go.AddComponent<CombatStats>();
            stats.InitializeDirect(maxHp, atk, attackSpeed);

            // CharacterMover — auto-adds Rigidbody2D and CircleCollider2D via RequireComponent.
            var mover = go.AddComponent<CharacterMover>();
            mover.SetMoveSpeed(moveSpeed);

            // Collider radius — match production default (LevelDataTypes: 0.05).
            var col = go.GetComponent<CircleCollider2D>();
            if (col != null)
                col.radius = 0.15f;

            // Rigidbody2D — configure for top-down 2D combat (no gravity).
            var rb = go.GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;

            // CombatController — requires CharacterMover and CombatStats (already added).
            var controller = go.AddComponent<CombatController>();
            controller.SetAttackRange(attackRange);

            return go;
        }

        /// <summary>
        /// Creates the minimal CombatWorld hierarchy for combat scenario tests.
        /// Includes WorldConveyor, Team/Enemies containers, and home anchors.
        /// </summary>
        public static (GameObject combatWorld, Transform teamContainer, Transform enemiesContainer, Transform teamAnchor, Transform enemiesAnchor) CreateCombatArena()
        {
            var combatWorld = new GameObject("CombatWorld");
            combatWorld.AddComponent<WorldConveyor>();

            var team = new GameObject(CombatSpawnManager.TeamContainerName);
            team.transform.SetParent(combatWorld.transform, false);

            var enemies = new GameObject(CombatSpawnManager.EnemiesContainerName);
            enemies.transform.SetParent(combatWorld.transform, false);

            var teamAnchor = new GameObject(CombatSpawnManager.TeamHomeAnchorName);
            teamAnchor.transform.SetParent(combatWorld.transform, false);
            teamAnchor.transform.localPosition = new Vector3(-1f, 0f, 0f);

            var enemiesAnchor = new GameObject(CombatSpawnManager.EnemiesHomeAnchorName);
            enemiesAnchor.transform.SetParent(combatWorld.transform, false);
            enemiesAnchor.transform.localPosition = new Vector3(2f, 0f, 0f);

            return (combatWorld, team.transform, enemies.transform, teamAnchor.transform, enemiesAnchor.transform);
        }

        /// <summary>
        /// Creates a simple anchor Transform at the given position.
        /// </summary>
        public static GameObject CreateAnchor(string name = "Anchor", Vector2? position = null)
        {
            var go = new GameObject(name);

            if (position.HasValue)
                go.transform.position = position.Value;

            return go;
        }
    }
}
