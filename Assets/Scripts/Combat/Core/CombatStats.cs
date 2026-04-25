using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace RogueliteAutoBattler.Combat.Core
{
    public class CombatStats : MonoBehaviour
    {
        private const int StatCount = 8;
        private const int AllStatsDirtyMask = (1 << StatCount) - 1;

        private readonly List<Modifier> _modifiers = new List<Modifier>();
        private readonly float[] _baseValues = new float[StatCount];
        private readonly float[] _cached = new float[StatCount];
        private int _dirtyMask;
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

        public StatBreakdownData GetBreakdown(StatType statType)
        {
            switch (statType)
            {
                case StatType.Hp:
                    return MakeBreakdown(statType, "HP", $"{_currentHp} / {_maxHp}", $"{_maxHp}");
                case StatType.Atk:
                    return MakeBreakdown(statType, "ATK", $"{_atk}");
                case StatType.Def:
                    return MakeBreakdown(statType, "DEF", "0");
                case StatType.Mana:
                    return MakeBreakdown(statType, "MANA", "0");
                case StatType.Power:
                    return MakeBreakdown(statType, "POWER", "0");
                case StatType.AttackSpeed:
                    return MakeBreakdown(statType, "SPD", _attackSpeed.ToString("F1", CultureInfo.InvariantCulture));
                case StatType.RegenHp:
                    return MakeBreakdown(statType, "REGEN", _regenHpPerSecond.ToString("F1", CultureInfo.InvariantCulture) + "/s");
                case StatType.CritRate:
                    return MakeBreakdown(statType, "CRIT", "0%");
                default:
                    return new StatBreakdownData("", "", System.Array.Empty<StatModifierEntry>());
            }
        }

        private StatBreakdownData MakeBreakdown(StatType statType, string statName, string formattedValue)
        {
            return MakeBreakdown(statType, statName, formattedValue, formattedValue);
        }

        private StatBreakdownData MakeBreakdown(StatType statType, string statName, string finalValue, string baseValue)
        {
            int modifierCount = CountModifiersForStat(statType);
            var entries = new StatModifierEntry[1 + modifierCount];
            entries[0] = new StatModifierEntry("Base", baseValue, true, ModifierTier.Base);

            if (modifierCount == 0)
                return new StatBreakdownData(statName, finalValue, entries);

            int writeIndex = 1;
            for (int i = 0; i < _modifiers.Count; i++)
            {
                var modifier = _modifiers[i];
                if (modifier.Stat != statType) continue;
                entries[writeIndex++] = new StatModifierEntry(
                    modifier.Source,
                    FormatModifierValue(modifier.Tier, modifier.Value),
                    modifier.Value >= 0f,
                    modifier.Tier);
            }
            return new StatBreakdownData(statName, finalValue, entries);
        }

        private int CountModifiersForStat(StatType statType)
        {
            int count = 0;
            for (int i = 0; i < _modifiers.Count; i++)
            {
                if (_modifiers[i].Stat == statType) count++;
            }
            return count;
        }

        private static string FormatModifierValue(ModifierTier tier, float value)
        {
            string sign = value >= 0f ? "+" : "-";
            float magnitude = value >= 0f ? value : -value;
            switch (tier)
            {
                case ModifierTier.Percent:
                    float percent = magnitude * 100f;
                    bool percentIsWhole = percent == (int)percent;
                    string percentStr = percentIsWhole
                        ? ((int)percent).ToString(CultureInfo.InvariantCulture)
                        : percent.ToString("0.##", CultureInfo.InvariantCulture);
                    return sign + percentStr + "%";
                case ModifierTier.Base:
                case ModifierTier.Flat:
                default:
                    bool isWhole = magnitude == (int)magnitude;
                    string numberStr = isWhole
                        ? ((int)magnitude).ToString(CultureInfo.InvariantCulture)
                        : magnitude.ToString("0.##", CultureInfo.InvariantCulture);
                    return sign + numberStr;
            }
        }

        public void InitializeDirect(int maxHp, int atk, float attackSpeed, float regenHpPerSecond = 0f)
        {
            _modifiers.Clear();
            for (int i = 0; i < StatCount; i++)
            {
                _baseValues[i] = 0f;
                _cached[i] = 0f;
            }

            _baseValues[(int)StatType.Hp] = maxHp;
            _baseValues[(int)StatType.Atk] = atk;
            _baseValues[(int)StatType.AttackSpeed] = attackSpeed;
            _baseValues[(int)StatType.RegenHp] = regenHpPerSecond;

            _dirtyMask = AllStatsDirtyMask;
            _currentHp = maxHp;

            for (int i = 0; i < StatCount; i++)
            {
                Recompute((StatType)i);
            }

            _currentHp = _maxHp;
            _regenAccumulator = 0f;
        }

        public void AddModifier(StatType stat, ModifierTier tier, string source, float value)
        {
            _modifiers.Add(new Modifier(stat, tier, source, value));
            _dirtyMask |= 1 << (int)stat;
            Recompute(stat);
        }

        public int RemoveModifiersFromSource(string source)
        {
            int removed = 0;
            int dirtyAccum = 0;
            for (int i = _modifiers.Count - 1; i >= 0; i--)
            {
                if (_modifiers[i].Source == source)
                {
                    dirtyAccum |= 1 << (int)_modifiers[i].Stat;
                    _modifiers.RemoveAt(i);
                    removed++;
                }
            }
            if (removed > 0)
            {
                _dirtyMask |= dirtyAccum;
                for (int i = 0; i < StatCount; i++)
                {
                    if ((dirtyAccum & (1 << i)) != 0)
                        Recompute((StatType)i);
                }
            }
            return removed;
        }

        internal float GetStatValue(StatType stat) => Recompute(stat);

        private float Recompute(StatType stat)
        {
            int idx = (int)stat;
            int bit = 1 << idx;
            if ((_dirtyMask & bit) == 0) return _cached[idx];

            float baseValue = _baseValues[idx];
            float sumBase = 0f;
            float sumPercent = 0f;
            float sumFlat = 0f;
            for (int i = 0; i < _modifiers.Count; i++)
            {
                var modifier = _modifiers[i];
                if (modifier.Stat != stat) continue;
                switch (modifier.Tier)
                {
                    case ModifierTier.Base:
                        sumBase += modifier.Value;
                        break;
                    case ModifierTier.Percent:
                        sumPercent += modifier.Value;
                        break;
                    case ModifierTier.Flat:
                        sumFlat += modifier.Value;
                        break;
                }
            }
            float result = (baseValue + sumBase) * (1f + sumPercent) + sumFlat;
            _cached[idx] = result;
            _dirtyMask &= ~bit;

            SyncBackingField(stat, result);
            return result;
        }

        private void SyncBackingField(StatType stat, float value)
        {
            switch (stat)
            {
                case StatType.Hp:
                    _maxHp = Mathf.RoundToInt(value);
                    if (_currentHp > _maxHp) _currentHp = _maxHp;
                    break;
                case StatType.Atk:
                    _atk = Mathf.RoundToInt(value);
                    break;
                case StatType.AttackSpeed:
                    _attackSpeed = value;
                    break;
                case StatType.RegenHp:
                    _regenHpPerSecond = value;
                    break;
            }
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
