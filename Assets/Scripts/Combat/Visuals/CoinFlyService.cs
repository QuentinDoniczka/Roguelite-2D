using System;
using System.Collections.Generic;
using RogueliteAutoBattler.UI.Widgets;
using UnityEngine;
using UnityEngine.UI;

namespace RogueliteAutoBattler.Combat.Visuals
{
    public static class CoinFlyService
    {
        private static readonly Queue<CoinFly> _pool = new Queue<CoinFly>();
        private static RectTransform _container;
        private static RectTransform _targetBadge;
        private static GoldHudBadge _targetBadgeComponent;
        private static Camera _camera;
        private static Canvas _canvas;
        private static Sprite _coinSprite;
        private static bool _isInitialized;

        private const int InitialPoolSize = 5;
        private const float Duration = 0.6f;
        private static readonly Color CoinColor = new Color32(255, 215, 0, 255);
        private const float CoinSize = 32f;

        public static void Initialize(RectTransform container, RectTransform targetBadge, Camera camera, Canvas canvas, Sprite coinSprite)
        {
            _container = container;
            _targetBadge = targetBadge;
            _targetBadgeComponent = targetBadge != null ? targetBadge.GetComponent<GoldHudBadge>() : null;
            _camera = camera;
            _canvas = canvas;
            _coinSprite = coinSprite;

            for (int i = 0; i < InitialPoolSize; i++)
            {
                _pool.Enqueue(CreateInstance());
            }

            _isInitialized = true;
        }

        public static void Show(Vector3 worldPosition, Action onComplete = null)
        {
            if (!_isInitialized)
                return;

            Vector3 screenPoint = _camera.WorldToScreenPoint(worldPosition);

            Camera uiCamera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _camera;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _container, screenPoint, uiCamera, out Vector2 startLocal);

            Vector2 targetScreenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, _targetBadge.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _container, targetScreenPoint, uiCamera, out Vector2 targetLocal);

            CoinFly coin = _pool.Count > 0
                ? _pool.Dequeue()
                : CreateInstance();

            coin.Play(startLocal, targetLocal, Duration, () => OnCoinArrived(onComplete));
        }

        private static void OnCoinArrived(Action onComplete)
        {
            if (_targetBadgeComponent != null)
                _targetBadgeComponent.Punch(onComplete);
            else
                onComplete?.Invoke();
        }

        private static CoinFly CreateInstance()
        {
            var go = new GameObject("CoinFly");
            go.transform.SetParent(_container, false);

            Image image = go.AddComponent<Image>();
            image.sprite = _coinSprite;
            image.color = CoinColor;

            RectTransform rectTransform = go.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(CoinSize, CoinSize);

            CoinFly coin = go.AddComponent<CoinFly>();
            coin.Initialize(ReturnToPool);

            go.SetActive(false);
            return coin;
        }

        private static void ReturnToPool(CoinFly instance)
        {
            _pool.Enqueue(instance);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnDomainReload()
        {
            _pool.Clear();
            _container = null;
            _targetBadge = null;
            _targetBadgeComponent = null;
            _camera = null;
            _canvas = null;
            _coinSprite = null;
            _isInitialized = false;
        }

        internal static void ResetForTest()
        {
            ResetOnDomainReload();
        }
    }
}
