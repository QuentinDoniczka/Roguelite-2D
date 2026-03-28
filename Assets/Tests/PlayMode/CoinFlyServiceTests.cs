using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.Combat;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class CoinFlyServiceTests : PlayModeTestBase
    {
        private Canvas _canvas;
        private Camera _camera;
        private RectTransform _container;
        private RectTransform _targetBadge;
        private Sprite _coinSprite;

        [SetUp]
        public void SetUp()
        {
            CoinFlyService.ResetForTest();

            var cameraGo = Track(new GameObject("TestCamera"));
            _camera = cameraGo.AddComponent<Camera>();
            _camera.orthographic = true;
            _camera.orthographicSize = 5f;

            var canvasGo = Track(new GameObject("TestCanvas"));
            _canvas = canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceCamera;
            _canvas.worldCamera = _camera;
            canvasGo.AddComponent<CanvasScaler>();

            var containerGo = new GameObject("CoinFlyContainer");
            containerGo.transform.SetParent(canvasGo.transform, false);
            _container = containerGo.AddComponent<RectTransform>();

            var badgeGo = new GameObject("TargetBadge");
            badgeGo.transform.SetParent(canvasGo.transform, false);
            _targetBadge = badgeGo.AddComponent<RectTransform>();
            _targetBadge.anchoredPosition = new Vector2(100f, 100f);

            Texture2D texture = new Texture2D(4, 4);
            _coinSprite = Sprite.Create(texture, new Rect(0, 0, 4, 4), Vector2.one * 0.5f);

            CoinFlyService.Initialize(_container, _targetBadge, _camera, _canvas, _coinSprite);
        }

        [UnityTest]
        public IEnumerator Initialize_CreatesPoolInstances()
        {
            yield return null;

            Assert.AreEqual(5, _container.childCount);

            for (int i = 0; i < _container.childCount; i++)
            {
                Assert.IsFalse(_container.GetChild(i).gameObject.activeSelf);
            }
        }

        [UnityTest]
        public IEnumerator Show_ActivatesOneInstance()
        {
            yield return null;

            CoinFlyService.Show(Vector3.zero);
            yield return null;

            int activeCount = CountActiveChildren(_container);
            Assert.AreEqual(1, activeCount);
        }

        [UnityTest]
        public IEnumerator Show_ReturnsToPoolAfterDuration()
        {
            yield return null;

            CoinFlyService.Show(Vector3.zero);
            yield return null;

            Assert.AreEqual(1, CountActiveChildren(_container));

            yield return new WaitForSeconds(0.8f);

            Assert.AreEqual(0, CountActiveChildren(_container));
        }

        [UnityTest]
        public IEnumerator Show_MultipleCallsReusePool()
        {
            yield return null;

            CoinFlyService.Show(Vector3.zero);
            yield return null;

            Assert.AreEqual(5, _container.childCount);

            yield return new WaitForSeconds(0.8f);

            Assert.AreEqual(0, CountActiveChildren(_container));

            CoinFlyService.Show(Vector3.one);
            yield return null;

            Assert.AreEqual(5, _container.childCount);
            Assert.AreEqual(1, CountActiveChildren(_container));
        }

        public override void TearDown()
        {
            CoinFlyService.ResetForTest();

            if (_coinSprite != null)
                Object.Destroy(_coinSprite);

            if (_coinSprite != null && _coinSprite.texture != null)
                Object.Destroy(_coinSprite.texture);

            base.TearDown();
        }

        private static int CountActiveChildren(Transform parent)
        {
            int count = 0;
            for (int i = 0; i < parent.childCount; i++)
            {
                if (parent.GetChild(i).gameObject.activeSelf)
                    count++;
            }
            return count;
        }
    }
}
