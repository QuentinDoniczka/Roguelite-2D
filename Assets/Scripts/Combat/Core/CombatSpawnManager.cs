using RogueliteAutoBattler.Common;
using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.Combat.Core
{
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

        public float CharacterScale => _characterScale;

        private void Start()
        {
            SpawnAllies();
        }

        public void SpawnAllies()
        {
            if (_teamDatabase == null || _teamDatabase.Allies.Count == 0)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[{nameof(CombatSpawnManager)}] No TeamDatabase assigned or no allies configured!", this);
#endif
                return;
            }

            Transform unusedEnemiesContainer = null;
            CombatSetupHelper.FindContainersIfNeeded(transform, ref _teamContainer, ref unusedEnemiesContainer, nameof(CombatSpawnManager));

            Transform teamAnchor = _teamHomeAnchor;
            Vector2 anchorPos = teamAnchor != null ? (Vector2)teamAnchor.position : Vector2.zero;

            var allies = _teamDatabase.Allies;
            Vector2[] positions = FormationLayout.GetPositions(anchorPos, allies.Count, facingRight: true, characterScale: _characterScale);

            for (int i = 0; i < allies.Count; i++)
            {
                Vector2 offset = positions[i] - anchorPos;
                SpawnAlly(allies[i], teamAnchor, positions[i], offset);
            }
        }

        public void RespawnAllies()
        {
            CombatSetupHelper.DestroyAllChildren(_teamContainer);
            SpawnAllies();
        }

        internal void InitializeForTest(TeamDatabase teamDatabase, Transform teamContainer, Transform teamHomeAnchor, float characterScale = DefaultCharacterScale)
        {
            _teamDatabase = teamDatabase;
            _teamContainer = teamContainer;
            _teamHomeAnchor = teamHomeAnchor;
            _characterScale = characterScale;
        }

        private void SpawnAlly(AllySpawnData data, Transform homeAnchor, Vector2 spawnPos, Vector2 homeOffset)
        {
            if (data.Prefab == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[{nameof(CombatSpawnManager)}] Ally '{data.AllyName}' has no prefab assigned.");
#endif
                return;
            }

            var ally = Instantiate(data.Prefab, new Vector3(spawnPos.x, spawnPos.y, 0f), Quaternion.identity, _teamContainer);
            ally.name = data.AllyName;
            ally.layer = PhysicsLayers.AllyLayer;
            ally.transform.localScale = new Vector3(-_characterScale, _characterScale, 1f);

            var setupConfig = new CharacterSetupConfig
            {
                MaxHp = data.MaxHp,
                Atk = data.Atk,
                AttackSpeed = data.AttackSpeed,
                RegenHpPerSecond = data.RegenHpPerSecond,
                MoveSpeed = data.MoveSpeed,
                HomeAnchor = homeAnchor,
                HomeOffset = homeOffset,
                ColliderRadius = data.ColliderRadius,
                Appearance = data.Appearance,
                CallerName = nameof(CombatSpawnManager),
                IsAlly = true,
                CharacterScale = _characterScale
            };
            var components = CombatSetupHelper.AssembleCharacter(ally, setupConfig);
            components.Controller.SetAttackerFacing(true);
        }
    }
}
