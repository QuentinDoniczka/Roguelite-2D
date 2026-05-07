using NUnit.Framework;
using RogueliteAutoBattler.Editor.Tools;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    internal sealed class AlignmentOverlayGeometryTests
    {
        private const float Tolerance = 1e-4f;

        [Test]
        public void ComputeRadiusCircleScreenRadius_ScalesByUnitSizeAndZoom()
        {
            float result = AlignmentOverlayGeometry.ComputeRadiusCircleScreenRadius(6f, 50f, 2f);

            Assert.AreEqual(600f, result, Tolerance);
        }

        [Test]
        public void ComputeRadiusCircleCenterScreen_TranslatesByOriginAndScaledUnit()
        {
            var nodePosUnits = new Vector2(1f, 1f);
            var origin = new Vector2(100f, 100f);
            float scaledUnit = 50f;

            Vector2 result = AlignmentOverlayGeometry.ComputeRadiusCircleCenterScreen(nodePosUnits, origin, scaledUnit);

            Assert.AreEqual(150f, result.x, Tolerance);
            Assert.AreEqual(150f, result.y, Tolerance);
        }
    }
}
