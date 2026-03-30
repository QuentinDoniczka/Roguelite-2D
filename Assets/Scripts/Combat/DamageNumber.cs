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

        public void Play(Vector3 worldPosition, int damageValue, Color color, DamageNumberConfig config, float arcDirectionSign = 1f, float arcHeight = -1f, float arcWidth = -1f)
        {
            if (_activeCoroutine != null)
                StopCoroutine(_activeCoroutine);

            float resolvedArcHeight = arcHeight < 0f ? config.ArcHeight : arcHeight;
            float resolvedArcWidth = arcWidth < 0f ? config.ArcWidth : arcWidth;

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
            _activeCoroutine = StartCoroutine(AnimateCoroutine(config, arcDirectionSign, resolvedArcHeight, resolvedArcWidth));
        }

        private IEnumerator AnimateCoroutine(DamageNumberConfig config, float directionSign, float arcHeight, float arcWidth)
        {
            Vector3 startPos = transform.localPosition;
            float elapsed = 0f;

            while (elapsed < config.Lifetime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / config.Lifetime);

                float x = startPos.x + arcWidth * t * directionSign;
                float y = startPos.y + arcHeight * 4f * t * (1f - t);

                transform.localPosition = new Vector3(x, y, startPos.z);

                Color c = _tmp.color;
                c.a = t < 0.5f ? 1f : 1f - (t - 0.5f) * 2f;
                _tmp.color = c;

                yield return null;
            }

            gameObject.SetActive(false);
            _returnToPool?.Invoke(this);
        }
    }
}
