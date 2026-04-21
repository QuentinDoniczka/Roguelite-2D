using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.UI.Toolkit
{
    public class NavigationHost : MonoBehaviour
    {
        private static readonly string[] TabButtonNames =
        {
            "tab-village", "tab-skilltree", "tab-autre", "tab-guilde", "tab-shop"
        };

        private static readonly string[] ScreenNames =
        {
            "screen-village", "screen-skilltree", "screen-autre", "screen-guilde", "screen-shop"
        };

        private const string DefaultScreenName = "screen-default";
        private const string HiddenClass = "hidden";

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

            BuildNavigation(_uiDocument.rootVisualElement);
        }

        internal void BuildNavigation(VisualElement root)
        {
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

            VisualElement defaultElement = root.Q<VisualElement>(DefaultScreenName);
            if (defaultElement == null)
            {
                Debug.LogWarning("[NavigationHost] Default screen element not found.");
                return;
            }

            var defaultScreen = new ContainerScreen(defaultElement);
            Navigation = new NavigationManager(tabButtons, defaultScreen);

            RegisterTabScreens(root);

            root.RegisterCallback<GeometryChangedEvent>(ValidateFontOnFirstLayout);
        }

        private void ValidateFontOnFirstLayout(GeometryChangedEvent evt)
        {
            VisualElement root = _uiDocument.rootVisualElement;
            root.UnregisterCallback<GeometryChangedEvent>(ValidateFontOnFirstLayout);

            Label firstLabel = root.Q<Label>();
            if (firstLabel == null)
                return;

            FontDefinition fontDef = firstLabel.resolvedStyle.unityFontDefinition;
            if (fontDef.fontAsset == null && fontDef.font == null)
            {
                Debug.LogError("[NavigationHost] Font not resolved — check USS font path in MainStyle.uss (.root -unity-font-definition). Text will be invisible.");
            }
        }

        private Button[] QueryTabButtons(VisualElement root)
        {
            var buttons = new Button[TabButtonNames.Length];

            for (int i = 0; i < TabButtonNames.Length; i++)
            {
                buttons[i] = root.Q<Button>(TabButtonNames[i]);
                if (buttons[i] != null)
                {
                    continue;
                }

                Debug.LogWarning($"[NavigationHost] Tab button '{TabButtonNames[i]}' not found.");
                return null;
            }

            return buttons;
        }

        private void RegisterTabScreens(VisualElement root)
        {
            for (int i = 0; i < ScreenNames.Length; i++)
            {
                VisualElement screenElement = root.Q<VisualElement>(ScreenNames[i]);
                if (screenElement == null)
                {
                    Debug.LogWarning($"[NavigationHost] Screen element '{ScreenNames[i]}' not found.");
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
                Root.RemoveFromClassList(HiddenClass);
            }

            public void OnHide()
            {
                Root.AddToClassList(HiddenClass);
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
