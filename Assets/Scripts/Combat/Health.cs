using System;
using UnityEngine;
using UnityEngine.Events;

namespace Combat
{
    public class Health : MonoBehaviour
    {
        public enum State : byte
        {
            Dead = 0,
            Alive = 1
        }

        [Header("Settings")]
        [SerializeField] private int maxValue = 5;
        [SerializeField] private bool destroyOnDeath = true;

        [Header("Runtime")]
        [SerializeField] private int value;

        public int Value => value;
        public int MaxValue => maxValue;
        public State LifeState { get; private set; } = State.Alive;
        public bool IsAlive => LifeState == State.Alive;

        public UnityEvent OnDamaged;
        public UnityEvent OnKilled;
        public event Action OnKilledEvent;
        public event Action<int> OnDamageTaken;

        private void Awake()
        {
            value = maxValue;
            LifeState = State.Alive;
        }

        public void ApplyMaxHealth(int max)
        {
            maxValue = Mathf.Max(1, max);
            value = maxValue;
            LifeState = State.Alive;
            enabled = true;
        }

        public void TakeDamage(int amount)
        {
            if (LifeState == State.Dead)
                return;

            if (amount <= 0)
                return;

            int prev = value;
            value = Mathf.Max(0, value - amount);
            int delta = prev - value;

            OnDamageTaken?.Invoke(delta);
            OnDamaged?.Invoke();

            if (value <= 0)
                Kill();
        }

        public void TakeHeal(int amount)
        {
            if (LifeState == State.Dead)
                return;

            value = Mathf.Min(maxValue, value + Mathf.Max(0, amount));
        }

        [ContextMenu("Test Kill")]
        public void Kill()
        {
            if (LifeState == State.Dead)
                return;

            LifeState = State.Dead;
            value = 0;
            OnKilled?.Invoke();
            OnKilledEvent?.Invoke();

            if (destroyOnDeath)
                Destroy(gameObject);
            else
                enabled = false;
        }
    }
}
