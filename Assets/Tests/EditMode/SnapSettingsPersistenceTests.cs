using NUnit.Framework;
using RogueliteAutoBattler.Editor.Tools;
using UnityEditor;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class SnapSettingsPersistenceTests
    {
        private const float Tolerance = 1e-4f;

        [SetUp]
        public void SetUp()
        {
            EditorPrefs.DeleteKey(SnapSettingsPersistence.EditorPrefKeyEnabled);
            EditorPrefs.DeleteKey(SnapSettingsPersistence.EditorPrefKeyThreshold);
        }

        [TearDown]
        public void TearDown()
        {
            EditorPrefs.DeleteKey(SnapSettingsPersistence.EditorPrefKeyEnabled);
            EditorPrefs.DeleteKey(SnapSettingsPersistence.EditorPrefKeyThreshold);
        }

        [Test]
        public void LoadEnabled_KeyAbsent_ReturnsDefault()
        {
            bool result = SnapSettingsPersistence.LoadEnabled();

            Assert.That(result, Is.EqualTo(SnapSettingsPersistence.DefaultEnabled));
        }

        [Test]
        public void LoadThreshold_KeyAbsent_ReturnsDefault()
        {
            float result = SnapSettingsPersistence.LoadThreshold();

            Assert.That(result, Is.EqualTo(SnapSettingsPersistence.DefaultThreshold).Within(Tolerance));
        }

        [Test]
        public void SaveLoadEnabled_RoundTrip()
        {
            SnapSettingsPersistence.SaveEnabled(false);

            bool result = SnapSettingsPersistence.LoadEnabled();

            Assert.That(result, Is.False);
        }

        [Test]
        public void SaveLoadThreshold_RoundTrip()
        {
            SnapSettingsPersistence.SaveThreshold(1.5f);

            float result = SnapSettingsPersistence.LoadThreshold();

            Assert.That(result, Is.EqualTo(1.5f).Within(Tolerance));
        }

        [Test]
        public void ApplyTo_KeyAbsent_AppliesDefaults()
        {
            var settings = new BranchPreviewSettings
            {
                snapEnabled = false,
                snapThresholdUnits = 99f
            };

            SnapSettingsPersistence.ApplyTo(ref settings);

            Assert.That(settings.snapEnabled, Is.EqualTo(SnapSettingsPersistence.DefaultEnabled));
            Assert.That(settings.snapThresholdUnits, Is.EqualTo(SnapSettingsPersistence.DefaultThreshold).Within(Tolerance));
        }

        [Test]
        public void ApplyTo_KeysPresent_OverwritesSettings()
        {
            SnapSettingsPersistence.SaveEnabled(false);
            SnapSettingsPersistence.SaveThreshold(0.75f);

            var settings = new BranchPreviewSettings
            {
                snapEnabled = true,
                snapThresholdUnits = 0f
            };

            SnapSettingsPersistence.ApplyTo(ref settings);

            Assert.That(settings.snapEnabled, Is.False);
            Assert.That(settings.snapThresholdUnits, Is.EqualTo(0.75f).Within(Tolerance));
        }
    }
}
