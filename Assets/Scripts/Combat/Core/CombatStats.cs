using System.Collections.Generic;
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

        private static readonly StatType[] DisplayOrderArray =
        {
            StatType.Hp, StatType.Atk, StatType.Def, StatType.Mana, StatType.Power,
            StatType.AttackSpeed, StatType.RegenHp, StatType.CritRate
        };

        public static IReadOnlyList<StatType> DisplayOrder => DisplayOrderArray;

        private readonly StatModifierEntry[] _singleModifierBuffer = new StatModifierEntry[1];

        public StatBreakdownData GetBreakdown(StatType statType)
        {
            switch (statType)
            {
                case StatType.Hp:
                    return MakeBaseBreakdown("HP", $"{_currentHp} / {_maxHp}", $"{_maxHp}");
                case StatType.Atk:
                    return MakeBaseBreakdown("ATK", $"{_atk}");
                case StatType.Def:
                    return MakeBaseBreakdown("DEF", "0");
                case StatType.Mana:
                    return MakeBaseBreakdown("MANA", "0");
                case StatType.Power:
                    return MakeBaseBreakdown("POWER", "0");
                case StatType.AttackSpeed:
                    return MakeBaseBreakdown("SPD", _attackSpeed.ToString("F1", CultureInfo.InvariantCulture));
                case StatType.RegenHp:
                    return MakeBaseBreakdown("REGEN", _regenHpPerSecond.ToString("F1", CultureInfo.InvariantCulture) + "/s");
                case StatType.CritRate:
                    return MakeBaseBreakdown("CRIT", "0%");
                default:
                    return new StatBreakdownData("", "", System.Array.Empty<StatModifierEntry>());
            }
        }

        private StatBreakdownData MakeBaseBreakdown(string statName, string formattedValue)
        {
            _singleModifierBuffer[0] = new StatModifierEntry("Base", formattedValue, true);
            return new StatBreakdownData(statName, formattedValue, _singleModifierBuffer);
        }

        private StatBreakdownData MakeBaseBreakdown(string statName, string finalValue, string baseValue)
        {
            _singleModifierBuffer[0] = new StatModifierEntry("Base", baseValue, true);
            return new StatBreakdownData(statName, finalValue, _singleModifierBuffer);
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
