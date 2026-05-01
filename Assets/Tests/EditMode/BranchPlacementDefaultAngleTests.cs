using NUnit.Framework;
using RogueliteAutoBattler.Editor.Tools;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class BranchPlacementDefaultAngleTests
    {
        private const float Tolerance = 1e-4f;

        [Test]
        public void ComputeDefaultAngle_ParentAtRight_Returns90()
        {
            float angle = BranchPlacement.ComputeDefaultAngle(new Vector2(1f, 0f));

            Assert.That(angle, Is.EqualTo(90f).Within(Tolerance));
        }

        [Test]
        public void ComputeDefaultAngle_ParentAtUp_Returns0()
        {
            float angle = BranchPlacement.ComputeDefaultAngle(new Vector2(0f, 1f));

            Assert.That(angle, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void ComputeDefaultAngle_ParentAtLeft_Returns270()
        {
            float angle = BranchPlacement.ComputeDefaultAngle(new Vector2(-1f, 0f));

            Assert.That(angle, Is.EqualTo(270f).Within(Tolerance));
        }

        [Test]
        public void ComputeDefaultAngle_ParentAtDown_Returns180()
        {
            float angle = BranchPlacement.ComputeDefaultAngle(new Vector2(0f, -1f));

            Assert.That(angle, Is.EqualTo(180f).Within(Tolerance));
        }

        [Test]
        public void ComputeDefaultAngle_ParentAtOrigin_Returns0()
        {
            float angle = BranchPlacement.ComputeDefaultAngle(Vector2.zero);

            Assert.That(angle, Is.EqualTo(0f).Within(Tolerance));
        }
    }
}
