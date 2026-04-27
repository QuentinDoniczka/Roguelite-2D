using NUnit.Framework;
using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class LevelDatabaseDefaultBackgroundTests
    {
        private LevelDatabase _database;

        [SetUp]
        public void SetUp()
        {
            _database = ScriptableObject.CreateInstance<LevelDatabase>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_database);
        }

        [Test]
        public void DefaultBackground_DefaultsToNull()
        {
            Assert.IsNull(_database.DefaultBackground);
        }

        [Test]
        public void DefaultBackground_SetterAndGetter_RoundTrip()
        {
            Texture2D texture = new Texture2D(2, 2);
            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, 2f, 2f), new Vector2(0.5f, 0.5f));

            try
            {
                _database.DefaultBackground = sprite;

                Assert.AreSame(sprite, _database.DefaultBackground);
            }
            finally
            {
                Object.DestroyImmediate(sprite);
                Object.DestroyImmediate(texture);
            }
        }
    }
}
