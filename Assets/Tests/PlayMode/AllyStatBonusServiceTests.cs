using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Core;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Economy;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class AllyStatBonusServiceTests : PlayModeTestBase
    {
        private const float TestCharacterScale = 1.5f;
        private const float FadeOutWaitSeconds = 0.35f;
        private const int BaseMaxHp = 100;
        private const int HpPerLevel = 5;

        private TeamRoster _roster;
        private Transform _teamContainer;
        private Transform _teamHomeAnchor;
        private GameObject _allyPrefab;
        private SkillTreeData _data;
        private SkillTreeProgress _progress;
        private AllyStatBonusService _service;

        [SetUp]
        public void SetUp()
        {
            var rosterGo = Track(new GameObject("TeamRoster"));
            _roster = rosterGo.AddComponent<TeamRoster>();

            var containerGo = Track(new GameObject("TeamContainer"));
            _teamContainer = containerGo.transform;

            var anchorGo = Track(new GameObject("TeamHomeAnchor"));
            _teamHomeAnchor = anchorGo.transform;

            _allyPrefab = Track(TestCharacterFactory.CreateCharacterPrefab());

            _data = ScriptableObject.CreateInstance<SkillTreeData>();
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>
            {
                new SkillTreeData.SkillNodeEntry
                {
                    id = 0,
                    maxLevel = 5,
                    statModifierType = StatType.Hp,
                    statModifierMode = SkillTreeData.StatModifierMode.Flat,
                    statModifierValuePerLevel = HpPerLevel,
                    connectedNodeIds = new List<int>()
                }
            });

            _progress = ScriptableObject.CreateInstance<SkillTreeProgress>();
        }

        [TearDown]
        public override void TearDown()
        {
            _service?.Dispose();
            _service = null;

            if (_data != null) Object.DestroyImmediate(_data);
            if (_progress != null) Object.DestroyImmediate(_progress);

            base.TearDown();
        }

        private static void ExpectAnimatorWarnings(int allyCount)
        {
            for (int i = 0; i < allyCount; i++)
                LogAssert.Expect(LogType.Warning, new Regex("No Animator found"));
        }

        private static bool HasTechTreeModifier(CombatStats stats, StatType statType, int nodeId)
        {
            string source = ModifierSources.TechTreeNode(nodeId);
            var breakdown = stats.GetBreakdown(statType);
            for (int i = 0; i < breakdown.Modifiers.Length; i++)
            {
                if (breakdown.Modifiers[i].Source == source)
                    return true;
            }
            return false;
        }

        [UnityTest]
        public IEnumerator OnMemberSpawned_AppliesTechtreeModifiers_AndCurrentHpEqualsNewMax()
        {
            const int level = 3;
            const int allyCount = 1;
            int expectedMaxHp = BaseMaxHp + HpPerLevel * level;

            _progress.SetLevel(0, level);
            _service = new AllyStatBonusService(_roster, _data, _progress);

            var teamDb = TestCharacterFactory.CreateTeamDatabase(allyCount, _allyPrefab);
            ExpectAnimatorWarnings(allyCount);
            _roster.Spawn(teamDb, _teamContainer, _teamHomeAnchor, TestCharacterScale);

            yield return null;

            TeamMember member = _roster.Members[0];
            Assert.AreEqual(expectedMaxHp, member.Stats.MaxHp, "MaxHp should include +15 HP from techtree level 3.");
            Assert.AreEqual(expectedMaxHp, member.Stats.CurrentHp, "CurrentHp should equal MaxHp after HealToFull.");
            Assert.IsTrue(HasTechTreeModifier(member.Stats, StatType.Hp, 0),
                "Breakdown should contain a techtree:node0 modifier on Hp.");
        }

        [UnityTest]
        public IEnumerator OnMemberSpawned_AtLevelZero_DoesNotAddModifier()
        {
            const int allyCount = 1;

            _service = new AllyStatBonusService(_roster, _data, _progress);

            var teamDb = TestCharacterFactory.CreateTeamDatabase(allyCount, _allyPrefab);
            ExpectAnimatorWarnings(allyCount);
            _roster.Spawn(teamDb, _teamContainer, _teamHomeAnchor, TestCharacterScale);

            yield return null;

            TeamMember member = _roster.Members[0];
            Assert.AreEqual(BaseMaxHp, member.Stats.MaxHp, "MaxHp should remain at base when node level is 0.");
            Assert.AreEqual(BaseMaxHp, member.Stats.CurrentHp, "CurrentHp should equal base MaxHp.");
            Assert.IsFalse(HasTechTreeModifier(member.Stats, StatType.Hp, 0),
                "Breakdown should NOT contain a techtree:node0 modifier when level is 0.");
        }

        [UnityTest]
        public IEnumerator OnMemberRevived_ReappliesTechtreeModifiers_AndFullHeals()
        {
            const int level = 3;
            const int allyCount = 1;
            int expectedMaxHp = BaseMaxHp + HpPerLevel * level;

            _progress.SetLevel(0, level);
            _service = new AllyStatBonusService(_roster, _data, _progress);

            var teamDb = TestCharacterFactory.CreateTeamDatabase(allyCount, _allyPrefab);
            ExpectAnimatorWarnings(allyCount);
            _roster.Spawn(teamDb, _teamContainer, _teamHomeAnchor, TestCharacterScale);

            yield return null;

            TeamMember member = _roster.Members[0];
            Assert.AreEqual(expectedMaxHp, member.Stats.MaxHp, "Precondition: MaxHp should be boosted on spawn.");

            member.Stats.TakeDamage(99999);

            yield return new WaitForSeconds(FadeOutWaitSeconds);

            Assert.IsTrue(member.IsDead, "Precondition: member should be dead before revive.");

            _roster.Revive(member);

            yield return null;

            Assert.AreEqual(expectedMaxHp, member.Stats.MaxHp,
                "After revive, MaxHp should still include techtree bonus (re-applied).");
            Assert.AreEqual(expectedMaxHp, member.Stats.CurrentHp,
                "After revive, CurrentHp should equal MaxHp (full heal).");
            Assert.IsTrue(HasTechTreeModifier(member.Stats, StatType.Hp, 0),
                "Breakdown should contain a techtree:node0 modifier on Hp after revive.");
        }

        [UnityTest]
        public IEnumerator OnLevelChanged_LiveMember_RecomputesAndFullHeals()
        {
            const int newLevel = 3;
            const int allyCount = 1;
            int expectedMaxHp = BaseMaxHp + HpPerLevel * newLevel;

            _service = new AllyStatBonusService(_roster, _data, _progress);

            var teamDb = TestCharacterFactory.CreateTeamDatabase(allyCount, _allyPrefab);
            ExpectAnimatorWarnings(allyCount);
            _roster.Spawn(teamDb, _teamContainer, _teamHomeAnchor, TestCharacterScale);

            yield return null;

            TeamMember member = _roster.Members[0];
            Assert.AreEqual(BaseMaxHp, member.Stats.MaxHp, "Precondition: MaxHp should be base before level up.");

            member.Stats.TakeDamage(50);
            Assert.AreEqual(BaseMaxHp - 50, member.Stats.CurrentHp, "Precondition: CurrentHp should be reduced by 50.");

            _progress.SetLevel(0, newLevel);

            yield return null;

            Assert.AreEqual(expectedMaxHp, member.Stats.MaxHp,
                "MaxHp should include techtree bonus after OnLevelChanged.");
            Assert.AreEqual(expectedMaxHp, member.Stats.CurrentHp,
                "CurrentHp should equal new MaxHp after full heal.");
            Assert.IsTrue(HasTechTreeModifier(member.Stats, StatType.Hp, 0),
                "Breakdown should contain a techtree:node0 modifier on Hp.");
        }

        [UnityTest]
        public IEnumerator OnLevelChanged_ResetAllSentinel_ClearsAllTechtreeModifiers()
        {
            const int initialLevel = 3;
            const int allyCount = 1;
            int boostedMaxHp = BaseMaxHp + HpPerLevel * initialLevel;

            _progress.SetLevel(0, initialLevel);
            _service = new AllyStatBonusService(_roster, _data, _progress);

            var teamDb = TestCharacterFactory.CreateTeamDatabase(allyCount, _allyPrefab);
            ExpectAnimatorWarnings(allyCount);
            _roster.Spawn(teamDb, _teamContainer, _teamHomeAnchor, TestCharacterScale);

            yield return null;

            TeamMember member = _roster.Members[0];
            Assert.AreEqual(boostedMaxHp, member.Stats.MaxHp, "Precondition: MaxHp should include techtree bonus.");
            Assert.AreEqual(boostedMaxHp, member.Stats.CurrentHp, "Precondition: CurrentHp should equal boosted MaxHp.");

            _progress.ResetAll();

            yield return null;

            Assert.AreEqual(BaseMaxHp, member.Stats.MaxHp,
                "MaxHp should drop back to base after ResetAll.");
            Assert.AreEqual(BaseMaxHp, member.Stats.CurrentHp,
                "CurrentHp should equal base MaxHp after full heal.");
            Assert.IsFalse(HasTechTreeModifier(member.Stats, StatType.Hp, 0),
                "Breakdown should NOT contain any techtree:node0 modifier after ResetAll.");
        }

        [UnityTest]
        public IEnumerator Integration_GameBootstrapWiresService_LevelUpAffectsSpawnedMembers()
        {
            const int newLevel = 4;
            const int allyCount = 1;
            int expectedMaxHp = BaseMaxHp + HpPerLevel * newLevel;

            GameBootstrap.ResetForTest();

            var combatWorldGo = Track(new GameObject(GameBootstrap.CombatWorldName));
            var bootstrappedRoster = combatWorldGo.AddComponent<TeamRoster>();
            _teamContainer = combatWorldGo.transform;
            _teamHomeAnchor = combatWorldGo.transform;

            var camGo = Track(new GameObject("MainCamera"));
            var cam = camGo.AddComponent<Camera>();
            cam.tag = "MainCamera";

            var walletGo = Track(new GameObject("GoldWallet"));
            var wallet = walletGo.AddComponent<GoldWallet>();

            string tempDirectory = Path.Combine(Path.GetTempPath(), "roguelite-tests", Guid.NewGuid().ToString());
            string tempFilePath = Path.Combine(tempDirectory, "progression.json");

            GameBootstrap.SkillTreeDataAssetForTest = _data;
            GameBootstrap.SkillTreeProgressAssetForTest = _progress;
            GameBootstrap.GoldWalletForTest = wallet;
            GameBootstrap.ProgressionFilePathForTest = tempFilePath;

            yield return null;

            LogAssert.Expect(LogType.Error, new Regex("navigation system"));

            TeamDatabase teamDb = null;
            try
            {
                GameBootstrap.Initialize();

                Assert.IsNotNull(GameBootstrap.ProgressionLoader,
                    "ProgressionLoader should be created by GameBootstrap.");
                Assert.IsNotNull(GameBootstrap.AllyStatBonusService,
                    "AllyStatBonusService should be created by GameBootstrap.");
                Assert.AreSame(bootstrappedRoster, GameBootstrap.TeamRoster,
                    "GameBootstrap should resolve the TeamRoster on the CombatWorld GameObject.");

                teamDb = TestCharacterFactory.CreateTeamDatabase(allyCount, _allyPrefab);
                ExpectAnimatorWarnings(allyCount);
                GameBootstrap.TeamRoster.Spawn(teamDb, _teamContainer, _teamHomeAnchor, TestCharacterScale);

                yield return null;

                TeamMember member = GameBootstrap.TeamRoster.Members[0];
                Assert.AreEqual(BaseMaxHp, member.Stats.MaxHp,
                    "Precondition: MaxHp should be base when node level is 0 at spawn time.");
                Assert.AreEqual(BaseMaxHp, member.Stats.CurrentHp,
                    "Precondition: CurrentHp should equal base MaxHp at spawn time.");
                Assert.IsFalse(HasTechTreeModifier(member.Stats, StatType.Hp, 0),
                    "Precondition: no techtree modifier should be applied at level 0.");

                _progress.SetLevel(0, newLevel);

                yield return null;

                Assert.AreEqual(expectedMaxHp, member.Stats.MaxHp,
                    "After SetLevel, MaxHp should include techtree bonus through the wired service.");
                Assert.AreEqual(expectedMaxHp, member.Stats.CurrentHp,
                    "After SetLevel, CurrentHp should equal MaxHp via HealToFull.");
                Assert.IsTrue(HasTechTreeModifier(member.Stats, StatType.Hp, 0),
                    "Breakdown should contain a techtree:node0 modifier after live level up.");
            }
            finally
            {
                GameBootstrap.ResetForTest();
                if (teamDb != null)
                    Object.DestroyImmediate(teamDb);
                if (Directory.Exists(tempDirectory))
                    Directory.Delete(tempDirectory, true);
            }
        }
    }
}
