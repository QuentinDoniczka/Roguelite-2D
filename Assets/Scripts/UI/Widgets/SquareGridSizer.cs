using UnityEngine;
using UnityEngine.UI;

namespace RogueliteAutoBattler.UI.Widgets
{
    [RequireComponent(typeof(GridLayoutGroup))]
    [ExecuteAlways]
    public class SquareGridSizer : MonoBehaviour
    {
        [SerializeField] private int _columnCount = 2;

        private GridLayoutGroup _grid;
        private RectTransform _rect;

        private void OnEnable()
        {
            _grid = GetComponent<GridLayoutGroup>();
            _rect = GetComponent<RectTransform>();
            UpdateCellSize();
        }

        private void OnRectTransformDimensionsChange()
        {
            UpdateCellSize();
        }

        private void UpdateCellSize()
        {
            if (_grid == null || _rect == null) return;
            float availableWidth = _rect.rect.width
                                   - _grid.padding.left - _grid.padding.right;
            float cellWidth = (availableWidth - _grid.spacing.x * (_columnCount - 1))
                              / _columnCount;
            if (cellWidth <= 0) return;
            _grid.cellSize = new Vector2(cellWidth, cellWidth);
        }

        internal int ColumnCount => _columnCount;
    }
}
