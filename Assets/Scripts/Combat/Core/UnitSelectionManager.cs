using RogueliteAutoBattler.Combat.Visuals;
using RogueliteAutoBattler.Common;
using RogueliteAutoBattler.Core;
using UnityEngine;
using UnityEngine.InputSystem;

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
        private Camera _camera;

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
            if (_selectedGo == null && _selectedOutline != null)
            {
                _selectedOutline = null;
                OnUnitDeselected?.Invoke();
            }
        }

        private void HandleClick(Vector2 screenPos)
        {
            if (UnityEngine.EventSystems.EventSystem.current != null
                && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                return;

            if (_camera == null) return;

            Vector2 worldPos = _camera.ScreenToWorldPoint(screenPos);
            Collider2D hit = Physics2D.OverlapPoint(worldPos, PhysicsLayers.SelectionLayerMask);

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
            var stats = characterRoot.GetComponent<CombatStats>();

            if (outline == null) return;

            bool isAlly = characterRoot.layer == PhysicsLayers.AllyLayer;
            Color color = isAlly ? AllyOutlineColor : EnemyOutlineColor;

            outline.SetOutline(true, color, OutlineWidth);

            _selectedOutline = outline;
            _selectedGo = characterRoot;

            OnUnitSelected?.Invoke(characterRoot);

            if (stats != null)
            {
                string team = isAlly ? "Ally" : "Enemy";
                Debug.Log($"[UnitSelection] Selected {characterRoot.name} | Team: {team} | HP: {stats.CurrentHp}/{stats.MaxHp} | ATK: {stats.Atk}");
            }
        }

        private void Deselect()
        {
            if (_selectedGo == null) return;

            if (_selectedOutline != null)
                _selectedOutline.ClearOutline();

            _selectedOutline = null;
            _selectedGo = null;

            OnUnitDeselected?.Invoke();
        }

        public void ForceDeselect() => Deselect();

        internal void SimulateClickAtWorldPos(Vector2 worldPos)
        {
            Collider2D hit = Physics2D.OverlapPoint(worldPos, PhysicsLayers.SelectionLayerMask);

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
    }
}
