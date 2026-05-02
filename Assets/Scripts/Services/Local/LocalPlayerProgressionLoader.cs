using System;
using System.IO;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Economy;
using UnityEngine;

namespace RogueliteAutoBattler.Services.Local
{
    public sealed class LocalPlayerProgressionLoader : IPlayerProgressionLoader, IDisposable
    {
        internal const string DefaultFileName = "player_progression.json";

        private readonly SkillTreeProgress _progress;
        private readonly GoldWallet _wallet;
        private readonly SkillPointWallet _spWallet;
        private readonly Func<SkillTreeData> _activeTreeProvider;
        private readonly string _filePath;
        private readonly Action<int, int> _levelChangedHandler;
        private readonly Action<int> _goldChangedHandler;
        private readonly Action<int> _skillPointChangedHandler;
        private bool _disposed;
        private bool _isSuppressingSave;

        public LocalPlayerProgressionLoader(
            SkillTreeProgress progress,
            GoldWallet wallet,
            SkillPointWallet skillPointWallet,
            Func<SkillTreeData> activeTreeProvider,
            string filePath = null)
        {
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            _spWallet = skillPointWallet ?? throw new ArgumentNullException(nameof(skillPointWallet));
            _activeTreeProvider = activeTreeProvider ?? throw new ArgumentNullException(nameof(activeTreeProvider));
            _filePath = string.IsNullOrEmpty(filePath)
                ? Path.Combine(Application.persistentDataPath, DefaultFileName)
                : filePath;

            _levelChangedHandler = (_, __) => HandleProgressChangedSaveIfNotSuppressed();
            _goldChangedHandler = _ => HandleProgressChangedSaveIfNotSuppressed();
            _skillPointChangedHandler = _ => HandleProgressChangedSaveIfNotSuppressed();

            _progress.OnLevelChanged += _levelChangedHandler;
            _wallet.OnGoldChanged += _goldChangedHandler;
            _spWallet.OnPointsChanged += _skillPointChangedHandler;
        }

        private void HandleProgressChangedSaveIfNotSuppressed()
        {
            if (_isSuppressingSave) return;
            Save();
        }

        public void Load()
        {
            if (!File.Exists(_filePath)) return;

            string json = File.ReadAllText(_filePath);
            if (string.IsNullOrEmpty(json)) return;

            var data = JsonUtility.FromJson<PersistedProgression>(json);
            if (data == null) return;

            var activeTree = _activeTreeProvider();
            if (activeTree != null && IsTreeMismatch(data, activeTree))
                ApplyRefund(data, activeTree);
            else
                ApplyNormalLoad(data);
        }

        private static bool IsTreeMismatch(PersistedProgression data, SkillTreeData activeTree)
        {
            bool hasGuid = !string.IsNullOrEmpty(data.activeTreeGuid);
            bool hasHash = !string.IsNullOrEmpty(data.contentHash);
            if (!hasGuid && !hasHash) return false;

            string currentGuid = activeTree.AssetGuid ?? string.Empty;
            string currentHash = SkillTreeData.ComputeGameplayHash(activeTree);
            bool guidMismatch = hasGuid && data.activeTreeGuid != currentGuid;
            bool hashMismatch = hasHash && data.contentHash != currentHash;
            return guidMismatch || hashMismatch;
        }

        private void ApplyNormalLoad(PersistedProgression data)
        {
            _isSuppressingSave = true;
            try
            {
                if (data.skillTreeLevels != null)
                {
                    for (int i = 0; i < data.skillTreeLevels.Length; i++)
                    {
                        int level = data.skillTreeLevels[i];
                        if (level > 0)
                            _progress.SetLevel(i, level);
                    }
                }
                _wallet.InitializeForPersistence(data.gold);
            }
            finally
            {
                _isSuppressingSave = false;
            }
        }

        private void ApplyRefund(PersistedProgression data, SkillTreeData activeTree)
        {
            var savedLevels = data.skillTreeLevels ?? Array.Empty<int>();
            var refund = SkillTreeRefundCalculator.Compute(activeTree, savedLevels);

            _isSuppressingSave = true;
            try
            {
                _progress.ResetAll();
                _wallet.InitializeForPersistence(data.gold + refund.Gold);
                _spWallet.ResetPoints();
                if (refund.SkillPoint > 0)
                    _spWallet.Add(refund.SkillPoint);
            }
            finally
            {
                _isSuppressingSave = false;
            }
            Save();
        }

        public void Save()
        {
            var activeTree = _activeTreeProvider();
            var data = new PersistedProgression
            {
                skillTreeLevels = SnapshotLevels(),
                gold = _wallet.Gold,
                activeTreeGuid = activeTree != null ? (activeTree.AssetGuid ?? string.Empty) : string.Empty,
                contentHash = activeTree != null ? SkillTreeData.ComputeGameplayHash(activeTree) : string.Empty,
            };

            string json = JsonUtility.ToJson(data);

            string directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(_filePath, json);
        }

        public void ResetAll()
        {
            _isSuppressingSave = true;
            try
            {
                _progress.ResetAll();
                _wallet.ResetGold();
                _spWallet.ResetPoints();
            }
            finally
            {
                _isSuppressingSave = false;
            }

            if (File.Exists(_filePath))
                File.Delete(_filePath);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _progress.OnLevelChanged -= _levelChangedHandler;
            _wallet.OnGoldChanged -= _goldChangedHandler;
            _spWallet.OnPointsChanged -= _skillPointChangedHandler;
        }

        private int[] SnapshotLevels()
        {
            var levels = _progress.Levels;
            int count = levels?.Count ?? 0;
            var array = new int[count];
            for (int i = 0; i < count; i++)
                array[i] = levels[i];
            return array;
        }

        [Serializable]
        private class PersistedProgression
        {
            public int[] skillTreeLevels;
            public int gold;
            public string activeTreeGuid;
            public string contentHash;
        }
    }
}
