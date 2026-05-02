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
        internal const string FieldNameSkillTreeLevels = nameof(PersistedProgression.skillTreeLevels);
        internal const string FieldNameGold = nameof(PersistedProgression.gold);
        internal const string FieldNameActiveTreeGuid = nameof(PersistedProgression.activeTreeGuid);
        internal const string FieldNameContentHash = nameof(PersistedProgression.contentHash);

        private readonly SkillTreeProgress _progress;
        private readonly GoldWallet _wallet;
        private readonly SkillPointWallet _skillPointWallet;
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
            _skillPointWallet = skillPointWallet ?? throw new ArgumentNullException(nameof(skillPointWallet));
            _activeTreeProvider = activeTreeProvider ?? throw new ArgumentNullException(nameof(activeTreeProvider));
            _filePath = string.IsNullOrEmpty(filePath)
                ? Path.Combine(Application.persistentDataPath, DefaultFileName)
                : filePath;

            _levelChangedHandler = (_, _) => HandleProgressChangedSaveIfNotSuppressed();
            _goldChangedHandler = _ => HandleProgressChangedSaveIfNotSuppressed();
            _skillPointChangedHandler = _ => HandleProgressChangedSaveIfNotSuppressed();

            _progress.OnLevelChanged += _levelChangedHandler;
            _wallet.OnGoldChanged += _goldChangedHandler;
            _skillPointWallet.OnPointsChanged += _skillPointChangedHandler;
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

            bool guidMismatch = hasGuid && data.activeTreeGuid != GuidOf(activeTree);
            bool hashMismatch = hasHash && data.contentHash != HashOf(activeTree);
            return guidMismatch || hashMismatch;
        }

        private static string GuidOf(SkillTreeData tree)
            => tree != null ? (tree.AssetGuid ?? string.Empty) : string.Empty;

        private static string HashOf(SkillTreeData tree)
            => tree != null ? SkillTreeData.ComputeGameplayHash(tree) : string.Empty;

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
                _skillPointWallet.ResetPoints();
                if (refund.SkillPoint > 0)
                    _skillPointWallet.Add(refund.SkillPoint);
            }
            finally
            {
                _isSuppressingSave = false;
            }
            // Persist refunded state immediately so next launch starts from a consistent baseline.
            Save();
        }

        public void Save()
        {
            var activeTree = _activeTreeProvider();
            var data = new PersistedProgression
            {
                skillTreeLevels = SnapshotLevels(),
                gold = _wallet.Gold,
                activeTreeGuid = GuidOf(activeTree),
                contentHash = HashOf(activeTree),
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
                _skillPointWallet.ResetPoints();
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
            _skillPointWallet.OnPointsChanged -= _skillPointChangedHandler;
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
