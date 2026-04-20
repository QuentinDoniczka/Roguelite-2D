using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.Combat.Core
{
    [RequireComponent(typeof(TeamRoster))]
    public class CombatSpawnManager : MonoBehaviour
    {
        [Header("Team")]
        [SerializeField] private TeamDatabase _teamDatabase;

        [Header("Containers")]
        [SerializeField] private Transform _teamContainer;

        [Header("Anchors")]
        [SerializeField] private Transform _teamHomeAnchor;

        private const float DefaultCharacterScale = 1.5f;
        [Header("Scale")]
        [SerializeField] private float _characterScale = DefaultCharacterScale;

        private TeamRoster _teamRoster;

        public float CharacterScale => _characterScale;

        private void Awake()
        {
            _teamRoster = GetComponent<TeamRoster>();
        }

        private void Start()
        {
            SpawnAllies();
        }

        public void SpawnAllies()
        {
            if (_teamRoster == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(CombatSpawnManager)}] TeamRoster component not found on the same GameObject.", this);
#endif
                return;
            }

            if (_teamDatabase == null || _teamDatabase.Allies.Count == 0)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[{nameof(CombatSpawnManager)}] No TeamDatabase assigned or no allies configured!", this);
#endif
                return;
            }

            Transform unusedEnemiesContainer = null;
            CombatSetupHelper.FindContainersIfNeeded(transform, ref _teamContainer, ref unusedEnemiesContainer, nameof(CombatSpawnManager));

            _teamRoster.Spawn(_teamDatabase, _teamContainer, _teamHomeAnchor, _characterScale);
        }

        public void RespawnAllies()
        {
            if (_teamRoster == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(CombatSpawnManager)}] TeamRoster component not found on the same GameObject.", this);
#endif
                return;
            }

            _teamRoster.ReviveAll();
        }

        internal void InitializeForTest(
            TeamDatabase teamDatabase,
            Transform teamContainer,
            Transform teamHomeAnchor,
            TeamRoster teamRoster,
            float characterScale = DefaultCharacterScale)
        {
            _teamDatabase = teamDatabase;
            _teamContainer = teamContainer;
            _teamHomeAnchor = teamHomeAnchor;
            _teamRoster = teamRoster;
            _characterScale = characterScale;
        }
    }
}
