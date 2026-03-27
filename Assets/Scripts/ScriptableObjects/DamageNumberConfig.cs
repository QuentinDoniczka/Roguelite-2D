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

        [Header("Pooling")]
        [SerializeField] private int _initialPoolSize = 20;

        [Header("Spawn")]
        [SerializeField] private float _spawnOffsetY = 0.3f;

        public float FontSize { get => _fontSize; internal set => _fontSize = value; }
        public float Lifetime { get => _lifetime; internal set => _lifetime = value; }
        public Vector2 SlideDirection => _slideDirection;
        public float SlideDistance { get => _slideDistance; internal set => _slideDistance = value; }
        public Color AllyDamageColor { get => _allyDamageColor; internal set => _allyDamageColor = value; }
        public Color EnemyDamageColor { get => _enemyDamageColor; internal set => _enemyDamageColor = value; }
        public int SortingOrder => _sortingOrder;
        public int InitialPoolSize => _initialPoolSize;
        public float SpawnOffsetY { get => _spawnOffsetY; internal set => _spawnOffsetY = value; }

        private float _defaultFontSize;
        private float _defaultLifetime;
        private float _defaultSlideDistance;
        private float _defaultSpawnOffsetY;
        private Color _defaultAllyDamageColor;
        private Color _defaultEnemyDamageColor;

        private void OnEnable()
        {
            _defaultFontSize = _fontSize;
            _defaultLifetime = _lifetime;
            _defaultSlideDistance = _slideDistance;
            _defaultSpawnOffsetY = _spawnOffsetY;
            _defaultAllyDamageColor = _allyDamageColor;
            _defaultEnemyDamageColor = _enemyDamageColor;
        }

        internal void RestoreDefaults()
        {
            _fontSize = _defaultFontSize;
            _lifetime = _defaultLifetime;
            _slideDistance = _defaultSlideDistance;
            _spawnOffsetY = _defaultSpawnOffsetY;
            _allyDamageColor = _defaultAllyDamageColor;
            _enemyDamageColor = _defaultEnemyDamageColor;
        }
    }
}
