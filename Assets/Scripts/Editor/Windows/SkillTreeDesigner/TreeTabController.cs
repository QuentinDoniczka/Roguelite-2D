using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Editor.Windows.SkillTreeDesigner
{
    internal sealed class TreeTabController
    {
        internal TreeTabController(VisualElement tabContent, SerializedObject serialized)
        {
            var inspector = new InspectorElement(serialized);
            tabContent.Add(inspector);
        }
    }
}
