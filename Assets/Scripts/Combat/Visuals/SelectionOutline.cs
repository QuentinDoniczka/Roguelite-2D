using UnityEngine;

namespace RogueliteAutoBattler.Combat.Visuals
{
    public class SelectionOutline : MonoBehaviour
    {
        private const string OutlineShaderName = "Custom/SpriteOutline2D";

        private static readonly int OutlineEnabledId = Shader.PropertyToID("_OutlineEnabled");
        private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
        private static readonly int OutlineWidthId = Shader.PropertyToID("_OutlineWidth");

        private static Material _sharedMaterial;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnDomainReload()
        {
            _sharedMaterial = null;
        }

        private SpriteRenderer[] _renderers;
        private MaterialPropertyBlock _mpb;
        private bool _isOutlined;

        public bool IsOutlined => _isOutlined;

        public void Initialize(Material outlineMaterial)
        {
            _renderers = GetComponentsInChildren<SpriteRenderer>();
            _mpb = new MaterialPropertyBlock();

            Material materialToUse = outlineMaterial != null ? outlineMaterial : _sharedMaterial;

            if (materialToUse == null)
                materialToUse = EnsureOutlineMaterial();

            if (materialToUse != null)
            {
                for (int i = 0; i < _renderers.Length; i++)
                    _renderers[i].sharedMaterial = materialToUse;
            }

            _mpb.SetFloat(OutlineEnabledId, 0f);
            for (int i = 0; i < _renderers.Length; i++)
                _renderers[i].SetPropertyBlock(_mpb);
        }

        public void SetOutline(bool enabled, Color color, float width = 1f)
        {
            _isOutlined = enabled;

            _mpb.SetFloat(OutlineEnabledId, enabled ? 1f : 0f);
            _mpb.SetColor(OutlineColorId, color);
            _mpb.SetFloat(OutlineWidthId, width);

            for (int i = 0; i < _renderers.Length; i++)
                _renderers[i].SetPropertyBlock(_mpb);
        }

        public void ClearOutline()
        {
            SetOutline(false, Color.clear);
        }

        private static Material EnsureOutlineMaterial()
        {
            if (_sharedMaterial != null)
                return _sharedMaterial;

            var shader = Shader.Find(OutlineShaderName);
            if (shader != null)
            {
                _sharedMaterial = new Material(shader);
                return _sharedMaterial;
            }

            Debug.LogWarning($"[{nameof(SelectionOutline)}] Shader '{OutlineShaderName}' not found. Outline rendering will not work.");
            return null;
        }
    }
}
