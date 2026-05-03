using UnityEngine;

namespace RogueliteAutoBattler.Editor.Tools
{
    internal static class NodeDragController
    {
        public readonly struct DragState
        {
            public readonly int NodeIndex;
            public readonly Vector2 NodeStartPositionUnits;
            public readonly Vector2 MouseStartCanvasPixels;
            public readonly int MirrorPartnerIndex;
            public readonly Vector2 MirrorPartnerStartPositionUnits;

            public DragState(int nodeIndex, Vector2 nodeStartUnits,
                             Vector2 mouseStartPx,
                             int mirrorPartnerIndex,
                             Vector2 mirrorPartnerStartUnits)
            {
                NodeIndex = nodeIndex;
                NodeStartPositionUnits = nodeStartUnits;
                MouseStartCanvasPixels = mouseStartPx;
                MirrorPartnerIndex = mirrorPartnerIndex;
                MirrorPartnerStartPositionUnits = mirrorPartnerStartUnits;
            }

            public bool IsActive => NodeIndex >= 0;
            public static DragState Inactive { get; } = new DragState(-1, Vector2.zero, Vector2.zero, -1, Vector2.zero);
        }

        public static Vector2 ComputeNewNodePosition(
            DragState state,
            Vector2 currentMouseCanvasPixels,
            float unitSize,
            float zoom)
        {
            float scale = unitSize * zoom;
            if (scale <= 0f) return state.NodeStartPositionUnits;
            Vector2 deltaPx = currentMouseCanvasPixels - state.MouseStartCanvasPixels;
            // Canvas Y grows downward in IMGUI; node positions use the same downward-Y convention
            // (screenPos = origin + position * scaledUnit), so pixel delta maps directly to unit delta.
            return state.NodeStartPositionUnits + deltaPx / scale;
        }
    }
}
