namespace RogueliteAutoBattler.Combat.Core
{
    /// <summary>
    /// Single source of truth for stat categories: consumed by <c>CombatStats</c>
    /// (modifier pipeline, breakdowns, display) and produced by <c>SkillTreeData</c>
    /// (node bonuses serialized into .asset YAML).
    /// CRITICAL LOCK: indices are stable and serialized into .asset YAML
    /// (e.g. SkillTreeData). NEVER reorder, NEVER insert in the middle, append only.
    /// Any violation silently corrupts existing assets.
    /// Invariants are locked by <c>StatTypeIndicesTests</c>.
    /// </summary>
    public enum StatType
    {
        Hp = 0,
        RegenHp = 1,
        Atk = 2,
        Def = 3,
        Mana = 4,
        Power = 5,
        AttackSpeed = 6,
        CritRate = 7,
        None = 8
    }
}
