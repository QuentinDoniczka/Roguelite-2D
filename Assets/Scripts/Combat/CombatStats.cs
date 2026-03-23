using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    /// <summary>
    /// Runtime component holding current HP for a character in combat.
    /// Attached to each character at spawn via <see cref="CombatSpawnManager"/>.
    /// </summary>
    public class CombatStats : MonoBehaviour
    {
        private CharacterStats _baseStats;
        private int _currentHp;

        /// <summary>The base stats ScriptableObject this character was initialized with.</summary>
        public CharacterStats BaseStats => _baseStats;

        /// <summary>Current health points.</summary>
        public int CurrentHp => _currentHp;

        /// <summary>Maximum health points, derived from base stats.</summary>
        public int MaxHp => _baseStats.maxHp;

        /// <summary>True when CurrentHp has reached zero.</summary>
        public bool IsDead => _currentHp <= 0;

        /// <summary>Initializes stats from the given ScriptableObject and sets HP to max.</summary>
        public void Initialize(CharacterStats stats)
        {
            _baseStats = stats;
            _currentHp = stats.maxHp;
        }

        /// <summary>Reduces CurrentHp by <paramref name="damage"/>, clamped to zero.</summary>
        public void TakeDamage(int damage)
        {
            _currentHp = Mathf.Max(0, _currentHp - damage);
            if (IsDead)
                Debug.Log($"[CombatStats] {gameObject.name} died!");
        }

        private void FixedUpdate()
        {
            if (_baseStats == null || IsDead)
                return;

            if (_baseStats.regenHpPerSecond > 0f && _currentHp < _baseStats.maxHp)
            {
                _currentHp = Mathf.Min(
                    _baseStats.maxHp,
                    _currentHp + Mathf.RoundToInt(_baseStats.regenHpPerSecond * Time.fixedDeltaTime)
                );
            }
        }
    }
}
