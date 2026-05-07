using NUnit.Framework;
using RogueliteAutoBattler.Editor.Tools;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class BranchPreviewSettingsDefaultsTests
    {
        private const float Tolerance = 1e-4f;

        [Test]
        public void Defaults_MirrorEnabled_IsFalse()
        {
            Assert.That(BranchPreviewSettings.Defaults.mirrorEnabled, Is.False);
        }

        [Test]
        public void Defaults_MirrorAxisDegrees_IsZero()
        {
            Assert.That(BranchPreviewSettings.Defaults.mirrorAxisDegrees, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void Defaults_DistanceAndAngleDegrees_Unchanged()
        {
            Assert.That(BranchPreviewSettings.Defaults.distance, Is.EqualTo(3f).Within(Tolerance));
            Assert.That(BranchPreviewSettings.Defaults.angleDegrees, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void Defaults_AlignmentRadiusUnits_Is6()
        {
            Assert.That(BranchPreviewSettings.Defaults.alignmentRadiusUnits, Is.EqualTo(6f).Within(Tolerance));
        }

        [Test]
        public void Defaults_AlignmentRadiusVisible_IsFalse()
        {
            Assert.That(BranchPreviewSettings.Defaults.alignmentRadiusVisible, Is.False);
        }
    }
}
