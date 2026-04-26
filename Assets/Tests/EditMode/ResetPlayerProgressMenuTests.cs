using System;
using System.IO;
using NUnit.Framework;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Editor.Tools;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class ResetPlayerProgressMenuTests
    {
        private string _tempDirectory;
        private string _tempFilePath;
        private SkillTreeProgress _progress;

        [SetUp]
        public void SetUp()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), "roguelite-tests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDirectory);
            _tempFilePath = Path.Combine(_tempDirectory, "progression.json");
            _progress = ScriptableObject.CreateInstance<SkillTreeProgress>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_progress != null)
                UnityEngine.Object.DestroyImmediate(_progress);

            if (Directory.Exists(_tempDirectory))
                Directory.Delete(_tempDirectory, true);
        }

        [Test]
        public void PerformReset_FileExists_DeletesFile()
        {
            File.WriteAllText(_tempFilePath, "{\"dummy\":true}");
            Assert.IsTrue(File.Exists(_tempFilePath));

            ResetPlayerProgressMenu.PerformReset(_tempFilePath, null);

            Assert.IsFalse(File.Exists(_tempFilePath));
        }

        [Test]
        public void PerformReset_FileMissing_NoOp()
        {
            Assert.IsFalse(File.Exists(_tempFilePath));

            Assert.DoesNotThrow(() => ResetPlayerProgressMenu.PerformReset(_tempFilePath, null));

            Assert.IsFalse(File.Exists(_tempFilePath));
        }

        [Test]
        public void PerformReset_ResetsProgressAssetLevels()
        {
            _progress.SetLevel(0, 5);
            _progress.SetLevel(2, 3);
            Assert.AreEqual(5, _progress.GetLevel(0));
            Assert.AreEqual(3, _progress.GetLevel(2));

            ResetPlayerProgressMenu.PerformReset(_tempFilePath, _progress);

            Assert.AreEqual(0, _progress.GetLevel(0));
            Assert.AreEqual(0, _progress.GetLevel(2));
        }

        [Test]
        public void PerformReset_NullAsset_NoException()
        {
            Assert.DoesNotThrow(() => ResetPlayerProgressMenu.PerformReset(_tempFilePath, null));
        }
    }
}
