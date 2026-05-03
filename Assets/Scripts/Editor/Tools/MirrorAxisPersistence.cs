using UnityEditor;

namespace RogueliteAutoBattler.Editor.Tools
{
    internal static class MirrorAxisPersistence
    {
        internal const string EditorPrefKey = "SkillTreeDesigner.MirrorAxisDegrees";
        private const float MirrorAxisDegreesDefault = 0f;

        public static float Load()
        {
            return EditorPrefs.GetFloat(EditorPrefKey, MirrorAxisDegreesDefault);
        }

        public static void Save(float value)
        {
            EditorPrefs.SetFloat(EditorPrefKey, value);
        }

        public static void ApplyTo(ref BranchPreviewSettings settings)
        {
            settings.mirrorAxisDegrees = Load();
        }
    }
}
