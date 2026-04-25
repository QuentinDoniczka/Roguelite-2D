using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Visuals;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class CharacterAppearanceTests : PlayModeTestBase
    {
        private const string PrefabPath = "Assets/Prefabs/Characters/sampleCharacterHuman.prefab";

        private readonly List<Object> _disposableAssets = new List<Object>();

        private GameObject InstantiatePrefab()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.IsNotNull(prefab, $"Prefab not found at {PrefabPath}");
            var instance = Object.Instantiate(prefab);
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
        public IEnumerator CharacterAppearance_Awake_ResolvesAllSlots()
        {
            var go = InstantiatePrefab();
            go.AddComponent<CharacterAppearance>();

            yield return null;

            var appearance = go.GetComponent<CharacterAppearance>();

            Assert.IsNotNull(appearance.HeadRenderer, "HeadRenderer should be resolved after Awake.");
            Assert.IsNotNull(appearance.HatRenderer, "HatRenderer should be resolved after Awake.");
            Assert.IsNotNull(appearance.WeaponRenderer, "WeaponRenderer should be resolved after Awake.");
            Assert.IsNotNull(appearance.ShieldRenderer, "ShieldRenderer should be resolved after Awake.");
        }

        [UnityTest]
        public IEnumerator CharacterAppearance_ApplyAppearance_ChangesSprites()
        {
            var go = InstantiatePrefab();
            go.AddComponent<CharacterAppearance>();

            yield return null;

            var appearance = go.GetComponent<CharacterAppearance>();

            var origHead = appearance.HeadRenderer.sprite;
            var origHat = appearance.HatRenderer.sprite;
            var origWeapon = appearance.WeaponRenderer.sprite;
            var origShield = appearance.ShieldRenderer.sprite;

            var testHead = CreateTestSprite();
            var testHat = CreateTestSprite();
            var testWeapon = CreateTestSprite();
            var testShield = CreateTestSprite();

            appearance.ApplyAppearance(testHead, testHat, testWeapon, testShield);

            Assert.AreEqual(testHead, appearance.HeadRenderer.sprite,
                "Head sprite should be the test sprite after ApplyAppearance.");
            Assert.AreEqual(testHat, appearance.HatRenderer.sprite,
                "Hat sprite should be the test sprite after ApplyAppearance.");
            Assert.AreEqual(testWeapon, appearance.WeaponRenderer.sprite,
                "Weapon sprite should be the test sprite after ApplyAppearance.");
            Assert.AreEqual(testShield, appearance.ShieldRenderer.sprite,
                "Shield sprite should be the test sprite after ApplyAppearance.");

            Assert.AreNotEqual(origHead, appearance.HeadRenderer.sprite,
                "Head sprite should differ from the original.");
            Assert.AreNotEqual(origHat, appearance.HatRenderer.sprite,
                "Hat sprite should differ from the original.");
            Assert.AreNotEqual(origWeapon, appearance.WeaponRenderer.sprite,
                "Weapon sprite should differ from the original.");
            Assert.AreNotEqual(origShield, appearance.ShieldRenderer.sprite,
                "Shield sprite should differ from the original.");
        }

        [UnityTest]
        public IEnumerator CharacterAppearance_ApplyAppearance_NullKeepsDefault()
        {
            var go = InstantiatePrefab();
            go.AddComponent<CharacterAppearance>();

            yield return null;

            var appearance = go.GetComponent<CharacterAppearance>();

            var origHead = appearance.HeadRenderer.sprite;
            var origHat = appearance.HatRenderer.sprite;
            var origWeapon = appearance.WeaponRenderer.sprite;
            var origShield = appearance.ShieldRenderer.sprite;

            appearance.ApplyAppearance(null, null, null, null);

            Assert.AreEqual(origHead, appearance.HeadRenderer.sprite,
                "Head sprite should remain the original when null is passed.");
            Assert.AreEqual(origHat, appearance.HatRenderer.sprite,
                "Hat sprite should remain the original when null is passed.");
            Assert.AreEqual(origWeapon, appearance.WeaponRenderer.sprite,
                "Weapon sprite should remain the original when null is passed.");
            Assert.AreEqual(origShield, appearance.ShieldRenderer.sprite,
                "Shield sprite should remain the original when null is passed.");
        }

        [UnityTest]
        public IEnumerator CharacterAppearance_ApplyAppearance_PartialUpdate()
        {
            var go = InstantiatePrefab();
            go.AddComponent<CharacterAppearance>();

            yield return null;

            var appearance = go.GetComponent<CharacterAppearance>();

            var origHat = appearance.HatRenderer.sprite;
            var origWeapon = appearance.WeaponRenderer.sprite;
            var origShield = appearance.ShieldRenderer.sprite;

            var testHead = CreateTestSprite();

            appearance.ApplyAppearance(testHead, null, null, null);

            Assert.AreEqual(testHead, appearance.HeadRenderer.sprite,
                "Head sprite should be the test sprite after partial ApplyAppearance.");
            Assert.AreEqual(origHat, appearance.HatRenderer.sprite,
                "Hat sprite should remain the original when null is passed.");
            Assert.AreEqual(origWeapon, appearance.WeaponRenderer.sprite,
                "Weapon sprite should remain the original when null is passed.");
            Assert.AreEqual(origShield, appearance.ShieldRenderer.sprite,
                "Shield sprite should remain the original when null is passed.");
        }

        [UnityTest]
        public IEnumerator CharacterAppearance_ApplyAppearance_WeaponSurvivesAnimatorFrames()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            var go = Object.Instantiate(prefab);
            Track(go);

            var appearance = go.AddComponent<CharacterAppearance>();
            yield return null;

            var testSprite = CreateTestSprite();
            appearance.ApplyAppearance(null, null, testSprite, null);

            yield return null;
            yield return null;
            yield return null;

            Assert.AreEqual(testSprite, appearance.WeaponRenderer.sprite,
                "Weapon sprite was overridden by Animator — ApplyAppearance must survive animation frames");
        }
    }
}
