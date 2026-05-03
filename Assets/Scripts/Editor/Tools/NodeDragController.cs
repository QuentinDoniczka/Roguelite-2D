using UnityEngine;

namespace RogueliteAutoBattler.Editor.Tools
{
    internal static class NodeDragController
    {
        public readonly struct DragState
        {
            public readonly int NodeIndex;
            public readonly Vector2 NodeStartPositionUnits;
            public readonly Vector2 MouseStartCanvasPixelsYDownward;
            public readonly int MirrorPartnerIndex;
            public readonly Vector2 MirrorPartnerStartPositionUnits;

            public DragState(int nodeIndex, Vector2 nodeStartUnits,
                             Vector2 mouseStartCanvasPixelsYDownward,
                             int mirrorPartnerIndex,
                             Vector2 mirrorPartnerStartUnits)
            {
                NodeIndex = nodeIndex;
                NodeStartPositionUnits = nodeStartUnits;
                MouseStartCanvasPixelsYDownward = mouseStartCanvasPixelsYDownward;
                MirrorPartnerIndex = mirrorPartnerIndex;
                MirrorPartnerStartPositionUnits = mirrorPartnerStartUnits;
            }

            public bool IsActive => NodeIndex >= 0;
            public static DragState Inactive { get; } = new DragState(-1, Vector2.zero, Vector2.zero, -1, Vector2.zero);
        }

        public static Vector2 ComputeNewNodePosition(
            DragState state,
            Vector2 currentMouseCanvasPixelsYDownward,
            float unitSize,
            float zoom)
        {
            float scale = unitSize * zoom;
            if (scale <= 0f) return state.NodeStartPositionUnits;
            Vector2 deltaPx = currentMouseCanvasPixelsYDownward - state.MouseStartCanvasPixelsYDownward;
            return state.NodeStartPositionUnits + deltaPx / scale;
        }
    }
}
