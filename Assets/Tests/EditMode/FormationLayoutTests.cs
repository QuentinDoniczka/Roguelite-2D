using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class FormationLayoutTests
    {
        [Test]
        public void GetPositions_ZeroCount_ReturnsEmpty()
        {
            Vector2[] positions = FormationLayout.GetPositions(Vector2.zero, 0, true);

            Assert.AreEqual(0, positions.Length);
        }

        [Test]
        public void GetPositions_SingleUnit_ReturnsAnchorPosition()
        {
            var anchor = new Vector2(3f, 1f);

            Vector2[] positions = FormationLayout.GetPositions(anchor, 1, true);

            Assert.AreEqual(1, positions.Length);
            Assert.That(positions[0].x, Is.EqualTo(anchor.x).Within(0.01f));
            Assert.That(positions[0].y, Is.EqualTo(anchor.y).Within(0.01f));
        }

        [Test]
        public void GetPositions_ThreeUnits_SingleColumn_CenteredVertically()
        {
            var anchor = new Vector2(0f, 0f);

            Vector2[] positions = FormationLayout.GetPositions(anchor, 3, true);

            Assert.AreEqual(3, positions.Length);

            Assert.That(positions[0].y, Is.EqualTo(0.5f).Within(0.01f));
            Assert.That(positions[1].y, Is.EqualTo(0.0f).Within(0.01f));
            Assert.That(positions[2].y, Is.EqualTo(-0.5f).Within(0.01f));

            Assert.That(positions[0].x, Is.EqualTo(0f).Within(0.01f));
            Assert.That(positions[1].x, Is.EqualTo(0f).Within(0.01f));
            Assert.That(positions[2].x, Is.EqualTo(0f).Within(0.01f));
        }

        [Test]
        public void GetPositions_FiveUnits_SingleColumn_MaxPerColumn()
        {
            var anchor = new Vector2(2f, 1f);

            Vector2[] positions = FormationLayout.GetPositions(anchor, 5, true);

            Assert.AreEqual(5, positions.Length);

            Assert.That(positions[0].y, Is.EqualTo(2.0f).Within(0.01f));
            Assert.That(positions[1].y, Is.EqualTo(1.5f).Within(0.01f));
            Assert.That(positions[2].y, Is.EqualTo(1.0f).Within(0.01f));
            Assert.That(positions[3].y, Is.EqualTo(0.5f).Within(0.01f));
            Assert.That(positions[4].y, Is.EqualTo(0.0f).Within(0.01f));

            for (int i = 0; i < 5; i++)
            {
                Assert.That(positions[i].x, Is.EqualTo(2f).Within(0.01f));
            }
        }

        [Test]
        public void GetPositions_SixUnits_TwoColumns_FacingRight()
        {
            var anchor = new Vector2(1f, 0f);

            Vector2[] positions = FormationLayout.GetPositions(anchor, 6, facingRight: true);

            Assert.AreEqual(6, positions.Length);

            for (int i = 0; i < 3; i++)
            {
                Assert.That(positions[i].x, Is.EqualTo(1.0f).Within(0.01f));
            }

            for (int i = 3; i < 6; i++)
            {
                Assert.That(positions[i].x, Is.EqualTo(0.5f).Within(0.01f));
            }
        }

        [Test]
        public void GetPositions_SixUnits_TwoColumns_FacingLeft()
        {
            var anchor = new Vector2(1f, 0f);

            Vector2[] positions = FormationLayout.GetPositions(anchor, 6, facingRight: false);

            Assert.AreEqual(6, positions.Length);

            for (int i = 0; i < 3; i++)
            {
                Assert.That(positions[i].x, Is.EqualTo(1.0f).Within(0.01f));
            }

            for (int i = 3; i < 6; i++)
            {
                Assert.That(positions[i].x, Is.EqualTo(1.5f).Within(0.01f));
            }
        }

        [Test]
        public void GetPositions_WithScaleFactor_SpacingScaled()
        {
            Vector2[] positions = FormationLayout.GetPositions(Vector2.zero, 3, true, characterScale: 1.5f);

            Assert.AreEqual(3, positions.Length);

            Assert.That(positions[0].y, Is.EqualTo(0.75f).Within(0.01f));
            Assert.That(positions[1].y, Is.EqualTo(0.0f).Within(0.01f));
            Assert.That(positions[2].y, Is.EqualTo(-0.75f).Within(0.01f));

            Assert.That(positions[0].x, Is.EqualTo(0f).Within(0.01f));
            Assert.That(positions[1].x, Is.EqualTo(0f).Within(0.01f));
            Assert.That(positions[2].x, Is.EqualTo(0f).Within(0.01f));
        }

        [Test]
        public void GetPositions_SixUnits_WithScaleFactor_ColumnSpacingScaled()
        {
            Vector2[] positions = FormationLayout.GetPositions(Vector2.zero, 6, facingRight: true, characterScale: 1.5f);

            Assert.AreEqual(6, positions.Length);

            for (int i = 0; i < 3; i++)
            {
                Assert.That(positions[i].x, Is.EqualTo(0f).Within(0.01f));
            }

            for (int i = 3; i < 6; i++)
            {
                Assert.That(positions[i].x, Is.EqualTo(-0.75f).Within(0.01f));
            }
        }
    }
}
