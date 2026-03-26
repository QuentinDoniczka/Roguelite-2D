using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    /// <summary>
    /// Runtime component holding current HP for a character in combat.
    /// Attached to each character at spawn via <see cref="CombatSpawnManager"/>.
    /// </summary>
    public class CombatStats : MonoBehaviour
    {
        private int _currentHp;
        private int _maxHp;
        private int _atk;
        private float _attackSpeed;
        private float _regenHpPerSecond;
        private float _regenAccumulator;

        /// <summary>Current health points.</summary>
        public int CurrentHp => _currentHp;

        /// <summary>Maximum health points.</summary>
        public int MaxHp => _maxHp;

        /// <summary>Damage dealt per attack.</summary>
        public int Atk => _atk;

        /// <summary>Attacks per second.</summary>
        public float AttackSpeed => _attackSpeed;

        /// <summary>True when CurrentHp has reached zero.</summary>
        public bool IsDead => _currentHp <= 0;

        /// <summary>Initializes stats directly from values (used by wave-spawned enemies without a SO).</summary>
        public void InitializeDirect(int maxHp, int atk, float attackSpeed, float regenHpPerSecond = 0f)
        {
            _maxHp = maxHp;
            _currentHp = maxHp;
            _atk = atk;
            _attackSpeed = attackSpeed;
            _regenHpPerSecond = regenHpPerSecond;
            _regenAccumulator = 0f;
        }

        /// <summary>Fired once when CurrentHp reaches zero.</summary>
        public event System.Action OnDied;

        // TODO: Server-authoritative — validate damage server-side
        /// <summary>Reduces CurrentHp by <paramref name="damage"/>, clamped to zero.</summary>
        public void TakeDamage(int damage)
        {
            if (IsDead) return;

            _currentHp = Mathf.Max(0, _currentHp - damage);
            if (IsDead)
            {
#if UNITY_EDITOR
                Debug.Log($"[CombatStats] {gameObject.name} died!");
#endif
                OnDied?.Invoke();
            }
        }

        private void FixedUpdate()
        {
            if (IsDead)
                return;

            if (_regenHpPerSecond > 0f && _currentHp < _maxHp)
            {
                _regenAccumulator += _regenHpPerSecond * Time.fixedDeltaTime;
                int heal = (int)_regenAccumulator;
                if (heal > 0)
                {
                    _regenAccumulator -= heal;
                    _currentHp = Mathf.Min(_maxHp, _currentHp + heal);
                }
            }
        }
    }
}
