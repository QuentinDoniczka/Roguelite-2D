namespace RogueliteAutoBattler.Editor.Tools
{
    internal struct BranchPreviewSettings
    {
        private const float DefaultDistanceUnits = 3f;
        private const float DefaultAngleDegrees = 0f;

        public float distance;
        public float angleDegrees;

        internal static readonly BranchPreviewSettings Defaults = new BranchPreviewSettings
        {
            distance = DefaultDistanceUnits,
            angleDegrees = DefaultAngleDegrees
        };
    }
}
