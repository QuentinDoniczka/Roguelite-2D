namespace RogueliteAutoBattler.Combat.Core
{
    public static class ModifierSources
    {
        public const string Base = "base";
        public const string TechTree = "techtree";
        public const string Item = "item";
        public const string Blessing = "blessing";
        public const string LevelUp = "levelup";

        public static string ItemSource(string instanceId) => "item:" + instanceId;
    }
}
