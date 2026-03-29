using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    public class CombatStats : MonoBehaviour
    {
        private int _currentHp;
        private int _maxHp;
        private int _atk;
        private float _attackSpeed;
        private float _regenHpPerSecond;
        private float _regenAccumulator;

        public int CurrentHp => _currentHp;
        public int MaxHp => _maxHp;
        public int Atk => _atk;
        public float AttackSpeed => _attackSpeed;
        public bool IsDead => _currentHp <= 0;

        public void InitializeDirect(int maxHp, int atk, float attackSpeed, float regenHpPerSecond = 0f)
        {
            _maxHp = maxHp;
            _currentHp = maxHp;
            _atk = atk;
            _attackSpeed = attackSpeed;
            _regenHpPerSecond = regenHpPerSecond;
            _regenAccumulator = 0f;
        }

        public event System.Action<int, int> OnDamageTaken;
        public event System.Action OnDied;

        public void TakeDamage(int damage)
        {
            if (IsDead) return;

            _currentHp = Mathf.Max(0, _currentHp - damage);
            OnDamageTaken?.Invoke(damage, _currentHp);
            if (IsDead)
            {
                AttackSlotRegistry.ReleaseAll(transform);
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
