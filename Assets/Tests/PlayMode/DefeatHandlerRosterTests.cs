using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Combat.Environment;
using RogueliteAutoBattler.Combat.Levels;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Tests;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class DefeatHandlerRosterTests : PlayModeTestBase
    {
        private GameObject _hostGo;
        private TeamRoster _teamRoster;
        private Transform _enemiesContainer;
        private Transform _enemiesHomeAnchor;
        private WorldConveyor _conveyor;
        private AllyTargetManager _allyTargetManager;

        private DefeatHandler CreateHandler()
        {
            _hostGo = Track(new GameObject("DefeatHandlerHost"));
            _teamRoster = _hostGo.AddComponent<TeamRoster>();

            var enemiesGo = Track(new GameObject("Enemies"));
            _enemiesContainer = enemiesGo.transform;

            var enemiesHomeGo = Track(new GameObject("EnemiesHome"));
            _enemiesHomeAnchor = enemiesHomeGo.transform;

            var conveyorGo = Track(new GameObject("Conveyor"));
            _conveyor = conveyorGo.AddComponent<WorldConveyor>();

            var teamContainerGo = Track(new GameObject("TeamContainer"));
            _allyTargetManager = new AllyTargetManager(
                teamContainerGo.transform,
                _enemiesContainer,
                () => float.MaxValue);

            return new DefeatHandler(
                _teamRoster,
                _enemiesContainer,
                _enemiesHomeAnchor,
                new WaitForSeconds(0.01f),
                _conveyor,
                () => 1f,
                _allyTargetManager);
        }

        private List<TeamMember> CreateAliveMembers(int count)
        {
            var members = new List<TeamMember>();
            for (int i = 0; i < count; i++)
            {
                var allyGo = Track(TestCharacterFactory.CreateCombatCharacter($"Ally{i}", maxHp: 50));
                var spawnData = new AllySpawnData { AllyName = allyGo.name };
                var member = new TeamMember(i, spawnData)
                {
                    GameObject = allyGo,
                    Stats = allyGo.GetComponent<CombatStats>()
                };
                members.Add(member);
            }
            return members;
        }

        [UnityTest]
        public IEnumerator WireAllyDeathTracking_SubscribesOnceToRosterEvent()
        {
            var handler = CreateHandler();
            var members = CreateAliveMembers(2);
            _teamRoster.InitializeForTest(members);

            handler.WireAllyDeathTracking();
            handler.WireAllyDeathTracking();

            Assert.AreEqual(2, handler.AliveAllyCount,
                "AliveAllyCount should reflect alive members after wiring.");

            LogAssert.Expect(LogType.Log, "[CombatStats] Ally0 died!");
            members[0].Stats.TakeDamage(9999);

            yield return null;

            Assert.AreEqual(1, handler.AliveAllyCount,
                "AliveAllyCount should decrement exactly once even after double-wiring.");
        }

        [UnityTest]
        public IEnumerator OnAllyDied_DecrementsAliveAllyCount()
        {
            var handler = CreateHandler();
            var members = CreateAliveMembers(3);
            _teamRoster.InitializeForTest(members);

            handler.WireAllyDeathTracking();

            Assert.AreEqual(3, handler.AliveAllyCount);

            LogAssert.Expect(LogType.Log, "[CombatStats] Ally1 died!");
            members[1].Stats.TakeDamage(9999);

            yield return null;

            Assert.AreEqual(2, handler.AliveAllyCount,
                "AliveAllyCount should be 2 after killing 1 of 3 members.");
        }

        [UnityTest]
        public IEnumerator OnAllyDied_ZeroAllies_InvokesDefeatCallback()
        {
            var handler = CreateHandler();
            var members = CreateAliveMembers(3);
            _teamRoster.InitializeForTest(members);

            int callbackCount = 0;
            handler.OnAllAlliesDead += () => callbackCount++;

            handler.WireAllyDeathTracking();

            LogAssert.Expect(LogType.Log, "[CombatStats] Ally0 died!");
            members[0].Stats.TakeDamage(9999);
            Assert.AreEqual(0, callbackCount,
                "Callback should not fire while allies remain.");

            LogAssert.Expect(LogType.Log, "[CombatStats] Ally1 died!");
            members[1].Stats.TakeDamage(9999);
            Assert.AreEqual(0, callbackCount,
                "Callback should not fire while allies remain.");

            LogAssert.Expect(LogType.Log, "[CombatStats] Ally2 died!");
            members[2].Stats.TakeDamage(9999);

            yield return null;

            Assert.AreEqual(1, callbackCount,
                "OnAllAlliesDead callback should fire exactly once when the last ally dies.");
        }
    }
}
