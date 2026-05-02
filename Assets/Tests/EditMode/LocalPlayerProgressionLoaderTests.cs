using System;
using System.IO;
using NUnit.Framework;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Economy;
using RogueliteAutoBattler.Services.Local;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class LocalPlayerProgressionLoaderTests
    {
        private SkillTreeProgress _progress;
        private GameObject _walletGameObject;
        private GoldWallet _wallet;
        private GameObject _spWalletGameObject;
        private SkillPointWallet _spWallet;
        private SkillTreeData _activeTree;
        private string _tempDirectory;
        private string _tempFilePath;

        [SetUp]
        public void SetUp()
        {
            _progress = ScriptableObject.CreateInstance<SkillTreeProgress>();
            _walletGameObject = new GameObject("TestWallet");
            _wallet = _walletGameObject.AddComponent<GoldWallet>();
            _spWalletGameObject = new GameObject("TestSpWallet");
            _spWallet = _spWalletGameObject.AddComponent<SkillPointWallet>();
            _activeTree = ScriptableObject.CreateInstance<SkillTreeData>();

            _tempDirectory = Path.Combine(Path.GetTempPath(), "roguelite-tests", Guid.NewGuid().ToString());
            _tempFilePath = Path.Combine(_tempDirectory, LocalPlayerProgressionLoader.DefaultFileName);
        }

        [TearDown]
        public void TearDown()
        {
            if (_progress != null)
                UnityEngine.Object.DestroyImmediate(_progress);
            if (_walletGameObject != null)
                UnityEngine.Object.DestroyImmediate(_walletGameObject);
            if (_spWalletGameObject != null)
                UnityEngine.Object.DestroyImmediate(_spWalletGameObject);
            if (_activeTree != null)
                UnityEngine.Object.DestroyImmediate(_activeTree);

            if (Directory.Exists(_tempDirectory))
                Directory.Delete(_tempDirectory, true);
        }

        [Test]
        public void Load_FileMissing_NoOp()
        {
            using var loader = new LocalPlayerProgressionLoader(_progress, _wallet, _spWallet, () => _activeTree, _tempFilePath);

            Assert.IsFalse(File.Exists(_tempFilePath));

            loader.Load();

            Assert.AreEqual(0, _wallet.Gold);
            Assert.AreEqual(0, _progress.GetLevel(0));
        }

        [Test]
        public void Save_ThenLoad_RestoresState()
        {
            using (var saver = new LocalPlayerProgressionLoader(_progress, _wallet, _spWallet, () => _activeTree, _tempFilePath))
            {
                _progress.SetLevel(0, 3);
                _progress.SetLevel(2, 5);
                _wallet.Add(150);
            }

            Assert.IsTrue(File.Exists(_tempFilePath));

            var freshProgress = ScriptableObject.CreateInstance<SkillTreeProgress>();
            var freshWalletGameObject = new GameObject("FreshWallet");
            var freshWallet = freshWalletGameObject.AddComponent<GoldWallet>();
            var freshSpWalletGameObject = new GameObject("FreshSpWallet");
            var freshSpWallet = freshSpWalletGameObject.AddComponent<SkillPointWallet>();
            try
            {
                using var loader = new LocalPlayerProgressionLoader(freshProgress, freshWallet, freshSpWallet, () => _activeTree, _tempFilePath);
                loader.Load();

                Assert.AreEqual(150, freshWallet.Gold);
                Assert.AreEqual(3, freshProgress.GetLevel(0));
                Assert.AreEqual(5, freshProgress.GetLevel(2));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(freshProgress);
                UnityEngine.Object.DestroyImmediate(freshWalletGameObject);
                UnityEngine.Object.DestroyImmediate(freshSpWalletGameObject);
            }
        }

        [Test]
        public void Save_PersistsImmediatelyOnLevelChange()
        {
            using var loader = new LocalPlayerProgressionLoader(_progress, _wallet, _spWallet, () => _activeTree, _tempFilePath);

            _progress.SetLevel(1, 4);

            Assert.IsTrue(File.Exists(_tempFilePath));
            string json = File.ReadAllText(_tempFilePath);
            StringAssert.Contains("\"skillTreeLevels\":[0,4]", json);
            StringAssert.Contains("\"gold\":0", json);
        }

        [Test]
        public void Save_PersistsImmediatelyOnGoldChange()
        {
            using var loader = new LocalPlayerProgressionLoader(_progress, _wallet, _spWallet, () => _activeTree, _tempFilePath);

            _wallet.Add(75);

            Assert.IsTrue(File.Exists(_tempFilePath));
            string json = File.ReadAllText(_tempFilePath);
            StringAssert.Contains("\"gold\":75", json);
        }

        [Test]
        public void ResetAll_DeletesFileAndZeroesState()
        {
            using var loader = new LocalPlayerProgressionLoader(_progress, _wallet, _spWallet, () => _activeTree, _tempFilePath);

            _progress.SetLevel(0, 3);
            _wallet.Add(200);
            _spWallet.Add(7);

            Assert.IsTrue(File.Exists(_tempFilePath));

            loader.ResetAll();

            Assert.IsFalse(File.Exists(_tempFilePath));
            Assert.AreEqual(0, _progress.GetLevel(0));
            Assert.AreEqual(0, _wallet.Gold);
            Assert.AreEqual(0, _spWallet.Points);
        }

        [Test]
        public void Dispose_StopsAutoSave()
        {
            var loader = new LocalPlayerProgressionLoader(_progress, _wallet, _spWallet, () => _activeTree, _tempFilePath);

            _progress.SetLevel(0, 1);
            Assert.IsTrue(File.Exists(_tempFilePath));

            loader.Dispose();

            File.Delete(_tempFilePath);
            Assert.IsFalse(File.Exists(_tempFilePath));

            _progress.SetLevel(0, 2);

            Assert.IsFalse(File.Exists(_tempFilePath));
        }

        [Test]
        public void Load_LegacyJsonWithoutGuidOrHash_AppliesLevelsWithoutRefund()
        {
            Directory.CreateDirectory(_tempDirectory);
            File.WriteAllText(_tempFilePath, "{\"skillTreeLevels\":[0,3],\"gold\":150}");

            using var loader = new LocalPlayerProgressionLoader(_progress, _wallet, _spWallet, () => _activeTree, _tempFilePath);
            loader.Load();

            Assert.AreEqual(150, _wallet.Gold);
            Assert.AreEqual(3, _progress.GetLevel(1));
            Assert.AreEqual(0, _spWallet.Points);
        }

        [Test]
        public void Save_WritesGuidAndHashFromActiveTree()
        {
            using var loader = new LocalPlayerProgressionLoader(_progress, _wallet, _spWallet, () => _activeTree, _tempFilePath);
            _wallet.Add(50);

            string json = File.ReadAllText(_tempFilePath);
            string expectedHash = SkillTreeData.ComputeGameplayHash(_activeTree);
            StringAssert.Contains($"\"contentHash\":\"{expectedHash}\"", json);
            StringAssert.Contains("\"activeTreeGuid\":\"\"", json);
        }
    }
}
