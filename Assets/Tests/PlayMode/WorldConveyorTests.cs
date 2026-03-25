using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RogueliteAutoBattler.Combat;
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

            // Wait one frame so Awake runs and Rigidbody2D is configured.
            yield return null;

            float distance = 5f;
            float maxSpeed = 10f;
            float acceleration = 20f;

            conveyor.ScrollBy(distance, maxSpeed, acceleration);

            // Wait enough time for the scroll to complete.
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

            // Use a longer distance with moderate acceleration to ensure deceleration phase occurs.
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

            // Use moderate values so the profile has clear acceleration and deceleration phases.
            conveyor.ScrollBy(8f, 4f, 2f);

            float peakSpeed = 0f;
            bool sawAcceleration = false;
            bool sawDeceleration = false;
            float previousSpeed = 0f;

            // Sample speed over time.
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

            // After completion, speed should be zero.
            Assert.That(conveyor.CurrentSpeed, Is.EqualTo(0f).Within(0.01f),
                "Speed should be zero after scroll completes.");
        }

        [UnityTest]
        public IEnumerator ScrollBy_ZeroDistance_DoesNotScroll()
        {
            var go = Track(TestCharacterFactory.CreateConveyor("TestConveyor"));
            var conveyor = go.GetComponent<WorldConveyor>();

            yield return null;

            float startX = go.transform.position.x;

            // ScrollBy with distance=0 logs a warning and does nothing.
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
