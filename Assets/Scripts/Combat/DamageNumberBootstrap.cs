using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    public class DamageNumberBootstrap : MonoBehaviour
    {
        [SerializeField] private DamageNumberConfig _config;
        [SerializeField] private Transform _effectsContainer;

        private void Awake()
        {
            if (_config == null)
            {
                Debug.LogError("[DamageNumberBootstrap] _config is not assigned. DamageNumberService will not initialize.");
                return;
            }

            if (_effectsContainer == null)
            {
                Debug.LogError("[DamageNumberBootstrap] _effectsContainer is not assigned. DamageNumberService will not initialize.");
                return;
            }

            DamageNumberSettingsPersistence.Load(_config);
            DamageNumberService.Initialize(_effectsContainer, _config);
        }
    }
}
