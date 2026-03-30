using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    public class CombatSpawnManager : MonoBehaviour
    {
        [Header("Team")]
        [SerializeField] private TeamDatabase _teamDatabase;

        [Header("Containers")]
        [SerializeField] private Transform _teamContainer;
        [SerializeField] private Transform _enemiesContainer;

        [Header("Anchors")]
        [SerializeField] private Transform _teamHomeAnchor;

        public const string TeamContainerName = CombatSetupHelper.TeamContainerName;
        public const string EnemiesContainerName = CombatSetupHelper.EnemiesContainerName;
        public const string TeamHomeAnchorName = CombatSetupHelper.TeamHomeAnchorName;
        public const string EnemiesHomeAnchorName = CombatSetupHelper.EnemiesHomeAnchorName;
        public const string CombatTriggerZoneName = CombatSetupHelper.CombatTriggerZoneName;

        private static readonly Vector3 FacingRightScale = new Vector3(-1f, 1f, 1f);

        private void Start()
        {
            SpawnAllies();
        }

        public void SpawnAllies()
        {
            if (_teamDatabase == null || _teamDatabase.Allies.Count == 0)
            {
                Debug.LogWarning($"[{nameof(CombatSpawnManager)}] No TeamDatabase assigned or no allies configured!", this);
                return;
            }

            CombatSetupHelper.FindContainersIfNeeded(transform, ref _teamContainer, ref _enemiesContainer, nameof(CombatSpawnManager));

            Transform teamAnchor = _teamHomeAnchor;
            Vector2 anchorPos = teamAnchor != null ? (Vector2)teamAnchor.position : Vector2.zero;

            var allies = _teamDatabase.Allies;
            Vector2[] positions = FormationLayout.GetPositions(anchorPos, allies.Count, facingRight: true);

            for (int i = 0; i < allies.Count; i++)
            {
                Vector2 offset = positions[i] - anchorPos;
                SpawnAlly(allies[i], teamAnchor, positions[i], offset);
            }
        }

        public void RespawnAllies()
        {
            DestroyAllChildren(_teamContainer);
            SpawnAllies();
        }

        private static void DestroyAllChildren(Transform container)
        {
            if (container == null)
                return;

            for (int i = container.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(container.GetChild(i).gameObject);
            }
        }

        internal void InitializeForTest(TeamDatabase teamDatabase, Transform teamContainer, Transform teamHomeAnchor)
        {
            _teamDatabase = teamDatabase;
            _teamContainer = teamContainer;
            _teamHomeAnchor = teamHomeAnchor;
        }

        private void SpawnAlly(AllySpawnData data, Transform homeAnchor, Vector2 spawnPos, Vector2 homeOffset)
        {
            if (data.Prefab == null)
            {
                Debug.LogWarning($"[{nameof(CombatSpawnManager)}] Ally '{data.AllyName}' has no prefab assigned.");
                return;
            }

            var ally = Instantiate(data.Prefab, new Vector3(spawnPos.x, spawnPos.y, 0f), Quaternion.identity, _teamContainer);
            ally.name = data.AllyName;
            ally.transform.localScale = FacingRightScale;

            var components = CombatSetupHelper.AssembleCharacter(
                ally,
                data.MaxHp,
                data.Atk,
                data.AttackSpeed,
                data.RegenHpPerSecond,
                data.MoveSpeed,
                homeAnchor,
                homeOffset,
                data.ColliderRadius,
                data.Appearance,
                nameof(CombatSpawnManager));
            components.Controller.SetAttackerFacing(true);
        }
    }
}
