using NUnit.Framework;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Editor.Builders;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class CombatWorldBuilderGroundTests
    {
        [SetUp]
        public void SetUp()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        [TearDown]
        public void TearDown()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        [Test]
        public void CombatWorld_Ground_UsesLevelDatabaseDefaultBackground_WhenAssigned()
        {
            var testTexture = new Texture2D(4, 4);
            testTexture.name = "TEST_DEFAULT_BG_TEX";
            var testSprite = Sprite.Create(
                testTexture,
                new Rect(0, 0, 4, 4),
                new Vector2(0.5f, 0.5f));
            testSprite.name = "TEST_DEFAULT_BG";

            var levelDb = ScriptableObject.CreateInstance<LevelDatabase>();
            levelDb.DefaultBackground = testSprite;

            var combatWorld = CombatWorldBuilder.CreateCombatWorld(levelDbOverride: levelDb);

            var groundGo = combatWorld.transform.Find("Ground");
            Assert.IsNotNull(groundGo, "Ground child GameObject must exist under CombatWorld.");
            var groundRenderer = groundGo.GetComponent<SpriteRenderer>();
            Assert.IsNotNull(groundRenderer, "Ground must have a SpriteRenderer.");
            Assert.AreEqual("TEST_DEFAULT_BG", groundRenderer.sprite.name,
                "Ground SpriteRenderer.sprite must be the DefaultBackground sprite from LevelDatabase.");

            Object.DestroyImmediate(testSprite);
            Object.DestroyImmediate(testTexture);
            Object.DestroyImmediate(levelDb);
        }

        [Test]
        public void CombatWorld_Ground_FallsBackToGridGroundAsset_WhenDefaultBackgroundNull()
        {
            var levelDb = ScriptableObject.CreateInstance<LevelDatabase>();
            levelDb.DefaultBackground = null;

            var combatWorld = CombatWorldBuilder.CreateCombatWorld(levelDbOverride: levelDb);

            var groundGo = combatWorld.transform.Find("Ground");
            Assert.IsNotNull(groundGo, "Ground child GameObject must exist under CombatWorld.");
            var groundRenderer = groundGo.GetComponent<SpriteRenderer>();
            Assert.IsNotNull(groundRenderer, "Ground must have a SpriteRenderer.");
            Assert.IsNotNull(groundRenderer.sprite,
                "Ground SpriteRenderer.sprite must not be null when falling back to grid_ground.png.");

            var spritePath = AssetDatabase.GetAssetPath(groundRenderer.sprite);
            StringAssert.Contains("grid_ground", spritePath,
                "Fallback sprite asset path must reference grid_ground.png.");

            Object.DestroyImmediate(levelDb);
        }
    }
}
