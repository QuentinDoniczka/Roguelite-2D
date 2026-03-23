using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    /// <summary>
    /// Renders a world-space HP bar above the character using SpriteRenderers.
    /// Must be placed on the same root GameObject as <see cref="CombatStats"/>.
    /// Compensates for a parent with negative X scale (flipped sprite) so the bar
    /// always reads left-to-right from the viewer's perspective.
    /// </summary>
    public class HealthBar : MonoBehaviour
    {
        [Header("Bar Dimensions")]
        [Tooltip("Total width of the health bar in world units.")]
        [SerializeField] private float _barWidth = 0.3f;

        [Tooltip("Height of the health bar in world units.")]
        [SerializeField] private float _barHeight = 0.04f;

        [Tooltip("Vertical offset above the character root position.")]
        [SerializeField] private float _yOffset = 0.25f;

        private static readonly Color ColorHealthy = new Color(0.2f, 0.8f, 0.2f, 0.9f);
        private static readonly Color ColorWarning = new Color(0.8f, 0.8f, 0.2f, 0.9f);
        private static readonly Color ColorCritical = new Color(0.8f, 0.2f, 0.2f, 0.9f);

        private CombatStats _stats;
        private Transform _fillTransform;
        private SpriteRenderer _fillRenderer;

        private void Awake()
        {
            _stats = GetComponent<CombatStats>();
            CreateBar();
        }

        private void CreateBar()
        {
            // Built-in white texture — no external asset dependency.
            var whiteTex = Texture2D.whiteTexture;
            // Left-aligned pivot (0, 0.5): scaling X from 0 grows from the left edge.
            var sprite = Sprite.Create(
                whiteTex,
                new Rect(0, 0, whiteTex.width, whiteTex.height),
                new Vector2(0f, 0.5f),
                whiteTex.width
            );

            // When this root has negative X scale (flipped character), child objects are
            // also mirrored. We compensate by inverting the bar scale and offset sign.
            float scaleSign = transform.localScale.x < 0f ? -1f : 1f;

            // Background (dark) — centered at bar position.
            var bgGo = new GameObject("HealthBar_BG");
            bgGo.transform.SetParent(transform, false);
            // Shift right by half width to re-center the left-pivot sprite visually,
            // then compensate for any parent X flip.
            bgGo.transform.localPosition = new Vector3(_barWidth / 2f * scaleSign, _yOffset, 0f);
            bgGo.transform.localScale = new Vector3(_barWidth * scaleSign, _barHeight, 1f);
            var bgRenderer = bgGo.AddComponent<SpriteRenderer>();
            bgRenderer.sprite = sprite;
            bgRenderer.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            bgRenderer.sortingLayerName = "Effects";
            bgRenderer.sortingOrder = 10;

            // Fill (green) — pivot is LEFT so scaling X shrinks from the right edge.
            var fillGo = new GameObject("HealthBar_Fill");
            fillGo.transform.SetParent(transform, false);
            fillGo.transform.localPosition = new Vector3(0f, _yOffset, 0f);
            fillGo.transform.localScale = new Vector3(_barWidth * scaleSign, _barHeight, 1f);
            _fillRenderer = fillGo.AddComponent<SpriteRenderer>();
            _fillRenderer.sprite = sprite;
            _fillRenderer.color = ColorHealthy;
            _fillRenderer.sortingLayerName = "Effects";
            _fillRenderer.sortingOrder = 11;
            _fillTransform = fillGo.transform;
        }

        private void LateUpdate()
        {
            if (_stats == null || _stats.BaseStats == null)
                return;

            float ratio = _stats.MaxHp > 0 ? (float)_stats.CurrentHp / _stats.MaxHp : 0f;

            // Scale X preserves the compensation sign baked at creation time.
            float scaleSign = transform.localScale.x < 0f ? -1f : 1f;
            var scale = _fillTransform.localScale;
            scale.x = _barWidth * ratio * scaleSign;
            _fillTransform.localScale = scale;

            _fillRenderer.color = ratio > 0.5f
                ? ColorHealthy
                : ratio > 0.25f
                    ? ColorWarning
                    : ColorCritical;
        }
    }
}
