using UnityEngine;

namespace RogueliteAutoBattler.Common
{
    public static class PhysicsLayers
    {
        public const string Ally = "Ally";
        public const string Enemy = "Enemy";

        public static int AllyLayer => LayerMask.NameToLayer(Ally);
        public static int EnemyLayer => LayerMask.NameToLayer(Enemy);
    }
}
