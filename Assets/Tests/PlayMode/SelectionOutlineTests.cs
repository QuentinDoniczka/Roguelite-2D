using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Visuals;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class SelectionOutlineTests : PlayModeTestBase
    {
        private static readonly int OutlineEnabledId = Shader.PropertyToID("_OutlineEnabled");
        private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
        private static readonly int OutlineWidthId = Shader.PropertyToID("_OutlineWidth");

        private GameObject _character;
        private SelectionOutline _outline;
        private SpriteRenderer _renderer;
        private Material _testMaterial;

        [SetUp]
        public void SetUp()
        {
            _character = new GameObject("OutlineTestChar");
            Track(_character);

            var visual = new GameObject("Visual");
            visual.transform.SetParent(_character.transform, false);
            _renderer = visual.AddComponent<SpriteRenderer>();

            _testMaterial = new Material(Shader.Find("Sprites/Default"));
            _outline = _character.AddComponent<SelectionOutline>();
            _outline.Initialize(_testMaterial);
        }

        [UnityTest]
        public IEnumerator Initialize_CachesAllRenderers_SetOutlineDoesNotThrow()
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
            Assert.IsTrue(_outline.IsOutlined);

            _outline.ClearOutline();

            Assert.IsFalse(_outline.IsOutlined);
        }

        [UnityTest]
        public IEnumerator SetOutline_SetsPropertyBlockOnRenderers()
        {
            yield return null;

            Color expectedColor = Color.green;
            float expectedWidth = 1.5f;

            _outline.SetOutline(true, expectedColor, expectedWidth);

            var mpb = new MaterialPropertyBlock();
            _renderer.GetPropertyBlock(mpb);

            float outlineEnabled = mpb.GetFloat(OutlineEnabledId);
            Color outlineColor = mpb.GetColor(OutlineColorId);
            float outlineWidth = mpb.GetFloat(OutlineWidthId);

            Assert.AreEqual(1f, outlineEnabled, 0.001f);
            Assert.AreEqual(expectedColor, outlineColor);
            Assert.AreEqual(expectedWidth, outlineWidth, 0.001f);
        }

        [UnityTest]
        public IEnumerator SetOutline_Disabled_SetsPropertyBlockWithZeroEnabled()
        {
            yield return null;

            _outline.SetOutline(true, Color.green);
            _outline.SetOutline(false, Color.clear);

            var mpb = new MaterialPropertyBlock();
            _renderer.GetPropertyBlock(mpb);

            float outlineEnabled = mpb.GetFloat(OutlineEnabledId);

            Assert.AreEqual(0f, outlineEnabled, 0.001f);
            Assert.IsFalse(_outline.IsOutlined);
        }

        [UnityTest]
        public IEnumerator Initialize_WithMultipleRenderers_AllReceivePropertyBlock()
        {
            var secondVisual = new GameObject("Visual2");
            secondVisual.transform.SetParent(_character.transform, false);
            var secondRenderer = secondVisual.AddComponent<SpriteRenderer>();

            var multiOutline = _character.AddComponent<SelectionOutline>();
            Object.DestroyImmediate(_outline);
            multiOutline.Initialize(new Material(Shader.Find("Sprites/Default")));

            yield return null;

            multiOutline.SetOutline(true, Color.red, 2f);

            var mpb1 = new MaterialPropertyBlock();
            _renderer.GetPropertyBlock(mpb1);
            Assert.AreEqual(1f, mpb1.GetFloat(OutlineEnabledId), 0.001f);

            var mpb2 = new MaterialPropertyBlock();
            secondRenderer.GetPropertyBlock(mpb2);
            Assert.AreEqual(1f, mpb2.GetFloat(OutlineEnabledId), 0.001f);
        }
    }
}
