using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    /// <summary>
    /// ScriptableObject holding base stats for a character type.
    /// Create via Assets > Create > Roguelite > Character Stats.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCharacterStats", menuName = "Roguelite/Character Stats")]
    public class CharacterStats : ScriptableObject
    {
        [Header("Combat")]
        [Tooltip("Damage dealt per attack.")]
        public int atk = 10;

        [Tooltip("Maximum health points.")]
        public int maxHp = 100;

        [Tooltip("HP regenerated per second.")]
        public float regenHpPerSecond = 0f;

        [Tooltip("Attacks per second. Also scales animation speed.")]
        public float attackSpeed = 1f;

        [Header("Movement")]
        [Tooltip("Movement speed in world units per second.")]
        public float moveSpeed = 2f;

        [Header("Detection")]
        [Tooltip("Distance at which this character detects enemies and charges.")]
        public float detectionRadius = 3f;
    }
}
