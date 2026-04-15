using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.UI.Screens.SkillTree;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class SkillTreeScreenTests : PlayModeTestBase
    {
        [UnityTest]
        public IEnumerator VoidClick_DeselectsSelectedNode()
        {
            var canvasGo = new GameObject("ScreenCanvas");
            Track(canvasGo);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.transform.SetParent(canvasGo.transform, false);
            eventSystemGo.AddComponent<EventSystem>();

            var viewportGo = new GameObject("Viewport");
            viewportGo.transform.SetParent(canvasGo.transform, false);
            viewportGo.AddComponent<RectTransform>();
            viewportGo.AddComponent<Image>().raycastTarget = true;

            var contentGo = new GameObject("Content");
            contentGo.transform.SetParent(viewportGo.transform, false);
            var contentRect = contentGo.AddComponent<RectTransform>();

            var handler = viewportGo.AddComponent<SkillTreeInputHandler>();
            TestCharacterFactory.SetPrivateField(handler, "_content", contentRect);

            var managerGo = new GameObject("NodeManager");
            managerGo.transform.SetParent(canvasGo.transform, false);
            var manager = managerGo.AddComponent<SkillTreeNodeManager>();
            TestCharacterFactory.SetPrivateField(manager, "_content", contentRect);

            var screenGo = new GameObject("SkillTreeScreen");
            screenGo.transform.SetParent(canvasGo.transform, false);
            screenGo.SetActive(false);
            screenGo.AddComponent<CanvasGroup>();
            var screen = screenGo.AddComponent<SkillTreeScreen>();
            TestCharacterFactory.SetPrivateField(screen, "_inputHandler", handler);
            TestCharacterFactory.SetPrivateField(screen, "_nodeManager", manager);
            screenGo.SetActive(true);

            yield return null;

            var firstNodeGo = contentRect.GetChild(0).gameObject;
            var firstNode = firstNodeGo.GetComponent<SkillTreeNode>();

            var clickData = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute<IPointerClickHandler>(firstNodeGo, clickData, ExecuteEvents.pointerClickHandler);
            yield return null;

            Assert.IsTrue(firstNode.IsSelected);

            var voidClickData = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute<IPointerClickHandler>(viewportGo, voidClickData, ExecuteEvents.pointerClickHandler);
            yield return null;

            Assert.IsFalse(firstNode.IsSelected);
        }

        [UnityTest]
        public IEnumerator OnShow_OnHide_ToggleCanvasGroup()
        {
            var screenGo = new GameObject("SkillTreeScreen");
            Track(screenGo);
            screenGo.SetActive(false);

            var canvasGroup = screenGo.AddComponent<CanvasGroup>();

            var handlerGo = new GameObject("Handler");
            handlerGo.transform.SetParent(screenGo.transform, false);
            var contentGo = new GameObject("Content");
            contentGo.transform.SetParent(handlerGo.transform, false);
            var contentRect = contentGo.AddComponent<RectTransform>();
            var handler = handlerGo.AddComponent<SkillTreeInputHandler>();
            TestCharacterFactory.SetPrivateField(handler, "_content", contentRect);

            var managerGo = new GameObject("Manager");
            managerGo.transform.SetParent(screenGo.transform, false);
            var manager = managerGo.AddComponent<SkillTreeNodeManager>();
            TestCharacterFactory.SetPrivateField(manager, "_content", contentRect);

            var screen = screenGo.AddComponent<SkillTreeScreen>();
            TestCharacterFactory.SetPrivateField(screen, "_inputHandler", handler);
            TestCharacterFactory.SetPrivateField(screen, "_nodeManager", manager);
            screenGo.SetActive(true);

            yield return null;

            screen.OnShow();
            Assert.AreEqual(1f, canvasGroup.alpha, 0.01f);
            Assert.IsTrue(canvasGroup.blocksRaycasts);
            Assert.IsTrue(canvasGroup.interactable);

            screen.OnHide();
            Assert.AreEqual(0f, canvasGroup.alpha, 0.01f);
            Assert.IsFalse(canvasGroup.blocksRaycasts);
            Assert.IsFalse(canvasGroup.interactable);
        }

        [UnityTest]
        public IEnumerator OnShow_ExpandsGameArea()
        {
            var screenGo = new GameObject("SkillTreeScreen");
            Track(screenGo);
            screenGo.SetActive(false);
            screenGo.AddComponent<CanvasGroup>();

            var gameAreaGo = new GameObject("GameArea");
            gameAreaGo.transform.SetParent(screenGo.transform, false);
            var gameAreaRect = gameAreaGo.AddComponent<RectTransform>();
            gameAreaRect.anchorMin = new Vector2(0, 0.40f);
            gameAreaRect.anchorMax = new Vector2(1, 1f);

            var infoAreaGo = new GameObject("InfoArea");
            infoAreaGo.transform.SetParent(screenGo.transform, false);
            var infoAreaRect = infoAreaGo.AddComponent<RectTransform>();
            infoAreaRect.anchorMin = new Vector2(0, 0.08f);
            infoAreaRect.anchorMax = new Vector2(1, 0.40f);

            var screen = screenGo.AddComponent<SkillTreeScreen>();
            TestCharacterFactory.SetPrivateField(screen, "_gameArea", gameAreaRect);
            TestCharacterFactory.SetPrivateField(screen, "_infoArea", infoAreaRect);
            screenGo.SetActive(true);

            yield return null;

            screen.OnShow();
            Assert.AreEqual(0.08f, gameAreaRect.anchorMin.y, 0.001f);
            Assert.AreEqual(0.08f, infoAreaRect.anchorMax.y, 0.001f);
        }

        [UnityTest]
        public IEnumerator OnHide_RestoresGameArea()
        {
            var screenGo = new GameObject("SkillTreeScreen");
            Track(screenGo);
            screenGo.SetActive(false);
            screenGo.AddComponent<CanvasGroup>();

            var gameAreaGo = new GameObject("GameArea");
            gameAreaGo.transform.SetParent(screenGo.transform, false);
            var gameAreaRect = gameAreaGo.AddComponent<RectTransform>();
            gameAreaRect.anchorMin = new Vector2(0, 0.40f);
            gameAreaRect.anchorMax = new Vector2(1, 1f);

            var infoAreaGo = new GameObject("InfoArea");
            infoAreaGo.transform.SetParent(screenGo.transform, false);
            var infoAreaRect = infoAreaGo.AddComponent<RectTransform>();
            infoAreaRect.anchorMin = new Vector2(0, 0.08f);
            infoAreaRect.anchorMax = new Vector2(1, 0.40f);

            var screen = screenGo.AddComponent<SkillTreeScreen>();
            TestCharacterFactory.SetPrivateField(screen, "_gameArea", gameAreaRect);
            TestCharacterFactory.SetPrivateField(screen, "_infoArea", infoAreaRect);
            screenGo.SetActive(true);

            yield return null;

            screen.OnShow();
            screen.OnHide();
            Assert.AreEqual(0.40f, gameAreaRect.anchorMin.y, 0.001f);
            Assert.AreEqual(0.40f, infoAreaRect.anchorMax.y, 0.001f);
        }

        [UnityTest]
        public IEnumerator OnShow_NullAreas_DoesNotThrow()
        {
            var screenGo = new GameObject("SkillTreeScreen");
            Track(screenGo);
            screenGo.SetActive(false);
            screenGo.AddComponent<CanvasGroup>();
            var screen = screenGo.AddComponent<SkillTreeScreen>();
            screenGo.SetActive(true);

            yield return null;

            Assert.DoesNotThrow(() => screen.OnShow());
            Assert.DoesNotThrow(() => screen.OnHide());
        }
    }
}
