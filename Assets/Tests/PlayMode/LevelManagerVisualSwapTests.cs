using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Levels;
using RogueliteAutoBattler.Combat.Visuals;
using RogueliteAutoBattler.Core;
using RogueliteAutoBattler.Data;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class LevelManagerVisualSwapTests
    {
        private const string NewGameScenePath = "Assets/Scenes/NewGameScene.unity";
        private const string SceneNameWithoutExt = "NewGameScene";
        private const string CombatWorldName = "CombatWorld";
        private const string GroundName = "Ground";

        private bool _sceneWasLoaded;

        [SetUp]
        public void SetUp()
        {
            GameBootstrap.ResetForTest();
            DamageNumberService.ResetForTest();
            CoinFlyService.ResetForTest();
            _sceneWasLoaded = false;
            LogAssert.ignoreFailingMessages = true;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            LogAssert.ignoreFailingMessages = true;

            if (_sceneWasLoaded)
            {
                var emptyScene = SceneManager.CreateScene("EmptyTeardownScene_" + System.Guid.NewGuid().ToString("N"));
                SceneManager.SetActiveScene(emptyScene);

                for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
                {
                    var scene = SceneManager.GetSceneAt(i);
                    if (scene != emptyScene && scene.IsValid() && scene.name == SceneNameWithoutExt)
                    {
                        AsyncOperation unload = SceneManager.UnloadSceneAsync(scene);
                        if (unload != null)
                        {
                            while (!unload.isDone)
                                yield return null;
                        }
                    }
                }

                yield return null;
            }

            GameBootstrap.ResetForTest();
            DamageNumberService.ResetForTest();
            CoinFlyService.ResetForTest();
        }

        [UnityTest]
        public IEnumerator OnPlay_GroundStartsWithDefaultBackground_ThenSwapsToLevel1Sprite()
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(NewGameScenePath, LoadSceneMode.Single);
            if (op == null)
            {
                Assert.Inconclusive($"Scene '{NewGameScenePath}' could not be loaded — verify it is present in EditorBuildSettings.");
                yield break;
            }

            _sceneWasLoaded = true;
            while (!op.isDone)
                yield return null;

            Assert.AreEqual(SceneNameWithoutExt, SceneManager.GetActiveScene().name,
                "Active scene after load must be NewGameScene.");

            SpriteRenderer groundRenderer = FindGroundRenderer();
            Assert.IsNotNull(groundRenderer,
                $"Ground SpriteRenderer not found under '{CombatWorldName}/{GroundName}' in NewGameScene.");

            LevelManager levelManager = Object.FindFirstObjectByType<LevelManager>(FindObjectsInactive.Include);
            Assert.IsNotNull(levelManager,
                "LevelManager not found in NewGameScene.");

            LevelDatabase levelDatabase = GetLevelDatabase(levelManager);
            Assert.IsNotNull(levelDatabase,
                "LevelManager._levelDatabase is not assigned — cannot resolve canonical default/Level1 sprites.");

            Sprite expectedDefaultBackground = levelDatabase.DefaultBackground;
            Assert.IsNotNull(expectedDefaultBackground,
                "LevelDatabase.DefaultBackground is null — cannot validate scene default background.");
            Assert.IsTrue(levelDatabase.Stages != null && levelDatabase.Stages.Count > 0
                          && levelDatabase.Stages[0].Levels != null && levelDatabase.Stages[0].Levels.Count > 0,
                "LevelDatabase has no Stage[0]/Level[0] — cannot validate Level 1 background.");

            Sprite expectedLevel1Background = levelDatabase.Stages[0].Levels[0].Background;
            Assert.IsNotNull(expectedLevel1Background,
                "LevelDatabase Stage[0].Level[0].Background is null — Level 1 must declare a background sprite for ST12.");

            Assert.AreNotSame(expectedDefaultBackground, expectedLevel1Background,
                "Default background and Level 1 background point to the same Sprite — the swap would be invisible. Update LevelDatabase.asset so they differ.");

            Sprite preSwapSprite = groundRenderer.sprite;
            Assert.AreSame(expectedDefaultBackground, preSwapSprite,
                $"Pre-Play: Ground SpriteRenderer must reference LevelDatabase.DefaultBackground (canonical 'backgroundtest'). " +
                $"Got sprite '{(preSwapSprite != null ? preSwapSprite.name : "<null>")}', " +
                $"texture '{(preSwapSprite != null && preSwapSprite.texture != null ? preSwapSprite.texture.name : "<null>")}'.");

            const int maxFramesToWaitForSwap = 600;
            int framesWaited = 0;
            while (groundRenderer.sprite == preSwapSprite && framesWaited < maxFramesToWaitForSwap)
            {
                yield return null;
                framesWaited++;
            }

            Assert.AreSame(expectedLevel1Background, groundRenderer.sprite,
                $"Post-swap: after LevelManager auto-starts Level 1, the Ground SpriteRenderer must reference " +
                $"LevelDatabase.Stages[0].Levels[0].Background (canonical 'grid_ground'). " +
                $"Frames waited: {framesWaited}/{maxFramesToWaitForSwap}. " +
                $"Got sprite '{(groundRenderer.sprite != null ? groundRenderer.sprite.name : "<null>")}', " +
                $"texture '{(groundRenderer.sprite != null && groundRenderer.sprite.texture != null ? groundRenderer.sprite.texture.name : "<null>")}'. " +
                $"If this fails because LevelManager never auto-starts the first level on scene load, " +
                $"that is a follow-up fix — the end-to-end coverage still belongs here.");
        }

        private static SpriteRenderer FindGroundRenderer()
        {
            GameObject combatWorld = GameObject.Find(CombatWorldName);
            if (combatWorld == null) return null;

            Transform groundTransform = combatWorld.transform.Find(GroundName);
            if (groundTransform == null)
            {
                SpriteRenderer[] all = combatWorld.GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
                foreach (SpriteRenderer sr in all)
                {
                    if (sr != null && sr.gameObject.name == GroundName) return sr;
                }
                return null;
            }

            return groundTransform.GetComponent<SpriteRenderer>();
        }

        private static LevelDatabase GetLevelDatabase(LevelManager levelManager)
        {
            FieldInfo field = typeof(LevelManager).GetField("_levelDatabase", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, "LevelManager._levelDatabase field not found via reflection.");
            return field.GetValue(levelManager) as LevelDatabase;
        }
    }
}
