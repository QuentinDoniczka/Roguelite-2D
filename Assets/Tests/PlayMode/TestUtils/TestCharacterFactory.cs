using System.Collections.Generic;
using System.Reflection;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Combat.Environment;
using RogueliteAutoBattler.Combat.Visuals;
using RogueliteAutoBattler.Common;
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

        public static GameObject CreateCharacterPrefab(string name = "CharacterPrefab")
        {
            var go = new GameObject(name);

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            go.AddComponent<CircleCollider2D>();
            AddVisualChild(go);

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

        public static LevelDatabase CreateLevelDatabase(int enemyCount, GameObject enemyPrefab)
        {
            var levelDb = ScriptableObject.CreateInstance<LevelDatabase>();

            var enemies = new List<EnemySpawnData>();
            for (int i = 0; i < enemyCount; i++)
            {
                var enemy = new EnemySpawnData($"Enemy_{i}", 50, 5)
                {
                    Prefab = enemyPrefab,
                    AttackSpeed = 1f,
                    MoveSpeed = 2f,
                    AttackRange = 1f,
                    ColliderRadius = 0.3f,
                    GoldDrop = 10
                };
                enemies.Add(enemy);
            }

            var wave = new WaveData("Wave_0", 0f, enemies);

            var step = new StepData("Step_0", new List<WaveData> { wave });
            var level = new LevelData("Level_0", new List<StepData> { step });

            var stage = new StageData("Stage_0", null, new List<LevelData> { level });

            levelDb.Stages = new List<StageData> { stage };
            return levelDb;
        }

        public static GameObject CreateSelectableCharacter(
            string name = "TestSelectable",
            int maxHp = 100,
            int atk = 10,
            bool isAlly = true,
            Vector2? position = null)
        {
            var go = CreateCombatCharacter(name, maxHp, atk, position: position);

            int layer = isAlly ? LayerMask.NameToLayer(PhysicsLayers.Ally) : LayerMask.NameToLayer(PhysicsLayers.Enemy);
            if (layer >= 0)
                go.layer = layer;

            var outline = go.AddComponent<SelectionOutline>();
            outline.Initialize();

            CombatSetupHelper.AddSelectionHitbox(go, 0.5f);

            return go;
        }

        internal static void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(obj, value);
        }
    }
}
