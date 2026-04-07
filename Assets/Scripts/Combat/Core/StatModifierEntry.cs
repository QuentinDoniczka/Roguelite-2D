namespace RogueliteAutoBattler.Combat.Core
{
    public readonly struct StatModifierEntry
    {
        public readonly string Source;
        public readonly string Value;
        public readonly bool IsPositive;

        public StatModifierEntry(string source, string value, bool isPositive)
        {
            Source = source;
            Value = value;
            IsPositive = isPositive;
        }
    }
}
