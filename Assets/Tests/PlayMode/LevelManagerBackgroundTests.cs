using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Environment;
using RogueliteAutoBattler.Combat.Levels;
using RogueliteAutoBattler.Data;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class LevelManagerBackgroundTests : PlayModeTestBase
    {
        private LevelDatabase _levelDatabase;
        private readonly List<Sprite> _createdSprites = new List<Sprite>();
        private readonly List<Texture2D> _createdTextures = new List<Texture2D>();

        public override void TearDown()
        {
            base.TearDown();

            foreach (Sprite sprite in _createdSprites)
            {
                if (sprite != null)
                    Object.DestroyImmediate(sprite);
            }
            _createdSprites.Clear();

            foreach (Texture2D texture in _createdTextures)
            {
                if (texture != null)
                    Object.DestroyImmediate(texture);
            }
            _createdTextures.Clear();

            if (_levelDatabase != null)
                Object.DestroyImmediate(_levelDatabase);
        }

        private Sprite CreateTrackedSprite(string spriteName)
        {
            Texture2D texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            texture.name = spriteName + "Texture";
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, 4f, 4f),
                new Vector2(0.5f, 0.5f),
                pixelsPerUnit: 16f);
            sprite.name = spriteName;
            _createdSprites.Add(sprite);
            _createdTextures.Add(texture);
            return sprite;
        }

        private (LevelManager manager, SpriteRenderer groundRenderer, LevelData level) CreateLevelManager(
            Sprite levelBackground,
            Sprite databaseDefaultBackground)
        {
            _levelDatabase = ScriptableObject.CreateInstance<LevelDatabase>();
            _levelDatabase.DefaultBackground = databaseDefaultBackground;

            var emptyWave = new WaveData("EmptyWave", 0f, new List<EnemySpawnData>());
            var emptyStep = new StepData("Step0", new List<WaveData> { emptyWave });
            var level0 = new LevelData("Level0", new List<StepData> { emptyStep });
            level0.Background = levelBackground;

            var stage = new StageData("Stage0", null, new List<LevelData> { level0 });
            _levelDatabase.Stages.Add(stage);

            var levelManagerGo = new GameObject("TestLevelManager");
            levelManagerGo.AddComponent<WorldConveyor>();
            var manager = levelManagerGo.AddComponent<LevelManager>();
            manager.enabled = false;
            Track(levelManagerGo);

            var teamContainer = Track(new GameObject("Team"));
            var enemiesContainer = Track(new GameObject("Enemies"));

            var groundGo = Track(new GameObject("Ground"));
            var groundRenderer = groundGo.AddComponent<SpriteRenderer>();

            SetPrivateField(manager, "_groundRenderer", groundRenderer);

            manager.InitializeForTest(
                teamContainer.transform,
                enemiesContainer.transform,
                levelDatabase: _levelDatabase);

            return (manager, groundRenderer, level0);
        }

        [UnityTest]
        public IEnumerator ApplyLevel_WithAuthoredBackground_AssignsSprite()
        {
            Sprite authoredBackground = CreateTrackedSprite("AuthoredBackground");
            var (manager, groundRenderer, level) = CreateLevelManager(
                levelBackground: authoredBackground,
                databaseDefaultBackground: null);

            manager.ApplyLevel(level);

            yield return null;

            Assert.AreSame(authoredBackground, groundRenderer.sprite,
                "When LevelData.Background is authored, ApplyLevel must assign that sprite to the ground renderer.");
        }

        [UnityTest]
        public IEnumerator ApplyLevel_WithNullLevelBackground_FallsBackToDatabaseDefault()
        {
            Sprite defaultBackground = CreateTrackedSprite("DatabaseDefault");
            var (manager, groundRenderer, level) = CreateLevelManager(
                levelBackground: null,
                databaseDefaultBackground: defaultBackground);

            manager.ApplyLevel(level);

            yield return null;

            Assert.AreSame(defaultBackground, groundRenderer.sprite,
                "When LevelData.Background is null, ApplyLevel must fall back to LevelDatabase.DefaultBackground.");
        }

        [UnityTest]
        public IEnumerator ApplyLevel_WithBothNull_LogsWarningAndKeepsCurrentSprite()
        {
            Sprite previousSprite = CreateTrackedSprite("PreviousSprite");
            var (manager, groundRenderer, level) = CreateLevelManager(
                levelBackground: null,
                databaseDefaultBackground: null);

            groundRenderer.sprite = previousSprite;

            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("No background available"));

            manager.ApplyLevel(level);

            yield return null;

            Assert.AreSame(previousSprite, groundRenderer.sprite,
                "When both LevelData.Background and LevelDatabase.DefaultBackground are null, the current sprite must be preserved.");
        }

        [UnityTest]
        public IEnumerator ApplyLevel_WhenGroundRendererNull_DoesNotThrow()
        {
            var (manager, _, level) = CreateLevelManager(
                levelBackground: null,
                databaseDefaultBackground: null);

            SetPrivateField(manager, "_groundRenderer", null);

            Assert.DoesNotThrow(() => manager.ApplyLevel(level),
                "ApplyLevel must not throw when the ground renderer is unwired.");

            yield return null;
        }

        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            FieldInfo field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"Private field '{fieldName}' not found on {obj.GetType().Name}.");
            field.SetValue(obj, value);
        }
    }
}
