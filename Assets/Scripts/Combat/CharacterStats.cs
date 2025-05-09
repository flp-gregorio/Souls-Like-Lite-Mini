using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using Animation;

namespace Combat
{
    public class CharacterStats : MonoBehaviour
    {
        // Common animator reference
        protected Animator animator;

        [Header("Health")]
        // Health-related properties and events
        public UnityEvent<float, Vector2> damageableHit;
        public UnityEvent<float, float> healthChanged;

        [SerializeField]
        private HitStopManager hitStopManager;
        [SerializeField]
        private float maxHealth = 100;
        [SerializeField]
        private float health = 100;
        [SerializeField]
        private bool isAlive = true;
        [SerializeField]
        private bool isInvincible = false;
        [SerializeField]
        public float invincibilityTime = 0.25f;
        private float timeSinceHit;

        [Header("Stamina")]
        // Stamina-related properties and events
        public UnityEvent<float, float> staminaChanged;
        [SerializeField]
        private float maxStamina = 100;
        [SerializeField]
        private float staminaRegenRate = 5f;
        private float currentStamina;
        private bool isRegenerating = true;
        [Header("Stamina Display")]
        [SerializeField]
        private float staminaDisplaySpeed = 5f;
        private float displayStamina;
        private Coroutine staminaDisplayCoroutine;

        #region Properties

        public float MaxHealth { get => maxHealth; set => maxHealth = value; }

        public float Health
        {
            get => health;
            set
            {
                health = Mathf.Clamp(value, 0, maxHealth);
                healthChanged?.Invoke(health, maxHealth);

                //Death
                if (health <= 0)
                {
                    IsAlive = false;
                }
            }
        }

        public bool IsAlive
        {
            get => isAlive;
            private set
            {
                isAlive = value;
                animator.SetBool(AnimationStrings.IsAlive, value);
            }
        }

        public bool LockVelocity
        {
            get => animator.GetBool(AnimationStrings.LockVelocity);
            set => animator.SetBool(AnimationStrings.LockVelocity, value);
        }

        public float MaxStamina => maxStamina;
        public float CurrentStamina => currentStamina;

        #endregion

        void Awake()
        {
            animator = GetComponent<Animator>();
            currentStamina = maxStamina;
            displayStamina = maxStamina;
        }

        void Update()
        {
            // Handle invincibility
            if (isInvincible)
            {
                if (timeSinceHit > invincibilityTime)
                {
                    isInvincible = false;
                    timeSinceHit = 0;
                }
                timeSinceHit += Time.deltaTime;
            }

            // Handle stamina regeneration
            if (isRegenerating && currentStamina < maxStamina)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
                UpdateDisplayStamina();
            }
        }

        #region Health Methods

        public bool Hit(float damage, Vector2 knockback)
        {
            if (IsAlive && !isInvincible)
            {
                if (hitStopManager != null)
                    hitStopManager.TriggerHitStop();

                Health -= damage;
                isInvincible = true;

                // Notify other scripts that this object was hit to handle knockback, etc.
                animator.SetTrigger(AnimationStrings.HitTrigger);
                LockVelocity = true;
                damageableHit?.Invoke(damage, knockback);
                CharacterEvents.CharacterDamaged.Invoke(gameObject, damage);

                return true;
            }
            return false;
        }

        public bool Heal(float healedValue)
        {
            if (IsAlive)
            {
                Health += healedValue;
                CharacterEvents.CharacterHealed.Invoke(gameObject, healedValue);
                return true;
            }
            return false;
        }

        #endregion

        #region Stamina Methods

        public bool TryUseStamina(float amount)
        {
            if (currentStamina >= amount)
            {
                currentStamina -= amount;
                UpdateDisplayStamina();
                return true;
            }
            return false;
        }

        private void UpdateDisplayStamina()
        {
            if (staminaDisplayCoroutine != null)
                StopCoroutine(staminaDisplayCoroutine);

            staminaDisplayCoroutine = StartCoroutine(SmoothStaminaDisplay());
        }

        private IEnumerator SmoothStaminaDisplay()
        {
            while (Mathf.Abs(displayStamina - currentStamina) > 0.1f)
            {
                displayStamina = Mathf.Lerp(displayStamina, currentStamina, staminaDisplaySpeed * Time.deltaTime);
                staminaChanged?.Invoke(displayStamina, maxStamina);
                yield return null;
            }

            displayStamina = currentStamina;
            staminaChanged?.Invoke(displayStamina, maxStamina);
        }

        public void SetStaminaRegeneration(bool state) => isRegenerating = state;

        #endregion

        // Method for hit stop that was previously in HitStopManager
        public void TriggerHitStop()
        {
            if (hitStopManager != null)
                hitStopManager.TriggerHitStop();
        }
    }
}
