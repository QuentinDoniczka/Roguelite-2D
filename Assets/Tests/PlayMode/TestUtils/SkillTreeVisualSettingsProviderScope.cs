using System;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.UI.Toolkit.SkillTree;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public sealed class SkillTreeVisualSettingsProviderScope : IDisposable
    {
        private readonly Func<SkillTreeVisualSettings> _originalProvider;
        private readonly SkillTreeVisualSettings _stubSettings;
        private bool _disposed;

        public SkillTreeVisualSettingsProviderScope()
        {
            _stubSettings = ScriptableObject.CreateInstance<SkillTreeVisualSettings>();
            _originalProvider = SkillTreeVisualSettingsResolver.Provider;
            SkillTreeVisualSettingsResolver.Provider = () => _stubSettings;
            SkillTreeVisualSettingsResolver.ResetCache();
        }

        public SkillTreeVisualSettings Stub => _stubSettings;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            SkillTreeVisualSettingsResolver.Provider = _originalProvider;
            SkillTreeVisualSettingsResolver.ResetCache();
            if (_stubSettings != null)
                UnityEngine.Object.DestroyImmediate(_stubSettings);
        }
    }
}
