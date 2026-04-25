namespace RogueliteAutoBattler.Combat.Core
{
    /// <summary>
    /// Tags the category of a stat modifier in the <c>CombatStats</c> pipeline.
    /// The pipeline applies tiers in the order Base -> Percent -> Flat using the formula:
    /// <c>Final = (BaseValue + sum(BaseAdd)) * (1 + sum(Percent)) + sum(FlatAdd)</c>.
    /// Indices are stable: NEVER reorder existing values or insert in the middle, append only.
    /// Rationale: this enum may be serialized into future assets and persisted progress data.
    /// </summary>
    public enum ModifierTier
    {
        Base = 0,
        Percent = 1,
        Flat = 2
    }
}
