#if UNITY_EDITOR
using NUnit.Framework;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Editor.Windows;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class SkillTreePreviewToolbarTests
    {
        private SkillTreeVisualSettings _settings;

        [SetUp]
        public void SetUp()
        {
            _settings = ScriptableObject.CreateInstance<SkillTreeVisualSettings>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_settings);
        }

        [Test]
        public void BuildToolbar_Contains5Sliders()
        {
            var toolbar = new SkillTreePreviewToolbar(_settings, null);
            var root = toolbar.BuildToolbar();

            var sliders = root.Query<Slider>().ToList();
            Assert.AreEqual(5, sliders.Count, "Toolbar should contain exactly 5 sliders.");
        }

        [Test]
        public void SliderChange_WritesToSettings_AndCallsOnChanged()
        {
            bool fired = false;
            var toolbar = new SkillTreePreviewToolbar(_settings, () => fired = true);
            var root = toolbar.BuildToolbar();

            var sliders = root.Query<Slider>().ToList();
            Assert.IsTrue(sliders.Count > 0, "Expected at least one slider.");

            sliders[0].value = 150f;

            Assert.AreEqual(150f, _settings.HaloSize, 0.01f, "HaloSize should have been written to settings.");
            Assert.IsTrue(fired, "onChanged callback should have been invoked.");
        }

        [Test]
        public void SliderChange_MarksDirty()
        {
            var toolbar = new SkillTreePreviewToolbar(_settings, null);
            var root = toolbar.BuildToolbar();

            var sliders = root.Query<Slider>().ToList();
            Assert.IsTrue(sliders.Count > 0, "Expected at least one slider.");

            sliders[0].value = 160f;

            Assert.IsTrue(EditorUtility.IsDirty(_settings), "Settings ScriptableObject should be marked dirty after slider change.");
        }
    }
}
#endif
