using UnityEngine;

namespace Combat
{
    public class Damager : MonoBehaviour
    {
        [SerializeField] private int damage = 1;



        public void DealDamage(Health target)
        {
            if (target == null)
            {
                Debug.LogWarning($"[{name}] No currentTarget set for Damager component.");
                return;
            }

            target.TakeDamage(damage);
        }
    }
}
