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
            DamageNumberService.Initialize(_effectsContainer, _config);
        }
    }
}
