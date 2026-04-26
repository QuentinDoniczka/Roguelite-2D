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

        public static string TechTreeNode(int nodeId) => "techtree:node" + nodeId;

        public static string GetDisplayLabel(string source)
        {
            if (string.IsNullOrEmpty(source)) return string.Empty;
            if (source == Base) return "Base";
            if (source.StartsWith("techtree:node")) return "Tech Tree";
            if (source.StartsWith("item:")) return "Item";
            return source;
        }
    }
}
