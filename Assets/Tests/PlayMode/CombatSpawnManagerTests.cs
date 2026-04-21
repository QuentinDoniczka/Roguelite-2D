using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Tests;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class CombatSpawnManagerTests : PlayModeTestBase
    {
        private CombatSpawnManager _spawnManager;
        private TeamRoster _teamRoster;
        private Transform _teamContainer;
        private Transform _teamHomeAnchor;
        private GameObject _allyPrefab;

        [SetUp]
        public void SetUp()
        {
            var managerGo = Track(new GameObject("SpawnManager"));
            _teamRoster = managerGo.AddComponent<TeamRoster>();
            _spawnManager = managerGo.AddComponent<CombatSpawnManager>();
            _spawnManager.enabled = false;

            var containerGo = Track(new GameObject("TeamContainer"));
            _teamContainer = containerGo.transform;

            var anchorGo = Track(new GameObject("TeamHomeAnchor"));
            _teamHomeAnchor = anchorGo.transform;

            _allyPrefab = Track(TestCharacterFactory.CreateCharacterPrefab());
        }

        private void ExpectSpawnWarnings(int allyCount)
        {
            LogAssert.Expect(LogType.Warning, new Regex("Enemies.*container not found"));
            for (int i = 0; i < allyCount; i++)
            {
                LogAssert.Expect(LogType.Warning, new Regex("No Animator found"));
            }
        }

        [UnityTest]
        public IEnumerator SpawnAllies_CreatesCorrectNumberOfAllies()
        {
            var teamDb = TestCharacterFactory.CreateTeamDatabase(3, _allyPrefab);
            _spawnManager.InitializeForTest(teamDb, _teamContainer, _teamHomeAnchor, _teamRoster);

            ExpectSpawnWarnings(3);
            _spawnManager.SpawnAllies();

            yield return null;

            Assert.AreEqual(3, _teamContainer.childCount);
        }

        [UnityTest]
        public IEnumerator RespawnAllies_ReusesSameGameObjectInstances()
        {
            var teamDb = TestCharacterFactory.CreateTeamDatabase(3, _allyPrefab);
            _spawnManager.InitializeForTest(teamDb, _teamContainer, _teamHomeAnchor, _teamRoster);

            ExpectSpawnWarnings(3);
            _spawnManager.SpawnAllies();
            yield return null;

            var capturedGameObjects = new List<GameObject>();
            for (int i = 0; i < _teamContainer.childCount; i++)
                capturedGameObjects.Add(_teamContainer.GetChild(i).gameObject);

            _spawnManager.RespawnAllies();
            yield return null;

            Assert.AreEqual(3, _teamContainer.childCount);
            for (int i = 0; i < capturedGameObjects.Count; i++)
            {
                Assert.IsNotNull(capturedGameObjects[i], $"Ally {i} GameObject should not have been destroyed by RespawnAllies.");
                Assert.IsTrue(capturedGameObjects[i].activeSelf, $"Ally {i} GameObject should still be active after RespawnAllies.");
                Assert.AreEqual(_teamContainer, capturedGameObjects[i].transform.parent, $"Ally {i} should remain under team container.");
            }
        }

        [UnityTest]
        public IEnumerator RespawnAllies_AllAlliesHaveFullHp()
        {
            var teamDb = TestCharacterFactory.CreateTeamDatabase(3, _allyPrefab);
            _spawnManager.InitializeForTest(teamDb, _teamContainer, _teamHomeAnchor, _teamRoster);

            ExpectSpawnWarnings(3);
            _spawnManager.SpawnAllies();
            yield return null;

            _spawnManager.RespawnAllies();
            yield return null;

            for (int i = 0; i < _teamContainer.childCount; i++)
            {
                var stats = _teamContainer.GetChild(i).GetComponent<CombatStats>();
                Assert.IsNotNull(stats, $"Ally {i} should have CombatStats");
                Assert.AreEqual(stats.MaxHp, stats.CurrentHp, $"Ally {i} should have full HP");
            }
        }

        [UnityTest]
        public IEnumerator SpawnAllies_DelegatesToTeamRoster()
        {
            const int allyCount = 3;
            var teamDb = TestCharacterFactory.CreateTeamDatabase(allyCount, _allyPrefab);
            _spawnManager.InitializeForTest(teamDb, _teamContainer, _teamHomeAnchor, _teamRoster);

            ExpectSpawnWarnings(allyCount);
            _spawnManager.SpawnAllies();

            yield return null;

            Assert.AreEqual(allyCount, _teamRoster.Members.Count);
        }

        [UnityTest]
        public IEnumerator RespawnAllies_CallsReviveAll_NotDestroy()
        {
            const int allyCount = 3;
            var teamDb = TestCharacterFactory.CreateTeamDatabase(allyCount, _allyPrefab);
            _spawnManager.InitializeForTest(teamDb, _teamContainer, _teamHomeAnchor, _teamRoster);

            ExpectSpawnWarnings(allyCount);
            _spawnManager.SpawnAllies();
            yield return null;

            var capturedGameObjects = new List<GameObject>();
            foreach (var member in _teamRoster.Members)
            {
                capturedGameObjects.Add(member.GameObject);
                member.Stats.TakeDamage(99999);
            }

            yield return new WaitForSeconds(0.5f);

            _spawnManager.RespawnAllies();
            yield return null;

            Assert.AreEqual(allyCount, _teamRoster.Members.Count);
            for (int i = 0; i < capturedGameObjects.Count; i++)
            {
                Assert.IsNotNull(capturedGameObjects[i], $"Ally {i} GameObject should not have been destroyed.");
                Assert.AreSame(capturedGameObjects[i], _teamRoster.Members[i].GameObject,
                    $"Ally {i} should remain the exact same GameObject instance after RespawnAllies.");
            }
        }
    }
}
