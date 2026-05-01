namespace RogueliteAutoBattler.Editor.Tools
{
    internal struct BranchPreviewSettings
    {
        public float distance;
        public float angleDegrees;

        internal static readonly BranchPreviewSettings Defaults = new BranchPreviewSettings { distance = 3f, angleDegrees = 0f };
    }
}
