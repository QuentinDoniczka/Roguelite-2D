using NUnit.Framework;
using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class SkillTreeProgressTests
    {
        private SkillTreeProgress _progress;

        [SetUp]
        public void SetUp()
        {
            _progress = ScriptableObject.CreateInstance<SkillTreeProgress>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_progress);
        }

        [Test]
        public void GetLevel_DefaultIsZero()
        {
            Assert.AreEqual(0, _progress.GetLevel(0));
            Assert.AreEqual(0, _progress.GetLevel(5));
        }

        [Test]
        public void SetLevel_ThenGetLevel_ReturnsValue()
        {
            _progress.SetLevel(0, 3);
            Assert.AreEqual(3, _progress.GetLevel(0));
        }

        [Test]
        public void SetLevel_ExpandsList()
        {
            _progress.SetLevel(5, 2);
            Assert.AreEqual(2, _progress.GetLevel(5));
            Assert.AreEqual(0, _progress.GetLevel(3));
        }

        [Test]
        public void GetLevel_NegativeIndex_ReturnsZero()
        {
            Assert.AreEqual(0, _progress.GetLevel(-1));
        }

        [Test]
        public void ResetAll_ClearsAllLevels()
        {
            _progress.SetLevel(0, 5);
            _progress.SetLevel(2, 3);
            _progress.ResetAll();
            Assert.AreEqual(0, _progress.GetLevel(0));
            Assert.AreEqual(0, _progress.GetLevel(2));
        }
    }
}
