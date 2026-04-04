using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.UI.Screens.SkillTree;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class SkillTreeInputHandlerTests : PlayModeTestBase
    {
        private GameObject _root;
        private SkillTreeInputHandler _handler;
        private RectTransform _content;

        [SetUp]
        public void SetUp()
        {
            (_root, _handler, _content) = TestCharacterFactory.CreateSkillTreeViewport();
            Track(_root);
        }

        [UnityTest]
        public IEnumerator OnDrag_TranslatesContentPosition()
        {
            yield return null;

            Vector2 initialPosition = _content.anchoredPosition;
            Vector2 dragDelta = new Vector2(50f, 30f);

            var eventData = new PointerEventData(EventSystem.current) { delta = dragDelta };
            ExecuteEvents.Execute<IDragHandler>(_handler.gameObject, eventData, ExecuteEvents.dragHandler);
            yield return null;

            Vector2 expectedPosition = initialPosition + dragDelta;
            Assert.AreEqual(expectedPosition.x, _content.anchoredPosition.x, 0.01f);
            Assert.AreEqual(expectedPosition.y, _content.anchoredPosition.y, 0.01f);
        }

        [UnityTest]
        public IEnumerator OnScroll_ScalesContent()
        {
            yield return null;

            float initialScale = _content.localScale.x;

            var eventData = new PointerEventData(EventSystem.current)
            {
                scrollDelta = new Vector2(0f, 1f),
                position = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f)
            };
            ExecuteEvents.Execute<IScrollHandler>(_handler.gameObject, eventData, ExecuteEvents.scrollHandler);
            yield return null;

            Assert.Greater(_content.localScale.x, initialScale);
        }

        [UnityTest]
        public IEnumerator OnScroll_ClampsMinScale()
        {
            yield return null;

            for (int i = 0; i < 50; i++)
            {
                var eventData = new PointerEventData(EventSystem.current)
                {
                    scrollDelta = new Vector2(0f, -5f),
                    position = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f)
                };
                ExecuteEvents.Execute<IScrollHandler>(_handler.gameObject, eventData, ExecuteEvents.scrollHandler);
            }

            yield return null;

            Assert.GreaterOrEqual(_content.localScale.x, 0.3f - 0.001f);
        }

        [UnityTest]
        public IEnumerator OnScroll_ClampsMaxScale()
        {
            yield return null;

            for (int i = 0; i < 50; i++)
            {
                var eventData = new PointerEventData(EventSystem.current)
                {
                    scrollDelta = new Vector2(0f, 5f),
                    position = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f)
                };
                ExecuteEvents.Execute<IScrollHandler>(_handler.gameObject, eventData, ExecuteEvents.scrollHandler);
            }

            yield return null;

            Assert.LessOrEqual(_content.localScale.x, 3.0f + 0.001f);
        }

        [UnityTest]
        public IEnumerator OnPointerClick_FiresVoidClicked()
        {
            yield return null;

            bool voidClickFired = false;
            _handler.OnVoidClicked += () => voidClickFired = true;

            var eventData = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute<IPointerClickHandler>(_handler.gameObject, eventData, ExecuteEvents.pointerClickHandler);
            yield return null;

            Assert.IsTrue(voidClickFired);
        }
    }
}
