namespace RogueliteAutoBattler.Editor.Tools
{
    internal struct BranchPreviewSettings
    {
        public float distance;

        internal static readonly BranchPreviewSettings Defaults = new BranchPreviewSettings { distance = 3f };
    }
}
