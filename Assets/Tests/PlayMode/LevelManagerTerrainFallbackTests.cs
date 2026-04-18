using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Environment;
using RogueliteAutoBattler.Combat.Levels;
using RogueliteAutoBattler.Combat.Visuals;
using RogueliteAutoBattler.Data;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class LevelManagerTerrainFallbackTests : PlayModeTestBase
    {
        private LevelDatabase _levelDatabase;
        private Sprite _authoredTerrainSprite;
        private Texture2D _authoredTexture;

        public override void TearDown()
        {
            base.TearDown();

            if (_authoredTerrainSprite != null)
                Object.DestroyImmediate(_authoredTerrainSprite);
            if (_authoredTexture != null)
                Object.DestroyImmediate(_authoredTexture);
            if (_levelDatabase != null)
                Object.DestroyImmediate(_levelDatabase);

            InvokeResetProceduralGroundCache();
        }

        private (LevelManager manager, SpriteRenderer groundRenderer) CreateLevelManagerWithTerrain(Sprite terrain)
        {
            _levelDatabase = ScriptableObject.CreateInstance<LevelDatabase>();

            var emptyWave = new WaveData("EmptyWave", 0f, new List<EnemySpawnData>());
            var emptyStep = new StepData("Step0", new List<WaveData> { emptyWave });
            var level0 = new LevelData("Level0", new List<StepData> { emptyStep });

            var stage = new StageData("Stage0", terrain, new List<LevelData> { level0 });
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

            return (manager, groundRenderer);
        }

        [UnityTest]
        public IEnumerator ApplyStage_WithNullTerrain_UsesProceduralFallback()
        {
            var (manager, groundRenderer) = CreateLevelManagerWithTerrain(terrain: null);

            manager.ApplyStage(0);

            yield return null;

            Sprite expected = ProceduralGroundSprite.GetOrCreate();
            Assert.IsNotNull(groundRenderer.sprite,
                "Ground renderer's sprite should not remain null when terrain is null; procedural fallback must be applied.");
            Assert.AreSame(expected, groundRenderer.sprite,
                "When stage.Terrain is null, ground renderer must use the procedural fallback sprite returned by ProceduralGroundSprite.GetOrCreate().");
        }

        [UnityTest]
        public IEnumerator ApplyStage_WithAuthoredTerrain_UsesAuthoredSprite()
        {
            _authoredTexture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            _authoredTexture.name = "AuthoredTestTexture";
            _authoredTerrainSprite = Sprite.Create(
                _authoredTexture,
                new Rect(0f, 0f, 4f, 4f),
                new Vector2(0.5f, 0.5f),
                pixelsPerUnit: 16f);
            _authoredTerrainSprite.name = "AuthoredTestSprite";

            var (manager, groundRenderer) = CreateLevelManagerWithTerrain(_authoredTerrainSprite);

            manager.ApplyStage(0);

            yield return null;

            Assert.AreSame(_authoredTerrainSprite, groundRenderer.sprite,
                "When stage.Terrain is authored, the ground renderer must use that sprite and NOT the procedural fallback.");

            Sprite proceduralSprite = ProceduralGroundSprite.GetOrCreate();
            Assert.AreNotSame(proceduralSprite, groundRenderer.sprite,
                "Authored terrain must take precedence over the procedural fallback.");
        }

        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            FieldInfo field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"Private field '{fieldName}' not found on {obj.GetType().Name}.");
            field.SetValue(obj, value);
        }

        private static void InvokeResetProceduralGroundCache()
        {
            MethodInfo resetMethod = typeof(ProceduralGroundSprite).GetMethod(
                "ResetCacheForTests",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

            if (resetMethod != null)
                resetMethod.Invoke(null, null);
        }
    }
}
