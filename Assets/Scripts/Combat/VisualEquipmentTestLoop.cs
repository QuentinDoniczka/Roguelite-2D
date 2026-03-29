using System.Collections;
using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    public class VisualEquipmentTestLoop : MonoBehaviour
    {
#if UNITY_EDITOR
        [Header("Sprite Pools")]
        [SerializeField] private Sprite[] _headSprites;
        [SerializeField] private Sprite[] _weaponSprites;
        [SerializeField] private Sprite[] _hatSprites;
        [SerializeField] private Sprite[] _shieldSprites;

        [Header("Timing")]
        [SerializeField] private float _cycleInterval = 5f;

        [Header("Debug")]
        [SerializeField] private bool _logCycles;

        public Sprite[] HeadSprites { get => _headSprites; set => _headSprites = value; }
        public Sprite[] WeaponSprites { get => _weaponSprites; set => _weaponSprites = value; }
        public Sprite[] HatSprites { get => _hatSprites; set => _hatSprites = value; }
        public Sprite[] ShieldSprites { get => _shieldSprites; set => _shieldSprites = value; }
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
#endif
    }
}
