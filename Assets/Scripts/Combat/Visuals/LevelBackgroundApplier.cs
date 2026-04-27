using RogueliteAutoBattler.Combat.Environment;
using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.Combat.Visuals
{
    internal sealed class LevelBackgroundApplier
    {
        internal void Apply(SpriteRenderer groundRenderer, LevelData level, Sprite databaseDefault)
        {
            if (groundRenderer == null) return;

            Sprite levelBackground = level != null ? level.Background : null;
            Sprite sprite = levelBackground != null ? levelBackground : databaseDefault;

            if (sprite != null)
            {
                groundRenderer.sprite = sprite;
#if UNITY_EDITOR
                Debug.Log($"[{nameof(LevelBackgroundApplier)}] Applied background '{sprite.name}' for level '{level?.LevelName}'");
#endif
            }
            else
            {
                string levelName = level != null ? level.LevelName : "<null>";
                Debug.LogWarning($"[{nameof(LevelBackgroundApplier)}] No background available for level '{levelName}' (DefaultBackground also null) — keeping current sprite.");
            }

            if (level != null && groundRenderer.TryGetComponent(out GroundFitter fitter))
            {
                fitter.SetFitMode(level.Fit);
            }
        }
    }
}
