using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RogueliteAutoBattler.Combat
{
    public static class CoinFlyService
    {
        private static readonly Queue<CoinFly> _pool = new Queue<CoinFly>();
        private static RectTransform _container;
        private static RectTransform _targetBadge;
        private static Camera _camera;
        private static Canvas _canvas;
        private static Sprite _coinSprite;
        private static bool _isInitialized;

        private const int INITIAL_POOL_SIZE = 5;
        private const float DURATION = 0.6f;
        private static readonly Color COIN_COLOR = new Color32(255, 215, 0, 255);
        private const float COIN_SIZE = 32f;

        public static void Initialize(RectTransform container, RectTransform targetBadge, Camera camera, Canvas canvas, Sprite coinSprite)
        {
            _container = container;
            _targetBadge = targetBadge;
            _camera = camera;
            _canvas = canvas;
            _coinSprite = coinSprite;

            for (int i = 0; i < INITIAL_POOL_SIZE; i++)
            {
                _pool.Enqueue(CreateInstance());
            }

            _isInitialized = true;
        }

        public static void Show(Vector3 worldPosition, Action onArrive = null)
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

            coin.Play(startLocal, targetLocal, DURATION, onArrive);
        }

        private static CoinFly CreateInstance()
        {
            var go = new GameObject("CoinFly");
            go.transform.SetParent(_container, false);

            Image image = go.AddComponent<Image>();
            image.sprite = _coinSprite;
            image.color = COIN_COLOR;

            RectTransform rectTransform = go.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(COIN_SIZE, COIN_SIZE);

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
