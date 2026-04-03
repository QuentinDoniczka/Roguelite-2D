using System;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Combat.Visuals;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Economy;
using UnityEngine;

namespace RogueliteAutoBattler.Combat.Levels
{
    internal class EnemySpawner
    {
        private readonly Transform _enemiesContainer;
        private readonly Transform _teamContainer;
        private readonly Transform _enemiesHomeAnchor;
        private readonly Func<float> _characterScaleProvider;
        private readonly Func<GoldWallet> _goldWalletProvider;
        private readonly float _enemySpawnOffscreenX;

        internal EnemySpawner(
            Transform enemiesContainer,
            Transform teamContainer,
            Transform enemiesHomeAnchor,
            Func<float> characterScaleProvider,
            Func<GoldWallet> goldWalletProvider,
            float enemySpawnOffscreenX)
        {
            _enemiesContainer = enemiesContainer;
            _teamContainer = teamContainer;
            _enemiesHomeAnchor = enemiesHomeAnchor;
            _characterScaleProvider = characterScaleProvider;
            _goldWalletProvider = goldWalletProvider;
            _enemySpawnOffscreenX = enemySpawnOffscreenX;
        }

        internal event Action OnEnemyDied;

        internal int AliveEnemyCount { get; private set; }

        internal void ResetAliveEnemyCount()
        {
            AliveEnemyCount = 0;
        }

        internal Vector2[] CalculateSpawnPositions(int enemyCount, out Vector2[] homePositions)
        {
            float fallbackSpawnX = 1f;
            Vector2 anchorPos = _enemiesHomeAnchor != null
                ? (Vector2)_enemiesHomeAnchor.position
                : new Vector2(fallbackSpawnX, 0f);
            Vector2 spawnAnchor = new Vector2(anchorPos.x + _enemySpawnOffscreenX, anchorPos.y);
            float characterScale = _characterScaleProvider();
            Vector2[] spawnPositions = FormationLayout.GetPositions(spawnAnchor, enemyCount, facingRight: false, characterScale: characterScale);
            homePositions = FormationLayout.GetPositions(anchorPos, enemyCount, facingRight: false, characterScale: characterScale);
            return spawnPositions;
        }

        internal Vector2 GetAnchorPosition()
        {
            float fallbackSpawnX = 1f;
            return _enemiesHomeAnchor != null
                ? (Vector2)_enemiesHomeAnchor.position
                : new Vector2(fallbackSpawnX, 0f);
        }

        internal void SpawnEnemy(EnemySpawnData data, Vector2 spawnPos, Vector2 homeOffset, Action<Transform> onEnemySpawned)
        {
            if (data.Prefab == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[{nameof(LevelManager)}] EnemySpawnData '{data.EnemyName}' has no prefab assigned.");
#endif
                return;
            }

            Vector3 spawnPosition = new Vector3(spawnPos.x, spawnPos.y, 0f);

            GameObject enemy = UnityEngine.Object.Instantiate(data.Prefab, spawnPosition, Quaternion.identity, _enemiesContainer);
            enemy.name = data.EnemyName;

            float scale = _characterScaleProvider();
            enemy.transform.localScale = new Vector3(scale, scale, 1f);

            var setupConfig = new CharacterSetupConfig
            {
                MaxHp = data.Hp,
                Atk = data.Atk,
                AttackSpeed = data.AttackSpeed,
                RegenHpPerSecond = 0f,
                MoveSpeed = data.MoveSpeed,
                HomeAnchor = _enemiesHomeAnchor,
                HomeOffset = homeOffset,
                ColliderRadius = data.ColliderRadius,
                Appearance = data.Appearance,
                CallerName = nameof(LevelManager),
                HealthBarFillColor = HealthBar.EnemyFillColor,
                IsAlly = false,
                CharacterScale = scale
            };
            var components = CombatSetupHelper.AssembleCharacter(enemy, setupConfig);

            IgnoreCollisionWithOppositeTeam(enemy, _teamContainer);

            AliveEnemyCount++;
            components.Stats.OnDied += HandleEnemyDied;

            var enemyTransform = enemy.transform;

            int goldAmount = data.GoldDrop;
            if (goldAmount > 0)
            {
                components.Stats.OnDied += () =>
                {
                    if (enemyTransform == null) return;
                    CoinFlyService.Show(enemyTransform.position, () =>
                    {
                        var wallet = _goldWalletProvider();
                        if (wallet != null) wallet.Add(goldAmount);
                    });
                };
            }

            components.Controller.SetAttackRange(data.AttackRange);
            components.Controller.SetAttackerFacing(false);
            components.Controller.FindNewTarget = () => TargetFinder.LeastContested(_teamContainer, enemyTransform.position);

            Transform allyTarget = TargetFinder.LeastContested(_teamContainer, enemyTransform.position);
            if (allyTarget != null)
                components.Controller.Target = allyTarget;
#if UNITY_EDITOR
            else
                Debug.LogWarning($"[{nameof(LevelManager)}] No alive ally found for enemy '{data.EnemyName}' to target.");
#endif

            onEnemySpawned?.Invoke(enemy.transform);

#if UNITY_EDITOR
            Debug.Log($"[{nameof(LevelManager)}] Spawned enemy '{data.EnemyName}' at {spawnPosition}");
#endif
        }

        private void HandleEnemyDied()
        {
            AliveEnemyCount--;
            OnEnemyDied?.Invoke();
        }

        private static void IgnoreCollisionWithOppositeTeam(GameObject character, Transform oppositeContainer)
        {
            if (oppositeContainer == null) return;

            var col = character.GetComponent<Collider2D>();
            if (col == null) return;

            for (int i = 0; i < oppositeContainer.childCount; i++)
            {
                var otherCol = oppositeContainer.GetChild(i).GetComponent<Collider2D>();
                if (otherCol != null)
                    Physics2D.IgnoreCollision(col, otherCol, true);
            }
        }
    }
}
