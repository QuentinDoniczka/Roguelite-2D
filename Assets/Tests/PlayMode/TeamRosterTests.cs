using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Tests;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class TeamRosterTests : PlayModeTestBase
    {
        private const float TestCharacterScale = 1.5f;

        private TeamRoster _roster;
        private Transform _teamContainer;
        private Transform _teamHomeAnchor;
        private GameObject _allyPrefab;

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
        }

        private static void ExpectAnimatorWarnings(int allyCount)
        {
            for (int i = 0; i < allyCount; i++)
            {
                LogAssert.Expect(LogType.Warning, new Regex("No Animator found"));
            }
        }

        [UnityTest]
        public IEnumerator Spawn_CreatesOneMemberPerTeamDatabaseEntry()
        {
            const int allyCount = 3;
            var teamDb = TestCharacterFactory.CreateTeamDatabase(allyCount, _allyPrefab);

            ExpectAnimatorWarnings(allyCount);
            _roster.Spawn(teamDb, _teamContainer, _teamHomeAnchor, TestCharacterScale);

            yield return null;

            Assert.AreEqual(allyCount, _roster.Members.Count);
        }

        [UnityTest]
        public IEnumerator Spawn_AssignsGameObjectAndStatsToEachMember()
        {
            const int allyCount = 3;
            var teamDb = TestCharacterFactory.CreateTeamDatabase(allyCount, _allyPrefab);

            ExpectAnimatorWarnings(allyCount);
            _roster.Spawn(teamDb, _teamContainer, _teamHomeAnchor, TestCharacterScale);

            yield return null;

            foreach (var member in _roster.Members)
            {
                Assert.IsNotNull(member.GameObject, $"Member {member.Index} should have GameObject assigned");
                Assert.IsNotNull(member.Stats, $"Member {member.Index} should have Stats assigned");
            }
        }

        [UnityTest]
        public IEnumerator Spawn_TwiceIsIdempotent_LogsWarning()
        {
            const int allyCount = 3;
            var teamDb = TestCharacterFactory.CreateTeamDatabase(allyCount, _allyPrefab);

            ExpectAnimatorWarnings(allyCount);
            _roster.Spawn(teamDb, _teamContainer, _teamHomeAnchor, TestCharacterScale);

            yield return null;

            LogAssert.Expect(LogType.Warning, new Regex("Spawn called twice, ignoring"));
            _roster.Spawn(teamDb, _teamContainer, _teamHomeAnchor, TestCharacterScale);

            yield return null;

            Assert.AreEqual(allyCount, _roster.Members.Count);
        }

        [UnityTest]
        public IEnumerator Spawn_FiresOnMemberSpawnedForEach()
        {
            const int allyCount = 3;
            var teamDb = TestCharacterFactory.CreateTeamDatabase(allyCount, _allyPrefab);

            int callbackCount = 0;
            _roster.OnMemberSpawned += _ => callbackCount++;

            ExpectAnimatorWarnings(allyCount);
            _roster.Spawn(teamDb, _teamContainer, _teamHomeAnchor, TestCharacterScale);

            yield return null;

            Assert.AreEqual(allyCount, callbackCount);
        }

        [Test]
        public void InitializeForTest_AllowsInjectingSyntheticMembers()
        {
            var syntheticMembers = new List<TeamMember>
            {
                new TeamMember(0, new AllySpawnData()),
                new TeamMember(1, new AllySpawnData())
            };

            _roster.InitializeForTest(syntheticMembers);

            Assert.AreEqual(2, _roster.Members.Count);
            Assert.AreEqual(0, _roster.Members[0].Index);
            Assert.AreEqual(1, _roster.Members[1].Index);
        }

        private const float FadeOutWaitSeconds = 0.35f;

        [UnityTest]
        public IEnumerator MemberDies_FiresOnMemberDied()
        {
            const int allyCount = 1;
            var teamDb = TestCharacterFactory.CreateTeamDatabase(allyCount, _allyPrefab);

            ExpectAnimatorWarnings(allyCount);
            _roster.Spawn(teamDb, _teamContainer, _teamHomeAnchor, TestCharacterScale);

            yield return null;

            TeamMember member = _roster.Members[0];
            TeamMember receivedMember = null;
            int callbackCount = 0;
            _roster.OnMemberDied += m =>
            {
                receivedMember = m;
                callbackCount++;
            };

            member.Stats.TakeDamage(99999);

            Assert.AreEqual(1, callbackCount);
            Assert.AreSame(member, receivedMember);
        }

        [UnityTest]
        public IEnumerator ReviveAll_ReactivatesAllDeadMembers()
        {
            const int allyCount = 3;
            var teamDb = TestCharacterFactory.CreateTeamDatabase(allyCount, _allyPrefab);

            ExpectAnimatorWarnings(allyCount);
            _roster.Spawn(teamDb, _teamContainer, _teamHomeAnchor, TestCharacterScale);

            yield return null;

            foreach (var member in _roster.Members)
                member.Stats.TakeDamage(99999);

            yield return new WaitForSeconds(FadeOutWaitSeconds);

            _roster.ReviveAll();

            foreach (var member in _roster.Members)
            {
                Assert.IsTrue(member.GameObject.activeSelf, $"Member {member.Index} GameObject should be active after revive");
                Assert.IsFalse(member.IsDead, $"Member {member.Index} should no longer be dead after revive");
            }
        }

        [UnityTest]
        public IEnumerator ReviveAll_RestoresFullHp()
        {
            const int allyCount = 3;
            var teamDb = TestCharacterFactory.CreateTeamDatabase(allyCount, _allyPrefab);

            ExpectAnimatorWarnings(allyCount);
            _roster.Spawn(teamDb, _teamContainer, _teamHomeAnchor, TestCharacterScale);

            yield return null;

            foreach (var member in _roster.Members)
                member.Stats.TakeDamage(99999);

            yield return new WaitForSeconds(FadeOutWaitSeconds);

            _roster.ReviveAll();

            foreach (var member in _roster.Members)
            {
                Assert.AreEqual(member.Stats.MaxHp, member.Stats.CurrentHp,
                    $"Member {member.Index} should have full HP after revive");
            }
        }

        [UnityTest]
        public IEnumerator ReviveAll_FiresOnMemberRevivedPerDeadMember()
        {
            const int allyCount = 3;
            var teamDb = TestCharacterFactory.CreateTeamDatabase(allyCount, _allyPrefab);

            ExpectAnimatorWarnings(allyCount);
            _roster.Spawn(teamDb, _teamContainer, _teamHomeAnchor, TestCharacterScale);

            yield return null;

            _roster.Members[0].Stats.TakeDamage(99999);
            _roster.Members[1].Stats.TakeDamage(99999);

            yield return new WaitForSeconds(FadeOutWaitSeconds);

            int revivedCount = 0;
            _roster.OnMemberRevived += _ => revivedCount++;

            _roster.ReviveAll();

            Assert.AreEqual(2, revivedCount);
        }

        [UnityTest]
        public IEnumerator ReviveAll_DoesNotDuplicateMembers()
        {
            const int allyCount = 3;
            var teamDb = TestCharacterFactory.CreateTeamDatabase(allyCount, _allyPrefab);

            ExpectAnimatorWarnings(allyCount);
            _roster.Spawn(teamDb, _teamContainer, _teamHomeAnchor, TestCharacterScale);

            yield return null;

            int countBefore = _roster.Members.Count;

            foreach (var member in _roster.Members)
                member.Stats.TakeDamage(99999);

            yield return new WaitForSeconds(FadeOutWaitSeconds);

            _roster.ReviveAll();

            Assert.AreEqual(countBefore, _roster.Members.Count);
        }

        [UnityTest]
        public IEnumerator Revive_OnLiveMember_IsNoOp()
        {
            const int allyCount = 2;
            var teamDb = TestCharacterFactory.CreateTeamDatabase(allyCount, _allyPrefab);

            ExpectAnimatorWarnings(allyCount);
            _roster.Spawn(teamDb, _teamContainer, _teamHomeAnchor, TestCharacterScale);

            yield return null;

            TeamMember liveMember = _roster.Members[0];
            int hpBefore = liveMember.Stats.CurrentHp;

            int revivedCount = 0;
            _roster.OnMemberRevived += _ => revivedCount++;

            _roster.Revive(liveMember);

            Assert.AreEqual(hpBefore, liveMember.Stats.CurrentHp);
            Assert.AreEqual(0, revivedCount);
        }

        [UnityTest]
        public IEnumerator Revive_ReusesSameGameObjectInstance()
        {
            const int allyCount = 1;
            var teamDb = TestCharacterFactory.CreateTeamDatabase(allyCount, _allyPrefab);

            ExpectAnimatorWarnings(allyCount);
            _roster.Spawn(teamDb, _teamContainer, _teamHomeAnchor, TestCharacterScale);

            yield return null;

            TeamMember member = _roster.Members[0];
            GameObject originalGo = member.GameObject;

            member.Stats.TakeDamage(99999);

            yield return new WaitForSeconds(FadeOutWaitSeconds);

            _roster.Revive(member);

            Assert.AreSame(originalGo, member.GameObject);
        }

        [UnityTest]
        public IEnumerator Revive_SnapsMemberBackToFormationAnchorInsteadOfDeathPosition()
        {
            const int allyCount = 3;
            const float RepositionEpsilon = 0.01f;
            Vector2 deathPosition = new Vector2(25f, -12f);

            var teamDb = TestCharacterFactory.CreateTeamDatabase(allyCount, _allyPrefab);

            ExpectAnimatorWarnings(allyCount);
            _roster.Spawn(teamDb, _teamContainer, _teamHomeAnchor, TestCharacterScale);

            yield return null;

            TeamMember member = _roster.Members[1];
            Vector3 initialFormationPosition = member.GameObject.transform.position;

            Assert.IsTrue(
                Vector2.Distance(initialFormationPosition, deathPosition) > 1f,
                "Sanity check: initial formation slot must be far from simulated death position.");

            member.Stats.TakeDamage(99999);

            yield return new WaitForSeconds(FadeOutWaitSeconds);

            member.GameObject.transform.position = deathPosition;

            Assert.AreEqual(
                (Vector3)deathPosition,
                member.GameObject.transform.position,
                "Precondition: member GameObject must be positioned away from its formation slot before revive.");

            _roster.Revive(member);

            yield return null;

            Vector3 positionAfterRevive = member.GameObject.transform.position;
            float distanceFromAnchor = Vector3.Distance(positionAfterRevive, initialFormationPosition);
            float distanceFromDeathPosition = Vector3.Distance(positionAfterRevive, deathPosition);

            Assert.Less(
                distanceFromAnchor,
                RepositionEpsilon,
                $"After revive, member must be snapped back to its formation anchor. " +
                $"Expected ~{initialFormationPosition}, got {positionAfterRevive} " +
                $"(distance from anchor={distanceFromAnchor}).");

            Assert.Greater(
                distanceFromDeathPosition,
                1f,
                $"After revive, member must NOT remain at the death position {deathPosition}. " +
                $"Actual position {positionAfterRevive} (distance from death pos={distanceFromDeathPosition}).");
        }

        [UnityTest]
        public IEnumerator ReviveAll_SnapsEveryDeadMemberBackToItsOwnFormationSlot()
        {
            const int allyCount = 3;
            const float RepositionEpsilon = 0.01f;

            var teamDb = TestCharacterFactory.CreateTeamDatabase(allyCount, _allyPrefab);

            ExpectAnimatorWarnings(allyCount);
            _roster.Spawn(teamDb, _teamContainer, _teamHomeAnchor, TestCharacterScale);

            yield return null;

            Vector3[] originalFormationPositions = new Vector3[allyCount];
            for (int i = 0; i < allyCount; i++)
                originalFormationPositions[i] = _roster.Members[i].GameObject.transform.position;

            foreach (var member in _roster.Members)
                member.Stats.TakeDamage(99999);

            yield return new WaitForSeconds(FadeOutWaitSeconds);

            for (int i = 0; i < allyCount; i++)
                _roster.Members[i].GameObject.transform.position = new Vector3(50f + i * 3f, -20f, 0f);

            _roster.ReviveAll();

            yield return null;

            for (int i = 0; i < allyCount; i++)
            {
                Vector3 actual = _roster.Members[i].GameObject.transform.position;
                float distance = Vector3.Distance(actual, originalFormationPositions[i]);
                Assert.Less(
                    distance,
                    RepositionEpsilon,
                    $"Member {i} not snapped back to formation. " +
                    $"Expected {originalFormationPositions[i]}, got {actual} (distance={distance}).");
            }
        }
    }
}
