namespace RogueliteAutoBattler.Editor.Tools
{
    internal struct BranchPreviewSettings
    {
        private const float DefaultDistanceUnits = 3f;
        private const float DefaultAngleDegrees = 0f;
        private const bool DefaultMirrorEnabled = false;
        private const float DefaultMirrorAxisDegrees = 0f;

        public float distance;
        public float angleDegrees;
        public bool mirrorEnabled;
        public float mirrorAxisDegrees;
        public bool snapEnabled;
        public float snapThresholdUnits;

        internal static readonly BranchPreviewSettings Defaults = new BranchPreviewSettings
        {
            distance = DefaultDistanceUnits,
            angleDegrees = DefaultAngleDegrees,
            mirrorEnabled = DefaultMirrorEnabled,
            mirrorAxisDegrees = DefaultMirrorAxisDegrees,
            snapEnabled = true,
            snapThresholdUnits = 0.25f
        };
    }
}
