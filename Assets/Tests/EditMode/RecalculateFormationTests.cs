using NUnit.Framework;
using RogueliteAutoBattler.Combat;
using RogueliteAutoBattler.Tests;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class RecalculateFormationTests : EditModeTestBase
    {
        private GameObject _container;
        private GameObject _anchor;

        [SetUp]
        public void SetUp()
        {
            _container = Track(new GameObject("Container"));
            _anchor = Track(TestCharacterFactory.CreateAnchor("HomeAnchor", new Vector2(2f, 0f)));
        }

        [Test]
        public void ThreeAlive_AllGetNewOffsets()
        {
            var anchorPos = new Vector2(2f, 0f);
            var chars = new GameObject[3];
            for (int i = 0; i < 3; i++)
            {
                chars[i] = CreateFormationChild($"Char{i}");
                chars[i].GetComponent<CharacterMover>().SetHomeOffset(new Vector2(99f, 99f));
            }

            CombatSetupHelper.RecalculateFormation(_container.transform, _anchor.transform, facingRight: true);

            Vector2[] expected = FormationLayout.GetPositions(anchorPos, 3, true);
            for (int i = 0; i < 3; i++)
            {
                Vector2 expectedOffset = expected[i] - anchorPos;
                Vector2 actual = chars[i].GetComponent<CharacterMover>().HomeOffset;
                Assert.That(actual.x, Is.EqualTo(expectedOffset.x).Within(0.01f),
                    $"Char{i} X offset mismatch");
                Assert.That(actual.y, Is.EqualTo(expectedOffset.y).Within(0.01f),
                    $"Char{i} Y offset mismatch");
            }
        }

        [Test]
        public void OneDeadTwoAlive_DeadSkipped()
        {
            var anchorPos = new Vector2(2f, 0f);

            var char0 = CreateFormationChild("Alive0");
            var char1 = CreateFormationChild("Dead1");
            var char2 = CreateFormationChild("Alive2");

            char1.GetComponent<CombatStats>().TakeDamage(9999);
            Assert.IsTrue(char1.GetComponent<CombatStats>().IsDead, "Precondition: char1 should be dead");

            var deadOffsetBefore = char1.GetComponent<CharacterMover>().HomeOffset;

            CombatSetupHelper.RecalculateFormation(_container.transform, _anchor.transform, facingRight: true);

            Vector2[] expected = FormationLayout.GetPositions(anchorPos, 2, true);

            var alive0Offset = char0.GetComponent<CharacterMover>().HomeOffset;
            var alive2Offset = char2.GetComponent<CharacterMover>().HomeOffset;

            Vector2 expectedOffset0 = expected[0] - anchorPos;
            Vector2 expectedOffset1 = expected[1] - anchorPos;

            Assert.That(alive0Offset.x, Is.EqualTo(expectedOffset0.x).Within(0.01f), "Alive0 X");
            Assert.That(alive0Offset.y, Is.EqualTo(expectedOffset0.y).Within(0.01f), "Alive0 Y");
            Assert.That(alive2Offset.x, Is.EqualTo(expectedOffset1.x).Within(0.01f), "Alive2 X");
            Assert.That(alive2Offset.y, Is.EqualTo(expectedOffset1.y).Within(0.01f), "Alive2 Y");

            var deadOffsetAfter = char1.GetComponent<CharacterMover>().HomeOffset;
            Assert.That(deadOffsetAfter.x, Is.EqualTo(deadOffsetBefore.x).Within(0.001f),
                "Dead character X offset should not change");
            Assert.That(deadOffsetAfter.y, Is.EqualTo(deadOffsetBefore.y).Within(0.001f),
                "Dead character Y offset should not change");
        }

        [Test]
        public void AllDead_NoChanges()
        {
            var char0 = CreateFormationChild("Dead0");
            var char1 = CreateFormationChild("Dead1");

            char0.GetComponent<CombatStats>().TakeDamage(9999);
            char1.GetComponent<CombatStats>().TakeDamage(9999);

            var offset0Before = char0.GetComponent<CharacterMover>().HomeOffset;
            var offset1Before = char1.GetComponent<CharacterMover>().HomeOffset;

            CombatSetupHelper.RecalculateFormation(_container.transform, _anchor.transform, facingRight: true);

            var offset0After = char0.GetComponent<CharacterMover>().HomeOffset;
            var offset1After = char1.GetComponent<CharacterMover>().HomeOffset;

            Assert.That(offset0After.x, Is.EqualTo(offset0Before.x).Within(0.001f), "Dead0 X unchanged");
            Assert.That(offset0After.y, Is.EqualTo(offset0Before.y).Within(0.001f), "Dead0 Y unchanged");
            Assert.That(offset1After.x, Is.EqualTo(offset1Before.x).Within(0.001f), "Dead1 X unchanged");
            Assert.That(offset1After.y, Is.EqualTo(offset1Before.y).Within(0.001f), "Dead1 Y unchanged");
        }

        [Test]
        public void Enemies_FacingLeftOffsets()
        {
            var anchorPos = new Vector2(5f, 0f);
            _anchor.transform.position = anchorPos;

            var chars = new GameObject[3];
            for (int i = 0; i < 3; i++)
            {
                chars[i] = CreateFormationChild($"Enemy{i}");
            }

            CombatSetupHelper.RecalculateFormation(_container.transform, _anchor.transform, facingRight: false);

            Vector2[] expected = FormationLayout.GetPositions(anchorPos, 3, false);
            for (int i = 0; i < 3; i++)
            {
                Vector2 expectedOffset = expected[i] - anchorPos;
                Vector2 actual = chars[i].GetComponent<CharacterMover>().HomeOffset;
                Assert.That(actual.x, Is.EqualTo(expectedOffset.x).Within(0.01f),
                    $"Enemy{i} X offset mismatch");
                Assert.That(actual.y, Is.EqualTo(expectedOffset.y).Within(0.01f),
                    $"Enemy{i} Y offset mismatch");
            }
        }

        [Test]
        public void NullContainer_NoException()
        {
            Assert.DoesNotThrow(() =>
                CombatSetupHelper.RecalculateFormation(null, _anchor.transform, true));
        }

        [Test]
        public void NullAnchor_NoException()
        {
            Assert.DoesNotThrow(() =>
                CombatSetupHelper.RecalculateFormation(_container.transform, null, true));
        }

        private GameObject CreateFormationChild(string name)
        {
            var go = TestCharacterFactory.CreateFormationCharacter(name: name, maxHp: 100, atk: 10, attackSpeed: 1f, moveSpeed: 2f);
            go.transform.SetParent(_container.transform);
            return go;
        }
    }
}
