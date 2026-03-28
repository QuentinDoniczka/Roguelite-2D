using TMPro;
using UnityEngine;

namespace RogueliteAutoBattler.Data
{
    [CreateAssetMenu(fileName = "DamageNumberConfig", menuName = "Roguelite/Damage Number Config")]
    public class DamageNumberConfig : ScriptableObject
    {
        [Header("General")]
        [SerializeField] private bool _enabled = true;

        [Header("Text")]
        [SerializeField] private TMP_FontAsset _font;
        [SerializeField] private float _fontSize = 1.25f;

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

        public bool Enabled => _enabled;
        public TMP_FontAsset Font => _font;
        public float FontSize { get => _fontSize; set => _fontSize = value; }
        public float Lifetime { get => _lifetime; set => _lifetime = value; }
        public Vector2 SlideDirection => _slideDirection;
        public float SlideDistance { get => _slideDistance; set => _slideDistance = value; }
        public Color AllyDamageColor { get => _allyDamageColor; set => _allyDamageColor = value; }
        public Color EnemyDamageColor { get => _enemyDamageColor; set => _enemyDamageColor = value; }
        public int SortingOrder => _sortingOrder;
        public int InitialPoolSize => _initialPoolSize;
        public float SpawnOffsetY { get => _spawnOffsetY; set => _spawnOffsetY = value; }

        public void RestoreDefaults()
        {
            _fontSize = 1.25f;
            _lifetime = 0.8f;
            _slideDistance = 0.5f;
            _spawnOffsetY = 0.3f;
            _allyDamageColor = new Color(1f, 0.2f, 0.2f, 1f);
            _enemyDamageColor = Color.white;
        }
    }
}
