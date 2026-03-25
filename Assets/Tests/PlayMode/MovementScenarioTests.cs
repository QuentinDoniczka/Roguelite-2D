using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.Combat;
using RogueliteAutoBattler.Tests;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class MovementScenarioTests : PlayModeTestBase
    {
        private const float WalkTimeout = 0.8f;
        private const float ChargeTimeout = 0.5f;
        private const float ReturnTimeout = 1f;
        private const float ScrollTimeout = 2f;

        [UnityTest]
        public IEnumerator FullMovementCycle_WalkChargeReturnScroll()
        {
            // --- Phase 1: Walk to home anchor ---

            var conveyorGo = Track(TestCharacterFactory.CreateConveyor("ScenarioConveyor"));
            var charGo = Track(TestCharacterFactory.CreateMoverCharacter(
                name: "ScenarioChar",
                moveSpeed: 3f,
                parent: conveyorGo.transform,
                position: new Vector2(2f, 0f)));
            var anchor = Track(TestCharacterFactory.CreateAnchor("HomeAnchor", new Vector2(0f, 0f)));

            var mover = charGo.GetComponent<CharacterMover>();
            var conveyor = conveyorGo.GetComponent<WorldConveyor>();

            // Wait one frame so Awake runs.
            yield return null;

            mover.HomeAnchor = anchor.transform;
            mover.Target = null;

            float phase1StartX = charGo.transform.position.x;

            yield return new WaitForSeconds(WalkTimeout);

            float phase1EndX = charGo.transform.position.x;

            Assert.That(phase1EndX, Is.LessThan(phase1StartX),
                "Phase 1: Character should have moved toward home anchor (leftward).");

            // --- Phase 2: Charge toward target ---

            var targetGo = Track(TestCharacterFactory.CreateAnchor("Target", new Vector2(5f, 0f)));
            mover.Target = targetGo.transform;

            float phase2StartX = charGo.transform.position.x;

            yield return new WaitForSeconds(ChargeTimeout);

            float phase2EndX = charGo.transform.position.x;

            Assert.That(phase2EndX, Is.GreaterThan(phase2StartX),
                "Phase 2: Character should have moved toward target (rightward).");

            // --- Phase 3: Return after target removed ---

            mover.Target = null;

            float phase3StartX = charGo.transform.position.x;

            yield return new WaitForSeconds(ReturnTimeout);

            float phase3EndX = charGo.transform.position.x;

            Assert.That(phase3EndX, Is.LessThan(phase3StartX),
                "Phase 3: Character should have returned toward home anchor (leftward).");

            // --- Phase 4: Follow conveyor scroll ---

            float phase4StartX = charGo.transform.position.x;

            conveyor.ScrollBy(4f, 8f, 16f);

            yield return new WaitForSeconds(ScrollTimeout);

            float phase4EndX = charGo.transform.position.x;

            Assert.That(phase4EndX, Is.LessThan(phase4StartX),
                "Phase 4: Character world X should have shifted left with conveyor scroll.");
        }
    }
}
