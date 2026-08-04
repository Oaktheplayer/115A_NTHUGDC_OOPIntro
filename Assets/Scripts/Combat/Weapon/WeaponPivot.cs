using UnityEngine;

namespace Combat
{
    /// <summary>
    /// Rotational pivot for an equipped weapon. Rotate this transform toward the aim/target;
    /// the weapon child keeps a fixed local hold offset.
    /// When aiming left (pivot z angle in 90..270), flips the weapon Sprite's localScale.y.
    /// </summary>
    public class WeaponPivot : MonoBehaviour
    {
        [Tooltip("Weapon local offset under the pivot. X toward aim, Y sideways.")]
        [SerializeField] private Vector2 holdOffset = new Vector2(0.45f, -0.1f);

        private Transform weaponSprite;
        private float spriteBaseScaleY = 1f;

        public Vector2 HoldOffset => holdOffset;
        public Weapon CurrentWeapon { get; private set; }

        public void FaceDirection(Vector2 direction)
        {
            if (direction.sqrMagnitude < 0.0001f)
                return;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.localRotation = Quaternion.Euler(0f, 0f, angle);

            float angle360 = angle < 0f ? angle + 360f : angle;
            bool flipY = angle360 > 90f && angle360 < 270f;
            ApplySpriteFlipY(flipY);
        }

        public void Equip(Weapon weapon, string holderTag)
        {
            if (weapon == null)
                return;

            CurrentWeapon = weapon;
            weapon.EquipTo(transform, holderTag);
            weapon.transform.localPosition = holdOffset;
            weapon.transform.localRotation = Quaternion.identity;

            CacheWeaponSprite();
            ApplySpriteFlipY(false);
        }

        public Weapon UnequipToGround(Vector3 dropPosition)
        {
            if (CurrentWeapon == null)
                return null;

            ApplySpriteFlipY(false);

            Weapon dropped = CurrentWeapon;
            dropped.DropToGround(dropPosition);
            CurrentWeapon = null;
            weaponSprite = null;
            return dropped;
        }

        private void CacheWeaponSprite()
        {
            weaponSprite = null;
            if (CurrentWeapon == null)
                return;

            Transform found = CurrentWeapon.transform.Find("Sprite");
            if (found == null && CurrentWeapon.transform.childCount > 0)
                found = CurrentWeapon.transform.GetChild(0);

            if (found == null)
                return;

            weaponSprite = found;
            spriteBaseScaleY = Mathf.Abs(weaponSprite.localScale.y);
            if (spriteBaseScaleY < 0.0001f)
                spriteBaseScaleY = 1f;
        }

        private void ApplySpriteFlipY(bool flipY)
        {
            if (weaponSprite == null)
                CacheWeaponSprite();
            if (weaponSprite == null)
                return;

            Vector3 scale = weaponSprite.localScale;
            scale.y = flipY ? -spriteBaseScaleY : spriteBaseScaleY;
            weaponSprite.localScale = scale;
        }
    }
}
