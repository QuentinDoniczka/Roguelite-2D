using NUnit.Framework;
using RogueliteAutoBattler.Editor.Tools;
using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class BranchPreviewSettingsPersistenceTests
    {
        private const float Tolerance = 1e-4f;

        private const float RoundTripDistance = 7f;
        private const float RoundTripAngle = 42f;
        private const float ImmutabilityDistance = 13f;
        private const float ImmutabilityAngle = 91f;
        private const float SentinelPersistedAngle = 123.45f;
        private const float SentinelMirrorAxisDegrees = 99f;
        private const float MirrorAxisRoundTrip = 55f;

        private static readonly Vector2 ArbitraryParentPos = new Vector2(1f, 1f);
        private static readonly Vector2 NonOriginParentPos = new Vector2(3f, 4f);

        [SetUp]
        public void SetUp()
        {
            ClearPrefs();
        }

        [TearDown]
        public void TearDown()
        {
            ClearPrefs();
        }

        private static void ClearPrefs()
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
                distance = RoundTripDistance,
                angleDegrees = RoundTripAngle,
                mirrorEnabled = true,
                mirrorAxisDegrees = MirrorAxisRoundTrip
            };

            BranchPreviewSettingsPersistence.Save(settings);
            EditorPrefs.SetFloat(MirrorAxisPersistence.EditorPrefKey, settings.mirrorAxisDegrees);

            BranchPreviewSettings result = BranchPreviewSettingsPersistence.Load();

            Assert.That(result.distance, Is.EqualTo(RoundTripDistance).Within(Tolerance));
            Assert.That(result.angleDegrees, Is.EqualTo(RoundTripAngle).Within(Tolerance));
            Assert.That(result.mirrorEnabled, Is.True);
            // Load no longer touches the mirror axis: it stays at struct defaults regardless of EditorPrefs.
            Assert.That(result.mirrorAxisDegrees, Is.EqualTo(BranchPreviewSettings.Defaults.mirrorAxisDegrees).Within(Tolerance));
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
            float expected = BranchPlacement.ComputeDefaultAngle(NonOriginParentPos);

            float result = BranchPreviewSettingsPersistence.ResolveInitialAngle(false, 0f, NonOriginParentPos);

            Assert.That(result, Is.EqualTo(expected).Within(Tolerance));
        }

        [Test]
        public void ResolveInitialAngle_HasPersisted_ReturnsPersisted()
        {
            float result = BranchPreviewSettingsPersistence.ResolveInitialAngle(true, SentinelPersistedAngle, ArbitraryParentPos);

            Assert.That(result, Is.EqualTo(SentinelPersistedAngle).Within(Tolerance));
        }

        [Test]
        public void Save_DoesNotTouchMirrorAxisKey()
        {
            EditorPrefs.SetFloat(MirrorAxisPersistence.EditorPrefKey, SentinelMirrorAxisDegrees);

            var settings = new BranchPreviewSettings
            {
                distance = BranchPreviewSettings.Defaults.distance,
                angleDegrees = BranchPreviewSettings.Defaults.angleDegrees,
                mirrorEnabled = BranchPreviewSettings.Defaults.mirrorEnabled,
                mirrorAxisDegrees = MirrorAxisRoundTrip
            };
            BranchPreviewSettingsPersistence.Save(settings);

            float mirrorAxis = EditorPrefs.GetFloat(MirrorAxisPersistence.EditorPrefKey);
            Assert.That(mirrorAxis, Is.EqualTo(SentinelMirrorAxisDegrees).Within(Tolerance));
        }

        [Test]
        public void Save_RoundTrips_DoesNotMutateDefaults()
        {
            BranchPreviewSettings before = BranchPreviewSettings.Defaults;
            BranchPreviewSettings toSave = new BranchPreviewSettings
            {
                distance = ImmutabilityDistance,
                angleDegrees = ImmutabilityAngle,
                mirrorEnabled = true,
                mirrorAxisDegrees = before.mirrorAxisDegrees
            };
            BranchPreviewSettingsPersistence.Save(toSave);
            BranchPreviewSettings after = BranchPreviewSettings.Defaults;
            Assert.AreEqual(before.distance, after.distance);
            Assert.AreEqual(before.angleDegrees, after.angleDegrees);
            Assert.AreEqual(before.mirrorEnabled, after.mirrorEnabled);
            Assert.AreEqual(before.mirrorAxisDegrees, after.mirrorAxisDegrees);
        }
    }
}
