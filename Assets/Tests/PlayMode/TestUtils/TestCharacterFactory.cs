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

            AddVisualChild(go);

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

            AddVisualChild(go);

            // CharacterMover auto-adds Rigidbody2D and CircleCollider2D via RequireComponent.
            var mover = go.AddComponent<CharacterMover>();
            mover.SetMoveSpeed(moveSpeed);

            // Configure Rigidbody2D for test use.
            var rb = go.GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;

            return go;
        }

        /// <summary>
        /// Creates a character with CharacterMover + CombatController + CombatStats,
        /// suitable for testing combat state machine behavior (retarget, state transitions).
        /// CombatController auto-adds CharacterMover and CombatStats via RequireComponent.
        /// </summary>
        public static GameObject CreateFullCombatCharacter(
            string name = "TestFighter",
            int maxHp = 100,
            int atk = 10,
            float attackSpeed = 1f,
            float moveSpeed = 2f,
            Vector2? position = null)
        {
            var go = new GameObject(name);

            if (position.HasValue)
                go.transform.position = position.Value;

            AddVisualChild(go);

            // Rigidbody2D first (required by CharacterMover).
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            // CombatController auto-adds CharacterMover + CombatStats via RequireComponent.
            go.AddComponent<CombatController>();

            // Initialize components after they exist.
            var stats = go.GetComponent<CombatStats>();
            stats.InitializeDirect(maxHp, atk, attackSpeed);

            var mover = go.GetComponent<CharacterMover>();
            mover.SetMoveSpeed(moveSpeed);

            return go;
        }

        /// <summary>
        /// Adds a child "Visual" with SpriteRenderer (required by CharacterMover / CombatStats tests).
        /// </summary>
        private static void AddVisualChild(GameObject parent)
        {
            var visual = new GameObject("Visual");
            visual.transform.SetParent(parent.transform, false);
            visual.AddComponent<SpriteRenderer>();
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
