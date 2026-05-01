using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Editor.Windows.SkillTreeDesigner
{
    internal sealed class NodeClickedEvent : EventBase<NodeClickedEvent>
    {
        public int NodeId { get; private set; }

        public static NodeClickedEvent GetPooled(int nodeId)
        {
            var e = GetPooled();
            e.NodeId = nodeId;
            e.bubbles = true;
            e.tricklesDown = false;
            return e;
        }

        protected override void Init()
        {
            base.Init();
            NodeId = -1;
            bubbles = true;
            tricklesDown = false;
        }
    }
}
