using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Tests;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class HealthBarTrailTests : PlayModeTestBase
    {
        [UnityTest]
        public IEnumerator HealthBar_CreatesThreeLayerHierarchy()
        {
            var charGo = Track(TestCharacterFactory.CreateCharacterWithHealthBar(
                name: "TrailLayerChar",
                maxHp: 100));

            yield return null;

            var pivot = charGo.transform.Find("HealthBar_Pivot");
            Assert.IsNotNull(pivot, "HealthBar_Pivot child should exist.");
            Assert.AreEqual(3, pivot.childCount, "Pivot should have exactly 3 children: BG, TrailFill, Fill.");

            var bgChild = pivot.Find("BG");
            var trailFillChild = pivot.Find("TrailFill");
            var fillChild = pivot.Find("Fill");

            Assert.IsNotNull(bgChild, "BG child should exist under pivot.");
            Assert.IsNotNull(trailFillChild, "TrailFill child should exist under pivot.");
            Assert.IsNotNull(fillChild, "Fill child should exist under pivot.");

            var bgRenderer = bgChild.GetComponent<SpriteRenderer>();
            var trailFillRenderer = trailFillChild.GetComponent<SpriteRenderer>();
            var fillRenderer = fillChild.GetComponent<SpriteRenderer>();

            Assert.IsNotNull(bgRenderer, "BG should have a SpriteRenderer.");
            Assert.IsNotNull(trailFillRenderer, "TrailFill should have a SpriteRenderer.");
            Assert.IsNotNull(fillRenderer, "Fill should have a SpriteRenderer.");

            Assert.AreEqual(10, bgRenderer.sortingOrder, "BG sortingOrder should be 10.");
            Assert.AreEqual(11, trailFillRenderer.sortingOrder, "TrailFill sortingOrder should be 11.");
            Assert.AreEqual(12, fillRenderer.sortingOrder, "Fill sortingOrder should be 12.");
        }

        [UnityTest]
        public IEnumerator HealthBar_FillColorRemainsGreenAtLowHp()
        {
            var expectedGreenColor = new Color(0.20f, 0.80f, 0.20f, 1f);

            var charGo = Track(TestCharacterFactory.CreateCharacterWithHealthBar(
                name: "ColorTestChar",
                maxHp: 100));

            yield return null;

            var stats = charGo.GetComponent<CombatStats>();
            stats.TakeDamage(80);

            yield return null;

            var pivot = charGo.transform.Find("HealthBar_Pivot");
            var fillChild = pivot.Find("Fill");
            var fillRenderer = fillChild.GetComponent<SpriteRenderer>();

            Assert.AreEqual(expectedGreenColor, fillRenderer.color,
                "Fill color should remain green even at 20% HP.");
        }

        [UnityTest]
        public IEnumerator HealthBar_TrailStartsAtFullWidth()
        {
            var charGo = Track(TestCharacterFactory.CreateCharacterWithHealthBar(
                name: "TrailStartChar",
                maxHp: 100));

            yield return null;

            var pivot = charGo.transform.Find("HealthBar_Pivot");
            var fillChild = pivot.Find("Fill");
            var trailFillChild = pivot.Find("TrailFill");

            Assert.AreEqual(fillChild.localScale.x, trailFillChild.localScale.x,
                "TrailFill should start at the same width as Fill.");
        }

        [UnityTest]
        public IEnumerator HealthBar_TrailLagsBehindFillAfterDamage()
        {
            var charGo = Track(TestCharacterFactory.CreateCharacterWithHealthBar(
                name: "TrailLagChar",
                maxHp: 100));

            yield return null;

            var stats = charGo.GetComponent<CombatStats>();
            stats.TakeDamage(50);

            yield return null;

            var pivot = charGo.transform.Find("HealthBar_Pivot");
            var fillChild = pivot.Find("Fill");
            var trailFillChild = pivot.Find("TrailFill");

            Assert.Less(fillChild.localScale.x, trailFillChild.localScale.x,
                "Fill should be smaller than TrailFill immediately after damage.");
        }

        [UnityTest]
        public IEnumerator HealthBar_TrailCatchesUpAfterFadeDuration()
        {
            var charGo = Track(TestCharacterFactory.CreateCharacterWithHealthBar(
                name: "TrailCatchUpChar",
                maxHp: 100));

            yield return null;

            var stats = charGo.GetComponent<CombatStats>();
            stats.TakeDamage(50);

            yield return new WaitForSeconds(0.6f);

            var pivot = charGo.transform.Find("HealthBar_Pivot");
            var fillChild = pivot.Find("Fill");
            var trailFillChild = pivot.Find("TrailFill");

            Assert.AreEqual(fillChild.localScale.x, trailFillChild.localScale.x, 0.05f,
                "TrailFill should match Fill after fade duration elapses.");
        }

        [UnityTest]
        public IEnumerator HealthBar_TrailRestartsOnConsecutiveHits()
        {
            var charGo = Track(TestCharacterFactory.CreateCharacterWithHealthBar(
                name: "TrailConsecutiveChar",
                maxHp: 100));

            yield return null;

            var stats = charGo.GetComponent<CombatStats>();
            stats.TakeDamage(30);

            yield return new WaitForSeconds(0.2f);

            var pivot = charGo.transform.Find("HealthBar_Pivot");
            var trailFillChild = pivot.Find("TrailFill");
            float trailScaleBeforeSecondHit = trailFillChild.localScale.x;

            stats.TakeDamage(20);

            yield return null;

            Assert.AreEqual(trailScaleBeforeSecondHit, trailFillChild.localScale.x, 0.05f,
                "TrailFill should restart from its current position, not jump back to full.");
        }
    }
}
