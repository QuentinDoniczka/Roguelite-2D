using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class LevelDataBackgroundTests
    {
        private static LevelData CreateLevelData()
        {
            return new LevelData("Test", new List<StepData>());
        }

        [Test]
        public void LevelData_DefaultFit_IsTile()
        {
            LevelData level = CreateLevelData();
            Assert.AreEqual(BackgroundFit.Tile, level.Fit);
        }

        [Test]
        public void LevelData_DefaultBackground_IsNull()
        {
            LevelData level = CreateLevelData();
            Assert.IsNull(level.Background);
        }

        [Test]
        public void LevelData_BackgroundSetterAndGetter_RoundTrip()
        {
            LevelData level = CreateLevelData();
            Texture2D texture = Texture2D.whiteTexture;
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));

            try
            {
                level.Background = sprite;

                Assert.AreSame(sprite, level.Background);
            }
            finally
            {
                Object.DestroyImmediate(sprite);
            }
        }

        [Test]
        public void LevelData_FitSetterAndGetter_RoundTrip()
        {
            LevelData level = CreateLevelData();

            level.Fit = BackgroundFit.Stretch;
            Assert.AreEqual(BackgroundFit.Stretch, level.Fit);

            level.Fit = BackgroundFit.Tile;
            Assert.AreEqual(BackgroundFit.Tile, level.Fit);
        }

        [Test]
        public void BackgroundFit_TileEqualsZero_StretchEqualsOne()
        {
            Assert.AreEqual(0, (int)BackgroundFit.Tile);
            Assert.AreEqual(1, (int)BackgroundFit.Stretch);
        }
    }
}
