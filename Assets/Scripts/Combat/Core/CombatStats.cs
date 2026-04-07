using System.Globalization;
using UnityEngine;

namespace RogueliteAutoBattler.Combat.Core
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

        public static StatType[] DisplayOrder => new[]
        {
            StatType.Hp, StatType.Atk, StatType.Def,
            StatType.AttackSpeed, StatType.RegenHp, StatType.CritRate
        };

        public StatBreakdownData GetBreakdown(StatType statType)
        {
            switch (statType)
            {
                case StatType.Hp:
                {
                    string finalValue = $"{_currentHp} / {_maxHp}";
                    string baseValue = $"{_maxHp}";
                    return new StatBreakdownData("HP", finalValue, new[]
                    {
                        new StatModifierEntry("Base", baseValue, true)
                    });
                }
                case StatType.Atk:
                {
                    string value = $"{_atk}";
                    return new StatBreakdownData("ATK", value, new[]
                    {
                        new StatModifierEntry("Base", value, true)
                    });
                }
                case StatType.Def:
                {
                    return new StatBreakdownData("DEF", "0", new[]
                    {
                        new StatModifierEntry("Base", "0", true)
                    });
                }
                case StatType.AttackSpeed:
                {
                    string value = _attackSpeed.ToString("F1", CultureInfo.InvariantCulture);
                    return new StatBreakdownData("SPD", value, new[]
                    {
                        new StatModifierEntry("Base", value, true)
                    });
                }
                case StatType.RegenHp:
                {
                    string value = _regenHpPerSecond.ToString("F1", CultureInfo.InvariantCulture) + "/s";
                    return new StatBreakdownData("REGEN", value, new[]
                    {
                        new StatModifierEntry("Base", value, true)
                    });
                }
                case StatType.CritRate:
                {
                    return new StatBreakdownData("CRIT", "0%", new[]
                    {
                        new StatModifierEntry("Base", "0%", true)
                    });
                }
                default:
                {
                    return new StatBreakdownData("", "", new StatModifierEntry[0]);
                }
            }
        }

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
        public event System.Action<int, int> OnHealed;
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
                    OnHealed?.Invoke(heal, _currentHp);
                }
            }
        }
    }
}
