using UnityEngine;

namespace Combat
{
    [CreateAssetMenu(fileName = "ProjectileData", menuName = "Combat/Projectile Data")]
    public class ProjectileData : ScriptableObject
    {
        public GameObject prefab;
        public float speed = 12f;
        [Min(0.05f)] public float lifetime = 3f;
    }
}
