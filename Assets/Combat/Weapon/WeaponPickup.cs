using UnityEngine;

namespace Combat
{
    /// <summary>
    /// Marks a weapon that is on the ground and can be swapped with E.
    /// Enable this component for OnGround; disable when Equipped.
    /// </summary>
    [RequireComponent(typeof(Weapon))]
    public class WeaponPickup : MonoBehaviour
    {
        public Weapon Weapon { get; private set; }

        private void Awake()
        {
            Weapon = GetComponent<Weapon>();
        }
    }
}
