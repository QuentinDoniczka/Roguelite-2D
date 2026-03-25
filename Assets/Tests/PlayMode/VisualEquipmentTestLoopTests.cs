using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Combat;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class VisualEquipmentTestLoopTests : PlayModeTestBase
    {
        private const string PrefabPath = "Assets/Prefabs/Characters/sampleCharacterHuman.prefab";

        private readonly List<Object> _disposableAssets = new List<Object>();

        private GameObject InstantiatePrefab()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.IsNotNull(prefab, $"Prefab not found at {PrefabPath}");
            var instance = Object.Instantiate(prefab);

            // Disable the Animator so it does not override equipment sprites
            // via PPtrCurves on subsequent frames. CharacterAppearance.Awake()
            // only needs the Animator Transform to resolve slot paths; after
            // that it can be safely disabled.
            var animator = instance.GetComponentInChildren<Animator>();
            if (animator != null)
                animator.enabled = false;

            return Track(instance);
        }

        private Sprite CreateTestSprite()
        {
            var tex = new Texture2D(4, 4);
            _disposableAssets.Add(tex);
            var sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), Vector2.zero);
            _disposableAssets.Add(sprite);
            return sprite;
        }

        /// <summary>
        /// Creates a VisualEquipmentTestLoop with a very short cycle interval
        /// and the given sprite arrays assigned. The component is disabled so that
        /// Start does not run until the test explicitly enables it.
        /// </summary>
        private VisualEquipmentTestLoop CreateLoop(
            float interval,
            Sprite[] weapons,
            Sprite[] hats,
            Sprite[] shields,
            Sprite[] heads = null)
        {
            var go = new GameObject("VisualEquipmentTestLoop");
            Track(go);

            // Add component on active object (Awake runs immediately) but disable it
            // so Start does not run until we enable it.
            var loop = go.AddComponent<VisualEquipmentTestLoop>();
            loop.enabled = false;

            loop.CycleInterval = interval;
            loop.HeadSprites = heads ?? new Sprite[0];
            loop.WeaponSprites = weapons;
            loop.HatSprites = hats;
            loop.ShieldSprites = shields;

            return loop;
        }

        [TearDown]
        public new void TearDown()
        {
            foreach (var asset in _disposableAssets)
            {
                if (asset != null)
                    Object.DestroyImmediate(asset);
            }
            _disposableAssets.Clear();

            // NUnit calls [TearDown] in reverse hierarchy order (derived first, then base),
            // so base.TearDown() is already invoked automatically. No explicit call needed.
        }

        // ----------------------------------------------------------------
        // Test 1: After one cycle interval the loop applies random sprites
        // ----------------------------------------------------------------
        [UnityTest]
        public IEnumerator VisualEquipmentTestLoop_CyclesSpritesAfterInterval()
        {
            // Arrange -- instantiate character, let Awake resolve slots
            var character = InstantiatePrefab();
            character.AddComponent<CharacterAppearance>();
            yield return null; // Awake runs

            var appearance = character.GetComponent<CharacterAppearance>();
            Assert.IsNotNull(appearance.WeaponRenderer, "WeaponRenderer must be resolved.");
            Assert.IsNotNull(appearance.HatRenderer, "HatRenderer must be resolved.");
            Assert.IsNotNull(appearance.ShieldRenderer, "ShieldRenderer must be resolved.");

            // Record original sprites
            var origWeapon = appearance.WeaponRenderer.sprite;
            var origHat = appearance.HatRenderer.sprite;
            var origShield = appearance.ShieldRenderer.sprite;

            // Create test sprite arrays (single sprite each -- deterministic)
            var testWeapon = CreateTestSprite();
            var testHat = CreateTestSprite();
            var testShield = CreateTestSprite();

            var loop = CreateLoop(
                interval: 0.1f,
                weapons: new[] { testWeapon },
                hats: new[] { testHat },
                shields: new[] { testShield });

            // Enable component so Start runs, then yield a frame to ensure the coroutine is registered
            loop.enabled = true;
            yield return null;

            // Act -- wait long enough for at least one cycle (interval = 0.1s)
            yield return new WaitForSeconds(0.2f);

            // Assert -- sprites should now be the test sprites
            Assert.AreEqual(testWeapon, appearance.WeaponRenderer.sprite,
                "Weapon sprite should have been changed by the loop.");
            Assert.AreEqual(testHat, appearance.HatRenderer.sprite,
                "Hat sprite should have been changed by the loop.");
            Assert.AreEqual(testShield, appearance.ShieldRenderer.sprite,
                "Shield sprite should have been changed by the loop.");

            // They should differ from the originals
            Assert.AreNotEqual(origWeapon, appearance.WeaponRenderer.sprite,
                "Weapon sprite should differ from the original prefab sprite.");
            Assert.AreNotEqual(origHat, appearance.HatRenderer.sprite,
                "Hat sprite should differ from the original prefab sprite.");
            Assert.AreNotEqual(origShield, appearance.ShieldRenderer.sprite,
                "Shield sprite should differ from the original prefab sprite.");
        }

        // ----------------------------------------------------------------
        // Test 2: Empty arrays -- sprites stay at their defaults
        // ----------------------------------------------------------------
        [UnityTest]
        public IEnumerator VisualEquipmentTestLoop_EmptyArrays_KeepsDefaults()
        {
            // Arrange
            var character = InstantiatePrefab();
            character.AddComponent<CharacterAppearance>();
            yield return null;

            var appearance = character.GetComponent<CharacterAppearance>();

            var origWeapon = appearance.WeaponRenderer.sprite;
            var origHat = appearance.HatRenderer.sprite;
            var origShield = appearance.ShieldRenderer.sprite;

            var loop = CreateLoop(
                interval: 0.1f,
                weapons: new Sprite[0],
                hats: new Sprite[0],
                shields: new Sprite[0]);

            loop.enabled = true;
            yield return null; // Ensure Start() runs and coroutine is registered

            // Act -- wait for at least one cycle (interval = 0.1s)
            yield return new WaitForSeconds(0.2f);

            // Assert -- sprites unchanged because PickRandom returns null for empty arrays,
            // and ApplyAppearance skips null sprites
            Assert.AreEqual(origWeapon, appearance.WeaponRenderer.sprite,
                "Weapon sprite should remain the default when weapon array is empty.");
            Assert.AreEqual(origHat, appearance.HatRenderer.sprite,
                "Hat sprite should remain the default when hat array is empty.");
            Assert.AreEqual(origShield, appearance.ShieldRenderer.sprite,
                "Shield sprite should remain the default when shield array is empty.");
        }

        // ----------------------------------------------------------------
        // Test 3: Multiple characters all get their sprites changed
        // ----------------------------------------------------------------
        [UnityTest]
        public IEnumerator VisualEquipmentTestLoop_AffectsAllCharacters()
        {
            // Arrange -- 3 characters
            var characters = new GameObject[3];
            var appearances = new CharacterAppearance[3];

            for (int i = 0; i < 3; i++)
            {
                characters[i] = InstantiatePrefab();
                characters[i].name = $"Character_{i}";
                characters[i].transform.position = new Vector3(i * 2f, 0f, 0f);
                characters[i].AddComponent<CharacterAppearance>();
            }

            yield return null; // Awake runs on all

            var origWeapons = new Sprite[3];
            var origHats = new Sprite[3];
            var origShields = new Sprite[3];

            for (int i = 0; i < 3; i++)
            {
                appearances[i] = characters[i].GetComponent<CharacterAppearance>();
                Assert.IsNotNull(appearances[i].WeaponRenderer,
                    $"Character {i}: WeaponRenderer must be resolved.");

                origWeapons[i] = appearances[i].WeaponRenderer.sprite;
                origHats[i] = appearances[i].HatRenderer.sprite;
                origShields[i] = appearances[i].ShieldRenderer.sprite;
            }

            // Single test sprite per slot
            var testWeapon = CreateTestSprite();
            var testHat = CreateTestSprite();
            var testShield = CreateTestSprite();

            var loop = CreateLoop(
                interval: 0.1f,
                weapons: new[] { testWeapon },
                hats: new[] { testHat },
                shields: new[] { testShield });

            loop.enabled = true;
            yield return null; // Ensure Start() runs and coroutine is registered

            // Act -- wait for at least one cycle (interval = 0.1s)
            yield return new WaitForSeconds(0.2f);

            // Assert -- all 3 characters should have the test sprites
            for (int i = 0; i < 3; i++)
            {
                Assert.AreEqual(testWeapon, appearances[i].WeaponRenderer.sprite,
                    $"Character {i}: weapon sprite should have been changed.");
                Assert.AreEqual(testHat, appearances[i].HatRenderer.sprite,
                    $"Character {i}: hat sprite should have been changed.");
                Assert.AreEqual(testShield, appearances[i].ShieldRenderer.sprite,
                    $"Character {i}: shield sprite should have been changed.");
            }
        }

        // ----------------------------------------------------------------
        // Test 4: Head sprites are cycled
        // ----------------------------------------------------------------
        [UnityTest]
        public IEnumerator VisualEquipmentTestLoop_CyclesHeadSprites()
        {
            // Arrange -- instantiate character, let Awake resolve slots
            var character = InstantiatePrefab();
            character.AddComponent<CharacterAppearance>();
            yield return null; // Awake runs

            var appearance = character.GetComponent<CharacterAppearance>();
            Assert.IsNotNull(appearance.HeadRenderer, "HeadRenderer must be resolved.");

            // Record original head sprite
            var origHead = appearance.HeadRenderer.sprite;

            // Create test head sprite
            var testHead = CreateTestSprite();

            var loop = CreateLoop(
                interval: 0.1f,
                weapons: new Sprite[0],
                hats: new Sprite[0],
                shields: new Sprite[0],
                heads: new[] { testHead });

            // Enable component so Start runs
            loop.enabled = true;
            yield return null;

            // Act -- wait for at least one cycle
            yield return new WaitForSeconds(0.2f);

            // Assert -- head should have changed to the test sprite
            Assert.AreEqual(testHead, appearance.HeadRenderer.sprite,
                "Head sprite should have been cycled by VisualEquipmentTestLoop.");
            Assert.AreNotEqual(origHead, appearance.HeadRenderer.sprite,
                "Head sprite should differ from the original prefab sprite.");
        }
    }
}
