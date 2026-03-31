using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Environment;
using RogueliteAutoBattler.Tests;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class WorldConveyorTests : PlayModeTestBase
    {
        private const float ScrollTimeout = 2f;
        private const float LongScrollTimeout = 5f;
        private const int MaxSampleFrames = 200;

        [UnityTest]
        public IEnumerator ScrollBy_MovesPositionLeftward()
        {
            var go = Track(TestCharacterFactory.CreateConveyor("TestConveyor"));
            var conveyor = go.GetComponent<WorldConveyor>();
            float startX = go.transform.position.x;

            yield return null;

            float distance = 5f;
            float maxSpeed = 10f;
            float acceleration = 20f;

            conveyor.ScrollBy(distance, maxSpeed, acceleration);

            yield return new WaitForSeconds(ScrollTimeout);

            float endX = go.transform.position.x;
            float expectedX = startX - distance;

            Assert.That(endX, Is.EqualTo(expectedX).Within(0.2f),
                $"Conveyor should have moved ~{distance} units left. Start={startX}, End={endX}, Expected={expectedX}");
        }

        [UnityTest]
        public IEnumerator ScrollBy_FiresOnScrollComplete()
        {
            var go = Track(TestCharacterFactory.CreateConveyor("TestConveyor"));
            var conveyor = go.GetComponent<WorldConveyor>();
            bool fired = false;

            yield return null;

            conveyor.OnScrollComplete += () => fired = true;
            conveyor.ScrollBy(3f, 10f, 20f);

            yield return new WaitForSeconds(ScrollTimeout);

            Assert.IsTrue(fired, "OnScrollComplete should have fired after scroll finished.");
        }

        [UnityTest]
        public IEnumerator ScrollBy_FiresOnDecelerationStarted()
        {
            var go = Track(TestCharacterFactory.CreateConveyor("TestConveyor"));
            var conveyor = go.GetComponent<WorldConveyor>();
            bool fired = false;

            yield return null;

            conveyor.OnDecelerationStarted += () => fired = true;

            conveyor.ScrollBy(5f, 4f, 2f);

            yield return new WaitForSeconds(LongScrollTimeout);

            Assert.IsTrue(fired, "OnDecelerationStarted should have fired during scroll.");
        }

        [UnityTest]
        public IEnumerator ScrollBy_SpeedProfile_AcceleratesAndDecelerates()
        {
            var go = Track(TestCharacterFactory.CreateConveyor("TestConveyor"));
            var conveyor = go.GetComponent<WorldConveyor>();

            yield return null;

            conveyor.ScrollBy(8f, 4f, 2f);

            float peakSpeed = 0f;
            bool sawAcceleration = false;
            bool sawDeceleration = false;
            float previousSpeed = 0f;

            for (int i = 0; i < MaxSampleFrames; i++)
            {
                yield return new WaitForFixedUpdate();

                float speed = conveyor.CurrentSpeed;

                if (speed > peakSpeed)
                    peakSpeed = speed;

                if (speed > previousSpeed && previousSpeed >= 0f && speed > 0.01f)
                    sawAcceleration = true;

                if (speed < previousSpeed && previousSpeed > 0.1f)
                    sawDeceleration = true;

                previousSpeed = speed;

                if (!conveyor.IsScrolling)
                    break;
            }

            Assert.IsTrue(sawAcceleration, "Speed should have increased during acceleration phase.");
            Assert.IsTrue(sawDeceleration, "Speed should have decreased during deceleration phase.");
            Assert.That(peakSpeed, Is.GreaterThan(0.5f), "Peak speed should be meaningfully above zero.");

            Assert.That(conveyor.CurrentSpeed, Is.EqualTo(0f).Within(0.01f),
                "Speed should be zero after scroll completes.");
        }

        [UnityTest]
        public IEnumerator ResetPosition_AfterScroll_ReturnsToInitialPosition()
        {
            var go = Track(TestCharacterFactory.CreateConveyor("TestConveyor"));
            var conveyor = go.GetComponent<WorldConveyor>();

            yield return null;

            float initialX = go.transform.position.x;

            conveyor.ScrollBy(5f, 10f, 20f);

            yield return new WaitForSeconds(ScrollTimeout);

            Assert.That(go.transform.position.x, Is.Not.EqualTo(initialX).Within(0.01f),
                "Sanity: conveyor should have moved after scroll.");

            conveyor.ResetPosition();

            Assert.IsFalse(conveyor.IsScrolling,
                "IsScrolling should be false after ResetPosition.");
            Assert.That(go.transform.position.x, Is.EqualTo(initialX).Within(0.01f),
                "Conveyor X position should return to initial position after ResetPosition.");
            Assert.That(conveyor.CurrentSpeed, Is.EqualTo(0f).Within(0.01f),
                "CurrentSpeed should be zero after ResetPosition.");
        }

        [UnityTest]
        public IEnumerator ScrollBy_ZeroDistance_DoesNotScroll()
        {
            var go = Track(TestCharacterFactory.CreateConveyor("TestConveyor"));
            var conveyor = go.GetComponent<WorldConveyor>();

            yield return null;

            float startX = go.transform.position.x;

            LogAssert.Expect(LogType.Warning, new Regex("distance must be > 0"));

            conveyor.ScrollBy(0f, 10f, 5f);

            Assert.IsFalse(conveyor.IsScrolling, "IsScrolling should be false when distance is zero.");

            yield return new WaitForFixedUpdate();

            float endX = go.transform.position.x;
            Assert.That(endX, Is.EqualTo(startX).Within(0.01f),
                "Position should not change when distance is zero.");
        }
    }
}
