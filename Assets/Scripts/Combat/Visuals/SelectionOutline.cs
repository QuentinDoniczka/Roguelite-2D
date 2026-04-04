using UnityEngine;

namespace RogueliteAutoBattler.Combat.Visuals
{
    public class SelectionOutline : MonoBehaviour
    {
        private const string SilhouetteShaderName = "Custom/SpriteSilhouette2D";
        private const float ScaleMultiplier = 1.08f;

        private static Material _silhouetteMaterial;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnDomainReload()
        {
            _silhouetteMaterial = null;
        }

        private SpriteRenderer[] _originals;
        private SpriteRenderer[] _silhouettes;
        private GameObject _container;
        private bool _isOutlined;
        private int _minSortingOrder;

        public bool IsOutlined => _isOutlined;

        public void Initialize()
        {
            _originals = GetComponentsInChildren<SpriteRenderer>();

            _minSortingOrder = int.MaxValue;
            for (int i = 0; i < _originals.Length; i++)
            {
                if (_originals[i].sortingOrder < _minSortingOrder)
                    _minSortingOrder = _originals[i].sortingOrder;
            }
        }

        public void SetOutline(bool enabled, Color color, float width = 1f)
        {
            _isOutlined = enabled;
            if (enabled)
            {
                EnsureSilhouettes();
                SetSilhouetteColor(color);
                _container.SetActive(true);
                SyncSilhouettes();
            }
            else if (_container != null)
            {
                _container.SetActive(false);
            }
        }

        public void ClearOutline()
        {
            SetOutline(false, Color.clear);
        }

        private void LateUpdate()
        {
            if (!_isOutlined || _silhouettes == null)
                return;
            SyncSilhouettes();
        }

        private void EnsureSilhouettes()
        {
            if (_container != null) return;

            EnsureSilhouetteMaterial();

            _container = new GameObject("_OutlineSilhouettes");
            _container.transform.SetParent(transform, false);

            _silhouettes = new SpriteRenderer[_originals.Length];
            for (int i = 0; i < _originals.Length; i++)
            {
                var go = new GameObject($"sil_{i}");
                go.transform.SetParent(_container.transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                if (_silhouetteMaterial != null)
                    sr.material = _silhouetteMaterial;
                sr.sortingLayerID = _originals[i].sortingLayerID;
                sr.sortingOrder = _minSortingOrder - 1;
                _silhouettes[i] = sr;
            }
        }

        private void SyncSilhouettes()
        {
            Vector3 containerLossyScale = _container.transform.lossyScale;

            for (int i = 0; i < _originals.Length; i++)
            {
                var orig = _originals[i];
                var sil = _silhouettes[i];
                if (orig == null)
                {
                    if (sil != null && sil.enabled)
                        sil.enabled = false;
                    continue;
                }

                sil.transform.position = orig.transform.position;
                sil.transform.rotation = orig.transform.rotation;

                Vector3 origLossy = orig.transform.lossyScale;
                sil.transform.localScale = new Vector3(
                    origLossy.x / containerLossyScale.x * ScaleMultiplier,
                    origLossy.y / containerLossyScale.y * ScaleMultiplier,
                    1f);

                sil.sprite = orig.sprite;
                sil.flipX = orig.flipX;
                sil.flipY = orig.flipY;

                if (!sil.enabled)
                    sil.enabled = true;
            }
        }

        private void SetSilhouetteColor(Color color)
        {
            for (int i = 0; i < _silhouettes.Length; i++)
                _silhouettes[i].color = color;
        }

        private static void EnsureSilhouetteMaterial()
        {
            if (_silhouetteMaterial != null)
                return;

            var shader = Shader.Find(SilhouetteShaderName);
            if (shader != null)
            {
                _silhouetteMaterial = new Material(shader);
                return;
            }

            Debug.LogWarning($"[{nameof(SelectionOutline)}] Shader '{SilhouetteShaderName}' not found. Silhouette outline will use default sprite material.");
        }
    }
}
