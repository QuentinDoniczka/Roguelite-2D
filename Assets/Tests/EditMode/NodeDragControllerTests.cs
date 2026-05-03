using NUnit.Framework;
using RogueliteAutoBattler.Editor.Tools;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class NodeDragControllerTests
    {
        private const float Tolerance = 1e-4f;

        private static NodeDragController.DragState MakeState(Vector2 nodeStart, Vector2 mouseStart)
        {
            return new NodeDragController.DragState(0, nodeStart, mouseStart, -1, Vector2.zero);
        }

        [Test]
        public void ComputeNewNodePosition_ZeroDelta_ReturnsStartPosition()
        {
            var state = MakeState(new Vector2(3f, 4f), new Vector2(100f, 200f));

            Vector2 result = NodeDragController.ComputeNewNodePosition(state, new Vector2(100f, 200f), 40f, 1f);

            Assert.That(result.x, Is.EqualTo(3f).Within(Tolerance));
            Assert.That(result.y, Is.EqualTo(4f).Within(Tolerance));
        }

        [Test]
        public void ComputeNewNodePosition_PositiveXDelta_TranslatesRightInUnits()
        {
            var state = MakeState(new Vector2(0f, 0f), new Vector2(0f, 0f));

            // 80px right at unitSize=40, zoom=1 → delta = 80/40 = 2 units
            Vector2 result = NodeDragController.ComputeNewNodePosition(state, new Vector2(80f, 0f), 40f, 1f);

            Assert.That(result.x, Is.EqualTo(2f).Within(Tolerance));
            Assert.That(result.y, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void ComputeNewNodePosition_YDelta_TranslatesAccordingToCanvasConvention()
        {
            // Canvas Y is downward; a positive pixel-Y delta must produce a positive unit-Y delta
            var state = MakeState(new Vector2(0f, 0f), new Vector2(0f, 0f));

            Vector2 result = NodeDragController.ComputeNewNodePosition(state, new Vector2(0f, 40f), 40f, 1f);

            Assert.That(result.y, Is.EqualTo(1f).Within(Tolerance));
        }

        [Test]
        public void ComputeNewNodePosition_ZoomGreaterThanOne_ScalesDeltaInversely()
        {
            var state = MakeState(new Vector2(0f, 0f), new Vector2(0f, 0f));

            // 40px right at unitSize=40, zoom=2 → scale=80 → delta = 40/80 = 0.5 units
            Vector2 result = NodeDragController.ComputeNewNodePosition(state, new Vector2(40f, 0f), 40f, 2f);

            Assert.That(result.x, Is.EqualTo(0.5f).Within(Tolerance));
        }

        [Test]
        public void ComputeNewNodePosition_ZoomLessThanOne_ScalesDeltaInversely()
        {
            var state = MakeState(new Vector2(0f, 0f), new Vector2(0f, 0f));

            // 40px right at unitSize=40, zoom=0.5 → scale=20 → delta = 40/20 = 2 units
            Vector2 result = NodeDragController.ComputeNewNodePosition(state, new Vector2(40f, 0f), 40f, 0.5f);

            Assert.That(result.x, Is.EqualTo(2f).Within(Tolerance));
        }

        [Test]
        public void ComputeNewNodePosition_ZeroScale_ReturnsStartPosition()
        {
            var state = MakeState(new Vector2(5f, 7f), new Vector2(0f, 0f));

            Vector2 result = NodeDragController.ComputeNewNodePosition(state, new Vector2(100f, 100f), 0f, 1f);

            Assert.That(result.x, Is.EqualTo(5f).Within(Tolerance));
            Assert.That(result.y, Is.EqualTo(7f).Within(Tolerance));
        }

        [Test]
        public void DragState_Inactive_HasNodeIndexMinusOne()
        {
            Assert.That(NodeDragController.DragState.Inactive.NodeIndex, Is.EqualTo(-1));
        }

        [Test]
        public void DragState_Constructed_IsActiveTrue()
        {
            var state = new NodeDragController.DragState(2, Vector2.zero, Vector2.zero, -1, Vector2.zero);

            Assert.IsTrue(state.IsActive);
        }
    }
}
