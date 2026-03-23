using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    /// <summary>
    /// Reads the LevelDatabase at runtime and applies the current stage's terrain sprite
    /// to the Ground SpriteRenderer. Attach to the CombatWorld root.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private LevelDatabase _levelDatabase;
        [SerializeField] private SpriteRenderer _groundRenderer;
        [SerializeField] private int _currentStageIndex;

        private void Start()
        {
            ApplyStage(_currentStageIndex);
        }

        /// <summary>
        /// Applies the terrain sprite for the given stage index.
        /// </summary>
        public void ApplyStage(int stageIndex)
        {
            if (_levelDatabase == null)
            {
                Debug.LogWarning($"[{nameof(LevelManager)}] No LevelDatabase assigned.");
                return;
            }

            if (_levelDatabase.Stages == null || stageIndex < 0 || stageIndex >= _levelDatabase.Stages.Count)
            {
                Debug.LogWarning($"[{nameof(LevelManager)}] Stage index {stageIndex} out of range.");
                return;
            }

            var stage = _levelDatabase.Stages[stageIndex];
            _currentStageIndex = stageIndex;

            if (stage.Terrain != null && _groundRenderer != null)
            {
                _groundRenderer.sprite = stage.Terrain;
                Debug.Log($"[{nameof(LevelManager)}] Applied terrain '{stage.Terrain.name}' for stage '{stage.StageName}'");
            }
        }
    }
}
