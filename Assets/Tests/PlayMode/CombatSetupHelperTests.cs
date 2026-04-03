using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Combat.Visuals;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Tests;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class CombatSetupHelperTests : PlayModeTestBase
    {
        private const int DefaultMaxHp = 200;
        private const int DefaultAtk = 25;
        private const float DefaultAttackSpeed = 1.5f;
        private const float DefaultRegenHpPerSecond = 2f;
        private const float DefaultMoveSpeed = 3f;
        private const float DefaultColliderRadius = 0.4f;

        private GameObject _characterPrefab;
        private Transform _homeAnchor;
        private AppearanceData _appearance;

        [SetUp]
        public void SetUp()
        {
            DamageNumberService.ResetForTest();
            AttackSlotRegistry.Clear();

            _characterPrefab = Track(TestCharacterFactory.CreateCharacterPrefab("SetupTestChar"));
            _homeAnchor = Track(TestCharacterFactory.CreateAnchor("HomeAnchor")).transform;
            _appearance = new AppearanceData();
        }

        private CharacterComponents AssembleDefaultCharacter(
            Color? fillColor = null,
            Color? trailColor = null,
            float characterScale = 1f)
        {
            LogAssert.Expect(LogType.Warning, new Regex("No Animator found"));
            return CombatSetupHelper.AssembleCharacter(
                _characterPrefab,
                DefaultMaxHp,
                DefaultAtk,
                DefaultAttackSpeed,
                DefaultRegenHpPerSecond,
                DefaultMoveSpeed,
                _homeAnchor,
                Vector2.zero,
                DefaultColliderRadius,
                _appearance,
                "Test",
                healthBarFillColor: fillColor,
                healthBarTrailColor: trailColor,
                characterScale: characterScale);
        }

        [UnityTest]
        public IEnumerator AssembleCharacter_AddsCombatStats_WithCorrectValues()
        {
            var result = AssembleDefaultCharacter();

            yield return null;

            var stats = _characterPrefab.GetComponent<CombatStats>();
            Assert.IsNotNull(stats, "CombatStats should be added to character.");
            Assert.AreEqual(DefaultMaxHp, stats.MaxHp, "MaxHp should match configured value.");
            Assert.AreEqual(DefaultAtk, stats.Atk, "Atk should match configured value.");
            Assert.AreEqual(DefaultAttackSpeed, stats.AttackSpeed, "AttackSpeed should match configured value.");
        }

        [UnityTest]
        public IEnumerator AssembleCharacter_AddsCharacterMover_WithCorrectSpeed()
        {
            AssembleDefaultCharacter();

            yield return null;

            var mover = _characterPrefab.GetComponent<CharacterMover>();
            Assert.IsNotNull(mover, "CharacterMover should be added to character.");
            Assert.AreEqual(_homeAnchor, mover.HomeAnchor, "HomeAnchor should be set on mover.");
        }

        [UnityTest]
        public IEnumerator AssembleCharacter_AddsHealthBar()
        {
            AssembleDefaultCharacter();

            yield return null;

            var healthBar = _characterPrefab.GetComponent<HealthBar>();
            Assert.IsNotNull(healthBar, "HealthBar should be added to character.");

            var pivot = _characterPrefab.transform.Find("HealthBar_Pivot");
            Assert.IsNotNull(pivot, "HealthBar_Pivot child should exist after assembly.");
        }

        [UnityTest]
        public IEnumerator AssembleCharacter_AddsCombatController()
        {
            AssembleDefaultCharacter();

            yield return null;

            var controller = _characterPrefab.GetComponent<CombatController>();
            Assert.IsNotNull(controller, "CombatController should be added to character.");
        }

        [UnityTest]
        public IEnumerator AssembleCharacter_SetsColliderRadius()
        {
            float expectedRadius = DefaultColliderRadius;
            AssembleDefaultCharacter();

            yield return null;

            var col = _characterPrefab.GetComponent<CircleCollider2D>();
            Assert.IsNotNull(col, "CircleCollider2D should exist on character prefab.");
            Assert.AreEqual(expectedRadius, col.radius, 0.001f,
                "Collider radius should equal colliderRadius / characterScale.");
        }

        [UnityTest]
        public IEnumerator AssembleCharacter_SetsColliderRadius_WithCustomScale()
        {
            float customScale = 2f;
            float expectedRadius = DefaultColliderRadius / customScale;

            LogAssert.Expect(LogType.Warning, new Regex("No Animator found"));
            CombatSetupHelper.AssembleCharacter(
                _characterPrefab,
                DefaultMaxHp,
                DefaultAtk,
                DefaultAttackSpeed,
                DefaultRegenHpPerSecond,
                DefaultMoveSpeed,
                _homeAnchor,
                Vector2.zero,
                DefaultColliderRadius,
                _appearance,
                "Test",
                characterScale: customScale);

            yield return null;

            var col = _characterPrefab.GetComponent<CircleCollider2D>();
            Assert.AreEqual(expectedRadius, col.radius, 0.001f,
                "Collider radius should be divided by characterScale.");
        }

        [UnityTest]
        public IEnumerator AssembleCharacter_ReturnsCorrectComponents()
        {
            var result = AssembleDefaultCharacter();

            yield return null;

            Assert.IsNotNull(result.Stats, "Returned Stats should not be null.");
            Assert.IsNotNull(result.Controller, "Returned Controller should not be null.");
            Assert.AreEqual(_characterPrefab.GetComponent<CombatStats>(), result.Stats,
                "Returned Stats should be the CombatStats on the character.");
            Assert.AreEqual(_characterPrefab.GetComponent<CombatController>(), result.Controller,
                "Returned Controller should be the CombatController on the character.");
        }

        [UnityTest]
        public IEnumerator AssembleCharacter_CustomHealthBarColors_Applied()
        {
            var customFillColor = new Color(0.9f, 0.1f, 0.1f, 1f);
            var customTrailColor = new Color(1f, 0.5f, 0f, 1f);

            AssembleDefaultCharacter(fillColor: customFillColor, trailColor: customTrailColor);

            yield return null;

            var pivot = _characterPrefab.transform.Find("HealthBar_Pivot");
            Assert.IsNotNull(pivot, "HealthBar_Pivot should exist.");

            var fillChild = pivot.Find("Fill");
            Assert.IsNotNull(fillChild, "Fill child should exist under pivot.");
            var fillRenderer = fillChild.GetComponent<SpriteRenderer>();
            Assert.AreEqual(customFillColor, fillRenderer.color,
                "Fill renderer color should match custom fill color.");

            var trailFillChild = pivot.Find("TrailFill");
            Assert.IsNotNull(trailFillChild, "TrailFill child should exist under pivot.");
            var trailFillRenderer = trailFillChild.GetComponent<SpriteRenderer>();
            Assert.AreEqual(customTrailColor, trailFillRenderer.color,
                "TrailFill renderer color should match custom trail color.");
        }

        [UnityTest]
        public IEnumerator RecalculateFormation_UpdatesHomeOffsets()
        {
            var container = Track(new GameObject("FormationContainer"));
            var anchor = Track(TestCharacterFactory.CreateAnchor("FormationAnchor", new Vector2(0f, 0f)));

            var charA = Track(TestCharacterFactory.CreateFormationCharacter("CharA"));
            charA.transform.SetParent(container.transform);
            var charB = Track(TestCharacterFactory.CreateFormationCharacter("CharB"));
            charB.transform.SetParent(container.transform);
            var charC = Track(TestCharacterFactory.CreateFormationCharacter("CharC"));
            charC.transform.SetParent(container.transform);

            yield return null;

            var moverA = charA.GetComponent<CharacterMover>();
            var moverB = charB.GetComponent<CharacterMover>();
            var moverC = charC.GetComponent<CharacterMover>();

            Vector2 offsetABefore = moverA.HomeOffset;
            Vector2 offsetBBefore = moverB.HomeOffset;
            Vector2 offsetCBefore = moverC.HomeOffset;

            CombatSetupHelper.RecalculateFormation(container.transform, anchor.transform, true);

            bool anyOffsetChanged =
                moverA.HomeOffset != offsetABefore ||
                moverB.HomeOffset != offsetBBefore ||
                moverC.HomeOffset != offsetCBefore;

            Assert.IsTrue(anyOffsetChanged, "At least one home offset should change after recalculation.");

            bool allOffsetsAreDistinct =
                moverA.HomeOffset != moverB.HomeOffset ||
                moverA.HomeOffset != moverC.HomeOffset;

            Assert.IsTrue(allOffsetsAreDistinct,
                "Not all offsets should be identical for 3 characters in formation.");
        }

        [UnityTest]
        public IEnumerator DestroyAllChildren_RemovesAll()
        {
            var container = Track(new GameObject("DestroyContainer"));
            var childA = new GameObject("ChildA");
            childA.transform.SetParent(container.transform);
            var childB = new GameObject("ChildB");
            childB.transform.SetParent(container.transform);
            var childC = new GameObject("ChildC");
            childC.transform.SetParent(container.transform);

            Assert.AreEqual(3, container.transform.childCount, "Container should have 3 children before destroy.");

            CombatSetupHelper.DestroyAllChildren(container.transform);

            yield return null;

            Assert.AreEqual(0, container.transform.childCount,
                "Container should have 0 children after DestroyAllChildren and one frame.");
        }

        [UnityTest]
        public IEnumerator FindContainersIfNeeded_FindsNamedContainers()
        {
            var parent = Track(new GameObject("CombatWorld"));
            var teamGo = new GameObject(CombatSetupHelper.TeamContainerName);
            teamGo.transform.SetParent(parent.transform);
            var enemiesGo = new GameObject(CombatSetupHelper.EnemiesContainerName);
            enemiesGo.transform.SetParent(parent.transform);

            yield return null;

            Transform teamContainer = null;
            Transform enemiesContainer = null;

            CombatSetupHelper.FindContainersIfNeeded(parent.transform, ref teamContainer, ref enemiesContainer, "Test");

            Assert.IsNotNull(teamContainer, "Team container should be found.");
            Assert.IsNotNull(enemiesContainer, "Enemies container should be found.");
            Assert.AreEqual(CombatSetupHelper.TeamContainerName, teamContainer.name,
                "Team container should have correct name.");
            Assert.AreEqual(CombatSetupHelper.EnemiesContainerName, enemiesContainer.name,
                "Enemies container should have correct name.");
        }
    }
}
