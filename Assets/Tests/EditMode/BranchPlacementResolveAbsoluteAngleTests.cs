using NUnit.Framework;
using RogueliteAutoBattler.Editor.Tools;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class BranchPlacementResolveAbsoluteAngleTests
    {
        private const float Tolerance = 1e-4f;

        [Test]
        public void ResolveAbsoluteAngle_NotRelative_ReturnsInputUnchanged()
        {
            float result = BranchPlacement.ResolveAbsoluteAngle(73f, 200f, isRelative: false);

            Assert.That(result, Is.EqualTo(73f).Within(Tolerance));
        }

        [Test]
        public void ResolveAbsoluteAngle_Relative_AddsAxisToRelative()
        {
            float result = BranchPlacement.ResolveAbsoluteAngle(0f, 45f, isRelative: true);

            Assert.That(result, Is.EqualTo(45f).Within(Tolerance));
        }

        [Test]
        public void ResolveAbsoluteAngle_Relative_WrapsAround360()
        {
            float result = BranchPlacement.ResolveAbsoluteAngle(90f, 270f, isRelative: true);

            Assert.That(result, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void ResolveAbsoluteAngle_Relative_NormalizesNegativeRelative()
        {
            float result = BranchPlacement.ResolveAbsoluteAngle(-10f, 0f, isRelative: true);

            Assert.That(result, Is.EqualTo(350f).Within(Tolerance));
        }

        [Test]
        public void ResolveAbsoluteAngle_Relative_NormalizesAxisAbove360()
        {
            float result = BranchPlacement.ResolveAbsoluteAngle(30f, 420f, isRelative: true);

            Assert.That(result, Is.EqualTo(90f).Within(Tolerance));
        }

        [Test]
        public void ResolveAbsoluteAngle_Relative_AlwaysProducesResultInUnitCircleRange()
        {
            System.Random random = new System.Random(424242);
            for (int i = 0; i < 32; i++)
            {
                float relative = (float)(random.NextDouble() * 1440.0 - 720.0);
                float axis = (float)(random.NextDouble() * 1440.0 - 720.0);

                float result = BranchPlacement.ResolveAbsoluteAngle(relative, axis, isRelative: true);

                Assert.That(result, Is.GreaterThanOrEqualTo(0f));
                Assert.That(result, Is.LessThan(360f));
            }
        }
    }
}
