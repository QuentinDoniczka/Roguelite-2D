using RogueliteAutoBattler.Common;
using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.Combat.Visuals
{
    public static class DamageNumberService
    {
        private static readonly StaticPool<DamageNumber> _pool = new StaticPool<DamageNumber>();
        private static Transform _container;
        private static DamageNumberConfig _config;

        public static void Initialize(Transform effectsContainer, DamageNumberConfig config)
        {
            _container = effectsContainer;
            _config = config;

            _pool.Initialize(() => CreateInstance(), config.InitialPoolSize);
        }

        public static void Show(Vector3 worldPosition, int value, bool isAlly)
        {
            if (!_pool.IsInitialized || !_config.Enabled)
                return;

            Color color = isAlly ? _config.AllyDamageColor : _config.EnemyDamageColor;
            Vector3 spawnPosition = worldPosition + new Vector3(0f, _config.SpawnOffsetY, 0f);
            float arcDirection = isAlly ? -1f : 1f;

            float arcHeight = _config.ArcHeight + Random.Range(-_config.ArcHeightRandomness, _config.ArcHeightRandomness);
            float arcWidth = _config.ArcWidth + Random.Range(-_config.ArcWidthRandomness, _config.ArcWidthRandomness);

            DamageNumber instance = _pool.Get();
            instance.Play(spawnPosition, value, color, _config, arcDirection, arcHeight, arcWidth);
        }

        private static DamageNumber CreateInstance()
        {
            var go = new GameObject("DamageNumber");
            go.transform.SetParent(_container, false);
            var instance = go.AddComponent<DamageNumber>();
            instance.Initialize(ReturnToPool);
            go.SetActive(false);
            return instance;
        }

        private static void ReturnToPool(DamageNumber instance)
        {
            _pool.Return(instance);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnDomainReload()
        {
            _pool.Clear();
            _container = null;
            _config = null;
        }

        internal static void ResetForTest()
        {
            ResetOnDomainReload();
        }
    }
}
