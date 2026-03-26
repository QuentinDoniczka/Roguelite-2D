using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.Combat;
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

        /// <summary>
        /// Creates a LevelManager on a fresh GameObject with WorldConveyor (which auto-adds Rigidbody2D).
        /// Also creates Team and Enemies container GameObjects.
        /// Must yield a frame after calling this so Awake runs on all components.
        /// </summary>
        private void CreateLevelManagerSetup()
        {
            _levelManagerGo = new GameObject("TestLevelManager");

            // WorldConveyor has [RequireComponent(typeof(Rigidbody2D))], auto-adds it.
            _levelManagerGo.AddComponent<WorldConveyor>();

            // LevelManager has [RequireComponent(typeof(WorldConveyor))], already satisfied.
            _levelManager = _levelManagerGo.AddComponent<LevelManager>();

            Track(_levelManagerGo);

            _teamContainer = Track(new GameObject("Team"));
            _enemiesContainer = Track(new GameObject("Enemies"));
        }

        // ---------------------------------------------------------------
        // Test 1: WireAllyDeathTracking correctly counts alive allies
        // ---------------------------------------------------------------

        [UnityTest]
        public IEnumerator WireAllyDeathTracking_CountsAliveAllies()
        {
            // Arrange
            CreateLevelManagerSetup();

            var ally1 = Track(TestCharacterFactory.CreateCombatCharacter("Ally1", maxHp: 100));
            var ally2 = Track(TestCharacterFactory.CreateCombatCharacter("Ally2", maxHp: 100));
            var ally3 = Track(TestCharacterFactory.CreateCombatCharacter("Ally3", maxHp: 100));
            ally1.transform.SetParent(_teamContainer.transform);
            ally2.transform.SetParent(_teamContainer.transform);
            ally3.transform.SetParent(_teamContainer.transform);

            // Wait a frame so Awake runs on all components.
            yield return null;

            _levelManager.InitializeForTest(_teamContainer.transform, _enemiesContainer.transform);
            _levelManager.WireAllyDeathTrackingForTest();

            // Assert
            Assert.AreEqual(3, _levelManager.AliveAllyCount,
                "AliveAllyCount should be 3 after wiring 3 alive allies.");
        }

        // ---------------------------------------------------------------
        // Test 2: All allies dying sets LevelInProgress to false
        // ---------------------------------------------------------------

        [UnityTest]
        public IEnumerator AllAlliesDie_LevelInProgressBecomesFalse()
        {
            // Arrange
            CreateLevelManagerSetup();

            var ally1 = Track(TestCharacterFactory.CreateCombatCharacter("Ally1", maxHp: 50));
            var ally2 = Track(TestCharacterFactory.CreateCombatCharacter("Ally2", maxHp: 50));
            ally1.transform.SetParent(_teamContainer.transform);
            ally2.transform.SetParent(_teamContainer.transform);

            yield return null;

            _levelManager.InitializeForTest(_teamContainer.transform, _enemiesContainer.transform);
            _levelManager.WireAllyDeathTrackingForTest();

            Assert.IsTrue(_levelManager.LevelInProgress, "Sanity: LevelInProgress should be true initially.");

            // Act -- kill both allies.
            LogAssert.Expect(LogType.Log, "[CombatStats] Ally1 died!");
            ally1.GetComponent<CombatStats>().TakeDamage(9999);

            LogAssert.Expect(LogType.Log, "[CombatStats] Ally2 died!");
            LogAssert.Expect(LogType.Log, "[LevelManager] Level lost! All allies defeated.");
            ally2.GetComponent<CombatStats>().TakeDamage(9999);

            yield return null;

            // Assert
            Assert.IsFalse(_levelManager.LevelInProgress,
                "LevelInProgress should be false after all allies die.");
        }

        // ---------------------------------------------------------------
        // Test 3: All allies dying clears enemy targets
        // ---------------------------------------------------------------

        [UnityTest]
        public IEnumerator AllAlliesDie_ClearsEnemyTargets()
        {
            // Arrange -- 2 allies and 2 enemies with targets pointing at allies.
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

            // Wait a frame so Awake runs (CombatController caches CharacterMover etc.).
            yield return null;

            // Give enemies targets pointing at allies.
            var enemyCtrl1 = enemy1.GetComponent<CombatController>();
            var enemyCtrl2 = enemy2.GetComponent<CombatController>();
            enemyCtrl1.Target = ally1.transform;
            enemyCtrl2.Target = ally2.transform;

            _levelManager.InitializeForTest(_teamContainer.transform, _enemiesContainer.transform);
            _levelManager.WireAllyDeathTrackingForTest();

            // Act -- kill all allies.
            LogAssert.Expect(LogType.Log, "[CombatStats] Ally1 died!");
            ally1.GetComponent<CombatStats>().TakeDamage(9999);

            LogAssert.Expect(LogType.Log, "[CombatStats] Ally2 died!");
            LogAssert.Expect(LogType.Log, "[LevelManager] Level lost! All allies defeated.");
            ally2.GetComponent<CombatStats>().TakeDamage(9999);

            yield return null;

            // Assert -- enemies should have had their targets cleared.
            Assert.IsNull(enemyCtrl1.Target,
                "Enemy1 target should be null after all allies died.");
            Assert.IsNull(enemyCtrl2.Target,
                "Enemy2 target should be null after all allies died.");
        }

        // ---------------------------------------------------------------
        // Test 4: Partial ally death keeps LevelInProgress true
        // ---------------------------------------------------------------

        [UnityTest]
        public IEnumerator PartialAllyDeath_LevelStillInProgress()
        {
            // Arrange
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

            // Act -- kill 2 out of 3 allies.
            LogAssert.Expect(LogType.Log, "[CombatStats] Ally1 died!");
            ally1.GetComponent<CombatStats>().TakeDamage(9999);

            LogAssert.Expect(LogType.Log, "[CombatStats] Ally2 died!");
            ally2.GetComponent<CombatStats>().TakeDamage(9999);

            yield return null;

            // Assert -- level still in progress, 1 ally alive.
            Assert.IsTrue(_levelManager.LevelInProgress,
                "LevelInProgress should remain true when 1 ally is still alive.");
            Assert.AreEqual(1, _levelManager.AliveAllyCount,
                "AliveAllyCount should be 1 after 2 of 3 allies die.");
        }

        // ---------------------------------------------------------------
        // Test 5: After all allies die, enemies return toward home anchor
        // ---------------------------------------------------------------

        [UnityTest]
        public IEnumerator AllAlliesDie_EnemiesReturnTowardAnchor()
        {
            // Arrange -- 1 ally and 2 enemies positioned away from their home anchor.
            CreateLevelManagerSetup();

            var ally1 = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "Ally1", maxHp: 50, position: new Vector2(-3f, 0f)));
            ally1.transform.SetParent(_teamContainer.transform);

            // Home anchor at X=5. Enemies start at X=0 (far from home).
            var homeAnchor = Track(TestCharacterFactory.CreateAnchor("EnemyHome", new Vector2(5f, 0f)));

            var enemy1 = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "Enemy1", maxHp: 100, moveSpeed: 3f, position: new Vector2(0f, 0f)));
            var enemy2 = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "Enemy2", maxHp: 100, moveSpeed: 3f, position: new Vector2(0f, 1f)));
            enemy1.transform.SetParent(_enemiesContainer.transform);
            enemy2.transform.SetParent(_enemiesContainer.transform);

            // Wait a frame so Awake runs on all components.
            yield return null;

            // Set home anchor on enemies' CharacterMover so they know where to return.
            enemy1.GetComponent<CharacterMover>().HomeAnchor = homeAnchor.transform;
            enemy2.GetComponent<CharacterMover>().HomeAnchor = homeAnchor.transform;

            // Give enemies a target so they are in combat.
            var enemyCtrl1 = enemy1.GetComponent<CombatController>();
            var enemyCtrl2 = enemy2.GetComponent<CombatController>();
            enemyCtrl1.Target = ally1.transform;
            enemyCtrl2.Target = ally1.transform;

            _levelManager.InitializeForTest(_teamContainer.transform, _enemiesContainer.transform);
            _levelManager.WireAllyDeathTrackingForTest();

            // Record initial positions before the ally dies.
            float enemy1StartX = enemy1.transform.position.x;
            float enemy2StartX = enemy2.transform.position.x;

            // Act -- kill the ally. This triggers ClearEnemyTargets -> Disengage -> walk to home.
            LogAssert.Expect(LogType.Log, "[CombatStats] Ally1 died!");
            LogAssert.Expect(LogType.Log, "[LevelManager] Level lost! All allies defeated.");
            ally1.GetComponent<CombatStats>().TakeDamage(9999);

            // Wait several FixedUpdate frames for physics-based movement toward home anchor.
            for (int i = 0; i < 20; i++)
                yield return new WaitForFixedUpdate();

            // Assert -- enemies should have moved closer to home anchor (X=5) from X=0.
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

        // ---------------------------------------------------------------
        // Test 6: ClearEnemyTargets disengages all enemies
        // ---------------------------------------------------------------

        [UnityTest]
        public IEnumerator ClearEnemyTargets_DisengagesAllEnemies()
        {
            // Arrange -- 2 enemies with targets set.
            CreateLevelManagerSetup();

            var dummyTarget = Track(TestCharacterFactory.CreateCombatCharacter(
                "DummyTarget", maxHp: 100, position: new Vector2(-2f, 0f)));

            var enemy1 = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "Enemy1", maxHp: 100, position: new Vector2(2f, 0f)));
            var enemy2 = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "Enemy2", maxHp: 100, position: new Vector2(2f, 1f)));
            enemy1.transform.SetParent(_enemiesContainer.transform);
            enemy2.transform.SetParent(_enemiesContainer.transform);

            // Wait a frame so Awake runs.
            yield return null;

            var enemyCtrl1 = enemy1.GetComponent<CombatController>();
            var enemyCtrl2 = enemy2.GetComponent<CombatController>();
            enemyCtrl1.Target = dummyTarget.transform;
            enemyCtrl2.Target = dummyTarget.transform;

            _levelManager.InitializeForTest(_teamContainer.transform, _enemiesContainer.transform);

            // Sanity -- targets are set.
            Assert.IsNotNull(enemyCtrl1.Target, "Sanity: Enemy1 should have a target before clear.");
            Assert.IsNotNull(enemyCtrl2.Target, "Sanity: Enemy2 should have a target before clear.");

            // Act
            _levelManager.ClearEnemyTargetsForTest();

            yield return null;

            // Assert -- both enemies should have null target and Moving state.
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
