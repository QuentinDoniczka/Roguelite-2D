using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Editor.Tools
{
    internal static class BranchPreviewSettingsPersistence
    {
        internal const string DistanceKey = "SkillTreeDesigner.BranchDistance";
        internal const string AngleKey = "SkillTreeDesigner.BranchAngleDegrees";
        internal const string MirrorEnabledKey = "SkillTreeDesigner.BranchMirrorEnabled";

        public static BranchPreviewSettings Load()
        {
            BranchPreviewSettings settings = BranchPreviewSettings.Defaults;

            if (EditorPrefs.HasKey(DistanceKey))
                settings.distance = EditorPrefs.GetFloat(DistanceKey);

            if (EditorPrefs.HasKey(AngleKey))
                settings.angleDegrees = EditorPrefs.GetFloat(AngleKey);

            if (EditorPrefs.HasKey(MirrorEnabledKey))
                settings.mirrorEnabled = EditorPrefs.GetBool(MirrorEnabledKey);

            MirrorAxisPersistence.ApplyTo(ref settings);

            return settings;
        }

        public static void Save(in BranchPreviewSettings settings)
        {
            EditorPrefs.SetFloat(DistanceKey, settings.distance);
            EditorPrefs.SetFloat(AngleKey, settings.angleDegrees);
            EditorPrefs.SetBool(MirrorEnabledKey, settings.mirrorEnabled);
        }

        public static bool HasPersistedAngle()
        {
            return EditorPrefs.HasKey(AngleKey);
        }

        public static float ResolveInitialAngle(bool hasPersisted, float persisted, Vector2 parentPos)
        {
            return hasPersisted ? persisted : BranchPlacement.ComputeDefaultAngle(parentPos);
        }
    }
}
