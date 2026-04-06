using UnityEngine;
using UnityEngine.UI;

namespace RogueliteAutoBattler.UI.Screens.SkillTree
{
    public class SkillTreeDarknessOverlay : MonoBehaviour
    {
        private const string DarknessShaderName = "Custom/SkillTreeDarkness";
        private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");
        private static readonly int CenterUVPropertyId = Shader.PropertyToID("_CenterUV");
        private static readonly int LightRadiusPropertyId = Shader.PropertyToID("_LightRadius");
        private static readonly int LightSoftnessPropertyId = Shader.PropertyToID("_LightSoftness");
        private static readonly int LightIntensityPropertyId = Shader.PropertyToID("_LightIntensity");
        private static readonly int LightColorPropertyId = Shader.PropertyToID("_LightColor");

        [Header("Darkness Settings")]
        [SerializeField] private Color _darknessColor = new Color(0f, 0f, 0f, 1f);

        [Header("Center Light")]
        [SerializeField] private float _lightRadius = 0.05f;
        [SerializeField] private float _lightSoftness = 0.45f;
        [SerializeField] private float _lightIntensity = 1.0f;
        [SerializeField] private Color _lightColor = new Color(1f, 0.92f, 0.75f, 1f);

        [SerializeField] private RectTransform _content;

        private RawImage _overlayImage;
        private Material _materialInstance;

        public Color DarknessColor => _darknessColor;
        public RawImage OverlayImage => _overlayImage;
        public Material MaterialInstance => _materialInstance;
        public RectTransform Content => _content;

        public void Initialize()
        {
            if (_overlayImage != null) return;

            var overlayGo = new GameObject("DarknessOverlay");
            overlayGo.transform.SetParent(transform, false);

            var overlayRect = overlayGo.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;

            _overlayImage = overlayGo.AddComponent<RawImage>();
            _overlayImage.raycastTarget = false;

            var shader = Shader.Find(DarknessShaderName);
            if (shader == null)
            {
                Debug.LogWarning($"[{nameof(SkillTreeDarknessOverlay)}] Shader '{DarknessShaderName}' not found. Using fallback color.");
                _overlayImage.color = _darknessColor;
                _overlayImage.rectTransform.SetAsLastSibling();
                return;
            }

            _materialInstance = new Material(shader);
            _materialInstance.SetColor(ColorPropertyId, _darknessColor);
            _materialInstance.SetFloat(LightRadiusPropertyId, _lightRadius);
            _materialInstance.SetFloat(LightSoftnessPropertyId, _lightSoftness);
            _materialInstance.SetFloat(LightIntensityPropertyId, _lightIntensity);
            _materialInstance.SetColor(LightColorPropertyId, _lightColor);
            _overlayImage.material = _materialInstance;

            _overlayImage.rectTransform.SetAsLastSibling();
        }

        private void LateUpdate()
        {
            if (_materialInstance == null || _content == null || _overlayImage == null) return;

            RectTransform viewportRect = (RectTransform)transform;

            Vector2 viewportSize = viewportRect.rect.size;
            if (viewportSize.x <= 0f || viewportSize.y <= 0f) return;

            Vector2 contentOriginInViewport = _content.anchoredPosition;

            Vector2 centerUV = new Vector2(
                0.5f + contentOriginInViewport.x / viewportSize.x,
                0.5f + contentOriginInViewport.y / viewportSize.y
            );

            _materialInstance.SetVector(CenterUVPropertyId, new Vector4(centerUV.x, centerUV.y, 0f, 0f));

            float zoomScale = _content.localScale.x;
            _materialInstance.SetFloat(LightRadiusPropertyId, _lightRadius * zoomScale);
            _materialInstance.SetFloat(LightSoftnessPropertyId, _lightSoftness * zoomScale);
        }

        public void SetDarknessColor(Color color)
        {
            _darknessColor = color;

            if (_materialInstance != null)
            {
                _materialInstance.SetColor(ColorPropertyId, _darknessColor);
                return;
            }

            if (_overlayImage != null)
                _overlayImage.color = _darknessColor;
        }

        public void SetOpacity(float alpha)
        {
            _darknessColor.a = alpha;
            SetDarknessColor(_darknessColor);
        }

        private void OnDestroy()
        {
            if (_materialInstance != null)
                Destroy(_materialInstance);
        }
    }
}
