using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    public class CharacterAppearance : MonoBehaviour
    {
        private const string HeadPath = "human_head_skin_1_0";
        private const string HatPath = "human_head_skin_1_0/metal_15";
        private const string WeaponPath = "human_hand_0/polearm_8";
        private const string ShieldPath = "shield_42";

        private SpriteRenderer _headRenderer;
        private SpriteRenderer _hatRenderer;
        private SpriteRenderer _weaponRenderer;
        private SpriteRenderer _shieldRenderer;

        private Sprite _appliedHead;
        private Sprite _appliedHat;
        private Sprite _appliedWeapon;
        private Sprite _appliedShield;

        public SpriteRenderer HeadRenderer => _headRenderer;
        public SpriteRenderer HatRenderer => _hatRenderer;
        public SpriteRenderer WeaponRenderer => _weaponRenderer;
        public SpriteRenderer ShieldRenderer => _shieldRenderer;

        private void Awake()
        {
            Animator animator = GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogWarning($"[CharacterAppearance] No Animator found in children of {name}. Cannot resolve sprite slots.");
                return;
            }

            Transform visual = animator.transform;

            _headRenderer = FindSlotRenderer(visual, HeadPath, "Head");
            _hatRenderer = FindSlotRenderer(visual, HatPath, "Hat");
            _weaponRenderer = FindSlotRenderer(visual, WeaponPath, "Weapon");
            _shieldRenderer = FindSlotRenderer(visual, ShieldPath, "Shield");
        }

        public void ApplyAppearance(Sprite head, Sprite hat, Sprite weapon, Sprite shield)
        {
            if (head != null && _headRenderer != null)
            {
                _appliedHead = head;
                _headRenderer.sprite = head;
            }

            if (hat != null && _hatRenderer != null)
            {
                _appliedHat = hat;
                _hatRenderer.sprite = hat;
            }

            if (weapon != null && _weaponRenderer != null)
            {
                _appliedWeapon = weapon;
                _weaponRenderer.sprite = weapon;
            }

            if (shield != null && _shieldRenderer != null)
            {
                _appliedShield = shield;
                _shieldRenderer.sprite = shield;
            }
        }

        public void ApplyAppearance(AppearanceData appearance)
        {
            if (appearance == null) return;
            ApplyAppearance(appearance.HeadSprite, appearance.HatSprite,
                appearance.WeaponSprite, appearance.ShieldSprite);
        }

        private void LateUpdate()
        {
            if (_appliedHead != null && _headRenderer != null)
                _headRenderer.sprite = _appliedHead;

            if (_appliedHat != null && _hatRenderer != null)
                _hatRenderer.sprite = _appliedHat;

            if (_appliedWeapon != null && _weaponRenderer != null)
                _weaponRenderer.sprite = _appliedWeapon;

            if (_appliedShield != null && _shieldRenderer != null)
                _shieldRenderer.sprite = _appliedShield;
        }

        private static SpriteRenderer FindSlotRenderer(Transform visual, string path, string slotName)
        {
            Transform child = visual.Find(path);
            if (child == null)
            {
                Debug.LogWarning($"[CharacterAppearance] Slot '{slotName}' not found at path '{path}'.");
                return null;
            }

            if (child.TryGetComponent(out SpriteRenderer renderer))
                return renderer;

            Debug.LogWarning($"[CharacterAppearance] Slot '{slotName}' found at '{path}' but has no SpriteRenderer.");
            return null;
        }
    }
}
