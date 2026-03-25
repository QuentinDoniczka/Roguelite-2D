using System.Collections;
using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    /// <summary>
    /// Debug component that randomly cycles head, weapon, hat, and shield sprites on all
    /// <see cref="CharacterAppearance"/> instances every <see cref="_cycleInterval"/> seconds.
    /// Sprite arrays must be populated at editor time (e.g. by CombatWorldBuilder)
    /// because AssetDatabase is unavailable at runtime.
    /// </summary>
    public class VisualEquipmentTestLoop : MonoBehaviour
    {
        [Header("Sprite Pools")]
        [Tooltip("Available head sprites to cycle through.")]
        [SerializeField] private Sprite[] _headSprites;

        [Tooltip("Available weapon sprites to cycle through.")]
        [SerializeField] private Sprite[] _weaponSprites;

        [Tooltip("Available hat/armor sprites to cycle through.")]
        [SerializeField] private Sprite[] _hatSprites;

        [Tooltip("Available shield sprites to cycle through.")]
        [SerializeField] private Sprite[] _shieldSprites;

        [Header("Timing")]
        [Tooltip("Seconds between each random equipment swap.")]
        [SerializeField] private float _cycleInterval = 5f;

        [Header("Debug")]
        [Tooltip("Log each cycle with the number of characters affected.")]
        [SerializeField] private bool _logCycles;

        /// <summary>Head sprite pool (for editor wiring).</summary>
        public Sprite[] HeadSprites { get => _headSprites; set => _headSprites = value; }

        /// <summary>Weapon sprite pool (for editor wiring).</summary>
        public Sprite[] WeaponSprites { get => _weaponSprites; set => _weaponSprites = value; }

        /// <summary>Hat sprite pool (for editor wiring).</summary>
        public Sprite[] HatSprites { get => _hatSprites; set => _hatSprites = value; }

        /// <summary>Shield sprite pool (for editor wiring).</summary>
        public Sprite[] ShieldSprites { get => _shieldSprites; set => _shieldSprites = value; }

        /// <summary>Seconds between each equipment swap cycle (for testing).</summary>
        public float CycleInterval { get => _cycleInterval; set => _cycleInterval = value; }

        private void Start()
        {
            StartCoroutine(CycleEquipmentLoop());
        }

        private IEnumerator CycleEquipmentLoop()
        {
            var wait = new WaitForSeconds(_cycleInterval);

            while (true)
            {
                yield return wait;

                var characters = FindObjectsByType<CharacterAppearance>(FindObjectsSortMode.None);
                int count = characters.Length;

                for (int i = 0; i < count; i++)
                {
                    Sprite head = PickRandom(_headSprites);
                    Sprite hat = PickRandom(_hatSprites);
                    Sprite weapon = PickRandom(_weaponSprites);
                    Sprite shield = PickRandom(_shieldSprites);

                    characters[i].ApplyAppearance(head, hat, weapon, shield);
                }

                if (_logCycles)
                    Debug.Log($"[VisualEquipmentTestLoop] Cycled equipment on {count} character(s).");
            }
        }

        private static Sprite PickRandom(Sprite[] sprites)
        {
            if (sprites == null || sprites.Length == 0)
                return null;

            return sprites[Random.Range(0, sprites.Length)];
        }
    }
}
