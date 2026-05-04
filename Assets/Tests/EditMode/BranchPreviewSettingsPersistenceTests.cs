using NUnit.Framework;
using RogueliteAutoBattler.Editor.Tools;
using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class BranchPreviewSettingsPersistenceTests
    {
        private const float Tolerance = 1e-4f;

        [SetUp]
        public void SetUp()
        {
            EditorPrefs.DeleteKey(BranchPreviewSettingsPersistence.DistanceKey);
            EditorPrefs.DeleteKey(BranchPreviewSettingsPersistence.AngleKey);
            EditorPrefs.DeleteKey(BranchPreviewSettingsPersistence.MirrorEnabledKey);
            EditorPrefs.DeleteKey(MirrorAxisPersistence.EditorPrefKey);
        }

        [TearDown]
        public void TearDown()
        {
            EditorPrefs.DeleteKey(BranchPreviewSettingsPersistence.DistanceKey);
            EditorPrefs.DeleteKey(BranchPreviewSettingsPersistence.AngleKey);
            EditorPrefs.DeleteKey(BranchPreviewSettingsPersistence.MirrorEnabledKey);
            EditorPrefs.DeleteKey(MirrorAxisPersistence.EditorPrefKey);
        }

        [Test]
        public void Load_KeysAbsent_ReturnsDefaults()
        {
            BranchPreviewSettings result = BranchPreviewSettingsPersistence.Load();

            BranchPreviewSettings defaults = BranchPreviewSettings.Defaults;
            Assert.That(result.distance, Is.EqualTo(defaults.distance).Within(Tolerance));
            Assert.That(result.angleDegrees, Is.EqualTo(defaults.angleDegrees).Within(Tolerance));
            Assert.That(result.mirrorEnabled, Is.EqualTo(defaults.mirrorEnabled));
            Assert.That(result.mirrorAxisDegrees, Is.EqualTo(defaults.mirrorAxisDegrees).Within(Tolerance));
        }

        [Test]
        public void SaveThenLoad_RoundTripsAllFields()
        {
            var settings = new BranchPreviewSettings
            {
                distance = 7f,
                angleDegrees = 42f,
                mirrorEnabled = true,
                mirrorAxisDegrees = BranchPreviewSettings.Defaults.mirrorAxisDegrees
            };

            BranchPreviewSettingsPersistence.Save(settings);
            EditorPrefs.SetFloat(MirrorAxisPersistence.EditorPrefKey, settings.mirrorAxisDegrees);

            BranchPreviewSettings result = BranchPreviewSettingsPersistence.Load();

            Assert.That(result.distance, Is.EqualTo(7f).Within(Tolerance));
            Assert.That(result.angleDegrees, Is.EqualTo(42f).Within(Tolerance));
            Assert.That(result.mirrorEnabled, Is.True);
            Assert.That(result.mirrorAxisDegrees, Is.EqualTo(settings.mirrorAxisDegrees).Within(Tolerance));
        }

        [Test]
        public void HasPersistedAngle_KeyAbsent_ReturnsFalse()
        {
            bool result = BranchPreviewSettingsPersistence.HasPersistedAngle();

            Assert.That(result, Is.False);
        }

        [Test]
        public void HasPersistedAngle_AfterSave_ReturnsTrue()
        {
            var settings = BranchPreviewSettings.Defaults;
            BranchPreviewSettingsPersistence.Save(settings);

            bool result = BranchPreviewSettingsPersistence.HasPersistedAngle();

            Assert.That(result, Is.True);
        }

        [Test]
        public void ResolveInitialAngle_NoPersisted_FallsBackToComputeDefault()
        {
            var parentPos = new Vector2(3f, 4f);
            float expected = BranchPlacement.ComputeDefaultAngle(parentPos);

            float result = BranchPreviewSettingsPersistence.ResolveInitialAngle(false, 0f, parentPos);

            Assert.That(result, Is.EqualTo(expected).Within(Tolerance));
        }

        [Test]
        public void ResolveInitialAngle_HasPersisted_ReturnsPersisted()
        {
            float result = BranchPreviewSettingsPersistence.ResolveInitialAngle(true, 123.45f, new Vector2(1f, 1f));

            Assert.That(result, Is.EqualTo(123.45f).Within(Tolerance));
        }

        [Test]
        public void Save_DoesNotTouchMirrorAxisKey()
        {
            EditorPrefs.SetFloat(MirrorAxisPersistence.EditorPrefKey, 99f);

            var settings = new BranchPreviewSettings
            {
                distance = BranchPreviewSettings.Defaults.distance,
                angleDegrees = BranchPreviewSettings.Defaults.angleDegrees,
                mirrorEnabled = BranchPreviewSettings.Defaults.mirrorEnabled,
                mirrorAxisDegrees = 55f
            };
            BranchPreviewSettingsPersistence.Save(settings);

            float mirrorAxis = EditorPrefs.GetFloat(MirrorAxisPersistence.EditorPrefKey);
            Assert.That(mirrorAxis, Is.EqualTo(99f).Within(Tolerance));
        }
    }
}
