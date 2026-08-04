using System.Collections;
using UnityEngine;

namespace Combat
{
    /// <summary>
    /// Optional / legacy auto-attacker using RangeDetect. Not used by the main course path.
    /// </summary>
    public class RepeatAttacker : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RangeDetect rangeDetect;
        [SerializeField] private Weapon weapon;

        [Header("Settings")]
        [SerializeField] private bool waitForTarget = true;
        [Tooltip("The interval between attacks in seconds.")]
        [SerializeField] private float attackDelay = 0f;
        [SerializeField] private float attackInterval = 1f;

        private void Awake()
        {
            if (weapon == null)
                weapon = GetComponent<Weapon>();
        }

        private void Attack()
        {
            var target = rangeDetect.GetTarget();
            if (target == null)
                return;

            Vector2 direction = ((Vector2)target.position - (Vector2)transform.position);
            if (direction.sqrMagnitude < 0.0001f)
                direction = Vector2.up;

            weapon.Attack(direction.normalized, target);
        }

        private void Start()
        {
            StartCoroutine(AttackCoroutine());
        }

        private IEnumerator AttackCoroutine()
        {
            while (true)
            {
                if (waitForTarget)
                    yield return rangeDetect.WaitForTarget();

                if (attackDelay != 0f)
                    yield return new WaitForSeconds(attackDelay);

                Attack();

                yield return new WaitForSeconds(attackInterval);
            }
        }
    }
}
