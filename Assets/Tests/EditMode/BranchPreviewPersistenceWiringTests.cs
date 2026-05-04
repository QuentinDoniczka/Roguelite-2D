using NUnit.Framework;
using RogueliteAutoBattler.Editor.Tools;
using UnityEditor;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class BranchPreviewPersistenceWiringTests
    {
        private const float Tolerance = 1e-4f;

        [SetUp]
        public void SetUp()
        {
            DeleteAllKeys();
        }

        [TearDown]
        public void TearDown()
        {
            DeleteAllKeys();
        }

        private static void DeleteAllKeys()
        {
            EditorPrefs.DeleteKey(BranchPreviewPersistence.DistanceKey);
            EditorPrefs.DeleteKey(BranchPreviewPersistence.AngleKey);
            EditorPrefs.DeleteKey(BranchPreviewPersistence.MirrorEnabledKey);
            EditorPrefs.DeleteKey(MirrorAxisPersistence.EditorPrefKey);
        }

        [Test]
        public void ApplyTo_KeysAbsent_LeavesSettingsAtDefaults()
        {
            BranchPreviewSettings settings = BranchPreviewSettings.Defaults;

            BranchPreviewPersistence.ApplyTo(ref settings);

            Assert.That(settings.distance, Is.EqualTo(BranchPreviewSettings.Defaults.distance).Within(Tolerance));
            Assert.That(settings.angleDegrees, Is.EqualTo(BranchPreviewSettings.Defaults.angleDegrees).Within(Tolerance));
            Assert.That(settings.mirrorEnabled, Is.EqualTo(BranchPreviewSettings.Defaults.mirrorEnabled));
            Assert.That(settings.mirrorAxisDegrees, Is.EqualTo(BranchPreviewSettings.Defaults.mirrorAxisDegrees).Within(Tolerance));
        }

        [Test]
        public void ApplyTo_KeysPresent_OverwritesDistanceAngleAndMirrorEnabled()
        {
            BranchPreviewPersistence.SaveDistance(4.5f);
            BranchPreviewPersistence.SaveAngle(123.5f);
            BranchPreviewPersistence.SaveMirrorEnabled(true);

            BranchPreviewSettings settings = BranchPreviewSettings.Defaults;
            BranchPreviewPersistence.ApplyTo(ref settings);

            Assert.That(settings.distance, Is.EqualTo(4.5f).Within(Tolerance));
            Assert.That(settings.angleDegrees, Is.EqualTo(123.5f).Within(Tolerance));
            Assert.IsTrue(settings.mirrorEnabled);
        }

        [Test]
        public void ApplyTo_DelegatesMirrorAxisToMirrorAxisPersistence()
        {
            MirrorAxisPersistence.Save(67.5f);

            BranchPreviewSettings settings = BranchPreviewSettings.Defaults;
            BranchPreviewPersistence.ApplyTo(ref settings);

            Assert.That(settings.mirrorAxisDegrees, Is.EqualTo(67.5f).Within(Tolerance));
        }

        [Test]
        public void ApplyTo_MirrorAxisKeyAbsent_LeavesAxisAtZero()
        {
            BranchPreviewSettings settings = BranchPreviewSettings.Defaults;
            settings.mirrorAxisDegrees = 0f;

            BranchPreviewPersistence.ApplyTo(ref settings);

            Assert.That(settings.mirrorAxisDegrees, Is.EqualTo(0f).Within(Tolerance));
        }
    }
}
