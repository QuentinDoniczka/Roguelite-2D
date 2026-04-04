using UnityEngine;

namespace RogueliteAutoBattler.Common
{
    public static class PhysicsLayers
    {
        public const string Ally = "Ally";
        public const string Enemy = "Enemy";
        public const string Selection = "Selection";

        public static int AllyLayer => LayerMask.NameToLayer(Ally);
        public static int EnemyLayer => LayerMask.NameToLayer(Enemy);
        public static int SelectionLayer => LayerMask.NameToLayer(Selection);
        public static int SelectionLayerMask => 1 << SelectionLayer;
    }
}
