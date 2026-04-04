using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Visuals;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class SelectionOutlineTests : PlayModeTestBase
    {
        private GameObject _character;
        private SelectionOutline _outline;
        private SpriteRenderer _renderer;

        [SetUp]
        public void SetUp()
        {
            _character = new GameObject("OutlineTestChar");
            Track(_character);

            var visual = new GameObject("Visual");
            visual.transform.SetParent(_character.transform, false);
            _renderer = visual.AddComponent<SpriteRenderer>();

            _outline = _character.AddComponent<SelectionOutline>();
            _outline.Initialize();
        }

        [UnityTest]
        public IEnumerator Initialize_CachesRenderers_SetOutlineDoesNotThrow()
        {
            yield return null;
            Assert.DoesNotThrow(() => _outline.SetOutline(true, Color.green));
            Assert.IsTrue(_outline.IsOutlined);
        }

        [UnityTest]
        public IEnumerator SetOutline_Enabled_SetsIsOutlinedTrue()
        {
            yield return null;
            _outline.SetOutline(true, Color.green);
            Assert.IsTrue(_outline.IsOutlined);
        }

        [UnityTest]
        public IEnumerator ClearOutline_AfterEnable_SetsIsOutlinedFalse()
        {
            yield return null;
            _outline.SetOutline(true, Color.green);
            _outline.ClearOutline();
            Assert.IsFalse(_outline.IsOutlined);
        }

        [UnityTest]
        public IEnumerator SetOutline_CreatesSilhouetteContainer()
        {
            yield return null;
            _outline.SetOutline(true, Color.green);

            var container = _character.transform.Find("_OutlineSilhouettes");
            Assert.IsNotNull(container, "Silhouette container should be created");
            Assert.IsTrue(container.gameObject.activeSelf);
        }

        [UnityTest]
        public IEnumerator SetOutline_CreatesSilhouettePerRenderer()
        {
            yield return null;
            _outline.SetOutline(true, Color.green);

            var container = _character.transform.Find("_OutlineSilhouettes");
            Assert.AreEqual(1, container.childCount, "Should have one silhouette per renderer");

            var silRenderer = container.GetChild(0).GetComponent<SpriteRenderer>();
            Assert.IsNotNull(silRenderer);
        }

        [UnityTest]
        public IEnumerator SetOutline_SilhouetteHasCorrectColor()
        {
            yield return null;
            Color outlineColor = Color.green;
            _outline.SetOutline(true, outlineColor);

            var container = _character.transform.Find("_OutlineSilhouettes");
            var silRenderer = container.GetChild(0).GetComponent<SpriteRenderer>();
            Assert.AreEqual(outlineColor, silRenderer.color);
        }

        [UnityTest]
        public IEnumerator ClearOutline_DisablesSilhouetteContainer()
        {
            yield return null;
            _outline.SetOutline(true, Color.green);
            _outline.ClearOutline();

            var container = _character.transform.Find("_OutlineSilhouettes");
            Assert.IsNotNull(container);
            Assert.IsFalse(container.gameObject.activeSelf);
        }

        [UnityTest]
        public IEnumerator SetOutline_SilhouetteSyncsSprite()
        {
            var sprite = Sprite.Create(
                Texture2D.whiteTexture,
                new Rect(0, 0, 4, 4),
                new Vector2(0.5f, 0.5f));
            _renderer.sprite = sprite;

            yield return null;
            _outline.SetOutline(true, Color.green);
            yield return null;

            var container = _character.transform.Find("_OutlineSilhouettes");
            var silRenderer = container.GetChild(0).GetComponent<SpriteRenderer>();
            Assert.AreEqual(sprite, silRenderer.sprite, "Silhouette should sync original's sprite");
        }

        [UnityTest]
        public IEnumerator MultipleRenderers_AllGetSilhouettes()
        {
            var secondVisual = new GameObject("Visual2");
            secondVisual.transform.SetParent(_character.transform, false);
            secondVisual.AddComponent<SpriteRenderer>();

            Object.DestroyImmediate(_outline);
            _outline = _character.AddComponent<SelectionOutline>();
            _outline.Initialize();

            yield return null;
            _outline.SetOutline(true, Color.red);

            var container = _character.transform.Find("_OutlineSilhouettes");
            Assert.AreEqual(2, container.childCount, "Should have two silhouettes");

            for (int i = 0; i < container.childCount; i++)
            {
                var sr = container.GetChild(i).GetComponent<SpriteRenderer>();
                Assert.AreEqual(Color.red, sr.color);
            }
        }
    }
}
