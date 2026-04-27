using NUnit.Framework;
using RogueliteAutoBattler.Data;
using UnityEditor;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class LevelDatabaseAssetIntegrityTests
    {
        private const string LevelDatabaseAssetPath = "Assets/Data/LevelDatabase.asset";

        [Test]
        public void LevelDatabaseAsset_HasAtLeastOneStage_AndOneLevel()
        {
            var db = AssetDatabase.LoadAssetAtPath<LevelDatabase>(LevelDatabaseAssetPath);
            Assert.IsNotNull(db, "LevelDatabase asset not found");
            Assert.Greater(db.Stages.Count, 0, "LevelDatabase must have at least one stage");
            Assert.Greater(db.Stages[0].Levels.Count, 0, "Stage 0 must have at least one level");
        }

        [Test]
        public void LevelDatabaseAsset_DefaultBackground_IsAssigned()
        {
            var db = AssetDatabase.LoadAssetAtPath<LevelDatabase>(LevelDatabaseAssetPath);
            Assert.IsNotNull(db, "LevelDatabase asset not found");
            Assert.IsNotNull(db.DefaultBackground, "DefaultBackground must be assigned");
            Assert.That(db.DefaultBackground.name, Does.Contain("backgroundtest").IgnoreCase);
        }

        [Test]
        public void LevelDatabaseAsset_AllLevels_FitDefaultsToTile()
        {
            var db = AssetDatabase.LoadAssetAtPath<LevelDatabase>(LevelDatabaseAssetPath);
            Assert.IsNotNull(db, "LevelDatabase asset not found");
            foreach (var stage in db.Stages)
            {
                foreach (var level in stage.Levels)
                {
                    Assert.AreEqual(BackgroundFit.Tile, level.Fit,
                        $"Level '{level.LevelName}' in stage '{stage.StageName}' must have Fit=Tile");
                }
            }
        }
    }
}
