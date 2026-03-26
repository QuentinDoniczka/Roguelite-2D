using UnityEngine;

namespace RogueliteAutoBattler.Data
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
        [SerializeField] private int atk = 10;

        [Tooltip("Maximum health points.")]
        [SerializeField] private int maxHp = 100;

        [Tooltip("HP regenerated per second.")]
        [SerializeField] private float regenHpPerSecond = 0f;

        [Tooltip("Attacks per second. Also scales animation speed.")]
        [SerializeField] private float attackSpeed = 1f;

        [Header("Movement")]
        [Tooltip("Movement speed in world units per second.")]
        [SerializeField] private float moveSpeed = 2f;

        public int Atk => atk;
        public int MaxHp => maxHp;
        public float RegenHpPerSecond => regenHpPerSecond;
        public float AttackSpeed => attackSpeed;
        public float MoveSpeed => moveSpeed;
    }
}
