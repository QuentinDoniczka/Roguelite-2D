using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    /// <summary>
    /// Renders a world-space HP bar above the character using SpriteRenderers.
    /// Must be placed on the same root GameObject as <see cref="CombatStats"/>.
    ///
    /// Structure:
    ///   HealthBar_Pivot  (empty GO, child of character root — absorbs parent flip)
    ///   ├── BG           (SpriteRenderer, dark, centered pivot)
    ///   └── Fill         (SpriteRenderer, colored, left-aligned pivot)
    ///
    /// URP 2D uses Lit sprites by default: without a Light2D the bar would appear
    /// black. Both renderers are explicitly set to Sprite-Unlit-Default.
    /// </summary>
    [RequireComponent(typeof(CombatStats))]
    public class HealthBar : MonoBehaviour
    {
        private const string EffectsSortingLayer = "Effects";
        private const string DefaultSpriteMaterial = "Universal Render Pipeline/2D/Sprite-Unlit-Default";

        [Header("Bar Dimensions")]
        [Tooltip("Total width of the health bar in world units.")]
        [SerializeField] private float _barWidth = 0.3f;

        [Tooltip("Height of the health bar in world units.")]
        [SerializeField] private float _barHeight = 0.04f;

        [Tooltip("Vertical offset above the character root position.")]
        [SerializeField] private float _yOffset = 0.3f;

        // Full opacity — Unlit material, no alpha blending needed for visibility.
        private static readonly Color ColorBg      = new Color(0.15f, 0.15f, 0.15f, 1f);
        private static readonly Color ColorHealthy = new Color(0.20f, 0.80f, 0.20f, 1f);
        private static readonly Color ColorWarning = new Color(0.80f, 0.80f, 0.20f, 1f);
        private static readonly Color ColorCritical = new Color(0.80f, 0.20f, 0.20f, 1f);

        // Shared Unlit material — created once, reused by all HealthBar instances.
        private static Material _unlitMaterial;

        // Shared sprites — created once, reused by all HealthBar instances.
        private static Sprite _centeredSprite;
        private static Sprite _leftAlignedSprite;

        private CombatStats _stats;
        private Transform   _pivotTransform;
        private Transform   _fillTransform;
        private SpriteRenderer _fillRenderer;

        private void Awake()
        {
            _stats = GetComponent<CombatStats>();
            EnsureUnlitMaterial();
            CreateBar();
        }

        // ------------------------------------------------------------------
        // Bar construction
        // ------------------------------------------------------------------

        private void CreateBar()
        {
            // Sprites —————————————————————————————————————————————————————
            // Centered sprite (pivot 0.5, 0.5) — used for BG.
            var bgSprite = GetOrCreateSprite(ref _centeredSprite, new Vector2(0.5f, 0.5f));
            // Left-aligned sprite (pivot 0, 0.5) — used for Fill so scaling X
            // from 0 grows toward the right from the left edge.
            var fillSprite = GetOrCreateSprite(ref _leftAlignedSprite, new Vector2(0f, 0.5f));

            // Pivot ———————————————————————————————————————————————————————
            // An empty intermediary absorbs the parent's X flip.
            // If the parent has localScale.x < 0 (flipped character), this pivot
            // counteracts it with localScale.x = -1 so all children stay in
            // normal (unflipped) space from the viewer's perspective.
            var pivotGo = new GameObject("HealthBar_Pivot");
            pivotGo.transform.SetParent(transform, false);
            pivotGo.transform.localPosition = new Vector3(0f, _yOffset, 0f);
            ApplyFlipCompensation(pivotGo.transform);
            _pivotTransform = pivotGo.transform;

            // BG ——————————————————————————————————————————————————————————
            // Centered in pivot space — full bar width, always visible.
            var bgGo = new GameObject("BG");
            bgGo.transform.SetParent(_pivotTransform, false);
            bgGo.transform.localPosition = Vector3.zero;
            bgGo.transform.localScale    = new Vector3(_barWidth, _barHeight, 1f);
            var bgRenderer = bgGo.AddComponent<SpriteRenderer>();
            bgRenderer.sprite           = bgSprite;
            bgRenderer.color            = ColorBg;
            bgRenderer.sortingLayerName = EffectsSortingLayer;
            bgRenderer.sortingOrder     = 10;
            bgRenderer.material         = _unlitMaterial;

            // Fill ————————————————————————————————————————————————————————
            // Left-aligned: position at the left edge of the BG (-width/2 in pivot
            // space). Scale X = barWidth * ratio so it grows rightward from that edge.
            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(_pivotTransform, false);
            fillGo.transform.localPosition = new Vector3(-_barWidth * 0.5f, 0f, 0f);
            fillGo.transform.localScale    = new Vector3(_barWidth, _barHeight, 1f);
            _fillRenderer = fillGo.AddComponent<SpriteRenderer>();
            _fillRenderer.sprite           = fillSprite;
            _fillRenderer.color            = ColorHealthy;
            _fillRenderer.sortingLayerName = EffectsSortingLayer;
            _fillRenderer.sortingOrder     = 11;
            _fillRenderer.material         = _unlitMaterial;
            _fillTransform = fillGo.transform;
        }

        // ------------------------------------------------------------------
        // Runtime update
        // ------------------------------------------------------------------

        private void LateUpdate()
        {
            if (_stats == null || _stats.MaxHp <= 0)
                return;

            // Reapply flip compensation every frame — the parent scale can change
            // (e.g. character reverses direction mid-combat).
            ApplyFlipCompensation(_pivotTransform);

            float ratio = (float)_stats.CurrentHp / _stats.MaxHp;

            // Fill width represents HP ratio; height and Z stay unchanged.
            var scale = _fillTransform.localScale;
            scale.x = _barWidth * ratio;
            _fillTransform.localScale = scale;

            _fillRenderer.color = ratio > 0.5f
                ? ColorHealthy
                : ratio > 0.25f
                    ? ColorWarning
                    : ColorCritical;
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        /// <summary>
        /// Sets pivot.localScale.x = -1 when the parent's world X scale is negative,
        /// so all children of the pivot render unflipped from the viewer's perspective.
        /// </summary>
        private void ApplyFlipCompensation(Transform pivot)
        {
            float sign = transform.localScale.x < 0f ? -1f : 1f;
            pivot.localScale = new Vector3(sign, 1f, 1f);
        }

        /// <summary>
        /// Returns a cached 1x1 white sprite with the given pivot, creating it on first call.
        /// Shared across all HealthBar instances to avoid per-instance sprite allocations.
        /// </summary>
        private static Sprite GetOrCreateSprite(ref Sprite cached, Vector2 pivot)
        {
            if (cached != null)
                return cached;

            var tex = Texture2D.whiteTexture;
            cached = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), pivot, tex.width);
            return cached;
        }

        /// <summary>
        /// Lazily creates the shared Unlit material.
        /// URP 2D Lit sprites appear black without a Light2D — Unlit bypasses lighting entirely.
        /// </summary>
        private static void EnsureUnlitMaterial()
        {
            if (_unlitMaterial != null)
                return;

            var shader = Shader.Find(DefaultSpriteMaterial);
            if (shader != null)
            {
                _unlitMaterial = new Material(shader);
            }
            else
            {
                Debug.LogWarning("[HealthBar] Shader 'Sprite-Unlit-Default' not found. " +
                                 "HP bar may render black. Falling back to default sprite material.");
            }
        }
    }
}
