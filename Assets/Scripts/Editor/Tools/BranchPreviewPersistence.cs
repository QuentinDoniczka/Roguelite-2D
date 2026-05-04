using UnityEditor;

namespace RogueliteAutoBattler.Editor.Tools
{
    internal static class BranchPreviewPersistence
    {
        internal const string DistanceKey = "SkillTreeDesigner.Branch.DistanceUnits";
        internal const string AngleKey = "SkillTreeDesigner.Branch.AngleDegrees";
        internal const string MirrorEnabledKey = "SkillTreeDesigner.Branch.MirrorEnabled";

        private const bool MirrorEnabledDefault = false;

        internal static float LoadDistance()
        {
            return EditorPrefs.GetFloat(DistanceKey, BranchPreviewSettings.Defaults.distance);
        }

        internal static void SaveDistance(float value)
        {
            EditorPrefs.SetFloat(DistanceKey, value);
        }

        internal static float LoadAngle()
        {
            return EditorPrefs.GetFloat(AngleKey, BranchPreviewSettings.Defaults.angleDegrees);
        }

        internal static void SaveAngle(float value)
        {
            EditorPrefs.SetFloat(AngleKey, value);
        }

        internal static bool HasAngle()
        {
            return EditorPrefs.HasKey(AngleKey);
        }

        internal static bool LoadMirrorEnabled()
        {
            return EditorPrefs.GetBool(MirrorEnabledKey, MirrorEnabledDefault);
        }

        internal static void SaveMirrorEnabled(bool value)
        {
            EditorPrefs.SetBool(MirrorEnabledKey, value);
        }

        internal static void ApplyTo(ref BranchPreviewSettings settings)
        {
            settings.distance = LoadDistance();
            settings.angleDegrees = LoadAngle();
            settings.mirrorEnabled = LoadMirrorEnabled();
            MirrorAxisPersistence.ApplyTo(ref settings);
        }
    }
}
