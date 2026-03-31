using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Tests;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class TargetFinderTests : EditModeTestBase
    {
        private GameObject _container;

        [Test]
        public void Closest_NullContainer_ReturnsNull()
        {
            Transform result = TargetFinder.Closest(null, Vector3.zero);

            Assert.IsNull(result);
        }

        [Test]
        public void Closest_ReturnsNearestAliveTarget()
        {
            _container = Track(new GameObject("Container"));

            CreateChild("Far", new Vector3(10f, 0f, 0f));
            CreateChild("Mid", new Vector3(5f, 0f, 0f));
            var near = CreateChild("Near", new Vector3(2f, 0f, 0f));

            Transform result = TargetFinder.Closest(_container.transform, Vector3.zero);

            Assert.IsNotNull(result);
            Assert.AreEqual(near.transform, result);
        }

        [Test]
        public void Closest_SkipsDeadTargets()
        {
            _container = Track(new GameObject("Container"));

            var nearest = CreateChild("Nearest_Dead", new Vector3(1f, 0f, 0f));
            nearest.GetComponent<CombatStats>().TakeDamage(100);

            var secondNearest = CreateChild("Second", new Vector3(3f, 0f, 0f));
            CreateChild("Farthest", new Vector3(8f, 0f, 0f));

            Transform result = TargetFinder.Closest(_container.transform, Vector3.zero);

            Assert.IsNotNull(result);
            Assert.AreEqual(secondNearest.transform, result);
        }

        [Test]
        public void LowestHp_ReturnsLowestHpTarget()
        {
            _container = Track(new GameObject("Container"));

            CreateChild("HighHp", Vector3.zero);
            var low = CreateChild("LowHp", Vector3.zero);
            low.GetComponent<CombatStats>().TakeDamage(70);

            var mid = CreateChild("MidHp", Vector3.zero);
            mid.GetComponent<CombatStats>().TakeDamage(40);

            Transform result = TargetFinder.LowestHp(_container.transform);

            Assert.IsNotNull(result);
            Assert.AreEqual(low.transform, result);
        }

        [Test]
        public void HighestHp_ReturnsHighestHpTarget()
        {
            _container = Track(new GameObject("Container"));

            var full = CreateChild("FullHp", Vector3.zero);
            var damaged = CreateChild("DamagedHp", Vector3.zero);
            damaged.GetComponent<CombatStats>().TakeDamage(50);

            var lowHp = CreateChild("LowHp", Vector3.zero);
            lowHp.GetComponent<CombatStats>().TakeDamage(80);

            Transform result = TargetFinder.HighestHp(_container.transform);

            Assert.IsNotNull(result);
            Assert.AreEqual(full.transform, result);
        }

        private GameObject CreateChild(string name, Vector3 position, int maxHp = 100, int atk = 10, float attackSpeed = 1f)
        {
            var child = TestCharacterFactory.CreateCombatCharacter(
                name: name,
                maxHp: maxHp,
                atk: atk,
                attackSpeed: attackSpeed,
                position: position);
            child.transform.SetParent(_container.transform);
            return child;
        }
    }
}
