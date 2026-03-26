using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.Combat;
using RogueliteAutoBattler.Tests;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class FormationRecalculationTests : PlayModeTestBase
    {
        private GameObject _levelManagerGo;
        private LevelManager _levelManager;
        private GameObject _teamContainer;
        private GameObject _enemiesContainer;
        private GameObject _teamHomeAnchor;
        private GameObject _enemiesHomeAnchor;

        /// <summary>
        /// Creates a LevelManager on a fresh GameObject with WorldConveyor.
        /// Also creates Team and Enemies containers, and home anchors.
        /// Disable LevelManager immediately so Start() does not run.
        /// </summary>
        private void CreateLevelManagerSetup(
            Vector2? teamAnchorPos = null,
            Vector2? enemiesAnchorPos = null)
        {
            _levelManagerGo = new GameObject("TestLevelManager");
            _levelManagerGo.AddComponent<WorldConveyor>();
            _levelManager = _levelManagerGo.AddComponent<LevelManager>();
            _levelManager.enabled = false;
            Track(_levelManagerGo);

            _teamContainer = Track(new GameObject("Team"));
            _enemiesContainer = Track(new GameObject("Enemies"));

            _teamHomeAnchor = Track(TestCharacterFactory.CreateAnchor(
                "TeamHomeAnchor", teamAnchorPos ?? Vector2.zero));
            _enemiesHomeAnchor = Track(TestCharacterFactory.CreateAnchor(
                "EnemiesHomeAnchor", enemiesAnchorPos ?? new Vector2(5f, 0f)));
        }

        // ---------------------------------------------------------------
        // Test 1: Allies win, one ally dead, formation recalculated
        // ---------------------------------------------------------------

        [UnityTest]
        public IEnumerator AlliesWin_OneAllyDead_FormationRecalculated()
        {
            // Arrange
            CreateLevelManagerSetup(
                teamAnchorPos: Vector2.zero,
                enemiesAnchorPos: new Vector2(5f, 0f));

            var ally1 = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "Ally1", maxHp: 100, position: new Vector2(-1f, 0.5f)));
            var ally2 = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "Ally2", maxHp: 100, position: new Vector2(-1f, 0f)));
            var ally3 = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "Ally3", maxHp: 100, position: new Vector2(-1f, -0.5f)));

            ally1.transform.SetParent(_teamContainer.transform);
            ally2.transform.SetParent(_teamContainer.transform);
            ally3.transform.SetParent(_teamContainer.transform);

            // Wait a frame so Awake runs on all components.
            yield return null;

            // Set initial home offsets for a 3-unit formation.
            Vector2 teamAnchorPos = (Vector2)_teamHomeAnchor.transform.position;
            Vector2[] initial3Positions = FormationLayout.GetPositions(teamAnchorPos, 3, facingRight: true);

            var mover1 = ally1.GetComponent<CharacterMover>();
            var mover2 = ally2.GetComponent<CharacterMover>();
            var mover3 = ally3.GetComponent<CharacterMover>();

            mover1.HomeAnchor = _teamHomeAnchor.transform;
            mover2.HomeAnchor = _teamHomeAnchor.transform;
            mover3.HomeAnchor = _teamHomeAnchor.transform;

            Vector2 initialOffset1 = initial3Positions[0] - teamAnchorPos;
            Vector2 initialOffset2 = initial3Positions[1] - teamAnchorPos;
            Vector2 initialOffset3 = initial3Positions[2] - teamAnchorPos;

            mover1.SetHomeOffset(initialOffset1);
            mover2.SetHomeOffset(initialOffset2);
            mover3.SetHomeOffset(initialOffset3);

            _levelManager.InitializeForTest(
                _teamContainer.transform, _enemiesContainer.transform,
                _teamHomeAnchor.transform, _enemiesHomeAnchor.transform);
            _levelManager.WireAllyDeathTrackingForTest();

            // Act -- kill ally #2 (the middle one).
            LogAssert.Expect(LogType.Log, "[CombatStats] Ally2 died!");
            ally2.GetComponent<CombatStats>().TakeDamage(9999);

            // Simulate end-of-combat: clear targets and recalculate.
            _levelManager.ClearAllyTargetsForTest();
            _levelManager.RecalculateAllyFormationForTest();

            yield return null;

            // Assert -- compute expected 2-unit formation.
            Vector2[] expected2Positions = FormationLayout.GetPositions(teamAnchorPos, 2, facingRight: true);
            Vector2 expectedOffset0 = expected2Positions[0] - teamAnchorPos;
            Vector2 expectedOffset1 = expected2Positions[1] - teamAnchorPos;

            // Ally1 (alive) should have the first position of the 2-unit formation.
            Assert.AreEqual(expectedOffset0.x, mover1.HomeOffset.x, 0.001f,
                "Ally1 HomeOffset.x should match 2-unit formation position 0.");
            Assert.AreEqual(expectedOffset0.y, mover1.HomeOffset.y, 0.001f,
                "Ally1 HomeOffset.y should match 2-unit formation position 0.");

            // Ally3 (alive) should have the second position of the 2-unit formation.
            Assert.AreEqual(expectedOffset1.x, mover3.HomeOffset.x, 0.001f,
                "Ally3 HomeOffset.x should match 2-unit formation position 1.");
            Assert.AreEqual(expectedOffset1.y, mover3.HomeOffset.y, 0.001f,
                "Ally3 HomeOffset.y should match 2-unit formation position 1.");

            // Ally2 (dead) should still have its original offset (not recalculated).
            Assert.AreEqual(initialOffset2.x, mover2.HomeOffset.x, 0.001f,
                "Dead Ally2 HomeOffset.x should be unchanged.");
            Assert.AreEqual(initialOffset2.y, mover2.HomeOffset.y, 0.001f,
                "Dead Ally2 HomeOffset.y should be unchanged.");
        }

        // ---------------------------------------------------------------
        // Test 2: Enemies win, one enemy dead, formation recalculated
        // ---------------------------------------------------------------

        [UnityTest]
        public IEnumerator EnemiesWin_OneEnemyDead_FormationRecalculated()
        {
            // Arrange
            CreateLevelManagerSetup(
                teamAnchorPos: Vector2.zero,
                enemiesAnchorPos: new Vector2(5f, 0f));

            var enemy1 = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "Enemy1", maxHp: 100, position: new Vector2(6f, 0.5f)));
            var enemy2 = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "Enemy2", maxHp: 100, position: new Vector2(6f, 0f)));
            var enemy3 = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "Enemy3", maxHp: 100, position: new Vector2(6f, -0.5f)));

            enemy1.transform.SetParent(_enemiesContainer.transform);
            enemy2.transform.SetParent(_enemiesContainer.transform);
            enemy3.transform.SetParent(_enemiesContainer.transform);

            // Wait a frame so Awake runs.
            yield return null;

            // Set initial home offsets for a 3-unit enemy formation (facingRight: false).
            Vector2 enemyAnchorPos = (Vector2)_enemiesHomeAnchor.transform.position;
            Vector2[] initial3Positions = FormationLayout.GetPositions(enemyAnchorPos, 3, facingRight: false);

            var mover1 = enemy1.GetComponent<CharacterMover>();
            var mover2 = enemy2.GetComponent<CharacterMover>();
            var mover3 = enemy3.GetComponent<CharacterMover>();

            mover1.HomeAnchor = _enemiesHomeAnchor.transform;
            mover2.HomeAnchor = _enemiesHomeAnchor.transform;
            mover3.HomeAnchor = _enemiesHomeAnchor.transform;

            Vector2 initialOffset1 = initial3Positions[0] - enemyAnchorPos;
            Vector2 initialOffset2 = initial3Positions[1] - enemyAnchorPos;
            Vector2 initialOffset3 = initial3Positions[2] - enemyAnchorPos;

            mover1.SetHomeOffset(initialOffset1);
            mover2.SetHomeOffset(initialOffset2);
            mover3.SetHomeOffset(initialOffset3);

            _levelManager.InitializeForTest(
                _teamContainer.transform, _enemiesContainer.transform,
                _teamHomeAnchor.transform, _enemiesHomeAnchor.transform);

            // Act -- kill enemy #2 (the middle one).
            LogAssert.Expect(LogType.Log, "[CombatStats] Enemy2 died!");
            enemy2.GetComponent<CombatStats>().TakeDamage(9999);

            // Simulate end-of-combat: clear targets and recalculate.
            _levelManager.ClearEnemyTargetsForTest();
            _levelManager.RecalculateEnemyFormationForTest();

            yield return null;

            // Assert -- compute expected 2-unit formation (facingRight: false).
            Vector2[] expected2Positions = FormationLayout.GetPositions(enemyAnchorPos, 2, facingRight: false);
            Vector2 expectedOffset0 = expected2Positions[0] - enemyAnchorPos;
            Vector2 expectedOffset1 = expected2Positions[1] - enemyAnchorPos;

            // Enemy1 (alive) should have the first position of the 2-unit formation.
            Assert.AreEqual(expectedOffset0.x, mover1.HomeOffset.x, 0.001f,
                "Enemy1 HomeOffset.x should match 2-unit formation position 0.");
            Assert.AreEqual(expectedOffset0.y, mover1.HomeOffset.y, 0.001f,
                "Enemy1 HomeOffset.y should match 2-unit formation position 0.");

            // Enemy3 (alive) should have the second position of the 2-unit formation.
            Assert.AreEqual(expectedOffset1.x, mover3.HomeOffset.x, 0.001f,
                "Enemy3 HomeOffset.x should match 2-unit formation position 1.");
            Assert.AreEqual(expectedOffset1.y, mover3.HomeOffset.y, 0.001f,
                "Enemy3 HomeOffset.y should match 2-unit formation position 1.");

            // Enemy2 (dead) should still have its original offset.
            Assert.AreEqual(initialOffset2.x, mover2.HomeOffset.x, 0.001f,
                "Dead Enemy2 HomeOffset.x should be unchanged.");
            Assert.AreEqual(initialOffset2.y, mover2.HomeOffset.y, 0.001f,
                "Dead Enemy2 HomeOffset.y should be unchanged.");
        }

        // ---------------------------------------------------------------
        // Test 3: Survivors walk toward new home positions after recalc
        // ---------------------------------------------------------------

        [UnityTest]
        public IEnumerator RecalculateFormation_SurvivorsWalkToNewPositions()
        {
            // Arrange -- 3 allies with CharacterMover, no target (homing mode).
            CreateLevelManagerSetup(
                teamAnchorPos: Vector2.zero,
                enemiesAnchorPos: new Vector2(5f, 0f));

            // Place allies at their initial 3-unit formation positions so they start "at home".
            Vector2 teamAnchorPos = (Vector2)_teamHomeAnchor.transform.position;
            Vector2[] initial3Positions = FormationLayout.GetPositions(teamAnchorPos, 3, facingRight: true);

            var ally1 = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "Ally1", maxHp: 100, moveSpeed: 3f, position: initial3Positions[0]));
            var ally2 = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "Ally2", maxHp: 100, moveSpeed: 3f, position: initial3Positions[1]));
            var ally3 = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "Ally3", maxHp: 100, moveSpeed: 3f, position: initial3Positions[2]));

            ally1.transform.SetParent(_teamContainer.transform);
            ally2.transform.SetParent(_teamContainer.transform);
            ally3.transform.SetParent(_teamContainer.transform);

            // Wait a frame so Awake runs.
            yield return null;

            var mover1 = ally1.GetComponent<CharacterMover>();
            var mover3 = ally3.GetComponent<CharacterMover>();

            mover1.HomeAnchor = _teamHomeAnchor.transform;
            ally2.GetComponent<CharacterMover>().HomeAnchor = _teamHomeAnchor.transform;
            mover3.HomeAnchor = _teamHomeAnchor.transform;

            Vector2 initialOffset1 = initial3Positions[0] - teamAnchorPos;
            Vector2 initialOffset2 = initial3Positions[1] - teamAnchorPos;
            Vector2 initialOffset3 = initial3Positions[2] - teamAnchorPos;

            mover1.SetHomeOffset(initialOffset1);
            ally2.GetComponent<CharacterMover>().SetHomeOffset(initialOffset2);
            mover3.SetHomeOffset(initialOffset3);

            _levelManager.InitializeForTest(
                _teamContainer.transform, _enemiesContainer.transform,
                _teamHomeAnchor.transform, _enemiesHomeAnchor.transform);
            _levelManager.WireAllyDeathTrackingForTest();

            // Kill ally #2.
            LogAssert.Expect(LogType.Log, "[CombatStats] Ally2 died!");
            ally2.GetComponent<CombatStats>().TakeDamage(9999);

            // Simulate disengage so CombatController re-enables CharacterMover.
            _levelManager.ClearAllyTargetsForTest();

            // Record positions BEFORE recalculation.
            Vector2 ally1PosBefore = (Vector2)ally1.transform.position;
            Vector2 ally3PosBefore = (Vector2)ally3.transform.position;

            // Act -- recalculate formation (this updates HomeOffset, homing logic moves them).
            _levelManager.RecalculateAllyFormationForTest();

            // Compute the new home positions that survivors should walk toward.
            Vector2[] expected2Positions = FormationLayout.GetPositions(teamAnchorPos, 2, facingRight: true);
            Vector2 newHome1 = expected2Positions[0];
            Vector2 newHome3 = expected2Positions[1];

            float ally1DistBefore = Vector2.Distance(ally1PosBefore, newHome1);
            float ally3DistBefore = Vector2.Distance(ally3PosBefore, newHome3);

            // Wait for physics to move characters toward their new home positions.
            for (int i = 0; i < 30; i++)
                yield return new WaitForFixedUpdate();

            // Assert -- survivors have moved closer to their new home positions.
            Vector2 ally1PosAfter = (Vector2)ally1.transform.position;
            Vector2 ally3PosAfter = (Vector2)ally3.transform.position;

            float ally1DistAfter = Vector2.Distance(ally1PosAfter, newHome1);
            float ally3DistAfter = Vector2.Distance(ally3PosAfter, newHome3);

            Assert.Less(ally1DistAfter, ally1DistBefore,
                $"Ally1 should be closer to new home. Before={ally1DistBefore:F3}, After={ally1DistAfter:F3}");
            Assert.Less(ally3DistAfter, ally3DistBefore,
                $"Ally3 should be closer to new home. Before={ally3DistBefore:F3}, After={ally3DistAfter:F3}");
        }
    }
}
