using System;
using System.Collections;
using RogueliteAutoBattler.Data;
using TMPro;
using UnityEngine;
using static TMPro.ShaderUtilities;

namespace RogueliteAutoBattler.Combat
{
    [RequireComponent(typeof(TextMeshPro))]
    public class DamageNumber : MonoBehaviour
    {
        private TextMeshPro _tmp;
        private Action<DamageNumber> _returnToPool;
        private Coroutine _activeCoroutine;
        private int _effectsSortingLayerId;

        private void Awake()
        {
            _tmp = GetComponent<TextMeshPro>();
            _tmp.alignment = TextAlignmentOptions.Center;
            _effectsSortingLayerId = SortingLayer.NameToID(SortingLayers.Effects);
        }

        public void Initialize(Action<DamageNumber> returnToPool)
        {
            _returnToPool = returnToPool;
        }

        public void Play(Vector3 worldPosition, int damageValue, Color color, DamageNumberConfig config)
        {
            if (_activeCoroutine != null)
                StopCoroutine(_activeCoroutine);

            transform.position = worldPosition;
            _tmp.text = damageValue.ToString();
            _tmp.color = color;
            if (config.Font != null)
                _tmp.font = config.Font;
            _tmp.fontSize = config.FontSize;
            _tmp.fontMaterial.SetFloat(ID_OutlineWidth, config.OutlineWidth);
            _tmp.fontMaterial.SetColor(ID_OutlineColor, config.OutlineColor);
            _tmp.ForceMeshUpdate();
            _tmp.sortingLayerID = _effectsSortingLayerId;
            _tmp.sortingOrder = config.SortingOrder;
            gameObject.SetActive(true);
            _activeCoroutine = StartCoroutine(AnimateCoroutine(config));
        }

        private IEnumerator AnimateCoroutine(DamageNumberConfig config)
        {
            Vector3 slideVector = (Vector3)(config.SlideDirection.normalized * config.SlideDistance);
            Vector3 startLocalPos = transform.localPosition;
            float elapsed = 0f;

            while (elapsed < config.Lifetime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / config.Lifetime);
                transform.localPosition = startLocalPos + slideVector * t;
                Color c = _tmp.color;
                c.a = 1f - t;
                _tmp.color = c;
                yield return null;
            }

            gameObject.SetActive(false);
            _returnToPool?.Invoke(this);
        }
    }
}
