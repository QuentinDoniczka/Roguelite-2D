using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    [RequireComponent(typeof(CombatStats))]
    public class HealthBar : MonoBehaviour
    {
        private const string EffectsSortingLayer = "Effects";
        private const string UnlitShaderName = "Universal Render Pipeline/2D/Sprite-Unlit-Default";
        private const int SortingOrderBackground = 10;
        private const int SortingOrderTrailFill = 11;
        private const int SortingOrderFill = 12;

        [Header("Bar Dimensions")]
        [SerializeField] private float _barWidth = 0.3f;
        [SerializeField] private float _barHeight = 0.04f;
        [SerializeField] private float _yOffset = 0.3f;

        private static readonly Color ColorBg = new Color(0.15f, 0.15f, 0.15f, 1f);
        private static readonly Color ColorTrail = new Color(0.80f, 0.20f, 0.20f, 0.80f);
        private static readonly Color ColorHealthy = new Color(0.20f, 0.80f, 0.20f, 1f);

        private static Material _unlitMaterial;
        private static Sprite _centeredSprite;
        private static Sprite _leftAlignedSprite;

        [Header("Trail Settings")]
        [SerializeField] private float _trailFadeDuration = 0.5f;

        private CombatStats _stats;
        private bool _hasStats;
        private Transform _pivotTransform;
        private Transform _trailFillTransform;
        private SpriteRenderer _trailFillRenderer;
        private Transform _fillTransform;
        private SpriteRenderer _fillRenderer;

        private float _trailRatio = 1f;
        private float _trailStartRatio;
        private float _trailTargetRatio;
        private float _trailElapsed;
        private bool _isTrailLerping;

        private void Awake()
        {
            _stats = GetComponent<CombatStats>();
            _hasStats = _stats != null;
            EnsureUnlitMaterial();
            CreateBar();
            _trailRatio = 1f;
            _stats.OnDamageTaken += HandleDamageTaken;
        }

        private void OnDestroy()
        {
            if (_hasStats)
                _stats.OnDamageTaken -= HandleDamageTaken;
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
            bgRenderer.sortingOrder = SortingOrderBackground;
            bgRenderer.material = _unlitMaterial;

            var fillLocalPosition = new Vector3(-_barWidth * 0.5f, 0f, 0f);
            var fillLocalScale = new Vector3(_barWidth, _barHeight, 1f);

            var trailFillGo = new GameObject("TrailFill");
            trailFillGo.transform.SetParent(_pivotTransform, false);
            trailFillGo.transform.localPosition = fillLocalPosition;
            trailFillGo.transform.localScale = fillLocalScale;
            _trailFillRenderer = trailFillGo.AddComponent<SpriteRenderer>();
            _trailFillRenderer.sprite = fillSprite;
            _trailFillRenderer.color = ColorTrail;
            _trailFillRenderer.sortingLayerName = EffectsSortingLayer;
            _trailFillRenderer.sortingOrder = SortingOrderTrailFill;
            _trailFillRenderer.material = _unlitMaterial;
            _trailFillTransform = trailFillGo.transform;

            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(_pivotTransform, false);
            fillGo.transform.localPosition = fillLocalPosition;
            fillGo.transform.localScale = fillLocalScale;
            _fillRenderer = fillGo.AddComponent<SpriteRenderer>();
            _fillRenderer.sprite = fillSprite;
            _fillRenderer.color = ColorHealthy;
            _fillRenderer.sortingLayerName = EffectsSortingLayer;
            _fillRenderer.sortingOrder = SortingOrderFill;
            _fillRenderer.material = _unlitMaterial;
            _fillTransform = fillGo.transform;
        }

        private void HandleDamageTaken(int damage, int currentHp)
        {
            float newRatio = (float)currentHp / _stats.MaxHp;
            _trailStartRatio = _trailRatio;
            _trailTargetRatio = newRatio;
            _trailElapsed = 0f;
            _isTrailLerping = true;
        }

        private void LateUpdate()
        {
            if (!_hasStats || _stats.MaxHp <= 0)
                return;

            ApplyFlipCompensation(_pivotTransform);

            float ratio = (float)_stats.CurrentHp / _stats.MaxHp;

            var scale = _fillTransform.localScale;
            scale.x = _barWidth * ratio;
            _fillTransform.localScale = scale;

            if (_isTrailLerping)
            {
                _trailElapsed += Time.deltaTime;
                float t = Mathf.Clamp01(_trailElapsed / _trailFadeDuration);
                _trailRatio = Mathf.Lerp(_trailStartRatio, _trailTargetRatio, t);
                if (t >= 1f)
                    _isTrailLerping = false;
            }

            if (_trailRatio < ratio)
                _trailRatio = ratio;

            var trailScale = _trailFillTransform.localScale;
            trailScale.x = _barWidth * _trailRatio;
            _trailFillTransform.localScale = trailScale;
        }

        private void ApplyFlipCompensation(Transform pivot)
        {
            float sign = transform.localScale.x < 0f ? -1f : 1f;
            var pivotScale = pivot.localScale;
            pivotScale.x = sign;
            pivot.localScale = pivotScale;
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
                Debug.LogWarning($"[HealthBar] Shader '{UnlitShaderName}' not found. HP bar may render black. Falling back to default sprite material.");
            }
        }
    }
}
