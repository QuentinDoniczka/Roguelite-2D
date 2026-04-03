using System.Collections.Generic;
using RogueliteAutoBattler.Combat.Visuals;
using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.Combat.Core
{
    public struct CharacterSetupConfig
    {
        public int MaxHp;
        public int Atk;
        public float AttackSpeed;
        public float RegenHpPerSecond;
        public float MoveSpeed;
        public Transform HomeAnchor;
        public Vector2 HomeOffset;
        public float ColliderRadius;
        public AppearanceData Appearance;
        public string CallerName;
        public Color? HealthBarFillColor;
        public Color? HealthBarTrailColor;
        public bool IsAlly;
        public float CharacterScale;
    }

    public struct CharacterComponents
    {
        public CombatStats Stats;
        public CombatController Controller;
    }

    public static class CombatSetupHelper
    {
        public const string TeamContainerName = "Team";
        public const string EnemiesContainerName = "Enemies";
        public const string TeamHomeAnchorName = "TeamHomeAnchor";
        public const string EnemiesHomeAnchorName = "EnemiesHomeAnchor";
        public const string CombatTriggerZoneName = "CombatTriggerZone";

        public static CharacterComponents AssembleCharacter(GameObject character, CharacterSetupConfig config)
        {
            float characterScale = config.CharacterScale > 0f ? config.CharacterScale : 1f;

            var combatStats = character.AddComponent<CombatStats>();
            combatStats.InitializeDirect(config.MaxHp, config.Atk, config.AttackSpeed, config.RegenHpPerSecond);

            var healthBar = character.AddComponent<HealthBar>();
            if (config.HealthBarFillColor.HasValue || config.HealthBarTrailColor.HasValue)
            {
                healthBar.SetColors(
                    config.HealthBarFillColor ?? HealthBar.AllyFillColor,
                    config.HealthBarTrailColor ?? HealthBar.DefaultTrailColor);
            }

            var mover = character.AddComponent<CharacterMover>();
            mover.SetMoveSpeed(config.MoveSpeed);
            if (config.HomeAnchor != null)
                mover.HomeAnchor = config.HomeAnchor;
            mover.SetHomeOffset(config.HomeOffset);

            var col = character.GetComponent<CircleCollider2D>();
            if (col != null)
                col.radius = config.ColliderRadius / characterScale;

            var controller = character.AddComponent<CombatController>();
            WireAnimationRelay(character, controller, config.CallerName);

            var appearanceComp = character.AddComponent<CharacterAppearance>();
            appearanceComp.ApplyAppearance(config.Appearance);

            var characterTransform = character.transform;
            bool ally = config.IsAlly;
            combatStats.OnDamageTaken += (damage, _) =>
            {
                if (characterTransform != null)
                    DamageNumberService.Show(characterTransform.position, damage, ally);
            };

            return new CharacterComponents
            {
                Stats = combatStats,
                Controller = controller
            };
        }

        private static void WireAnimationRelay(GameObject character, CombatController controller, string callerName)
        {
            var animator = character.GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogWarning($"[{callerName}] No Animator found on {character.name} — AnimationEventRelay not added.");
                return;
            }

            var relay = animator.gameObject.AddComponent<AnimationEventRelay>();
            relay.Initialize(controller);
        }

        public static void RecalculateFormation(Transform container, Transform homeAnchor, bool facingRight, float characterScale = 1f)
        {
            if (container == null || homeAnchor == null)
                return;

            var aliveList = new List<CharacterMover>();

            for (int i = 0; i < container.childCount; i++)
            {
                Transform child = container.GetChild(i);
                if (child.TryGetComponent<CombatStats>(out var stats)
                    && !stats.IsDead
                    && child.TryGetComponent<CharacterMover>(out var mover))
                {
                    aliveList.Add(mover);
                }
            }

            if (aliveList.Count == 0)
                return;

            Vector2 anchorPos = (Vector2)homeAnchor.position;
            Vector2[] positions = FormationLayout.GetPositions(anchorPos, aliveList.Count, facingRight, characterScale: characterScale);

            for (int i = 0; i < aliveList.Count; i++)
            {
                Vector2 offset = positions[i] - anchorPos;
                aliveList[i].SetHomeOffset(offset);
            }
        }

        public static void DestroyAllChildren(Transform container)
        {
            if (container == null)
                return;

            for (int i = container.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(container.GetChild(i).gameObject);
            }
        }

        public static void FindContainersIfNeeded(Transform parent, ref Transform teamContainer, ref Transform enemiesContainer, string callerName)
        {
            if (teamContainer == null)
                teamContainer = parent.Find(TeamContainerName);
            if (enemiesContainer == null)
                enemiesContainer = parent.Find(EnemiesContainerName);

            if (teamContainer == null)
                Debug.LogWarning($"[{callerName}] '{TeamContainerName}' container not found!");
            if (enemiesContainer == null)
                Debug.LogWarning($"[{callerName}] '{EnemiesContainerName}' container not found!");
        }
    }
}
