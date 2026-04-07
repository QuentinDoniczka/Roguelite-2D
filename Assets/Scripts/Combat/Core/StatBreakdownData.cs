namespace RogueliteAutoBattler.Combat.Core
{
    public readonly struct StatBreakdownData
    {
        public readonly string StatName;
        public readonly string FinalValue;
        public readonly StatModifierEntry[] Modifiers;

        public StatBreakdownData(string statName, string finalValue, StatModifierEntry[] modifiers)
        {
            StatName = statName;
            FinalValue = finalValue;
            Modifiers = modifiers;
        }
    }
}
