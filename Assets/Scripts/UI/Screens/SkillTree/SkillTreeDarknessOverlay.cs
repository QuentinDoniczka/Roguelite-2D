using UnityEngine;
using UnityEngine.UI;

namespace RogueliteAutoBattler.UI.Screens.SkillTree
{
    public class SkillTreeDarknessOverlay : MonoBehaviour
    {
        private const string DarknessShaderName = "Custom/SkillTreeDarkness";
        private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");

        [Header("Darkness Settings")]
        [SerializeField] private Color _darknessColor = new Color(0f, 0f, 0f, 0.85f);

        private RawImage _overlayImage;
        private Material _materialInstance;

        public Color DarknessColor => _darknessColor;
        public RawImage OverlayImage => _overlayImage;
        public Material MaterialInstance => _materialInstance;

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
            _overlayImage.material = _materialInstance;

            _overlayImage.rectTransform.SetAsLastSibling();
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
