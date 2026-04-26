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
        private readonly string _filePath;
        private readonly Action<int, int> _levelChangedHandler;
        private readonly Action<int> _goldChangedHandler;
        private bool _disposed;
        private bool _isSuppressingSave;

        public LocalPlayerProgressionLoader(SkillTreeProgress progress, GoldWallet wallet, string filePath = null)
        {
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            _filePath = string.IsNullOrEmpty(filePath)
                ? Path.Combine(Application.persistentDataPath, DefaultFileName)
                : filePath;

            _levelChangedHandler = (_, __) =>
            {
                if (_isSuppressingSave) return;
                Save();
            };
            _goldChangedHandler = _ =>
            {
                if (_isSuppressingSave) return;
                Save();
            };

            _progress.OnLevelChanged += _levelChangedHandler;
            _wallet.OnGoldChanged += _goldChangedHandler;
        }

        public void Load()
        {
            if (!File.Exists(_filePath)) return;

            string json = File.ReadAllText(_filePath);
            if (string.IsNullOrEmpty(json)) return;

            var data = JsonUtility.FromJson<PersistedProgression>(json);
            if (data == null) return;

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

        public void Save()
        {
            var data = new PersistedProgression
            {
                skillTreeLevels = SnapshotLevels(),
                gold = _wallet.Gold
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
        }
    }
}
