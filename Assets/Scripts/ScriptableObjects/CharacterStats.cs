using UnityEngine;

namespace RogueliteAutoBattler.Data
{
    [CreateAssetMenu(fileName = "NewCharacterStats", menuName = "Roguelite/Character Stats")]
    public class CharacterStats : ScriptableObject
    {
        [Header("Combat")]
        [SerializeField] private int atk = 10;
        [SerializeField] private int maxHp = 100;
        [SerializeField] private float regenHpPerSecond = 0f;
        [SerializeField] private float attackSpeed = 1f;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 2f;

        public int Atk => atk;
        public int MaxHp => maxHp;
        public float RegenHpPerSecond => regenHpPerSecond;
        public float AttackSpeed => attackSpeed;
        public float MoveSpeed => moveSpeed;
    }
}
