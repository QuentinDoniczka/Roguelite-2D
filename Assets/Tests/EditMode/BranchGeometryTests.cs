using NUnit.Framework;
using RogueliteAutoBattler.Editor.Windows.SkillTreeDesigner;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class BranchGeometryTests
    {
        [Test]
        public void ComputeBranchPosition_0Deg_ReturnsParentPlusUp()
        {
            var parent = new Vector2(10f, 20f);
            const float distance = 5f;
            const float angle = 0f;
            const float tolerance = 1e-4f;

            Vector2 result = BranchGeometry.ComputeBranchPosition(parent, distance, angle);

            Assert.That(result.x, Is.EqualTo(10f).Within(tolerance));
            Assert.That(result.y, Is.EqualTo(15f).Within(tolerance));
        }

        [Test]
        public void ComputeBranchPosition_90Deg_ReturnsParentPlusRight()
        {
            var parent = new Vector2(10f, 20f);
            const float distance = 5f;
            const float angle = 90f;
            const float tolerance = 1e-4f;

            Vector2 result = BranchGeometry.ComputeBranchPosition(parent, distance, angle);

            Assert.That(result.x, Is.EqualTo(15f).Within(tolerance));
            Assert.That(result.y, Is.EqualTo(20f).Within(tolerance));
        }

        [Test]
        public void ComputeBranchPosition_180Deg_ReturnsParentPlusDown()
        {
            var parent = new Vector2(10f, 20f);
            const float distance = 5f;
            const float angle = 180f;
            const float tolerance = 1e-4f;

            Vector2 result = BranchGeometry.ComputeBranchPosition(parent, distance, angle);

            Assert.That(result.x, Is.EqualTo(10f).Within(tolerance));
            Assert.That(result.y, Is.EqualTo(25f).Within(tolerance));
        }

        [Test]
        public void ComputeBranchPosition_270Deg_ReturnsParentPlusLeft()
        {
            var parent = new Vector2(10f, 20f);
            const float distance = 5f;
            const float angle = 270f;
            const float tolerance = 1e-4f;

            Vector2 result = BranchGeometry.ComputeBranchPosition(parent, distance, angle);

            Assert.That(result.x, Is.EqualTo(5f).Within(tolerance));
            Assert.That(result.y, Is.EqualTo(20f).Within(tolerance));
        }

        [Test]
        public void ComputeBranchPosition_45Deg_DiagonalIsNormalized()
        {
            var parent = new Vector2(0f, 0f);
            const float distance = 2f;
            const float angle = 45f;
            const float tolerance = 1e-4f;
            const float expectedComponent = 1.41421f;

            Vector2 result = BranchGeometry.ComputeBranchPosition(parent, distance, angle);
            float offsetMagnitude = new Vector2(result.x - parent.x, result.y - parent.y).magnitude;

            Assert.That(offsetMagnitude, Is.EqualTo(distance).Within(tolerance));
            Assert.That(result.x, Is.EqualTo(expectedComponent).Within(tolerance));
            Assert.That(Mathf.Abs(result.y), Is.EqualTo(expectedComponent).Within(tolerance));
        }

        [Test]
        public void ComputeBranchPosition_DistanceZero_ReturnsParent()
        {
            var parent = new Vector2(7f, 13f);
            const float distance = 0f;
            const float angle = 45f;
            const float tolerance = 1e-4f;

            Vector2 result = BranchGeometry.ComputeBranchPosition(parent, distance, angle);

            Assert.That(result.x, Is.EqualTo(parent.x).Within(tolerance));
            Assert.That(result.y, Is.EqualTo(parent.y).Within(tolerance));
        }
    }
}
