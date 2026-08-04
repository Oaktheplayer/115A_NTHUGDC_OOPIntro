using UnityEngine;

namespace Combat
{
    public abstract class Weapon : MonoBehaviour
    {
        public abstract void Attack(Transform target);
    }
}
