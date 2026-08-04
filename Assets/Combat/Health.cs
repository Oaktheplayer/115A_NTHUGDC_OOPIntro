using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.Controls;

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

        [Header("Runtime")]
        [SerializeField] private int value = 0;

        [SerializeField] private int damageMultiplier = 1;

        #region Public API
        public float Value => value;
        public float MaxValue => maxValue;
        public State LifeState { get; private set; }
        #endregion



        #region Events
        [Header("Events")]
        public UnityEvent OnKilled;
        public UnityEvent OnDamaged;
        public event Action OnKilledEvent;
        public event Action<int> onDamageTaken;
        #endregion



        #region Initialization
        private void Awake()
        {
            value = maxValue;
            LifeState = State.Alive;
        }
        #endregion



        #region Health Manipulation
        public void TakeDamage(int amount)
        {
            if (value <= 0)
            {
                Debug.LogWarning($"[{this.name}] Tried to take damage when health is already 0.");
                return;
            }

            amount *= damageMultiplier;

            int prevVal = value;
            value -= amount;
            int delta = prevVal - Mathf.Max(value, 0);
            onDamageTaken?.Invoke(delta);
            OnDamaged?.Invoke();
            Debug.Log($"[{this.name}] Took {delta} damage. Health is now {value}/{maxValue}.");
            if (value <= 0)
            {
                value = 0;
                Kill();
            }
        }

        public void TakeHeal(int amount)
        {
            value += amount;
            if (value > maxValue)
            {
                value = maxValue;
            }
        }
        #endregion



        #region Private Methods
        [ContextMenu("Test Kill")]
        private void Kill()
        {
            if (LifeState == State.Dead)
                return;

            LifeState = State.Dead;
        }
        #endregion

        private void Killed()
        {
            OnKilled?.Invoke();
            OnKilledEvent?.Invoke();
        }

        private void LateUpdate()
        {
            if(LifeState == State.Dead)
            {
                Killed();
                this.enabled = false;
            }
        }
    }
}