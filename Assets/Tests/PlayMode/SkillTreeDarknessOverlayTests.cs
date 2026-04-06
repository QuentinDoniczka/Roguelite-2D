using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.UI.Screens.SkillTree;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class SkillTreeDarknessOverlayTests : PlayModeTestBase
    {
        private SkillTreeDarknessOverlay CreateOverlay()
        {
            var go = new GameObject("TestViewport");
            Track(go);
            go.AddComponent<RectTransform>();
            var overlay = go.AddComponent<SkillTreeDarknessOverlay>();
            return overlay;
        }

        [Test]
        public void Initialize_CreatesOverlayChild()
        {
            var overlay = CreateOverlay();

            LogAssert.ignoreFailingMessages = true;
            overlay.Initialize();
            LogAssert.ignoreFailingMessages = false;

            var child = overlay.transform.Find("DarknessOverlay");
            Assert.IsNotNull(child, "Expected a child named 'DarknessOverlay' after Initialize()");
        }

        [Test]
        public void Initialize_OverlayHasRawImage()
        {
            var overlay = CreateOverlay();

            LogAssert.ignoreFailingMessages = true;
            overlay.Initialize();
            LogAssert.ignoreFailingMessages = false;

            Assert.IsNotNull(overlay.OverlayImage);
            var child = overlay.transform.Find("DarknessOverlay");
            var rawImage = child.GetComponent<RawImage>();
            Assert.IsNotNull(rawImage);
            Assert.AreSame(overlay.OverlayImage, rawImage);
        }

        [Test]
        public void Initialize_RaycastTargetIsFalse()
        {
            var overlay = CreateOverlay();

            LogAssert.ignoreFailingMessages = true;
            overlay.Initialize();
            LogAssert.ignoreFailingMessages = false;

            Assert.IsFalse(overlay.OverlayImage.raycastTarget);
        }

        [Test]
        public void Initialize_OverlayIsStretchedToParent()
        {
            var overlay = CreateOverlay();

            LogAssert.ignoreFailingMessages = true;
            overlay.Initialize();
            LogAssert.ignoreFailingMessages = false;

            var rect = overlay.OverlayImage.rectTransform;
            Assert.AreEqual(Vector2.zero, rect.anchorMin);
            Assert.AreEqual(Vector2.one, rect.anchorMax);
            Assert.AreEqual(Vector2.zero, rect.offsetMin);
            Assert.AreEqual(Vector2.zero, rect.offsetMax);
        }

        [Test]
        public void Initialize_OverlayIsLastSibling()
        {
            var overlay = CreateOverlay();

            var dummyChild = new GameObject("ExistingChild");
            dummyChild.transform.SetParent(overlay.transform, false);

            LogAssert.ignoreFailingMessages = true;
            overlay.Initialize();
            LogAssert.ignoreFailingMessages = false;

            var overlayChild = overlay.transform.Find("DarknessOverlay");
            int expectedIndex = overlay.transform.childCount - 1;
            Assert.AreEqual(expectedIndex, overlayChild.GetSiblingIndex());
        }

        [Test]
        public void Initialize_MaterialUsesCorrectShaderOrFallback()
        {
            var overlay = CreateOverlay();

            LogAssert.ignoreFailingMessages = true;
            overlay.Initialize();
            LogAssert.ignoreFailingMessages = false;

            if (overlay.MaterialInstance != null)
            {
                Assert.AreEqual("Custom/SkillTreeDarkness", overlay.MaterialInstance.shader.name);
            }
            else
            {
                Assert.AreEqual(overlay.DarknessColor, overlay.OverlayImage.color,
                    "When shader is unavailable, overlay image color should be set to DarknessColor as fallback");
            }
        }

        [Test]
        public void SetDarknessColor_UpdatesMaterialOrImageColor()
        {
            var overlay = CreateOverlay();

            LogAssert.ignoreFailingMessages = true;
            overlay.Initialize();
            LogAssert.ignoreFailingMessages = false;

            overlay.SetDarknessColor(Color.red);

            Assert.AreEqual(Color.red, overlay.DarknessColor);

            if (overlay.MaterialInstance != null)
            {
                Assert.AreEqual(Color.red, overlay.MaterialInstance.GetColor("_Color"));
            }
            else
            {
                Assert.AreEqual(Color.red, overlay.OverlayImage.color);
            }
        }

        [Test]
        public void SetOpacity_UpdatesAlpha()
        {
            var overlay = CreateOverlay();

            LogAssert.ignoreFailingMessages = true;
            overlay.Initialize();
            LogAssert.ignoreFailingMessages = false;

            overlay.SetOpacity(0.5f);

            Assert.AreEqual(0.5f, overlay.DarknessColor.a, 0.001f);
        }

        [UnityTest]
        public IEnumerator OnDestroy_CleansMaterialInstance()
        {
            var overlay = CreateOverlay();

            LogAssert.ignoreFailingMessages = true;
            overlay.Initialize();
            LogAssert.ignoreFailingMessages = false;

            var materialInstance = overlay.MaterialInstance;

            Object.DestroyImmediate(overlay);
            yield return null;

            if (materialInstance != null)
            {
                Assert.IsTrue(materialInstance == null,
                    "Material instance should be destroyed when SkillTreeDarknessOverlay is destroyed");
            }
        }

        [Test]
        public void Initialize_CalledTwice_DoesNotDuplicate()
        {
            var overlay = CreateOverlay();

            LogAssert.ignoreFailingMessages = true;
            overlay.Initialize();
            overlay.Initialize();
            LogAssert.ignoreFailingMessages = false;

            int darknessOverlayCount = 0;
            for (int i = 0; i < overlay.transform.childCount; i++)
            {
                if (overlay.transform.GetChild(i).name == "DarknessOverlay")
                    darknessOverlayCount++;
            }

            Assert.AreEqual(1, darknessOverlayCount,
                "Calling Initialize() twice should not create a second DarknessOverlay child");
        }
    }
}
