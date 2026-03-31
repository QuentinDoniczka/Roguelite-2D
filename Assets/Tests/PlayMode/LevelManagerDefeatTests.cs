using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Combat.Environment;
using RogueliteAutoBattler.Combat.Levels;
using RogueliteAutoBattler.Tests;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class LevelManagerDefeatTests : PlayModeTestBase
    {
        private GameObject _levelManagerGo;
        private LevelManager _levelManager;
        private GameObject _teamContainer;
        private GameObject _enemiesContainer;

        private void CreateLevelManagerSetup()
        {
            _levelManagerGo = new GameObject("TestLevelManager");
            _levelManagerGo.AddComponent<WorldConveyor>();
            _levelManager = _levelManagerGo.AddComponent<LevelManager>();
            _levelManager.enabled = false;
            Track(_levelManagerGo);

            _teamContainer = Track(new GameObject("Team"));
            _enemiesContainer = Track(new GameObject("Enemies"));
        }

        [UnityTest]
        public IEnumerator WireAllyDeathTracking_CountsAliveAllies()
        {
            CreateLevelManagerSetup();

            var ally1 = Track(TestCharacterFactory.CreateCombatCharacter("Ally1", maxHp: 100));
            var ally2 = Track(TestCharacterFactory.CreateCombatCharacter("Ally2", maxHp: 100));
            var ally3 = Track(TestCharacterFactory.CreateCombatCharacter("Ally3", maxHp: 100));
            ally1.transform.SetParent(_teamContainer.transform);
            ally2.transform.SetParent(_teamContainer.transform);
            ally3.transform.SetParent(_teamContainer.transform);

            yield return null;

            _levelManager.InitializeForTest(_teamContainer.transform, _enemiesContainer.transform);
            _levelManager.WireAllyDeathTrackingForTest();

            Assert.AreEqual(3, _levelManager.AliveAllyCount,
                "AliveAllyCount should be 3 after wiring 3 alive allies.");
        }

        [UnityTest]
        public IEnumerator AllAlliesDie_LevelInProgressBecomesFalse()
        {
            CreateLevelManagerSetup();

            var ally1 = Track(TestCharacterFactory.CreateCombatCharacter("Ally1", maxHp: 50));
            var ally2 = Track(TestCharacterFactory.CreateCombatCharacter("Ally2", maxHp: 50));
            ally1.transform.SetParent(_teamContainer.transform);
            ally2.transform.SetParent(_teamContainer.transform);

            yield return null;

            _levelManager.InitializeForTest(_teamContainer.transform, _enemiesContainer.transform);
            _levelManager.WireAllyDeathTrackingForTest();

            Assert.IsTrue(_levelManager.LevelInProgress, "Sanity: LevelInProgress should be true initially.");

            LogAssert.Expect(LogType.Log, "[CombatStats] Ally1 died!");
            ally1.GetComponent<CombatStats>().TakeDamage(9999);

            LogAssert.Expect(LogType.Log, "[CombatStats] Ally2 died!");
            LogAssert.Expect(LogType.Log, "[LevelManager] Level lost! All allies defeated.");
            ally2.GetComponent<CombatStats>().TakeDamage(9999);

            yield return null;

            Assert.IsFalse(_levelManager.LevelInProgress,
                "LevelInProgress should be false after all allies die.");
        }

        [UnityTest]
        public IEnumerator AllAlliesDie_ClearsEnemyTargets()
        {
            CreateLevelManagerSetup();

            var ally1 = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "Ally1", maxHp: 50, position: new Vector2(-2f, 0f)));
            var ally2 = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "Ally2", maxHp: 50, position: new Vector2(-2f, 1f)));
            ally1.transform.SetParent(_teamContainer.transform);
            ally2.transform.SetParent(_teamContainer.transform);

            var enemy1 = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "Enemy1", maxHp: 100, position: new Vector2(2f, 0f)));
            var enemy2 = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "Enemy2", maxHp: 100, position: new Vector2(2f, 1f)));
            enemy1.transform.SetParent(_enemiesContainer.transform);
            enemy2.transform.SetParent(_enemiesContainer.transform);

            yield return null;

            var enemyCtrl1 = enemy1.GetComponent<CombatController>();
            var enemyCtrl2 = enemy2.GetComponent<CombatController>();
            enemyCtrl1.Target = ally1.transform;
            enemyCtrl2.Target = ally2.transform;

            _levelManager.InitializeForTest(_teamContainer.transform, _enemiesContainer.transform);
            _levelManager.WireAllyDeathTrackingForTest();

            LogAssert.Expect(LogType.Log, "[CombatStats] Ally1 died!");
            ally1.GetComponent<CombatStats>().TakeDamage(9999);

            LogAssert.Expect(LogType.Log, "[CombatStats] Ally2 died!");
            LogAssert.Expect(LogType.Log, "[LevelManager] Level lost! All allies defeated.");
            ally2.GetComponent<CombatStats>().TakeDamage(9999);

            yield return null;

            Assert.IsNull(enemyCtrl1.Target,
                "Enemy1 target should be null after all allies died.");
            Assert.IsNull(enemyCtrl2.Target,
                "Enemy2 target should be null after all allies died.");
        }

        [UnityTest]
        public IEnumerator PartialAllyDeath_LevelStillInProgress()
        {
            CreateLevelManagerSetup();

            var ally1 = Track(TestCharacterFactory.CreateCombatCharacter("Ally1", maxHp: 50));
            var ally2 = Track(TestCharacterFactory.CreateCombatCharacter("Ally2", maxHp: 50));
            var ally3 = Track(TestCharacterFactory.CreateCombatCharacter("Ally3", maxHp: 50));
            ally1.transform.SetParent(_teamContainer.transform);
            ally2.transform.SetParent(_teamContainer.transform);
            ally3.transform.SetParent(_teamContainer.transform);

            var enemy1 = Track(TestCharacterFactory.CreateCombatCharacter("Enemy1", maxHp: 100));
            enemy1.transform.SetParent(_enemiesContainer.transform);

            yield return null;

            _levelManager.InitializeForTest(_teamContainer.transform, _enemiesContainer.transform);
            _levelManager.WireAllyDeathTrackingForTest();

            LogAssert.Expect(LogType.Log, "[CombatStats] Ally1 died!");
            ally1.GetComponent<CombatStats>().TakeDamage(9999);

            LogAssert.Expect(LogType.Log, "[CombatStats] Ally2 died!");
            ally2.GetComponent<CombatStats>().TakeDamage(9999);

            yield return null;

            Assert.IsTrue(_levelManager.LevelInProgress,
                "LevelInProgress should remain true when 1 ally is still alive.");
            Assert.AreEqual(1, _levelManager.AliveAllyCount,
                "AliveAllyCount should be 1 after 2 of 3 allies die.");
        }

        [UnityTest]
        public IEnumerator AllAlliesDie_EnemiesReturnTowardAnchor()
        {
            CreateLevelManagerSetup();

            var ally1 = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "Ally1", maxHp: 50, position: new Vector2(-3f, 0f)));
            ally1.transform.SetParent(_teamContainer.transform);

            var homeAnchor = Track(TestCharacterFactory.CreateAnchor("EnemyHome", new Vector2(5f, 0f)));

            var enemy1 = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "Enemy1", maxHp: 100, moveSpeed: 3f, position: new Vector2(0f, 0f)));
            var enemy2 = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "Enemy2", maxHp: 100, moveSpeed: 3f, position: new Vector2(0f, 1f)));
            enemy1.transform.SetParent(_enemiesContainer.transform);
            enemy2.transform.SetParent(_enemiesContainer.transform);

            yield return null;

            enemy1.GetComponent<CharacterMover>().HomeAnchor = homeAnchor.transform;
            enemy2.GetComponent<CharacterMover>().HomeAnchor = homeAnchor.transform;

            var enemyCtrl1 = enemy1.GetComponent<CombatController>();
            var enemyCtrl2 = enemy2.GetComponent<CombatController>();
            enemyCtrl1.Target = ally1.transform;
            enemyCtrl2.Target = ally1.transform;

            _levelManager.InitializeForTest(_teamContainer.transform, _enemiesContainer.transform);
            _levelManager.WireAllyDeathTrackingForTest();

            float enemy1StartX = enemy1.transform.position.x;
            float enemy2StartX = enemy2.transform.position.x;

            LogAssert.Expect(LogType.Log, "[CombatStats] Ally1 died!");
            LogAssert.Expect(LogType.Log, "[LevelManager] Level lost! All allies defeated.");
            ally1.GetComponent<CombatStats>().TakeDamage(9999);

            for (int i = 0; i < 20; i++)
                yield return new WaitForFixedUpdate();

            float enemy1EndX = enemy1.transform.position.x;
            float enemy2EndX = enemy2.transform.position.x;
            float homeX = homeAnchor.transform.position.x;

            float enemy1StartDist = Mathf.Abs(homeX - enemy1StartX);
            float enemy1EndDist = Mathf.Abs(homeX - enemy1EndX);
            float enemy2StartDist = Mathf.Abs(homeX - enemy2StartX);
            float enemy2EndDist = Mathf.Abs(homeX - enemy2EndX);

            Assert.Less(enemy1EndDist, enemy1StartDist,
                $"Enemy1 should be closer to home anchor. Start dist={enemy1StartDist:F2}, End dist={enemy1EndDist:F2}");
            Assert.Less(enemy2EndDist, enemy2StartDist,
                $"Enemy2 should be closer to home anchor. Start dist={enemy2StartDist:F2}, End dist={enemy2EndDist:F2}");
        }

        [UnityTest]
        public IEnumerator ClearEnemyTargets_DisengagesAllEnemies()
        {
            CreateLevelManagerSetup();

            var dummyTarget = Track(TestCharacterFactory.CreateCombatCharacter(
                "DummyTarget", maxHp: 100, position: new Vector2(-2f, 0f)));

            var enemy1 = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "Enemy1", maxHp: 100, position: new Vector2(2f, 0f)));
            var enemy2 = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "Enemy2", maxHp: 100, position: new Vector2(2f, 1f)));
            enemy1.transform.SetParent(_enemiesContainer.transform);
            enemy2.transform.SetParent(_enemiesContainer.transform);

            yield return null;

            var enemyCtrl1 = enemy1.GetComponent<CombatController>();
            var enemyCtrl2 = enemy2.GetComponent<CombatController>();
            enemyCtrl1.Target = dummyTarget.transform;
            enemyCtrl2.Target = dummyTarget.transform;

            _levelManager.InitializeForTest(_teamContainer.transform, _enemiesContainer.transform);

            Assert.IsNotNull(enemyCtrl1.Target, "Sanity: Enemy1 should have a target before clear.");
            Assert.IsNotNull(enemyCtrl2.Target, "Sanity: Enemy2 should have a target before clear.");

            _levelManager.ClearEnemyTargetsForTest();

            yield return null;

            Assert.IsNull(enemyCtrl1.Target,
                "Enemy1 target should be null after ClearEnemyTargets.");
            Assert.IsNull(enemyCtrl2.Target,
                "Enemy2 target should be null after ClearEnemyTargets.");
            Assert.AreEqual(CombatState.Moving, enemyCtrl1.State,
                "Enemy1 should be in Moving state after Disengage.");
            Assert.AreEqual(CombatState.Moving, enemyCtrl2.State,
                "Enemy2 should be in Moving state after Disengage.");
        }
    }
}
