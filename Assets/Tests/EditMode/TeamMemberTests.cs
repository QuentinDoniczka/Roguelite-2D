using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Tests.PlayMode;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class TeamMemberTests : PlayModeTestBase
    {
        private AllySpawnData _spawnData;

        [SetUp]
        public void SetUp()
        {
            _spawnData = new AllySpawnData();
        }

        [Test]
        public void Constructor_SetsIndexAndSpawnData()
        {
            var member = new TeamMember(3, _spawnData);

            Assert.AreEqual(3, member.Index);
            Assert.AreSame(_spawnData, member.SpawnData);
            Assert.IsNull(member.GameObject);
            Assert.IsNull(member.Stats);
        }

        [Test]
        public void IsDead_ReturnsFalse_WhenStatsNull()
        {
            var member = new TeamMember(0, _spawnData);

            Assert.IsFalse(member.IsDead);
        }

        [Test]
        public void IsDead_ReturnsFalse_WhenStatsAliveHpAboveZero()
        {
            var go = Track(new GameObject("AliveUnit"));
            var stats = go.AddComponent<CombatStats>();
            stats.InitializeDirect(maxHp: 100, atk: 10, attackSpeed: 1f);

            var member = new TeamMember(1, _spawnData)
            {
                GameObject = go,
                Stats = stats
            };

            Assert.IsFalse(member.IsDead);
        }

        [Test]
        public void IsDead_ReturnsTrue_WhenStatsAtZeroHp()
        {
            var go = Track(new GameObject("DyingUnit"));
            var stats = go.AddComponent<CombatStats>();
            stats.InitializeDirect(maxHp: 100, atk: 10, attackSpeed: 1f);
            stats.TakeDamage(99999);

            var member = new TeamMember(2, _spawnData)
            {
                GameObject = go,
                Stats = stats
            };

            Assert.IsTrue(member.IsDead);
            Assert.AreEqual(0, stats.CurrentHp);
        }
    }
}
