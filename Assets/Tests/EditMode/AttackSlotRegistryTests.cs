using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Tests.PlayMode;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class AttackSlotRegistryTests : PlayModeTestBase
    {
        [SetUp]
        public void SetUp()
        {
            AttackSlotRegistry.Clear();
        }

        [Test]
        public void Acquire_FirstAttacker_ReturnsFrontOffset()
        {
            var target = Track(new GameObject("Target")).transform;
            var attacker = Track(new GameObject("Attacker")).transform;

            Vector2 offset = AttackSlotRegistry.Acquire(target, attacker, attackerFacesRight: true);

            Assert.AreEqual(-AttackSlotRegistry.FaceOffset, offset.x, 0.001f);
            Assert.AreEqual(0f, offset.y, 0.001f);
        }

        [Test]
        public void Acquire_SameAttackerTwice_ReturnsSameSlot()
        {
            var target = Track(new GameObject("Target")).transform;
            var attacker = Track(new GameObject("Attacker")).transform;

            Vector2 first = AttackSlotRegistry.Acquire(target, attacker, attackerFacesRight: true);
            Vector2 second = AttackSlotRegistry.Acquire(target, attacker, attackerFacesRight: true);

            Assert.AreEqual(first, second);
            Assert.AreEqual(1, AttackSlotRegistry.AttackerCount(target));
        }

        [Test]
        public void Acquire_TwoAttackers_DifferentSlots()
        {
            var target = Track(new GameObject("Target")).transform;
            var attackerA = Track(new GameObject("AttackerA")).transform;
            var attackerB = Track(new GameObject("AttackerB")).transform;

            Vector2 offsetA = AttackSlotRegistry.Acquire(target, attackerA, attackerFacesRight: true);
            Vector2 offsetB = AttackSlotRegistry.Acquire(target, attackerB, attackerFacesRight: true);

            Assert.AreEqual(0f, offsetA.y, 0.001f);
            Assert.AreEqual(AttackSlotRegistry.VerticalSpacing, offsetB.y, 0.001f);
            Assert.AreNotEqual(offsetA, offsetB);
        }

        [Test]
        public void Release_RemovesAttacker_CountDecreases()
        {
            var target = Track(new GameObject("Target")).transform;
            var attackerA = Track(new GameObject("AttackerA")).transform;
            var attackerB = Track(new GameObject("AttackerB")).transform;

            AttackSlotRegistry.Acquire(target, attackerA, attackerFacesRight: true);
            AttackSlotRegistry.Acquire(target, attackerB, attackerFacesRight: true);
            Assert.AreEqual(2, AttackSlotRegistry.AttackerCount(target));

            AttackSlotRegistry.Release(target, attackerA);

            Assert.AreEqual(1, AttackSlotRegistry.AttackerCount(target));
        }

        [Test]
        public void ReleaseAll_ClearsAllAttackers()
        {
            var target = Track(new GameObject("Target")).transform;
            var attackerA = Track(new GameObject("AttackerA")).transform;
            var attackerB = Track(new GameObject("AttackerB")).transform;

            AttackSlotRegistry.Acquire(target, attackerA, attackerFacesRight: true);
            AttackSlotRegistry.Acquire(target, attackerB, attackerFacesRight: true);

            AttackSlotRegistry.ReleaseAll(target);

            Assert.AreEqual(0, AttackSlotRegistry.AttackerCount(target));
        }

        [Test]
        public void Clear_ResetsEverything()
        {
            var targetA = Track(new GameObject("TargetA")).transform;
            var targetB = Track(new GameObject("TargetB")).transform;
            var attacker = Track(new GameObject("Attacker")).transform;

            AttackSlotRegistry.Acquire(targetA, attacker, attackerFacesRight: true);
            AttackSlotRegistry.Acquire(targetB, attacker, attackerFacesRight: false);

            AttackSlotRegistry.Clear();

            Assert.AreEqual(0, AttackSlotRegistry.AttackerCount(targetA));
            Assert.AreEqual(0, AttackSlotRegistry.AttackerCount(targetB));
        }

        [Test]
        public void Acquire_OverflowBeyondMaxFrontSlots_GoesToBackRow()
        {
            var target = Track(new GameObject("Target")).transform;
            var frontAttackers = new Transform[5];
            for (int i = 0; i < 5; i++)
            {
                var go = Track(new GameObject("FrontAttacker" + i));
                frontAttackers[i] = go.transform;
                AttackSlotRegistry.Acquire(target, frontAttackers[i], attackerFacesRight: true);
            }

            var overflowAttacker = Track(new GameObject("OverflowAttacker")).transform;
            Vector2 overflowOffset = AttackSlotRegistry.Acquire(target, overflowAttacker, attackerFacesRight: true);

            Assert.AreEqual(AttackSlotRegistry.FaceOffset, overflowOffset.x, 0.001f);
            Assert.AreEqual(0f, overflowOffset.y, 0.001f);
        }

        [Test]
        public void AttackerCount_IgnoresDestroyedTransforms()
        {
            var target = Track(new GameObject("Target")).transform;
            var attackerA = Track(new GameObject("AttackerA")).transform;
            var attackerB = Track(new GameObject("AttackerB")).transform;

            AttackSlotRegistry.Acquire(target, attackerA, attackerFacesRight: true);
            AttackSlotRegistry.Acquire(target, attackerB, attackerFacesRight: true);

            Object.DestroyImmediate(attackerA.gameObject);

            Assert.AreEqual(1, AttackSlotRegistry.AttackerCount(target));
        }
    }
}
