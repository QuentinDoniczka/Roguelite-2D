using NUnit.Framework;
using UnityEditor;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class EditorBuildSettingsSceneTests
    {
        private const string NewGameScenePath = "Assets/Scenes/NewGameScene.unity";
        private const string LegacyGameScenePath = "Assets/Scenes/GameScene.unity";

        [Test]
        public void NewGameScene_IsPresent_InBuildSettings()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;

            int matchCount = 0;
            EditorBuildSettingsScene matched = null;
            foreach (EditorBuildSettingsScene scene in scenes)
            {
                if (scene.path == NewGameScenePath)
                {
                    matchCount++;
                    matched = scene;
                }
            }

            Assert.AreEqual(1, matchCount,
                $"Exactly one entry with path '{NewGameScenePath}' must be present in EditorBuildSettings.scenes (found {matchCount}).");
            Assert.IsTrue(matched.enabled,
                $"Scene '{NewGameScenePath}' must be enabled in EditorBuildSettings.scenes.");
        }

        [Test]
        public void LegacyGameScene_IsAbsent_InBuildSettings()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;

            foreach (EditorBuildSettingsScene scene in scenes)
            {
                Assert.AreNotEqual(LegacyGameScenePath, scene.path,
                    $"Legacy scene '{LegacyGameScenePath}' must not be listed in EditorBuildSettings.scenes — it was deleted in #199.");
            }
        }

        [Test]
        public void BuildSettings_ContainsExactlyOneEnabledScene()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;

            int enabledCount = 0;
            foreach (EditorBuildSettingsScene scene in scenes)
            {
                if (scene.enabled)
                    enabledCount++;
            }

            Assert.AreEqual(1, enabledCount,
                "Exactly one enabled scene is expected in EditorBuildSettings (NewGameScene only).");
        }
    }
}
