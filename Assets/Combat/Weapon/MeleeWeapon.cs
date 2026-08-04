using UnityEngine;
using Combat;

public class MeleeWeapon : Weapon
{
    [SerializeField] float knockBackForce = 5f;
    [SerializeField] int damage;
    [SerializeField] Rigidbody2D rbSelf;

    public override void Attack(Transform target)
    {
        if (target == null)
        {
            Debug.LogError($"[{name}:Attack] Target is null. Cannot attack.");
            return;
        }



        if (target.TryGetComponent<Health>(out var health))
        {
            health.TakeDamage(damage);
        }

        // TODO: knock back callback to play animation and shit
        if (target.TryGetComponent<Rigidbody2D>(out var rb))
        {
            Vector2 direction = (target.transform.position - transform.position).normalized;

            rb.AddForce(
                direction * knockBackForce,
                ForceMode2D.Impulse
                );

            if (rbSelf != null)
            {
                rbSelf.AddForce(
                    -direction * knockBackForce,
                    ForceMode2D.Impulse
                    );
            }
        }
    }

}
