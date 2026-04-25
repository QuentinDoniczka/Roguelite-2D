namespace RogueliteAutoBattler.Combat.Core
{
    internal readonly struct Modifier
    {
        public readonly StatType Stat;
        public readonly ModifierTier Tier;
        public readonly string Source;
        public readonly float Value;

        public Modifier(StatType stat, ModifierTier tier, string source, float value)
        {
            Stat = stat;
            Tier = tier;
            Source = source;
            Value = value;
        }
    }
}
