using UnityEngine;

namespace Combat
{
    [RequireComponent(typeof(WeaponOwner))]
    public abstract class Weapon : MonoBehaviour
    {
        [SerializeField] protected WeaponData data;
        [SerializeField] protected WeaponOwner owner;
        [SerializeField] protected GameObject hurtBoxObject;

        private float lastAttackTime = -999f;
        private WeaponPickup pickup;

        public WeaponData Data => data;
        public WeaponOwner Owner => owner;
        public bool IsEquipped { get; private set; }

        public bool IsReady
        {
            get
            {
                if (!IsEquipped || data == null)
                    return false;
                return Time.time >= lastAttackTime + data.attackCooldown;
            }
        }

        protected virtual void Awake()
        {
            if (owner == null)
                owner = GetComponent<WeaponOwner>();
            pickup = GetComponent<WeaponPickup>();

            if (hurtBoxObject == null)
            {
                var hurtBox = GetComponentInChildren<HurtBox>(true);
                if (hurtBox != null)
                    hurtBoxObject = hurtBox.gameObject;
            }

            // Default: scene-placed weapons on the ground have no parent.
            // If parented under a character / WeaponPivot, equip using the nearest Player/Enemy tag.
            if (transform.parent == null)
            {
                DropToGround(transform.position, keepWorldPosition: true);
                return;
            }

            string holderTag = ResolveHolderTag();
            EquipTo(transform.parent, holderTag);
        }

        private static string ResolveHolderTag(Transform from)
        {
            for (Transform t = from; t != null; t = t.parent)
            {
                if (t.CompareTag(WeaponOwner.PlayerTag))
                    return WeaponOwner.PlayerTag;
                if (t.CompareTag(WeaponOwner.EnemyTag))
                    return WeaponOwner.EnemyTag;
            }

            return WeaponOwner.EnemyTag;
        }

        private string ResolveHolderTag()
        {
            return ResolveHolderTag(transform);
        }

        public void Attack(Vector2 direction, Transform target = null)
        {
            if (!IsEquipped || data == null)
                return;

            if (Time.time < lastAttackTime + data.attackCooldown)
                return;

            if (direction.sqrMagnitude < 0.0001f)
                return;

            lastAttackTime = Time.time;
            PerformAttack(direction.normalized, target);
        }

        protected abstract void PerformAttack(Vector2 direction, Transform target);

        public void EquipTo(Transform holder, string holderTag)
        {
            transform.SetParent(holder, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            IsEquipped = true;
            owner.ConfigureForHolder(holderTag);

            if (pickup != null)
                pickup.enabled = false;

            if (hurtBoxObject != null)
                hurtBoxObject.SetActive(false);
        }

        public void DropToGround(Vector3 worldPosition, bool keepWorldPosition = false)
        {
            transform.SetParent(null, true);
            if (!keepWorldPosition)
                transform.position = worldPosition;

            IsEquipped = false;

            if (pickup != null)
                pickup.enabled = true;

            if (hurtBoxObject != null)
                hurtBoxObject.SetActive(false);
        }

        protected void EnsureHurtBoxInactive()
        {
            if (hurtBoxObject != null)
                hurtBoxObject.SetActive(false);
        }
    }
}
