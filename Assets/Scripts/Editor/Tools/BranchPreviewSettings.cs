namespace RogueliteAutoBattler.Editor.Tools
{
    internal struct BranchPreviewSettings
    {
        private const float DefaultDistanceUnits = 3f;
        private const float DefaultAngleDegrees = 0f;
        private const bool DefaultMirrorEnabled = false;
        private const float DefaultMirrorAxisDegrees = 0f;
        private const float DefaultAlignmentRadiusUnits = 6f;
        private const bool DefaultAlignmentRadiusVisible = false;

        public float distance;
        public float angleDegrees;
        public bool mirrorEnabled;
        public float mirrorAxisDegrees;
        public float alignmentRadiusUnits;
        public bool alignmentRadiusVisible;

        internal static readonly BranchPreviewSettings Defaults = new BranchPreviewSettings
        {
            distance = DefaultDistanceUnits,
            angleDegrees = DefaultAngleDegrees,
            mirrorEnabled = DefaultMirrorEnabled,
            mirrorAxisDegrees = DefaultMirrorAxisDegrees,
            alignmentRadiusUnits = DefaultAlignmentRadiusUnits,
            alignmentRadiusVisible = DefaultAlignmentRadiusVisible
        };
    }
}
