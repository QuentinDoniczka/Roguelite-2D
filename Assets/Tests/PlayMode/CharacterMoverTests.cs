using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Combat.Environment;
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
        public IEnumerator Scroll_NoAnchor_CharacterMovesWithScroll()
        {
            var conveyorGo = Track(TestCharacterFactory.CreateConveyor("ConveyorParent"));
            var charGo = Track(TestCharacterFactory.CreateMoverCharacter(
                name: "ScrollRider",
                moveSpeed: 2f,
                parent: conveyorGo.transform,
                position: new Vector2(0f, 0f)));

            var mover = charGo.GetComponent<CharacterMover>();
            var conveyor = conveyorGo.GetComponent<WorldConveyor>();

            yield return null;

            mover.Target = null;
            mover.HomeAnchor = null;

            float startX = charGo.transform.position.x;

            conveyor.ScrollBy(4f, 8f, 16f);

            yield return new WaitForSeconds(ScrollTestTimeout);

            float endX = charGo.transform.position.x;

            Assert.That(endX, Is.LessThan(startX - 1f),
                "Character should move with scroll when no home anchor is set.");
        }

        [UnityTest]
        public IEnumerator ProportionalDamping_SlowsNearHome()
        {
            var anchor = Track(TestCharacterFactory.CreateAnchor("HomeAnchor", new Vector2(0f, 0f)));

            var charGo = Track(TestCharacterFactory.CreateMoverCharacter(
                name: "DampedChar",
                moveSpeed: 5f,
                position: new Vector2(0.05f, 0f)));

            var mover = charGo.GetComponent<CharacterMover>();

            yield return null;

            mover.HomeAnchor = anchor.transform;
            mover.Target = null;

            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            var rb = charGo.GetComponent<Rigidbody2D>();
            float velocity = Mathf.Abs(rb.linearVelocity.x);

            Assert.That(velocity, Is.LessThan(2f),
                "Velocity near home should be proportionally damped (much less than moveSpeed).");
        }

        [UnityTest]
        public IEnumerator Scroll_WithAnchor_CharacterWalksTowardHome()
        {
            var conveyorGo = Track(TestCharacterFactory.CreateConveyor("ScrollAnchorConveyor"));
            var anchor = Track(TestCharacterFactory.CreateAnchor("HomeAnchor", new Vector2(0f, 0f)));
            var charGo = Track(TestCharacterFactory.CreateMoverCharacter(
                name: "HomingRider",
                moveSpeed: 3f,
                parent: conveyorGo.transform,
                position: new Vector2(0f, 0f)));

            var mover = charGo.GetComponent<CharacterMover>();
            var conveyor = conveyorGo.GetComponent<WorldConveyor>();

            yield return null;

            mover.HomeAnchor = anchor.transform;
            mover.Target = null;

            conveyor.ScrollBy(4f, 8f, 16f);

            yield return new WaitForSeconds(0.5f);

            float midDrift = Mathf.Abs(charGo.transform.position.x - anchor.transform.position.x);

            Assert.That(midDrift, Is.LessThan(2f),
                $"Character should stay near anchor during scroll thanks to homing (drift was {midDrift:F2}).");
        }

        [UnityTest]
        public IEnumerator ScrollEnds_CharacterWalksBackToAnchor()
        {
            var conveyorGo = Track(TestCharacterFactory.CreateConveyor("ReturnConveyor"));
            var anchor = Track(TestCharacterFactory.CreateAnchor("HomeAnchor", new Vector2(0f, 0f)));
            var charGo = Track(TestCharacterFactory.CreateMoverCharacter(
                name: "WalkBack",
                moveSpeed: 5f,
                parent: conveyorGo.transform,
                position: new Vector2(0f, 0f)));

            var mover = charGo.GetComponent<CharacterMover>();
            var conveyor = conveyorGo.GetComponent<WorldConveyor>();

            yield return null;

            mover.HomeAnchor = anchor.transform;
            mover.Target = null;

            conveyor.ScrollBy(2f, 8f, 16f);

            yield return new WaitForSeconds(3f);

            float endDrift = Mathf.Abs(charGo.transform.position.x - anchor.transform.position.x);

            Assert.That(endDrift, Is.LessThan(0.5f),
                $"Character should return near anchor after scroll (drift was {endDrift:F2}).");
        }

        [UnityTest]
        public IEnumerator FlipToward_FlipsSpriteCorrectly()
        {
            var charGo = Track(TestCharacterFactory.CreateMoverCharacter(
                name: "Flipper",
                moveSpeed: 2f,
                position: new Vector2(0f, 0f)));

            var mover = charGo.GetComponent<CharacterMover>();

            yield return null;

            mover.FlipToward(1f);
            yield return null;

            Assert.That(charGo.transform.localScale.x, Is.EqualTo(-1f).Within(0.01f),
                "FlipToward(+1) should set localScale.x to -1 (face right).");

            mover.FlipToward(-1f);
            yield return null;

            Assert.That(charGo.transform.localScale.x, Is.EqualTo(1f).Within(0.01f),
                "FlipToward(-1) should set localScale.x to 1 (face left, native).");
        }

        [UnityTest]
        public IEnumerator FlipToward_PreservesScaleMagnitude()
        {
            var charGo = Track(TestCharacterFactory.CreateMoverCharacter(
                name: "ScaledFlipper",
                moveSpeed: 2f,
                position: new Vector2(0f, 0f)));

            charGo.transform.localScale = new Vector3(1.5f, 1.5f, 1f);

            var mover = charGo.GetComponent<CharacterMover>();

            yield return null;

            mover.FlipToward(1f);
            yield return null;

            Assert.That(charGo.transform.localScale.x, Is.EqualTo(-1.5f).Within(0.01f),
                "FlipToward(+1) should flip to -1.5 preserving magnitude.");

            mover.FlipToward(-1f);
            yield return null;

            Assert.That(charGo.transform.localScale.x, Is.EqualTo(1.5f).Within(0.01f),
                "FlipToward(-1) should flip to +1.5 preserving magnitude.");

            mover.FlipToward(1f);
            yield return null;

            Assert.That(charGo.transform.localScale.x, Is.EqualTo(-1.5f).Within(0.01f),
                "FlipToward(+1) again should be -1.5 with no drift.");
        }
    }
}
