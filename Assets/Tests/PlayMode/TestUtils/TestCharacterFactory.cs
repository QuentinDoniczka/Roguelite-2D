using RogueliteAutoBattler.Combat;
using UnityEngine;

namespace RogueliteAutoBattler.Tests
{
    public static class TestCharacterFactory
    {
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

        public static GameObject CreateConveyor(string name = "TestConveyor")
        {
            var go = new GameObject(name);
            go.AddComponent<WorldConveyor>();
            return go;
        }

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

            var mover = go.AddComponent<CharacterMover>();
            mover.SetMoveSpeed(moveSpeed);

            var rb = go.GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;

            return go;
        }

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

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            go.AddComponent<CombatController>();

            var stats = go.GetComponent<CombatStats>();
            stats.InitializeDirect(maxHp, atk, attackSpeed);

            var mover = go.GetComponent<CharacterMover>();
            mover.SetMoveSpeed(moveSpeed);

            return go;
        }

        private static void AddVisualChild(GameObject parent)
        {
            var visual = new GameObject("Visual");
            visual.transform.SetParent(parent.transform, false);
            visual.AddComponent<SpriteRenderer>();
        }

        public static GameObject CreateFormationCharacter(
            string name = "TestFormationChar",
            int maxHp = 100,
            int atk = 10,
            float attackSpeed = 1f,
            float moveSpeed = 2f,
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
            stats.InitializeDirect(maxHp, atk, attackSpeed);

            var mover = go.AddComponent<CharacterMover>();
            mover.SetMoveSpeed(moveSpeed);

            return go;
        }

        public static GameObject CreateAnchor(string name = "Anchor", Vector2? position = null)
        {
            var go = new GameObject(name);

            if (position.HasValue)
                go.transform.position = position.Value;

            return go;
        }
    }
}
