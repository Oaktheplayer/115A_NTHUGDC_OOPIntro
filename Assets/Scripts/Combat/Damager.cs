using UnityEngine;

namespace Combat
{
    public class Damager : MonoBehaviour
    {
        [SerializeField] private int damage = 1;

        public int Damage => damage;

        public void SetDamage(int value)
        {
            damage = Mathf.Max(0, value);
        }

        public void DealDamage(Health target)
        {
            if (target == null)
            {
                Debug.LogWarning($"[{name}] Damager received a null Health target.");
                return;
            }

            target.TakeDamage(damage);
        }
    }
}
