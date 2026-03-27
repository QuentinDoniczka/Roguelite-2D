using System.Collections.Generic;
using TMPro;
using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    public static class DamageNumberService
    {
        private static readonly Queue<DamageNumber> _pool = new Queue<DamageNumber>();
        private static Transform _container;
        private static DamageNumberConfig _config;
        private static bool _isInitialized;
        private const int DEFAULT_POOL_SIZE = 20;

        public static void Initialize(Transform effectsContainer, DamageNumberConfig config)
        {
            _container = effectsContainer;
            _config = config;

            for (int i = 0; i < DEFAULT_POOL_SIZE; i++)
            {
                _pool.Enqueue(CreateInstance());
            }

            _isInitialized = true;
        }

        public static void Show(Vector3 worldPosition, int value, bool isAlly)
        {
            if (!_isInitialized)
                return;

            Color color = isAlly ? _config.AllyDamageColor : _config.EnemyDamageColor;

            DamageNumber instance = _pool.Count > 0
                ? _pool.Dequeue()
                : CreateInstance();

            instance.Play(worldPosition, value, color, _config);
        }

        private static DamageNumber CreateInstance()
        {
            var go = new GameObject("DamageNumber");
            go.transform.SetParent(_container, false);
            go.AddComponent<TextMeshPro>();
            var instance = go.AddComponent<DamageNumber>();
            instance.Initialize(ReturnToPool);
            go.SetActive(false);
            return instance;
        }

        private static void ReturnToPool(DamageNumber instance)
        {
            _pool.Enqueue(instance);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnDomainReload()
        {
            _pool.Clear();
            _container = null;
            _config = null;
            _isInitialized = false;
        }

        internal static void ResetForTest()
        {
            ResetOnDomainReload();
        }
    }
}
