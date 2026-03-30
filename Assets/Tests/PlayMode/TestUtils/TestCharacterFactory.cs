using System.Collections.Generic;
using RogueliteAutoBattler.Combat;
using RogueliteAutoBattler.Data;
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

        public static GameObject CreateCharacterWithHealthBar(
            string name = "TestHealthBarChar",
            int maxHp = 100,
            int atk = 10,
            float attackSpeed = 1f,
            float regenHpPerSecond = 0f)
        {
            var go = CreateCombatCharacter(name, maxHp, atk, attackSpeed, regenHpPerSecond);
            go.AddComponent<HealthBar>();
            return go;
        }

        public static GameObject CreateAnchor(string name = "Anchor", Vector2? position = null)
        {
            var go = new GameObject(name);

            if (position.HasValue)
                go.transform.position = position.Value;

            return go;
        }

        public static GameObject CreateAllyPrefab(string name = "AllyPrefab")
        {
            var go = new GameObject(name);

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            go.AddComponent<CircleCollider2D>();

            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            visual.AddComponent<SpriteRenderer>();

            return go;
        }

        public static TeamDatabase CreateTeamDatabase(int allyCount, GameObject prefab)
        {
            var teamDb = ScriptableObject.CreateInstance<TeamDatabase>();

            var allyList = new List<AllySpawnData>();
            for (int i = 0; i < allyCount; i++)
            {
                var ally = new AllySpawnData
                {
                    AllyName = $"Ally_{i}",
                    Prefab = prefab,
                    MaxHp = 100,
                    Atk = 10,
                    AttackSpeed = 1f,
                    MoveSpeed = 2f,
                    ColliderRadius = 0.3f
                };
                allyList.Add(ally);
            }

            teamDb.Allies = allyList;
            return teamDb;
        }
    }
}
