using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    [RequireComponent(typeof(CombatStats))]
    public class HealthBar : MonoBehaviour
    {
        private const string EffectsSortingLayer = "Effects";
        private const string UnlitShaderName = "Universal Render Pipeline/2D/Sprite-Unlit-Default";

        [Header("Bar Dimensions")]
        [SerializeField] private float _barWidth = 0.3f;
        [SerializeField] private float _barHeight = 0.04f;
        [SerializeField] private float _yOffset = 0.3f;

        private static readonly Color ColorBg = new Color(0.15f, 0.15f, 0.15f, 1f);
        private static readonly Color ColorHealthy = new Color(0.20f, 0.80f, 0.20f, 1f);
        private static readonly Color ColorWarning = new Color(0.80f, 0.80f, 0.20f, 1f);
        private static readonly Color ColorCritical = new Color(0.80f, 0.20f, 0.20f, 1f);

        private static Material _unlitMaterial;
        private static Sprite _centeredSprite;
        private static Sprite _leftAlignedSprite;

        private CombatStats _stats;
        private Transform _pivotTransform;
        private Transform _fillTransform;
        private SpriteRenderer _fillRenderer;

        private void Awake()
        {
            _stats = GetComponent<CombatStats>();
            EnsureUnlitMaterial();
            CreateBar();
        }

        private void CreateBar()
        {
            var bgSprite = GetOrCreateSprite(ref _centeredSprite, new Vector2(0.5f, 0.5f));
            var fillSprite = GetOrCreateSprite(ref _leftAlignedSprite, new Vector2(0f, 0.5f));

            var pivotGo = new GameObject("HealthBar_Pivot");
            pivotGo.transform.SetParent(transform, false);
            pivotGo.transform.localPosition = new Vector3(0f, _yOffset, 0f);
            ApplyFlipCompensation(pivotGo.transform);
            _pivotTransform = pivotGo.transform;

            var bgGo = new GameObject("BG");
            bgGo.transform.SetParent(_pivotTransform, false);
            bgGo.transform.localPosition = Vector3.zero;
            bgGo.transform.localScale = new Vector3(_barWidth, _barHeight, 1f);
            var bgRenderer = bgGo.AddComponent<SpriteRenderer>();
            bgRenderer.sprite = bgSprite;
            bgRenderer.color = ColorBg;
            bgRenderer.sortingLayerName = EffectsSortingLayer;
            bgRenderer.sortingOrder = 10;
            bgRenderer.material = _unlitMaterial;

            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(_pivotTransform, false);
            fillGo.transform.localPosition = new Vector3(-_barWidth * 0.5f, 0f, 0f);
            fillGo.transform.localScale = new Vector3(_barWidth, _barHeight, 1f);
            _fillRenderer = fillGo.AddComponent<SpriteRenderer>();
            _fillRenderer.sprite = fillSprite;
            _fillRenderer.color = ColorHealthy;
            _fillRenderer.sortingLayerName = EffectsSortingLayer;
            _fillRenderer.sortingOrder = 11;
            _fillRenderer.material = _unlitMaterial;
            _fillTransform = fillGo.transform;
        }

        private void LateUpdate()
        {
            if (_stats == null || _stats.MaxHp <= 0)
                return;

            ApplyFlipCompensation(_pivotTransform);

            float ratio = (float)_stats.CurrentHp / _stats.MaxHp;

            var scale = _fillTransform.localScale;
            scale.x = _barWidth * ratio;
            _fillTransform.localScale = scale;

            _fillRenderer.color = ratio > 0.5f
                ? ColorHealthy
                : ratio > 0.25f
                    ? ColorWarning
                    : ColorCritical;
        }

        private void ApplyFlipCompensation(Transform pivot)
        {
            float sign = transform.localScale.x < 0f ? -1f : 1f;
            pivot.localScale = new Vector3(sign, 1f, 1f);
        }

        private static Sprite GetOrCreateSprite(ref Sprite cached, Vector2 pivot)
        {
            if (cached != null)
                return cached;

            var tex = Texture2D.whiteTexture;
            cached = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), pivot, tex.width);
            return cached;
        }

        private static void EnsureUnlitMaterial()
        {
            if (_unlitMaterial != null)
                return;

            var shader = Shader.Find(UnlitShaderName);
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
