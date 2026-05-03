using UnityEditor;

namespace RogueliteAutoBattler.Editor.Tools
{
    internal static class SnapSettingsPersistence
    {
        internal const string EditorPrefKeyEnabled   = "SkillTreeDesigner.SnapEnabled";
        internal const string EditorPrefKeyThreshold = "SkillTreeDesigner.SnapThresholdUnits";

        public const bool  DefaultEnabled   = true;
        public const float DefaultThreshold = 0.25f;

        public static bool LoadEnabled() => EditorPrefs.GetBool(EditorPrefKeyEnabled, DefaultEnabled);
        public static float LoadThreshold() => EditorPrefs.GetFloat(EditorPrefKeyThreshold, DefaultThreshold);
        public static void SaveEnabled(bool value) => EditorPrefs.SetBool(EditorPrefKeyEnabled, value);
        public static void SaveThreshold(float value) => EditorPrefs.SetFloat(EditorPrefKeyThreshold, value);

        public static void ApplyTo(ref BranchPreviewSettings settings)
        {
            settings.snapEnabled = LoadEnabled();
            settings.snapThresholdUnits = LoadThreshold();
        }
    }
}
