using NUnit.Framework;
using RogueliteAutoBattler.Editor.Tools;
using UnityEditor;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class BranchPreviewPersistenceTests
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
        }

        [Test]
        public void SaveLoadDistance_RoundTrips_FirstValue()
        {
            BranchPreviewPersistence.SaveDistance(1.5f);

            Assert.That(BranchPreviewPersistence.LoadDistance(), Is.EqualTo(1.5f).Within(Tolerance));
        }

        [Test]
        public void SaveLoadDistance_RoundTrips_SecondValue()
        {
            BranchPreviewPersistence.SaveDistance(7.25f);

            Assert.That(BranchPreviewPersistence.LoadDistance(), Is.EqualTo(7.25f).Within(Tolerance));
        }

        [Test]
        public void SaveLoadAngle_RoundTrips_FirstValue()
        {
            BranchPreviewPersistence.SaveAngle(42.5f);

            Assert.That(BranchPreviewPersistence.LoadAngle(), Is.EqualTo(42.5f).Within(Tolerance));
        }

        [Test]
        public void SaveLoadAngle_RoundTrips_SecondValue()
        {
            BranchPreviewPersistence.SaveAngle(317.75f);

            Assert.That(BranchPreviewPersistence.LoadAngle(), Is.EqualTo(317.75f).Within(Tolerance));
        }

        [Test]
        public void SaveLoadMirrorEnabled_RoundTrips_True()
        {
            BranchPreviewPersistence.SaveMirrorEnabled(true);

            Assert.IsTrue(BranchPreviewPersistence.LoadMirrorEnabled());
        }

        [Test]
        public void SaveLoadMirrorEnabled_RoundTrips_False()
        {
            BranchPreviewPersistence.SaveMirrorEnabled(false);

            Assert.IsFalse(BranchPreviewPersistence.LoadMirrorEnabled());
        }

        [Test]
        public void LoadDistance_KeyAbsent_ReturnsDefault()
        {
            float result = BranchPreviewPersistence.LoadDistance();

            Assert.That(result, Is.EqualTo(BranchPreviewSettings.Defaults.distance).Within(Tolerance));
        }

        [Test]
        public void LoadAngle_KeyAbsent_ReturnsDefault()
        {
            float result = BranchPreviewPersistence.LoadAngle();

            Assert.That(result, Is.EqualTo(BranchPreviewSettings.Defaults.angleDegrees).Within(Tolerance));
        }

        [Test]
        public void LoadMirrorEnabled_KeyAbsent_ReturnsFalse()
        {
            bool result = BranchPreviewPersistence.LoadMirrorEnabled();

            Assert.IsFalse(result);
        }

        [Test]
        public void HasAngle_KeyAbsent_ReturnsFalse()
        {
            Assert.IsFalse(BranchPreviewPersistence.HasAngle());
        }

        [Test]
        public void HasAngle_AfterSave_ReturnsTrue()
        {
            BranchPreviewPersistence.SaveAngle(60f);

            Assert.IsTrue(BranchPreviewPersistence.HasAngle());
        }
    }
}
