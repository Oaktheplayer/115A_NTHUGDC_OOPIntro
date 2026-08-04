using System;
using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{
    /// <summary>
    /// Tracks living enemies placed in the scene. Win when count reaches zero.
    /// </summary>
    public class EnemyRegistry : MonoBehaviour
    {
        private static readonly HashSet<EnemyController> Living = new();
        private static EnemyRegistry instance;

        public static int RemainingCount => Living.Count;
        public static event Action OnAllEnemiesDefeated;

        [SerializeField] private bool logWin = true;

        private void Awake()
        {
            instance = this;
        }

        private static void EnsureInstance()
        {
            if (instance != null)
                return;

            var go = new GameObject(nameof(EnemyRegistry));
            instance = go.AddComponent<EnemyRegistry>();
        }

        public static void Register(EnemyController enemy)
        {
            if (enemy == null)
                return;

            EnsureInstance();
            Living.Add(enemy);
        }

        public static void Unregister(EnemyController enemy)
        {
            if (enemy == null)
                return;

            if (!Living.Remove(enemy))
                return;

            if (Living.Count == 0)
            {
                OnAllEnemiesDefeated?.Invoke();
                if (instance == null || instance.logWin)
                    Debug.Log("[EnemyRegistry] All enemies defeated. Win!");
            }
        }
    }
}
