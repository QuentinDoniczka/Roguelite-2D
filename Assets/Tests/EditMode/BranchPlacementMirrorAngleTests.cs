using NUnit.Framework;
using RogueliteAutoBattler.Editor.Tools;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class BranchPlacementMirrorAngleTests
    {
        private const float Tolerance = 1e-4f;

        [Test]
        public void MirrorAngle_60AcrossVerticalAxis_Returns300()
        {
            float result = BranchPlacement.MirrorAngle(60f, 0f);

            Assert.That(result, Is.EqualTo(300f).Within(Tolerance));
        }

        [Test]
        public void MirrorAngle_OnAxisInput_ReturnsItself()
        {
            float result = BranchPlacement.MirrorAngle(90f, 90f);

            Assert.That(result, Is.EqualTo(90f).Within(Tolerance));
        }

        [Test]
        public void MirrorAngle_OppositeAxisLine_BehavesIdentically()
        {
            float result = BranchPlacement.MirrorAngle(90f, 270f);

            Assert.That(result, Is.EqualTo(90f).Within(Tolerance));
        }

        [Test]
        public void MirrorAngle_CrossQuadrant_45AcrossHorizontal_Returns135()
        {
            float result = BranchPlacement.MirrorAngle(45f, 90f);

            Assert.That(result, Is.EqualTo(135f).Within(Tolerance));
        }

        [Test]
        public void MirrorAngle_NegativeInput_NormalizedToPositive()
        {
            float result = BranchPlacement.MirrorAngle(-30f, 0f);

            Assert.That(result, Is.EqualTo(30f).Within(Tolerance));
        }

        [Test]
        public void MirrorAngle_InputAbove360_NormalizedFirst()
        {
            float result = BranchPlacement.MirrorAngle(420f, 0f);

            Assert.That(result, Is.EqualTo(300f).Within(Tolerance));
        }

        [Test]
        public void MirrorAngle_AxisAngleAbove360_NormalizedFirst()
        {
            float result = BranchPlacement.MirrorAngle(60f, 360f);

            Assert.That(result, Is.EqualTo(300f).Within(Tolerance));
        }

        [Test]
        public void MirrorAngle_ResultAlwaysInUnitCircleRange()
        {
            System.Random random = new System.Random(12345);
            for (int i = 0; i < 16; i++)
            {
                float angle = (float)(random.NextDouble() * 1440.0 - 720.0);
                float axis = (float)(random.NextDouble() * 1440.0 - 720.0);

                float result = BranchPlacement.MirrorAngle(angle, axis);

                Assert.That(result, Is.GreaterThanOrEqualTo(0f));
                Assert.That(result, Is.LessThan(360f));
            }
        }

        [Test]
        public void PositionTolerance_IsOneUnit()
        {
            Assert.That(BranchPlacement.PositionTolerance, Is.EqualTo(1f).Within(Tolerance));
        }
    }
}
