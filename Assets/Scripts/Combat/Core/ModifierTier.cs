namespace RogueliteAutoBattler.Combat.Core
{
    /// <summary>
    /// Stat modifier categories. Indices are stable (potentially serialized in
    /// future assets): NEVER reorder existing values or insert in the middle,
    /// append only.
    /// </summary>
    public enum ModifierTier
    {
        Base = 0,
        Percent = 1,
        Flat = 2
    }
}
