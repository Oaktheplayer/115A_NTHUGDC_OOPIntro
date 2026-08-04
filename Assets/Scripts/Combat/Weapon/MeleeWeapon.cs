using System.Collections;
using UnityEngine;

namespace Combat
{
    public class MeleeWeapon : Weapon
    {
        [SerializeField] private HurtBox hurtBox;
        [SerializeField] private MeleeSwingVisual swingVisual;
        [SerializeField] private float hurtBoxDistance = 0.6f;

        private Coroutine activeSwing;

        protected override void Awake()
        {
            base.Awake();

            if (hurtBox == null && hurtBoxObject != null)
                hurtBox = hurtBoxObject.GetComponent<HurtBox>();

            if (swingVisual == null)
                swingVisual = GetComponentInChildren<MeleeSwingVisual>(true);
        }

        protected override void PerformAttack(Vector2 direction, Transform target)
        {
            PositionHurtBox(direction);
            swingVisual?.PlaySwing(direction);

            if (activeSwing != null)
                StopCoroutine(activeSwing);
            activeSwing = StartCoroutine(SwingHurtBoxRoutine());
        }

        private void PositionHurtBox(Vector2 direction)
        {
            if (hurtBoxObject == null)
                return;

            // WeaponPivot already faces the aim direction; place the HurtBox along local +X.
            hurtBoxObject.transform.localPosition = new Vector3(hurtBoxDistance, 0f, 0f);
            hurtBoxObject.transform.localRotation = Quaternion.identity;
        }

        private IEnumerator SwingHurtBoxRoutine()
        {
            float duration = data != null ? data.hurtboxActiveDuration : 0.15f;
            int damage = data != null ? data.damage : 1;

            if (hurtBoxObject != null)
                hurtBoxObject.SetActive(true);

            if (hurtBox != null)
            {
                hurtBox.ApplyDamageValue(damage);
                hurtBox.BeginSwing();
            }

            yield return new WaitForSeconds(duration);

            if (hurtBoxObject != null)
                hurtBoxObject.SetActive(false);

            activeSwing = null;
        }
    }
}
