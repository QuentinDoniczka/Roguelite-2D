using UnityEngine;

namespace RogueliteAutoBattler.Common
{
    public static class PhysicsLayers
    {
        public const string Ally = "Ally";
        public const string Enemy = "Enemy";
        public const string Selection = "Selection";

        private static int _allyLayer = -2;
        private static int _enemyLayer = -2;
        private static int _selectionLayer = -2;

        public static int AllyLayer => ResolveCached(ref _allyLayer, Ally);
        public static int EnemyLayer => ResolveCached(ref _enemyLayer, Enemy);
        public static int SelectionLayer => ResolveCached(ref _selectionLayer, Selection);
        public static int SelectionLayerMask => 1 << SelectionLayer;

        private static int ResolveCached(ref int cached, string layerName)
        {
            if (cached == -2)
                cached = LayerMask.NameToLayer(layerName);
            return cached;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnDomainReload()
        {
            _allyLayer = -2;
            _enemyLayer = -2;
            _selectionLayer = -2;
        }
    }
}
