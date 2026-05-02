using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Economy;
using RogueliteAutoBattler.Services.Local;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class SkillTreeChangeRefundTests
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
            if (_progress != null) UnityEngine.Object.DestroyImmediate(_progress);
            if (_walletGameObject != null) UnityEngine.Object.DestroyImmediate(_walletGameObject);
            if (_spWalletGameObject != null) UnityEngine.Object.DestroyImmediate(_spWalletGameObject);
            if (_activeTree != null) UnityEngine.Object.DestroyImmediate(_activeTree);
            if (Directory.Exists(_tempDirectory))
                Directory.Delete(_tempDirectory, true);
        }

        private static SkillTreeData.SkillNodeEntry MakeNode(
            int id,
            SkillTreeData.CostType costType,
            int baseCost,
            int maxLevel = 5)
        {
            return new SkillTreeData.SkillNodeEntry
            {
                id = id,
                position = Vector2.zero,
                connectedNodeIds = new List<int>(),
                costType = costType,
                maxLevel = maxLevel,
                baseCost = baseCost,
                costMultiplierOdd = 1f,
                costMultiplierEven = 1f,
                costAdditivePerLevel = 0,
                statModifierType = StatType.Hp,
                statModifierMode = SkillTreeData.StatModifierMode.Flat,
                statModifierValuePerLevel = 1f
            };
        }

        private static List<SkillTreeData.SkillNodeEntry> CentralPlus(params SkillTreeData.SkillNodeEntry[] nodes)
        {
            var list = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, SkillTreeData.CostType.Gold, 100)
            };
            list.AddRange(nodes);
            return list;
        }

        [Test]
        public void Load_NoMismatch_DoesNotRefund()
        {
            _activeTree.InitializeForTest(CentralPlus(MakeNode(1, SkillTreeData.CostType.Gold, 10)));

            using (var saver = new LocalPlayerProgressionLoader(_progress, _wallet, _spWallet, () => _activeTree, _tempFilePath))
            {
                _wallet.Add(200);
                _progress.SetLevel(1, 2);
            }

            var freshProgress = ScriptableObject.CreateInstance<SkillTreeProgress>();
            var freshWalletGo = new GameObject("FreshWallet");
            var freshWallet = freshWalletGo.AddComponent<GoldWallet>();
            var freshSpWalletGo = new GameObject("FreshSpWallet");
            var freshSpWallet = freshSpWalletGo.AddComponent<SkillPointWallet>();
            try
            {
                using var loader = new LocalPlayerProgressionLoader(freshProgress, freshWallet, freshSpWallet, () => _activeTree, _tempFilePath);
                loader.Load();

                Assert.AreEqual(200, freshWallet.Gold);
                Assert.AreEqual(2, freshProgress.GetLevel(1));
                Assert.AreEqual(0, freshSpWallet.Points);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(freshProgress);
                UnityEngine.Object.DestroyImmediate(freshWalletGo);
                UnityEngine.Object.DestroyImmediate(freshSpWalletGo);
            }
        }

        [Test]
        public void Load_HashMismatch_RefundsAndResets()
        {
            _activeTree.InitializeForTest(CentralPlus(MakeNode(1, SkillTreeData.CostType.Gold, 10)));

            using (var saver = new LocalPlayerProgressionLoader(_progress, _wallet, _spWallet, () => _activeTree, _tempFilePath))
            {
                _wallet.Add(50);
                _progress.SetLevel(1, 3);
            }

            string fileBeforeMutation = File.ReadAllText(_tempFilePath);
            string oldHashMarker = SkillTreeData.ComputeGameplayHash(_activeTree);
            StringAssert.Contains(oldHashMarker, fileBeforeMutation);

            _activeTree.InitializeForTest(CentralPlus(MakeNode(1, SkillTreeData.CostType.Gold, 999)));

            var freshProgress = ScriptableObject.CreateInstance<SkillTreeProgress>();
            var freshWalletGo = new GameObject("FreshWallet");
            var freshWallet = freshWalletGo.AddComponent<GoldWallet>();
            var freshSpWalletGo = new GameObject("FreshSpWallet");
            var freshSpWallet = freshSpWalletGo.AddComponent<SkillPointWallet>();
            try
            {
                using var loader = new LocalPlayerProgressionLoader(freshProgress, freshWallet, freshSpWallet, () => _activeTree, _tempFilePath);
                loader.Load();

                int expectedRefund = 999 + 999 + 999;
                Assert.AreEqual(50 + expectedRefund, freshWallet.Gold);
                Assert.AreEqual(0, freshProgress.GetLevel(1));
                Assert.AreEqual(0, freshSpWallet.Points);

                string newHash = SkillTreeData.ComputeGameplayHash(_activeTree);
                string fileAfter = File.ReadAllText(_tempFilePath);
                StringAssert.Contains($"\"contentHash\":\"{newHash}\"", fileAfter);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(freshProgress);
                UnityEngine.Object.DestroyImmediate(freshWalletGo);
                UnityEngine.Object.DestroyImmediate(freshSpWalletGo);
            }
        }

        [Test]
        public void Load_GuidMismatch_RefundsAndResets()
        {
            _activeTree.InitializeForTest(CentralPlus(MakeNode(1, SkillTreeData.CostType.Gold, 10)));

            Directory.CreateDirectory(_tempDirectory);
            File.WriteAllText(_tempFilePath,
                "{\"skillTreeLevels\":[0,2],\"gold\":100,\"activeTreeGuid\":\"deadbeef\",\"contentHash\":\"\"}");

            using var loader = new LocalPlayerProgressionLoader(_progress, _wallet, _spWallet, () => _activeTree, _tempFilePath);
            loader.Load();

            int expectedRefund = 10 + 10;
            Assert.AreEqual(100 + expectedRefund, _wallet.Gold);
            Assert.AreEqual(0, _progress.GetLevel(1));
            Assert.AreEqual(0, _spWallet.Points);
        }

        [Test]
        public void Load_HashMismatch_RefundSplitsByCostType()
        {
            _activeTree.InitializeForTest(CentralPlus(
                MakeNode(1, SkillTreeData.CostType.Gold, 10),
                MakeNode(2, SkillTreeData.CostType.SkillPoint, 5)));

            using (var saver = new LocalPlayerProgressionLoader(_progress, _wallet, _spWallet, () => _activeTree, _tempFilePath))
            {
                _wallet.Add(20);
                _progress.SetLevel(1, 2);
                _progress.SetLevel(2, 3);
            }

            _activeTree.InitializeForTest(CentralPlus(
                MakeNode(1, SkillTreeData.CostType.Gold, 777),
                MakeNode(2, SkillTreeData.CostType.SkillPoint, 5)));

            var freshProgress = ScriptableObject.CreateInstance<SkillTreeProgress>();
            var freshWalletGo = new GameObject("FreshWallet");
            var freshWallet = freshWalletGo.AddComponent<GoldWallet>();
            var freshSpWalletGo = new GameObject("FreshSpWallet");
            var freshSpWallet = freshSpWalletGo.AddComponent<SkillPointWallet>();
            try
            {
                using var loader = new LocalPlayerProgressionLoader(freshProgress, freshWallet, freshSpWallet, () => _activeTree, _tempFilePath);
                loader.Load();

                int expectedGoldRefund = 777 + 777;
                int expectedSpRefund = 5 + 5 + 5;
                Assert.AreEqual(20 + expectedGoldRefund, freshWallet.Gold);
                Assert.AreEqual(expectedSpRefund, freshSpWallet.Points);
                Assert.AreEqual(0, freshProgress.GetLevel(1));
                Assert.AreEqual(0, freshProgress.GetLevel(2));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(freshProgress);
                UnityEngine.Object.DestroyImmediate(freshWalletGo);
                UnityEngine.Object.DestroyImmediate(freshSpWalletGo);
            }
        }
    }
}
