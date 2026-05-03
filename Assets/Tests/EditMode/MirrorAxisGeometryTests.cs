using NUnit.Framework;
using RogueliteAutoBattler.Editor.Tools;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    internal sealed class MirrorAxisGeometryTests
    {
        private const float Tolerance = 1e-4f;

        [Test]
        public void ComputeAxisEndpoints_VerticalAxis_ReturnsVerticalLine()
        {
            var origin = new Vector2(100f, 100f);
            MirrorAxisGeometry.ComputeAxisEndpoints(origin, 0f, 50f, out var start, out var end);

            Assert.AreEqual(100f, start.x, Tolerance);
            Assert.AreEqual(50f, start.y, Tolerance);
            Assert.AreEqual(100f, end.x, Tolerance);
            Assert.AreEqual(150f, end.y, Tolerance);
        }

        [Test]
        public void ComputeAxisEndpoints_HorizontalAxis_ReturnsHorizontalLine()
        {
            var origin = new Vector2(100f, 100f);
            MirrorAxisGeometry.ComputeAxisEndpoints(origin, 90f, 50f, out var start, out var end);

            Assert.AreEqual(50f, start.x, Tolerance);
            Assert.AreEqual(100f, start.y, Tolerance);
            Assert.AreEqual(150f, end.x, Tolerance);
            Assert.AreEqual(100f, end.y, Tolerance);
        }

        [Test]
        public void ComputeAxisEndpoints_45Degrees_ReturnsDiagonal()
        {
            float halfSpan = Mathf.Sqrt(2f);
            MirrorAxisGeometry.ComputeAxisEndpoints(Vector2.zero, 45f, halfSpan, out var start, out var end);

            Assert.AreEqual(-1f, start.x, Tolerance);
            Assert.AreEqual(-1f, start.y, Tolerance);
            Assert.AreEqual(1f, end.x, Tolerance);
            Assert.AreEqual(1f, end.y, Tolerance);
        }

        [TestCase(0f)]
        [TestCase(30f)]
        [TestCase(45f)]
        [TestCase(90f)]
        [TestCase(180f)]
        public void ComputeAxisEndpoints_LineCrossesOrigin(float axisAngle)
        {
            var origin = new Vector2(37f, 83f);
            MirrorAxisGeometry.ComputeAxisEndpoints(origin, axisAngle, 100f, out var start, out var end);

            var midpoint = (start + end) * 0.5f;
            Assert.AreEqual(origin.x, midpoint.x, Tolerance);
            Assert.AreEqual(origin.y, midpoint.y, Tolerance);
        }
    }
}
