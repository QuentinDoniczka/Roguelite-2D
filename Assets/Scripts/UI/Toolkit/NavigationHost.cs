using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.UI.Toolkit
{
    public class NavigationHost : MonoBehaviour
    {
        private static readonly string[] TAB_BUTTON_NAMES =
        {
            "tab-village", "tab-skilltree", "tab-autre", "tab-guilde", "tab-shop"
        };

        private static readonly string[] SCREEN_NAMES =
        {
            "screen-village", "screen-skilltree", "screen-autre", "screen-guilde", "screen-shop"
        };

        private const string DEFAULT_SCREEN_NAME = "screen-default";
        private const string HIDDEN_CLASS = "hidden";

        [Header("UI Document")]
        [SerializeField] private UIDocument _uiDocument;

        [Header("Input")]
        [SerializeField] private InputActionReference _cancelAction;

        public static NavigationHost Instance { get; private set; }
        public NavigationManager Navigation { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnDomainReload()
        {
            Instance = null;
        }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (_uiDocument == null)
            {
                Debug.LogWarning("[NavigationHost] UIDocument is not assigned.");
                return;
            }

            VisualElement root = _uiDocument.rootVisualElement;
            if (root == null)
            {
                Debug.LogWarning("[NavigationHost] UIDocument rootVisualElement is null.");
                return;
            }

            Button[] tabButtons = QueryTabButtons(root);
            if (tabButtons == null)
            {
                return;
            }

            VisualElement defaultElement = root.Q<VisualElement>(DEFAULT_SCREEN_NAME);
            if (defaultElement == null)
            {
                Debug.LogWarning("[NavigationHost] Default screen element not found.");
                return;
            }

            var defaultScreen = new ContainerScreen(defaultElement);
            Navigation = new NavigationManager(tabButtons, defaultScreen);

            RegisterTabScreens(root);
        }

        private Button[] QueryTabButtons(VisualElement root)
        {
            var buttons = new Button[TAB_BUTTON_NAMES.Length];

            for (int i = 0; i < TAB_BUTTON_NAMES.Length; i++)
            {
                buttons[i] = root.Q<Button>(TAB_BUTTON_NAMES[i]);
                if (buttons[i] != null)
                {
                    continue;
                }

                Debug.LogWarning($"[NavigationHost] Tab button '{TAB_BUTTON_NAMES[i]}' not found.");
                return null;
            }

            return buttons;
        }

        private void RegisterTabScreens(VisualElement root)
        {
            for (int i = 0; i < SCREEN_NAMES.Length; i++)
            {
                VisualElement screenElement = root.Q<VisualElement>(SCREEN_NAMES[i]);
                if (screenElement == null)
                {
                    Debug.LogWarning($"[NavigationHost] Screen element '{SCREEN_NAMES[i]}' not found.");
                    continue;
                }

                Navigation.RegisterTab(i, new ContainerScreen(screenElement));
            }
        }

        private void OnEnable()
        {
            if (_cancelAction != null && _cancelAction.action != null)
            {
                _cancelAction.action.Enable();
                _cancelAction.action.performed += OnCancelPerformed;
            }
        }

        private void OnDisable()
        {
            if (_cancelAction != null && _cancelAction.action != null)
            {
                _cancelAction.action.performed -= OnCancelPerformed;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            Navigation?.Dispose();
        }

        private void OnCancelPerformed(InputAction.CallbackContext context)
        {
            Navigation?.HandleCancel();
        }

        private class ContainerScreen : IScreen
        {
            public VisualElement Root { get; }

            public ContainerScreen(VisualElement element)
            {
                Root = element;
            }

            public void OnShow()
            {
                Root.RemoveFromClassList(HIDDEN_CLASS);
            }

            public void OnHide()
            {
                Root.AddToClassList(HIDDEN_CLASS);
            }

            public void OnPush()
            {
                OnHide();
            }

            public void OnPop()
            {
                OnShow();
            }
        }
    }
}
