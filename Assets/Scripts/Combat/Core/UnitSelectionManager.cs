using RogueliteAutoBattler.Combat.Visuals;
using RogueliteAutoBattler.Common;
using RogueliteAutoBattler.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Combat.Core
{
    public class UnitSelectionManager : MonoBehaviour
    {
        private static readonly Color AllyOutlineColor = new Color(0f, 1f, 0f, 1f);
        private static readonly Color EnemyOutlineColor = new Color(1f, 0f, 0f, 1f);
        private const float OutlineWidth = 1.5f;

        public static UnitSelectionManager Instance { get; private set; }

        private SelectionOutline _selectedOutline;
        private GameObject _selectedGo;
        private bool _hasSelection;
        private Camera _camera;
        private int _cachedSelectionLayerMask;
        private int _cachedAllyLayer;

        public GameObject SelectedUnit => _selectedGo;

        public event System.Action<GameObject> OnUnitSelected;
        public event System.Action OnUnitDeselected;

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
            _camera = GameBootstrap.MainCamera;
            _cachedSelectionLayerMask = PhysicsLayers.SelectionLayerMask;
            _cachedAllyLayer = PhysicsLayers.AllyLayer;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Update()
        {
            var pointer = Pointer.current;
            if (pointer == null) return;
            if (!pointer.press.wasPressedThisFrame) return;

            HandleClick(pointer.position.ReadValue());
        }

        private void LateUpdate()
        {
            if (!_hasSelection) return;

            if (_selectedGo == null)
            {
                _hasSelection = false;
                _selectedOutline = null;
                OnUnitDeselected?.Invoke();
            }
        }

        private void HandleClick(Vector2 screenPos)
        {
            var eventSystem = EventSystem.current;
            if (eventSystem != null && eventSystem.IsPointerOverGameObject())
                return;

            if (IsPointerOverUIToolkit(screenPos))
                return;

            if (_camera == null) return;

            Vector2 worldPos = _camera.ScreenToWorldPoint(screenPos);
            SelectOrDeselectAtWorldPos(worldPos);
        }

        private static bool IsPointerOverUIToolkit(Vector2 screenPos)
        {
            var uiDoc = Object.FindFirstObjectByType<UIDocument>(FindObjectsInactive.Exclude);
            if (uiDoc == null || uiDoc.rootVisualElement == null) return false;

            var panel = uiDoc.rootVisualElement.panel;
            if (panel == null) return false;

            Vector2 panelPos = RuntimePanelUtils.ScreenToPanel(panel, new Vector2(screenPos.x, Screen.height - screenPos.y));
            VisualElement hit = panel.Pick(panelPos);
            return hit != null && hit.pickingMode != PickingMode.Ignore;
        }

        private void SelectOrDeselectAtWorldPos(Vector2 worldPos)
        {
            Collider2D hit = Physics2D.OverlapPoint(worldPos, _cachedSelectionLayerMask);

            if (hit != null)
            {
                GameObject characterRoot = hit.transform.parent != null
                    ? hit.transform.parent.gameObject
                    : hit.gameObject;
                Select(characterRoot);
            }
            else
            {
                Deselect();
            }
        }

        private void Select(GameObject characterRoot)
        {
            if (_selectedGo == characterRoot) return;

            if (_selectedOutline != null && _selectedGo != null)
                _selectedOutline.ClearOutline();

            var outline = characterRoot.GetComponent<SelectionOutline>();

            if (outline == null) return;

            bool isAlly = characterRoot.layer == _cachedAllyLayer;
            Color color = isAlly ? AllyOutlineColor : EnemyOutlineColor;

            outline.SetOutline(true, color, OutlineWidth);

            _selectedOutline = outline;
            _selectedGo = characterRoot;
            _hasSelection = true;

            OnUnitSelected?.Invoke(characterRoot);
        }

        private void Deselect()
        {
            if (!_hasSelection) return;

            if (_selectedOutline != null)
                _selectedOutline.ClearOutline();

            _selectedOutline = null;
            _selectedGo = null;
            _hasSelection = false;

            OnUnitDeselected?.Invoke();
        }

        public void ForceDeselect() => Deselect();

        public void ForceSelect(GameObject unit)
        {
            if (unit == null) return;

            Select(unit);
        }

        internal void SimulateClickAtWorldPos(Vector2 worldPos)
        {
            SelectOrDeselectAtWorldPos(worldPos);
        }
    }
}
