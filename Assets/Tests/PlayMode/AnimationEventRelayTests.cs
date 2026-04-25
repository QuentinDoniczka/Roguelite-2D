using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class AnimationEventRelayTests : PlayModeTestBase
    {
        [UnityTest]
        public IEnumerator OnAttackHit_WithController_DoesNotThrow()
        {
            var characterGo = Track(TestCharacterFactory.CreateFullCombatCharacter("RelayTestChar"));

            yield return null;

            var controller = characterGo.GetComponent<CombatController>();
            var relayGo = Track(new GameObject("RelayHolder"));
            var relay = relayGo.AddComponent<AnimationEventRelay>();
            relay.Initialize(controller);

            Assert.DoesNotThrow(() => relay.OnAttackHit(),
                "OnAttackHit should not throw when initialized with a valid controller.");
        }

        [UnityTest]
        public IEnumerator OnAttackHit_WithoutInitialize_DoesNotThrow()
        {
            var relayGo = Track(new GameObject("UninitializedRelay"));
            var relay = relayGo.AddComponent<AnimationEventRelay>();

            yield return null;

            Assert.DoesNotThrow(() => relay.OnAttackHit(),
                "OnAttackHit should not throw when controller has not been initialized (null check).");
        }

        [UnityTest]
        public IEnumerator Initialize_SetsController_WithoutException()
        {
            var characterGo = Track(TestCharacterFactory.CreateFullCombatCharacter("InitTestChar"));

            yield return null;

            var controller = characterGo.GetComponent<CombatController>();
            var relayGo = Track(new GameObject("InitRelay"));
            var relay = relayGo.AddComponent<AnimationEventRelay>();

            Assert.DoesNotThrow(() => relay.Initialize(controller),
                "Initialize should not throw when given a valid CombatController.");
        }
    }
}
