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
    public class LevelManagerReviveOnLevelTests : PlayModeTestBase
    {
        private GameObject _combatWorldGo;
        private LevelManager _levelManager;
        private CombatSpawnManager _spawnManager;
        private TeamRoster _teamRoster;
        private Transform _teamContainer;
        private Transform _enemiesContainer;
        private Transform _teamHomeAnchor;
        private Transform _enemiesHomeAnchor;
        private LevelDatabase _levelDatabase;

        private void CreateSetup(int allyCount, int stepCount, int enemiesPerStep)
        {
            _combatWorldGo = new GameObject("CombatWorld");
            Track(_combatWorldGo);

            var rb = _combatWorldGo.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            _combatWorldGo.AddComponent<WorldConveyor>();

            var teamContainerGo = new GameObject(CombatSetupHelper.TeamContainerName);
            teamContainerGo.transform.SetParent(_combatWorldGo.transform, false);
            _teamContainer = teamContainerGo.transform;

            var enemiesContainerGo = new GameObject(CombatSetupHelper.EnemiesContainerName);
            enemiesContainerGo.transform.SetParent(_combatWorldGo.transform, false);
            _enemiesContainer = enemiesContainerGo.transform;

            var teamHomeGo = new GameObject(CombatSetupHelper.TeamHomeAnchorName);
            teamHomeGo.transform.SetParent(_combatWorldGo.transform, false);
            teamHomeGo.transform.position = new Vector3(-3f, 0f, 0f);
            _teamHomeAnchor = teamHomeGo.transform;

            var enemiesHomeGo = new GameObject(CombatSetupHelper.EnemiesHomeAnchorName);
            enemiesHomeGo.transform.SetParent(_combatWorldGo.transform, false);
            enemiesHomeGo.transform.position = new Vector3(3f, 0f, 0f);
            _enemiesHomeAnchor = enemiesHomeGo.transform;

            var triggerZoneGo = new GameObject(CombatSetupHelper.CombatTriggerZoneName);
            triggerZoneGo.transform.SetParent(_combatWorldGo.transform, false);
            triggerZoneGo.transform.position = new Vector3(10f, 0f, 0f);
            triggerZoneGo.AddComponent<BoxCollider2D>();

            var groundGo = new GameObject("Ground");
            groundGo.transform.SetParent(_combatWorldGo.transform, false);
            groundGo.AddComponent<SpriteRenderer>();

            var allyPrefab = TestCharacterFactory.CreateCharacterPrefab("AllyPrefab");
            Track(allyPrefab);

            var enemyPrefab = TestCharacterFactory.CreateCharacterPrefab("EnemyPrefab");
            Track(enemyPrefab);

            var teamDb = TestCharacterFactory.CreateTeamDatabase(allyCount, allyPrefab);

            _levelDatabase = ScriptableObject.CreateInstance<LevelDatabase>();

            var steps = new List<StepData>();
            for (int s = 0; s < stepCount; s++)
            {
                var enemies = new List<EnemySpawnData>();
                for (int e = 0; e < enemiesPerStep; e++)
                {
                    enemies.Add(new EnemySpawnData($"Step{s}_Enemy_{e}", 50, 5)
                    {
                        Prefab = enemyPrefab,
                        AttackSpeed = 1f,
                        MoveSpeed = 2f,
                        AttackRange = 1f,
                        ColliderRadius = 0.3f,
                        GoldDrop = 0
                    });
                }
                var wave = new WaveData($"Wave_Step{s}", 0f, enemies);
                steps.Add(new StepData($"Step_{s}", new List<WaveData> { wave }));
            }

            var level0 = new LevelData("Level_0", steps);
            var level1 = new LevelData("Level_1", new List<StepData>
            {
                new StepData("Level1_Step_0", new List<WaveData>
                {
                    new WaveData("Level1_Wave", 0f, new List<EnemySpawnData>())
                })
            });

            var stage = new StageData("Stage_0", null, new List<LevelData> { level0, level1 });
            _levelDatabase.Stages = new List<StageData> { stage };

            _teamRoster = _combatWorldGo.AddComponent<TeamRoster>();
            _spawnManager = _combatWorldGo.AddComponent<CombatSpawnManager>();
            _spawnManager.enabled = false;
            _spawnManager.InitializeForTest(teamDb, _teamContainer, _teamHomeAnchor, _teamRoster);

            _levelManager = _combatWorldGo.AddComponent<LevelManager>();
            _levelManager.enabled = false;
            _levelManager.InitializeForTest(
                _teamContainer,
                _enemiesContainer,
                _teamHomeAnchor,
                _enemiesHomeAnchor,
                _levelDatabase);
            _levelManager.SetSpawnManagerForTest(_spawnManager);
        }

        public override void TearDown()
        {
            base.TearDown();

            if (_levelDatabase != null)
                Object.DestroyImmediate(_levelDatabase);
        }

        private const float FadeOutWaitSeconds = 0.35f;

        [UnityTest]
        public IEnumerator StartLevel_RevivesAllDeadMembers()
        {
            CreateSetup(allyCount: 2, stepCount: 1, enemiesPerStep: 1);

            yield return null;

            _spawnManager.SpawnAllies();
            _levelManager.WireAllyDeathTrackingForTest();

            yield return null;

            TeamMember victim = _teamRoster.Members[0];
            victim.Stats.TakeDamage(9999);

            yield return new WaitForSeconds(FadeOutWaitSeconds);

            Assert.IsTrue(victim.IsDead, "Sanity: member 0 should be dead before StartLevel.");

            _levelManager.ApplyStage(0);
            _levelManager.StartLevel(1);

            yield return null;

            foreach (var member in _teamRoster.Members)
            {
                Assert.IsFalse(member.IsDead,
                    $"Member {member.Index} should be alive after StartLevel revives team.");
                Assert.AreEqual(member.Stats.MaxHp, member.Stats.CurrentHp,
                    $"Member {member.Index} should have full HP after StartLevel revives team.");
            }
        }

        [UnityTest]
        public IEnumerator StartLevel_OnAliveTeam_IsNoOp()
        {
            CreateSetup(allyCount: 2, stepCount: 1, enemiesPerStep: 1);

            yield return null;

            _spawnManager.SpawnAllies();
            _levelManager.WireAllyDeathTrackingForTest();

            yield return null;

            TeamMember member0 = _teamRoster.Members[0];
            TeamMember member1 = _teamRoster.Members[1];

            int member0HpBefore = member0.Stats.CurrentHp;
            int member1HpBefore = member1.Stats.CurrentHp;

            int reviveCount = 0;
            _teamRoster.OnMemberRevived += _ => reviveCount++;

            _levelManager.ApplyStage(0);
            _levelManager.StartLevel(1);

            yield return null;

            Assert.AreEqual(0, reviveCount,
                "Revive should not fire for any member when the whole team is already alive.");
            Assert.AreEqual(member0HpBefore, member0.Stats.CurrentHp,
                "Member 0 HP should be unchanged after StartLevel on a live team.");
            Assert.AreEqual(member1HpBefore, member1.Stats.CurrentHp,
                "Member 1 HP should be unchanged after StartLevel on a live team.");
        }

        [UnityTest]
        public IEnumerator StartNextStep_DoesNotRevive()
        {
            CreateSetup(allyCount: 2, stepCount: 2, enemiesPerStep: 1);

            yield return null;

            _spawnManager.SpawnAllies();
            _levelManager.WireAllyDeathTrackingForTest();
            _levelManager.ApplyStage(0);
            _levelManager.StartLevel(0);

            yield return null;

            TeamMember victim = _teamRoster.Members[0];
            victim.Stats.TakeDamage(9999);

            yield return new WaitForSeconds(FadeOutWaitSeconds);

            Assert.IsTrue(victim.IsDead, "Sanity: member 0 should be dead before step transition.");

            int initialStepIndex = _levelManager.CurrentStepIndex;

            for (int i = _enemiesContainer.childCount - 1; i >= 0; i--)
            {
                var enemy = _enemiesContainer.GetChild(i);
                if (enemy.TryGetComponent<CombatStats>(out var stats) && !stats.IsDead)
                    stats.TakeDamage(9999);
            }

            yield return new WaitForSeconds(3f);

            Assert.AreNotEqual(initialStepIndex, _levelManager.CurrentStepIndex,
                "Sanity: step index should have advanced after killing all step enemies.");

            Assert.IsTrue(victim.IsDead,
                "Member killed before step transition should remain dead after StartNextStep (no revive on step change).");
        }
    }
}
