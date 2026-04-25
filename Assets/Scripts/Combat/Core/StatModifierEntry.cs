namespace RogueliteAutoBattler.Combat.Core
{
    public readonly struct StatModifierEntry
    {
        public readonly string Source;
        public readonly string Value;
        public readonly bool IsPositive;
        public readonly ModifierTier Tier;

        public StatModifierEntry(string source, string value, bool isPositive)
            : this(source, value, isPositive, ModifierTier.Flat)
        {
        }

        public StatModifierEntry(string source, string value, bool isPositive, ModifierTier tier)
        {
            Source = source;
            Value = value;
            IsPositive = isPositive;
            Tier = tier;
        }
    }
}
