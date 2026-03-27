using UnityEngine;

namespace RogueliteAutoBattler.Data
{
    [CreateAssetMenu(fileName = "DamageNumberConfig", menuName = "Roguelite/Damage Number Config")]
    public class DamageNumberConfig : ScriptableObject
    {
        [Header("Text")]
        [SerializeField] private float _fontSize = 5f;

        [Header("Animation")]
        [SerializeField] private float _lifetime = 0.8f;
        [SerializeField] private Vector2 _slideDirection = new Vector2(0f, 1f);
        [SerializeField] private float _slideDistance = 0.5f;

        [Header("Colors")]
        [SerializeField] private Color _allyDamageColor = new Color(1f, 0.2f, 0.2f, 1f);
        [SerializeField] private Color _enemyDamageColor = Color.white;

        [Header("Rendering")]
        [SerializeField] private int _sortingOrder = 20;

        public float FontSize => _fontSize;
        public float Lifetime => _lifetime;
        public Vector2 SlideDirection => _slideDirection;
        public float SlideDistance => _slideDistance;
        public Color AllyDamageColor => _allyDamageColor;
        public Color EnemyDamageColor => _enemyDamageColor;
        public int SortingOrder => _sortingOrder;
    }
}
