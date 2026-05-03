using NUnit.Framework;
using RogueliteAutoBattler.Editor.Tools;
using UnityEditor;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class MirrorAxisPersistenceWiringTests
    {
        private const float Tolerance = 1e-4f;

        [SetUp]
        public void SetUp()
        {
            EditorPrefs.DeleteKey(MirrorAxisPersistence.EditorPrefKey);
        }

        [TearDown]
        public void TearDown()
        {
            EditorPrefs.DeleteKey(MirrorAxisPersistence.EditorPrefKey);
        }

        [Test]
        public void ApplyPersistedAxisTo_KeyAbsent_LeavesAxisAtZero()
        {
            BranchPreviewSettings settings = BranchPreviewSettings.Defaults;

            MirrorAxisPersistence.ApplyTo(ref settings);

            Assert.That(settings.mirrorAxisDegrees, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void ApplyPersistedAxisTo_KeyPresent_OverwritesAxis()
        {
            MirrorAxisPersistence.Save(75f);
            BranchPreviewSettings settings = BranchPreviewSettings.Defaults;
            settings.mirrorAxisDegrees = 0f;

            MirrorAxisPersistence.ApplyTo(ref settings);

            Assert.That(settings.mirrorAxisDegrees, Is.EqualTo(75f).Within(Tolerance));
        }

        [Test]
        public void ApplyPersistedAxisTo_DoesNotTouchMirrorEnabled()
        {
            BranchPreviewSettings settings = BranchPreviewSettings.Defaults;
            settings.mirrorEnabled = true;
            MirrorAxisPersistence.Save(50f);

            MirrorAxisPersistence.ApplyTo(ref settings);

            Assert.That(settings.mirrorEnabled, Is.True);
        }

        [Test]
        public void ApplyPersistedAxisTo_DoesNotTouchDistanceOrAngle()
        {
            BranchPreviewSettings settings = BranchPreviewSettings.Defaults;
            settings.distance = 7f;
            settings.angleDegrees = 42f;

            MirrorAxisPersistence.ApplyTo(ref settings);

            Assert.That(settings.distance, Is.EqualTo(7f).Within(Tolerance));
            Assert.That(settings.angleDegrees, Is.EqualTo(42f).Within(Tolerance));
        }
    }
}
