using NUnit.Framework;
using RogueliteAutoBattler.Editor.Tools;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class BranchPlacementTests
    {
        private const float Tolerance = 1e-4f;

        [Test]
        public void ComputeBranchPosition_AtParentDirectionOutward_ReturnsCorrectPosition()
        {
            Vector2 parent = new Vector2(3f, 4f);
            float distance = 5f;

            Vector2 result = BranchPlacement.ComputeBranchPosition(parent, distance);

            Assert.That(result.x, Is.EqualTo(6f).Within(Tolerance));
            Assert.That(result.y, Is.EqualTo(8f).Within(Tolerance));
        }

        [Test]
        public void ComputeBranchPosition_DegenerateZeroParent_FallsBackToRight()
        {
            Vector2 parent = Vector2.zero;
            float distance = 3f;

            Vector2 result = BranchPlacement.ComputeBranchPosition(parent, distance);

            Assert.That(result.x, Is.EqualTo(3f).Within(Tolerance));
            Assert.That(result.y, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void ComputeBranchPosition_NegativeYAxis_PreservesDirection()
        {
            Vector2 parent = new Vector2(0f, -2f);
            float distance = 4f;

            Vector2 result = BranchPlacement.ComputeBranchPosition(parent, distance);

            Assert.That(result.x, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(result.y, Is.EqualTo(-6f).Within(Tolerance));
        }

        [Test]
        public void ComputeBranchPosition_VeryTinyParent_FallsBackToRight()
        {
            Vector2 parent = new Vector2(1e-4f, 1e-4f);
            float distance = 1f;

            Vector2 result = BranchPlacement.ComputeBranchPosition(parent, distance);

            Assert.That(result.x, Is.EqualTo(1.0001f).Within(Tolerance));
            Assert.That(result.y, Is.EqualTo(1e-4f).Within(Tolerance));
        }
    }
}
