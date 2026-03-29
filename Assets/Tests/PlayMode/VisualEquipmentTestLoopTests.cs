#if UNITY_EDITOR
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

        private VisualEquipmentTestLoop CreateLoop(
            float interval,
            Sprite[] weapons,
            Sprite[] hats,
            Sprite[] shields,
            Sprite[] heads = null)
        {
            var go = new GameObject("VisualEquipmentTestLoop");
            Track(go);

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
        public override void TearDown()
        {
            foreach (var asset in _disposableAssets)
            {
                if (asset != null)
                    Object.DestroyImmediate(asset);
            }
            _disposableAssets.Clear();

            base.TearDown();
        }

        [UnityTest]
        public IEnumerator VisualEquipmentTestLoop_CyclesSpritesAfterInterval()
        {
            var character = InstantiatePrefab();
            character.AddComponent<CharacterAppearance>();
            yield return null;

            var appearance = character.GetComponent<CharacterAppearance>();
            Assert.IsNotNull(appearance.WeaponRenderer, "WeaponRenderer must be resolved.");
            Assert.IsNotNull(appearance.HatRenderer, "HatRenderer must be resolved.");
            Assert.IsNotNull(appearance.ShieldRenderer, "ShieldRenderer must be resolved.");

            var origWeapon = appearance.WeaponRenderer.sprite;
            var origHat = appearance.HatRenderer.sprite;
            var origShield = appearance.ShieldRenderer.sprite;

            var testWeapon = CreateTestSprite();
            var testHat = CreateTestSprite();
            var testShield = CreateTestSprite();

            var loop = CreateLoop(
                interval: 0.1f,
                weapons: new[] { testWeapon },
                hats: new[] { testHat },
                shields: new[] { testShield });

            loop.enabled = true;
            yield return null;

            yield return new WaitForSeconds(0.2f);

            Assert.AreEqual(testWeapon, appearance.WeaponRenderer.sprite,
                "Weapon sprite should have been changed by the loop.");
            Assert.AreEqual(testHat, appearance.HatRenderer.sprite,
                "Hat sprite should have been changed by the loop.");
            Assert.AreEqual(testShield, appearance.ShieldRenderer.sprite,
                "Shield sprite should have been changed by the loop.");

            Assert.AreNotEqual(origWeapon, appearance.WeaponRenderer.sprite,
                "Weapon sprite should differ from the original prefab sprite.");
            Assert.AreNotEqual(origHat, appearance.HatRenderer.sprite,
                "Hat sprite should differ from the original prefab sprite.");
            Assert.AreNotEqual(origShield, appearance.ShieldRenderer.sprite,
                "Shield sprite should differ from the original prefab sprite.");
        }

        [UnityTest]
        public IEnumerator VisualEquipmentTestLoop_EmptyArrays_KeepsDefaults()
        {
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
            yield return null;

            yield return new WaitForSeconds(0.2f);

            Assert.AreEqual(origWeapon, appearance.WeaponRenderer.sprite,
                "Weapon sprite should remain the default when weapon array is empty.");
            Assert.AreEqual(origHat, appearance.HatRenderer.sprite,
                "Hat sprite should remain the default when hat array is empty.");
            Assert.AreEqual(origShield, appearance.ShieldRenderer.sprite,
                "Shield sprite should remain the default when shield array is empty.");
        }

        [UnityTest]
        public IEnumerator VisualEquipmentTestLoop_AffectsAllCharacters()
        {
            var characters = new GameObject[3];
            var appearances = new CharacterAppearance[3];

            for (int i = 0; i < 3; i++)
            {
                characters[i] = InstantiatePrefab();
                characters[i].name = $"Character_{i}";
                characters[i].transform.position = new Vector3(i * 2f, 0f, 0f);
                characters[i].AddComponent<CharacterAppearance>();
            }

            yield return null;

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

            var testWeapon = CreateTestSprite();
            var testHat = CreateTestSprite();
            var testShield = CreateTestSprite();

            var loop = CreateLoop(
                interval: 0.1f,
                weapons: new[] { testWeapon },
                hats: new[] { testHat },
                shields: new[] { testShield });

            loop.enabled = true;
            yield return null;

            yield return new WaitForSeconds(0.2f);

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

        [UnityTest]
        public IEnumerator VisualEquipmentTestLoop_CyclesHeadSprites()
        {
            var character = InstantiatePrefab();
            character.AddComponent<CharacterAppearance>();
            yield return null;

            var appearance = character.GetComponent<CharacterAppearance>();
            Assert.IsNotNull(appearance.HeadRenderer, "HeadRenderer must be resolved.");

            var origHead = appearance.HeadRenderer.sprite;

            var testHead = CreateTestSprite();

            var loop = CreateLoop(
                interval: 0.1f,
                weapons: new Sprite[0],
                hats: new Sprite[0],
                shields: new Sprite[0],
                heads: new[] { testHead });

            loop.enabled = true;
            yield return null;

            yield return new WaitForSeconds(0.2f);

            Assert.AreEqual(testHead, appearance.HeadRenderer.sprite,
                "Head sprite should have been cycled by VisualEquipmentTestLoop.");
            Assert.AreNotEqual(origHead, appearance.HeadRenderer.sprite,
                "Head sprite should differ from the original prefab sprite.");
        }
    }
}
#endif
