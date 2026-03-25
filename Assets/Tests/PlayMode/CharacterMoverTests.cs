using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.Combat;
using RogueliteAutoBattler.Tests;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class CharacterMoverTests : PlayModeTestBase
    {
        private const float MoveTimeout = 1f;
        private const float ScrollTestTimeout = 1.5f;

        [UnityTest]
        public IEnumerator WalkTowardHome_MovesCharacterTowardAnchor()
        {
            var anchor = Track(TestCharacterFactory.CreateAnchor("HomeAnchor", new Vector2(0f, 0f)));
            var charGo = Track(TestCharacterFactory.CreateMoverCharacter(
                name: "Walker",
                moveSpeed: 3f,
                position: new Vector2(3f, 0f)));

            var mover = charGo.GetComponent<CharacterMover>();

            // Wait a frame so Awake runs.
            yield return null;

            mover.HomeAnchor = anchor.transform;
            mover.Target = null;

            float startX = charGo.transform.position.x;

            yield return new WaitForSeconds(MoveTimeout);

            float endX = charGo.transform.position.x;

            Assert.That(endX, Is.LessThan(startX),
                "Character should have moved toward home anchor (leftward).");
            Assert.That(endX, Is.LessThan(2.0f),
                "Character should be significantly closer to anchor at X=0 after 1 second.");
        }

        [UnityTest]
        public IEnumerator ChargeTowardTarget_MovesCharacterTowardTarget()
        {
            var targetGo = Track(TestCharacterFactory.CreateAnchor("Target", new Vector2(5f, 0f)));
            var charGo = Track(TestCharacterFactory.CreateMoverCharacter(
                name: "Charger",
                moveSpeed: 3f,
                position: new Vector2(0f, 0f)));

            var mover = charGo.GetComponent<CharacterMover>();

            // Wait a frame so Awake runs.
            yield return null;

            mover.Target = targetGo.transform;

            float startX = charGo.transform.position.x;

            yield return new WaitForSeconds(MoveTimeout);

            float endX = charGo.transform.position.x;

            Assert.That(endX, Is.GreaterThan(startX),
                "Character should have moved toward target (rightward).");
            Assert.That(endX, Is.GreaterThan(1.5f),
                "Character should have moved meaningfully toward target at X=5 after 1 second.");
        }

        [UnityTest]
        public IEnumerator ScrollCompensation_CarriesCharacterWithConveyor()
        {
            var conveyorGo = Track(TestCharacterFactory.CreateConveyor("ConveyorParent"));
            var charGo = Track(TestCharacterFactory.CreateMoverCharacter(
                name: "ScrollRider",
                moveSpeed: 2f,
                parent: conveyorGo.transform,
                position: new Vector2(0f, 0f)));

            var mover = charGo.GetComponent<CharacterMover>();
            var conveyor = conveyorGo.GetComponent<WorldConveyor>();

            // Wait a frame so Awake runs (CharacterMover caches _conveyor in Awake via GetComponentInParent).
            yield return null;

            // No target, no home anchor — character should just follow conveyor scroll velocity.
            mover.Target = null;
            mover.HomeAnchor = null;

            float startX = charGo.transform.position.x;

            // Start scrolling the conveyor.
            conveyor.ScrollBy(4f, 8f, 16f);

            yield return new WaitForSeconds(ScrollTestTimeout);

            float endX = charGo.transform.position.x;

            // The character, as a child of the kinematic conveyor, moves with it.
            // The character's world position should have shifted left.
            Assert.That(endX, Is.LessThan(startX - 1f),
                "Character should have moved leftward with the conveyor scroll.");
        }

        [UnityTest]
        public IEnumerator ProportionalDamping_SlowsNearHome()
        {
            var anchor = Track(TestCharacterFactory.CreateAnchor("HomeAnchor", new Vector2(0f, 0f)));

            // Place character very close to anchor — damping should result in very low velocity.
            var charGo = Track(TestCharacterFactory.CreateMoverCharacter(
                name: "DampedChar",
                moveSpeed: 5f,
                position: new Vector2(0.05f, 0f)));

            var mover = charGo.GetComponent<CharacterMover>();

            yield return null;

            mover.HomeAnchor = anchor.transform;
            mover.Target = null;

            // Let physics run a few steps.
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            var rb = charGo.GetComponent<Rigidbody2D>();
            float velocity = Mathf.Abs(rb.linearVelocity.x);

            // At distance 0.05, correctionSpeed = min(0.05 * 8, 5) = 0.4 — well below moveSpeed of 5.
            Assert.That(velocity, Is.LessThan(2f),
                "Velocity near home should be proportionally damped (much less than moveSpeed).");
        }

        [UnityTest]
        public IEnumerator FlipToward_FlipsSpriteCorrectly()
        {
            var charGo = Track(TestCharacterFactory.CreateMoverCharacter(
                name: "Flipper",
                moveSpeed: 2f,
                position: new Vector2(0f, 0f)));

            var mover = charGo.GetComponent<CharacterMover>();

            // Wait a frame so Awake runs.
            yield return null;

            // FlipToward sets transform.localScale.x on the root.
            // directionX > 0 => localScale.x = -1 (face right, since sprites face LEFT natively)
            // directionX < 0 => localScale.x = 1 (face left, native)
            mover.FlipToward(1f);
            yield return null;

            Assert.That(charGo.transform.localScale.x, Is.EqualTo(-1f).Within(0.01f),
                "FlipToward(+1) should set localScale.x to -1 (face right).");

            mover.FlipToward(-1f);
            yield return null;

            Assert.That(charGo.transform.localScale.x, Is.EqualTo(1f).Within(0.01f),
                "FlipToward(-1) should set localScale.x to 1 (face left, native).");
        }
    }
}
