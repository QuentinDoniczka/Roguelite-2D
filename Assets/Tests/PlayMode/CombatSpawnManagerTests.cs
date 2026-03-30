using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RogueliteAutoBattler.Combat;
using RogueliteAutoBattler.Tests;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class CombatSpawnManagerTests : PlayModeTestBase
    {
        private CombatSpawnManager _spawnManager;
        private Transform _teamContainer;
        private Transform _teamHomeAnchor;
        private GameObject _allyPrefab;

        [SetUp]
        public void SetUp()
        {
            var managerGo = Track(new GameObject("SpawnManager"));
            _spawnManager = managerGo.AddComponent<CombatSpawnManager>();
            _spawnManager.enabled = false;

            var containerGo = Track(new GameObject("TeamContainer"));
            _teamContainer = containerGo.transform;

            var anchorGo = Track(new GameObject("TeamHomeAnchor"));
            _teamHomeAnchor = anchorGo.transform;

            _allyPrefab = Track(TestCharacterFactory.CreateAllyPrefab());
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
            _spawnManager.InitializeForTest(teamDb, _teamContainer, _teamHomeAnchor);

            ExpectSpawnWarnings(3);
            _spawnManager.SpawnAllies();

            yield return null;

            Assert.AreEqual(3, _teamContainer.childCount);
        }

        [UnityTest]
        public IEnumerator RespawnAllies_DestroysOldAndCreatesNew()
        {
            var teamDb = TestCharacterFactory.CreateTeamDatabase(3, _allyPrefab);
            _spawnManager.InitializeForTest(teamDb, _teamContainer, _teamHomeAnchor);

            ExpectSpawnWarnings(3);
            _spawnManager.SpawnAllies();
            yield return null;

            ExpectSpawnWarnings(3);
            _spawnManager.RespawnAllies();
            yield return null;

            Assert.AreEqual(3, _teamContainer.childCount);
        }

        [UnityTest]
        public IEnumerator RespawnAllies_AllAlliesHaveFullHp()
        {
            var teamDb = TestCharacterFactory.CreateTeamDatabase(3, _allyPrefab);
            _spawnManager.InitializeForTest(teamDb, _teamContainer, _teamHomeAnchor);

            ExpectSpawnWarnings(3);
            _spawnManager.RespawnAllies();
            yield return null;

            for (int i = 0; i < _teamContainer.childCount; i++)
            {
                var stats = _teamContainer.GetChild(i).GetComponent<CombatStats>();
                Assert.IsNotNull(stats, $"Ally {i} should have CombatStats");
                Assert.AreEqual(stats.MaxHp, stats.CurrentHp, $"Ally {i} should have full HP");
            }
        }
    }
}
