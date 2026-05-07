using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Editor.Tools
{
    internal static class BranchPreviewSettingsPersistence
    {
        internal const string DistanceKey = "SkillTreeDesigner.BranchDistance";
        internal const string AngleKey = "SkillTreeDesigner.BranchAngleDegrees";
        internal const string MirrorEnabledKey = "SkillTreeDesigner.BranchMirrorEnabled";
        internal const string AlignmentRadiusKey = "SkillTreeDesigner.AlignmentRadius";
        internal const string AlignmentRadiusVisibleKey = "SkillTreeDesigner.AlignmentRadiusVisible";

        internal static BranchPreviewSettings Load()
        {
            BranchPreviewSettings settings = BranchPreviewSettings.Defaults;

            if (EditorPrefs.HasKey(DistanceKey))
                settings.distance = EditorPrefs.GetFloat(DistanceKey);

            if (EditorPrefs.HasKey(AngleKey))
                settings.angleDegrees = EditorPrefs.GetFloat(AngleKey);

            if (EditorPrefs.HasKey(MirrorEnabledKey))
                settings.mirrorEnabled = EditorPrefs.GetBool(MirrorEnabledKey);

            if (EditorPrefs.HasKey(AlignmentRadiusKey))
                settings.alignmentRadiusUnits = EditorPrefs.GetFloat(AlignmentRadiusKey);

            if (EditorPrefs.HasKey(AlignmentRadiusVisibleKey))
                settings.alignmentRadiusVisible = EditorPrefs.GetBool(AlignmentRadiusVisibleKey);

            return settings;
        }

        internal static void Save(in BranchPreviewSettings settings)
        {
            EditorPrefs.SetFloat(DistanceKey, settings.distance);
            EditorPrefs.SetFloat(AngleKey, settings.angleDegrees);
            EditorPrefs.SetBool(MirrorEnabledKey, settings.mirrorEnabled);
            EditorPrefs.SetFloat(AlignmentRadiusKey, settings.alignmentRadiusUnits);
            EditorPrefs.SetBool(AlignmentRadiusVisibleKey, settings.alignmentRadiusVisible);
        }

        internal static bool HasPersistedAngle()
        {
            return EditorPrefs.HasKey(AngleKey);
        }

        internal static float ResolveInitialAngle(Vector2 parentPos)
        {
            float persisted = EditorPrefs.GetFloat(AngleKey, BranchPreviewSettings.Defaults.angleDegrees);
            return ResolveInitialAngle(HasPersistedAngle(), persisted, parentPos);
        }

        internal static float ResolveInitialAngle(bool hasPersisted, float persisted, Vector2 parentPos)
        {
            return hasPersisted ? persisted : BranchPlacement.ComputeDefaultAngle(parentPos);
        }
    }
}
