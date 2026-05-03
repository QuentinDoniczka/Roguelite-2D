using NUnit.Framework;
using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class SkillTreeGridTests
    {
        private const float QuantizeTolerance = 1e-5f;

        [Test]
        public void Step_Constant_Is001()
        {
            Assert.AreEqual(0.01f, SkillTreeGrid.Step, 1e-7f);
        }

        [Test]
        public void Quantize_Zero_ReturnsZero()
        {
            var result = SkillTreeGrid.Quantize(Vector2.zero);

            Assert.AreEqual(0f, result.x, QuantizeTolerance);
            Assert.AreEqual(0f, result.y, QuantizeTolerance);
        }

        [Test]
        public void Quantize_AlreadyOnGrid_Unchanged()
        {
            var input = new Vector2(0.35f, -0.42f);

            var result = SkillTreeGrid.Quantize(input);

            Assert.AreEqual(0.35f, result.x, QuantizeTolerance);
            Assert.AreEqual(-0.42f, result.y, QuantizeTolerance);
        }

        [Test]
        public void Quantize_HalfStep_RoundsToNearest()
        {
            var input = new Vector2(0.005f, 0.015f);
            float expectedX = Mathf.Round(input.x / SkillTreeGrid.Step) * SkillTreeGrid.Step;
            float expectedY = Mathf.Round(input.y / SkillTreeGrid.Step) * SkillTreeGrid.Step;

            var result = SkillTreeGrid.Quantize(input);

            Assert.AreEqual(expectedX, result.x, QuantizeTolerance);
            Assert.AreEqual(expectedY, result.y, QuantizeTolerance);
            Assert.That(result.x, Is.EqualTo(0f).Within(QuantizeTolerance).Or.EqualTo(0.01f).Within(QuantizeTolerance));
            Assert.That(result.y, Is.EqualTo(0.01f).Within(QuantizeTolerance).Or.EqualTo(0.02f).Within(QuantizeTolerance));
        }

        [Test]
        public void Quantize_NegativeNearZero_PreservesSign()
        {
            var input = new Vector2(-0.004f, -0.006f);

            var result = SkillTreeGrid.Quantize(input);

            Assert.AreEqual(0f, result.x, QuantizeTolerance);
            Assert.AreEqual(-0.01f, result.y, QuantizeTolerance);
        }

        [Test]
        public void Quantize_LargeMagnitude_StaysOnGrid()
        {
            var input = new Vector2(123.4567f, -98.7654f);

            var result = SkillTreeGrid.Quantize(input);

            Assert.AreEqual(123.46f, result.x, QuantizeTolerance);
            Assert.AreEqual(-98.77f, result.y, QuantizeTolerance);
        }

        [Test]
        public void Quantize_FloatGarbage_Removed()
        {
            var input = new Vector2(10.00054456f, -1.8822026f);

            var result = SkillTreeGrid.Quantize(input);

            Assert.AreEqual(10.00f, result.x, QuantizeTolerance);
            Assert.AreEqual(-1.88f, result.y, QuantizeTolerance);
        }

        [Test]
        public void ToDisplay_IntegerPercent_ReturnsRoundedInt()
        {
            var (x, y) = SkillTreeGrid.ToDisplay(new Vector2(0.35f, -0.42f));

            Assert.AreEqual(35, x);
            Assert.AreEqual(-42, y);
        }

        [Test]
        public void ToDisplay_FractionalSubGrid_RoundsHalfToNearest()
        {
            var (x, y) = SkillTreeGrid.ToDisplay(new Vector2(0.354f, 0.356f));

            Assert.AreEqual(35, x);
            Assert.AreEqual(36, y);
        }

        [Test]
        public void DistanceDisplay_AxisAligned_ReturnsExpectedInt()
        {
            int distance = SkillTreeGrid.DistanceDisplay(Vector2.zero, new Vector2(0.30f, 0f));

            Assert.AreEqual(30, distance);
        }

        [Test]
        public void DistanceDisplay_Diagonal_ReturnsExpectedInt()
        {
            int distance = SkillTreeGrid.DistanceDisplay(Vector2.zero, new Vector2(0.30f, 0.40f));

            Assert.AreEqual(50, distance);
        }

        [Test]
        public void DistanceDisplayFromUnits_TypicalValue_RoundsToHundredth()
        {
            Assert.AreEqual(30, SkillTreeGrid.DistanceDisplayFromUnits(0.30f));
            Assert.AreEqual(31, SkillTreeGrid.DistanceDisplayFromUnits(0.305f));
        }

        [Test]
        public void DistanceDisplayFromUnits_Zero_ReturnsZero()
        {
            Assert.AreEqual(0, SkillTreeGrid.DistanceDisplayFromUnits(0f));
        }
    }
}
