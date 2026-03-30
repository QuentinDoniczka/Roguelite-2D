using TMPro;
using UnityEngine;

namespace RogueliteAutoBattler.Data
{
    [CreateAssetMenu(fileName = "DamageNumberConfig", menuName = "Roguelite/Damage Number Config")]
    public class DamageNumberConfig : ScriptableObject
    {
        private const float DefaultFontSize = 5f;
        private const float DefaultLifetime = 0.8f;
        private const float DefaultArcHeight = 1f;
        private const float DefaultArcWidth = 0.8f;
        private const float DefaultSpawnOffsetY = 0.3f;
        private const float DefaultOutlineWidth = 0.2f;
        private static readonly Color DefaultAllyDamageColor = Color.white;
        private static readonly Color DefaultEnemyDamageColor = Color.white;

        [Header("General")]
        [SerializeField] private bool _enabled = true;

        [Header("Text")]
        [SerializeField] private TMP_FontAsset _font;
        [SerializeField] private float _fontSize = DefaultFontSize;

        [Header("Animation")]
        [SerializeField] private float _lifetime = DefaultLifetime;

        [Header("Arc Animation")]
        [SerializeField] private float _arcHeight = DefaultArcHeight;
        [SerializeField] private float _arcWidth = DefaultArcWidth;

        [Header("Colors")]
        [SerializeField] private Color _allyDamageColor = DefaultAllyDamageColor;
        [SerializeField] private Color _enemyDamageColor = DefaultEnemyDamageColor;

        [Header("Outline")]
        [SerializeField, Range(0f, 1f)] private float _outlineWidth = DefaultOutlineWidth;
        [SerializeField] private Color _outlineColor = Color.black;

        [Header("Rendering")]
        [SerializeField] private int _sortingOrder = 20;

        [Header("Pooling")]
        [SerializeField] private int _initialPoolSize = 20;

        [Header("Spawn")]
        [SerializeField] private float _spawnOffsetY = DefaultSpawnOffsetY;

        public bool Enabled => _enabled;
        public TMP_FontAsset Font => _font;
        public float FontSize { get => _fontSize; set => _fontSize = value; }
        public float Lifetime { get => _lifetime; set => _lifetime = value; }
        public float ArcHeight => _arcHeight;
        public float ArcWidth => _arcWidth;
        public Color AllyDamageColor { get => _allyDamageColor; set => _allyDamageColor = value; }
        public Color EnemyDamageColor { get => _enemyDamageColor; set => _enemyDamageColor = value; }
        public float OutlineWidth => _outlineWidth;
        public Color OutlineColor => _outlineColor;
        public int SortingOrder => _sortingOrder;
        public int InitialPoolSize => _initialPoolSize;
        public float SpawnOffsetY { get => _spawnOffsetY; set => _spawnOffsetY = value; }

        public void RestoreDefaults()
        {
            _fontSize = DefaultFontSize;
            _lifetime = DefaultLifetime;
            _spawnOffsetY = DefaultSpawnOffsetY;
            _allyDamageColor = DefaultAllyDamageColor;
            _enemyDamageColor = DefaultEnemyDamageColor;
            _outlineWidth = DefaultOutlineWidth;
            _outlineColor = Color.black;
        }
    }
}
