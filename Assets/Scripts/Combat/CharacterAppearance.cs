using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    /// <summary>
    /// Resolves equipment sprite slots on an animated character prefab and allows
    /// swapping sprites at runtime. Added via <c>AddComponent</c> at spawn time
    /// on the character root (same level as Rigidbody2D).
    /// </summary>
    /// <remarks>
    /// Equipment slots: head, hat, weapon, shield. Some animation clips (e.g.
    /// ChopAttack) contain PPtrCurves that override weapon/shield sprites.
    /// <see cref="LateUpdate"/> re-applies any explicitly set sprites every frame
    /// so they always win over Animator overrides.
    /// Body and hand sprites are controlled by Animator curves — do NOT swap them.
    /// Child names must match the prefab hierarchy exactly because animation clips
    /// reference children by path name.
    /// </remarks>
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

        /// <summary>SpriteRenderer for the head slot (may be null if not found).</summary>
        public SpriteRenderer HeadRenderer => _headRenderer;

        /// <summary>SpriteRenderer for the hat/armor slot (may be null if not found).</summary>
        public SpriteRenderer HatRenderer => _hatRenderer;

        /// <summary>SpriteRenderer for the weapon slot (may be null if not found).</summary>
        public SpriteRenderer WeaponRenderer => _weaponRenderer;

        /// <summary>SpriteRenderer for the shield slot (may be null if not found).</summary>
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

        /// <summary>
        /// Applies sprites to the equipment slots. Pass null for any slot to keep
        /// the prefab default sprite unchanged.
        /// </summary>
        /// <param name="head">Sprite for the head slot, or null to keep default.</param>
        /// <param name="hat">Sprite for the hat/armor slot, or null to keep default.</param>
        /// <param name="weapon">Sprite for the weapon slot, or null to keep default.</param>
        /// <param name="shield">Sprite for the shield slot, or null to keep default.</param>
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

        /// <summary>
        /// Convenience overload that unpacks an <see cref="AppearanceData"/> and applies its sprites.
        /// </summary>
        public void ApplyAppearance(AppearanceData appearance)
        {
            if (appearance == null) return;
            ApplyAppearance(appearance.HeadSprite, appearance.HatSprite,
                appearance.WeaponSprite, appearance.ShieldSprite);
        }

        /// <summary>
        /// Re-applies tracked sprites after the Animator has written its PPtrCurve
        /// changes. This ensures custom equipment sprites always win over animation
        /// defaults.
        /// </summary>
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
